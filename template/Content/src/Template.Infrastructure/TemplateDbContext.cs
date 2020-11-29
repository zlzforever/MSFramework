using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Ef;
using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure
{
	public class TemplateDbContext : DbContextBase
	{
		public TemplateDbContext(DbContextOptions options, UnitOfWorkManager unitOfWorkManager,
			IServiceProvider serviceProvider) : base(options, unitOfWorkManager, serviceProvider)
		{
		}
	}
}