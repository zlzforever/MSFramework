using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using MicroserviceFramework.Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Template.Application.Products;
using Template.Application.Products.Commands;
using Template.Application.Products.Queries;

namespace Template.API.Controllers;

/// <summary>商品业务端点，依赖应用层命令和读取契约，统一要求 JWT Api 策略。</summary>
/// <param name="mediator">框架命令与查询分发器。</param>
/// <param name="queries">商品只读查询契约。</param>
[ApiController]
[Route("api/v1.0/products")]
[Authorize(Policy = "Api")]
public class ProductController(IMediator mediator, IProductQueries queries) : ApiControllerBase
{
    /// <summary>按查询字符串分页读取商品。</summary>
    /// <param name="query">经模型验证的分页条件。</param>
    /// <param name="cancellationToken">HTTP 请求取消令牌。</param>
    /// <returns>数据库分页结果，响应由框架统一包装。</returns>
    /// <exception cref="ArgumentNullException">查询为空。</exception>
    /// <exception cref="OperationCanceledException">请求取消。</exception>
    [HttpGet]
    public Task<PaginationResult<ProductOut>> PagedQuery([FromQuery] PagedProductQuery query,
        CancellationToken cancellationToken) => mediator.SendAsync(query, cancellationToken);

    /// <summary>读取单个商品，不存在时返回 404。</summary>
    /// <param name="id">路由中的 ObjectId，由框架模型绑定器校验。</param>
    /// <param name="cancellationToken">HTTP 请求取消令牌。</param>
    /// <returns>商品输出或 404。</returns>
    /// <exception cref="OperationCanceledException">请求取消。</exception>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductOut>> Get([FromRoute] ObjectId id, CancellationToken cancellationToken)
    {
        var product = await queries.GetAsync(id, cancellationToken);
        return product == null ? NotFound() : Ok(product);
    }

    /// <summary>创建商品，由外层工作单元统一提交。</summary>
    /// <param name="command">经验证的商品名称、分单位价格及类别。</param>
    /// <param name="cancellationToken">HTTP 请求取消令牌。</param>
    /// <returns>新商品标识。</returns>
    /// <exception cref="ArgumentNullException">命令为空。</exception>
    /// <exception cref="MicroserviceFramework.MicroserviceFrameworkFriendlyException">领域约束不满足。</exception>
    /// <exception cref="OperationCanceledException">请求取消。</exception>
    [HttpPost]
    public Task<ObjectId> Create([FromBody] CreateProductCommand command, CancellationToken cancellationToken) =>
        mediator.SendAsync(command, cancellationToken);

    /// <summary>按路由标识幂等软删除商品。</summary>
    /// <param name="id">待删除商品的 ObjectId。</param>
    /// <param name="cancellationToken">HTTP 请求取消令牌。</param>
    /// <returns>标记完成的任务，提交由框架负责。</returns>
    /// <exception cref="OperationCanceledException">请求取消。</exception>
    [HttpDelete("{id}")]
    public Task Delete([FromRoute] ObjectId id, CancellationToken cancellationToken) =>
        mediator.SendAsync(new DeleteProductCommand(id), cancellationToken);
}
