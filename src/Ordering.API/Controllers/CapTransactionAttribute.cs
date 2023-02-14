using DotNetCore.CAP;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ordering.Infrastructure;

namespace Ordering.API.Controllers;

public class CapTransactionAttribute : ActionFilterAttribute
{
    private IDbContextTransaction _transaction;
    private ILogger _logger;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var dbContext = context.HttpContext.RequestServices.GetRequiredService<OrderingContext>();
        var capPublisher = context.HttpContext.RequestServices.GetService<ICapPublisher>();
        _logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger("CAP.CapTransactionFilter");

        _logger.LogDebug("开启 CAP EF 事务");
        _transaction = dbContext.Database.BeginTransaction(capPublisher);
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception == null)
        {
            _transaction.Commit();
            _logger.LogDebug("提交 CAP EF 事务成功");
        }
        else
        {
            _logger.LogDebug("尝试回滚 CAP EF 事务");
            _transaction.Rollback();
            // comments: 此处是否需要 try catch。暂认为不需要， 如果回滚失败， 全局异常捕获。
            _logger.LogDebug("回滚 CAP EF 事务成功");
        }

        _transaction?.Dispose();
    }
}
