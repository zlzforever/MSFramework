using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MSFramework.Domain;
using MSFramework.EventSource;
using MSFramework.Reflection;
using Ordering.Infrastructure;

namespace Ordering.API.Application.Behaviors
{
	public class TransactionBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	{
		private readonly ILogger _logger;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IEventSourceService _eventSourceService;

		public TransactionBehaviour(IUnitOfWork unitOfWork,
			IEventSourceService eventSourceService,
			ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
		{
			_unitOfWork = unitOfWork;
			_eventSourceService = eventSourceService ??
			                      throw new ArgumentException(nameof(_eventSourceService));
			_logger = logger ?? throw new ArgumentException(nameof(ILogger));
		}

		public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
			RequestHandlerDelegate<TResponse> next)
		{
			try
			{
				await _unitOfWork.BeginOrUseTransactionAsync(cancellationToken);
				await _eventSourceService.PublishEventsAsync();
				return await next();
			}
			catch (Exception ex)
			{
				var typeName = request.GetGenericTypeName();
				_logger.LogError(ex, "ERROR Handling transaction for {CommandName} ({@Command})", typeName, request);

				throw;
			}
		}
	}
}