using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Auditing.Model;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace MicroserviceFramework.LocalEvent;

/// <summary>
///
/// </summary>
/// <param name="serviceProvider"></param>
/// <param name="logger"></param>
/// <param name="localEventOptions"></param>
public class LocalEventService(
    IServiceProvider serviceProvider,
    ILogger<LocalEventService> logger,
    IOptions<LocalEventOptions> localEventOptions)
    : BackgroundService
{
    private static readonly Dictionary<Type, HashSet<EventDescriptor>> Descriptors =
        new();

    /// <summary>
    ///
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="handlerType"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void Register(Type eventType, Type handlerType)
    {
        var method = handlerType.GetMethod("HandleAsync", [eventType, typeof(CancellationToken)]);
        if (method == null)
        {
            throw new ArgumentException($"事件处理器 {handlerType.FullName} 没有实现事件 {eventType.FullName} 的处理方法");
        }

        if (!Descriptors.ContainsKey(eventType))
        {
            Descriptors.Add(eventType, new HashSet<EventDescriptor>());
        }

        Descriptors[eventType].Add(new EventDescriptor(handlerType, method));
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns></returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Factory.StartNew(async () =>
        {
            logger.LogInformation("本地事件消费服务启动");

            while (await LocalEventPublisher.EventChannel.Reader.WaitToReadAsync(stoppingToken))
            {
                while (LocalEventPublisher.EventChannel.Reader.TryRead(out var entry))
                {
                    try
                    {
                        var traceId = entry.Session == null
                            ? ObjectId.GenerateNewId().ToString()
                            : entry.Session.TraceIdentifier;
                        var eventType = entry.EventData.GetType();
                        var descriptors = GetDescriptors(eventType);

                        foreach (var descriptor in descriptors)
                        {
                            var handlerName = descriptor.HandlerType.FullName;

                            logger.LogDebug("{TraceId}, 处理器 {HandlerType} 执行开始", traceId, handlerName);

                            using var scope = serviceProvider.CreateScope();
                            var services = scope.ServiceProvider;
                            var handler = services.GetService(descriptor.HandlerType);
                            if (handler == null)
                            {
                                return;
                            }

                            var session = services.GetService<ISession>();
                            // 覆盖 session 对象
                            if (entry.Session != null)
                            {
                                session?.Load(entry.Session);
                            }

                            if (localEventOptions.Value.EnableAuditing)
                            {
                                var unitOfWork = services.GetService<IUnitOfWork>();
                                unitOfWork?.SetAuditOperationFactory(() =>
                                    CreateAuditedOperation(session, handlerName));
                            }

                            if (descriptor.HandleMethod.Invoke(handler, [entry.EventData, stoppingToken]) is not Task
                                task)
                            {
                                continue;
                            }

                            try
                            {
                                await task;
                                var unitOfWork = services.GetService<IUnitOfWork>();
                                if (unitOfWork != null)
                                {
                                    await unitOfWork.SaveChangesAsync(stoppingToken);
                                }

                                logger.LogDebug(
                                    "{TraceId}, 处理器 {HandlerType} 执行结束", traceId, handlerName);
                            }
                            catch (Exception e)
                            {
                                logger.LogError(e, "{TraceId}, 处理器 {HandlerType} 执行失败",
                                    traceId, handlerName);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "处理事件失败: {EventData}",
                            Defaults.JsonSerializer.Serialize(entry));
                    }
                }
            }
        }, stoppingToken);
    }

    private static IReadOnlyCollection<EventDescriptor> GetDescriptors(Type eventType)
    {
        if (!Descriptors.TryGetValue(eventType, out var value))
        {
            return Array.Empty<EventDescriptor>();
        }

        return value;
    }

    private AuditOperation CreateAuditedOperation(ISession session, string handlerType)
    {
        var auditedOperation = new AuditOperation(handlerType, null, null, null, null,
            null, null, session.TraceIdentifier);
        auditedOperation.SetCreation(session.UserId, session.UserDisplayName, DateTimeOffset.Now);
        return auditedOperation;
    }
}
