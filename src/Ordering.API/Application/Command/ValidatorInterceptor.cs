using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.Logging;
using MSFramework.Command;
using MSFramework.Domain;
using MSFramework.Reflection;
using Ordering.Domain;

namespace Ordering.API.Application.Command
{
	public class ValidatorInterceptor<TCommand> : ICommandInterceptor<TCommand> where TCommand : class, ICommand
	{
		private readonly ILogger _logger;
		private readonly IValidator<TCommand>[] _validators;

		public ValidatorInterceptor(
			//IValidator<TCommand>[] validators,
			ILogger<ValidatorInterceptor<TCommand>> logger)
		{
			_validators = new IValidator<TCommand>[0];
			_logger = logger;
		}

		public Task ExecuteAsync(TCommand command, Action<TCommand> next)
		{
			var typeName = command.GetGenericTypeName();

			_logger.LogInformation("----- Validating command {CommandType}", typeName);

			var failures = _validators
				.Select(v => v.Validate(command))
				.SelectMany(result => result.Errors)
				.Where(error => error != null)
				.ToList();

			if (failures.Any())
			{
				_logger.LogWarning(
					"Validation errors - {CommandType} - Command: {@Command} - Errors: {@ValidationErrors}", typeName,
					command, failures);

				throw new OrderingException(
					$"Command Validation Errors for type {typeof(TCommand).Name}",
					new FluentValidation.ValidationException("Validation exception", failures));
			}

			next(command);
			return Task.CompletedTask;
		}
	}
}