using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using MicroserviceFramework.Shared;
using Microsoft.AspNetCore.Http;
using ISession = MicroserviceFramework.Application.ISession;

namespace MicroserviceFramework.AspNetCore
{
	public class HttpContextSession : ISession
	{
		private readonly IHttpContextAccessor _accessor;
		private readonly Lazy<string> _userId;
		private readonly Lazy<string> _userName;
		private readonly Lazy<string> _email;
		private readonly Lazy<string> _phone;
		private readonly Lazy<HashSet<string>> _roles;

		public HttpContextSession(IHttpContextAccessor accessor)
		{
			Check.NotNull(accessor, nameof(accessor));

			_accessor = accessor;
			_userId = new Lazy<string>(() =>
			{
				if (_accessor.HttpContext?.User == null)
				{
					return null;
				}

				var value = _accessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = _accessor.HttpContext.User.FindFirst("sid")?.Value;
				}

				if (string.IsNullOrWhiteSpace(value))
				{
					value = _accessor.HttpContext.User.FindFirst("sub")?.Value;
				}

				return value;
			});
			_userName = new Lazy<string>(() =>
			{
				if (_accessor.HttpContext?.User == null)
				{
					return null;
				}

				var value = _accessor.HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = _accessor.HttpContext.User.FindFirst("name")?.Value;
				}

				return value;
			});
			_email = new Lazy<string>(() =>
			{
				if (_accessor.HttpContext?.User == null)
				{
					return null;
				}

				var value = _accessor.HttpContext.User.FindFirst(ClaimTypes.Email)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = _accessor.HttpContext.User.FindFirst("email")?.Value;
				}

				return value;
			});
			_phone = new Lazy<string>(() =>
			{
				if (_accessor.HttpContext?.User == null)
				{
					return null;
				}

				var value = _accessor.HttpContext.User.FindFirst(ClaimTypes.MobilePhone)?.Value;
				if (string.IsNullOrWhiteSpace(value))
				{
					value = _accessor.HttpContext.User.FindFirst("phone_number")?.Value;
				}

				return value;
			});
			_roles = new Lazy<HashSet<string>>(() =>
			{
				var roles = _accessor.HttpContext?.User?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
				return roles == null ? new HashSet<string>() : new HashSet<string>(roles);
			});
		}

		public string UserId => _userId.Value;

		public string UserName => _userName.Value;

		public string Email => _email.Value;

		public string PhoneNumber => _phone.Value;

		public HashSet<string> Roles => _roles.Value;

		public HttpContext HttpContext => _accessor.HttpContext;
	}
}