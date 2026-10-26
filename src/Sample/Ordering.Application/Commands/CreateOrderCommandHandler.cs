using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MicroserviceFramework.Application;
using MicroserviceFramework.Mediator;
using Microsoft.EntityFrameworkCore;
using Ordering.Domain;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Order;
using Ordering.Domain.Repositories.Order;
using CatalogProduct = Ordering.Domain.AggregateRoots.Product;

namespace Ordering.Application.Commands;

/// <summary>
/// 使用认证会话与一次商品批量查询构造订单，依赖仓储、会话及应用层上下文工厂；由外层工作单元提交。
/// </summary>
public class CreateOrderCommandHandler(
    IOrderRepository orderRepository,
    ISession session,
    IDbContextFactory dbContextFactory)
    : IRequestHandler<CreateOrderCommand, string>
{
    /// <summary>校验购买明细，批量读取服务端价格，再创建待提交订单。</summary>
    /// <param name="command">购买意向，明细不能为空且最多 100 行，商品标识和数量必须有效。</param>
    /// <param name="cancellationToken">请求取消令牌，传递给目录查询。</param>
    /// <returns>新订单标识，持久化由外层工作单元完成。</returns>
    /// <exception cref="ArgumentNullException">命令为空。</exception>
    /// <exception cref="UnauthorizedAccessException">会话中没有有效用户。</exception>
    /// <exception cref="OrderingDomainException">明细无效、商品不存在或价格不合法。</exception>
    /// <exception cref="InvalidOperationException">上下文工厂未提供商品上下文。</exception>
    /// <exception cref="OperationCanceledException">查询被取消。</exception>
    public async Task<string> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(session?.UserId))
        {
            throw new UnauthorizedAccessException("下单需要已认证用户。");
        }

        if (command.OrderItems is not { Count: > 0 and <= 100 } ||
            command.OrderItems.Any(item => item == null || string.IsNullOrWhiteSpace(item.ProductId) || item.Units <= 0))
        {
            throw new OrderingDomainException("订单明细必须包含 1 至 100 行有效商品和正数数量。");
        }

        // 所有商品一次 IN 查询；不能在明细循环中逐条读取价格。
        var productIds = command.OrderItems.Select(item => item.ProductId).Distinct().ToArray();
        var context = dbContextFactory.GetDbContext<CatalogProduct>()
            ?? throw new InvalidOperationException("未注册商品目录上下文。");
        var products = await context.Set<CatalogProduct>().AsNoTracking()
            .Where(product => productIds.Contains(product.Id))
            .ToDictionaryAsync(product => product.Id, cancellationToken);
        if (products.Count != productIds.Length || products.Values.Any(product => product.Price < 0))
        {
            throw new OrderingDomainException("商品不存在或价格无效。");
        }

        var order = Order.Create(UserInfo.Create(session.UserId, session.UserDisplayName),
            new Address
            {
                Street = command.Street, City = command.City, State = command.State,
                Country = command.Country, ZipCode = command.ZipCode
            }, command.Description);

        // 样例当前没有促销引擎：折扣固定为零，价格保持商品目录已有的货币单位，不接受客户端金额。
        foreach (var item in command.OrderItems)
        {
            var product = products[item.ProductId];
            order.AddItem(product.Id, product.Name, null, product.Price, item.Units, 0);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await orderRepository.AddAsync(order);
        return order.Id;
    }
}
