using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using DotNetCore.CAP.Dapr;
using DotNetCore.CAP.Messages;
using MicroserviceFramework;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using MicroserviceFramework.Ef.PostgreSql;
using MicroserviceFramework.EventBus;
using MicroserviceFramework.Extensions.DependencyInjection;
using MicroserviceFramework.Text.Json;
using Ordering.Infrastructure;

var webApplicationBuilder = WebApplication.CreateBuilder(args);
webApplicationBuilder.WebHost.UseUrls("http://+:5002");

// Add services to the container.

webApplicationBuilder.Services.AddControllers().AddDapr(x =>
{
    x.UseGrpcEndpoint("http://localhost:51002");
    x.UseHttpEndpoint("http://localhost:50002");
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
webApplicationBuilder.Services.AddEndpointsApiExplorer();
webApplicationBuilder.Services.AddSwaggerGen();
webApplicationBuilder.Services.AddMicroserviceFramework(builder =>
{
    builder.UseAssemblyScanPrefix("Ordering");
    builder.UseDependencyInjectionLoader();
    builder.UseOptionsType(webApplicationBuilder.Configuration);
    builder.UseAutoMapperObjectAssembler();
    // builder.UseMediator();
    builder.UseLocalEventPublisher();
    builder.UseAspNetCore();
    // builder.UseNewtonsoftJsonHelper(settings);

    builder.UseEntityFramework(x =>
    {
        // 添加 MySql 支持
        x.AddNpgsql<OrderingContext>(webApplicationBuilder.Configuration);
    });
});
webApplicationBuilder.Services.AddCap(x =>
{
    x.UseEntityFramework<OrderingContext>(y =>
    {
        y.Schema = "ordering_subscribe";
    });
    x.JsonSerializerOptions.AddDefaultConverters();
    x.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    // var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD");
    // x.UseRabbitMQ(configure =>
    // {
    //     configure.HostName = "192.168.100.254";
    //     configure.Password = password;
    //     configure.UserName = "admin";
    //     // configure.ExchangeName = "ordering";
    // });
    x.UseDapr(y => y.Pubsub = "rabbitmq-pubsub");
    x.FailedRetryCount = 3;
    x.FailedMessageExpiredAfter = 365 * 24 * 3600;
    x.FailedThresholdCallback += failed =>
    {
        var traceId = failed.Message.Headers[Headers.MessageId];
        var messageBuilder = new StringBuilder($"消息名称: {failed.Message.GetName()}[{failed.MessageType}] ");
        messageBuilder.AppendLine();
        messageBuilder.AppendLine($"消息组: {failed.Message.GetGroup()}[CAP]");
        messageBuilder.AppendLine("消息ID(Content):");
        messageBuilder.AppendLine(failed.Message.GetId());
        messageBuilder.AppendLine($"日志跟踪标识: {traceId}");
        messageBuilder.AppendLine($"发送时间: {failed.Message.Headers[Headers.SentTime]}");
        messageBuilder.AppendLine("错误消息:");
        messageBuilder.AppendLine(failed.Message.Headers[Headers.Exception]);
        var logger = failed.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("CAP");
        logger.LogError(messageBuilder.ToString());
    };
    x.TopicNamePrefix = "CAP";
    x.UseDashboard();
});
var app = webApplicationBuilder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCloudEvents();
app.MapSubscribeHandler();
app.UseDaprCap();
app.MapControllers();


app.Run();
