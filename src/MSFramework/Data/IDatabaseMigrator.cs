using System;

namespace MSFramework.Data
{
	public interface IDatabaseMigrator
	{
		void Migrate(Type type, string connectionString);
	}
}