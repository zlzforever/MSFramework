using System;

namespace MSFramework.Data
{
	public interface IDatabaseMigration
	{
		void Migrate(Type type, string connectionString);
	}
}