using System.Collections.Generic;
#if NETSTANDARD2_0
using Microsoft.EntityFrameworkCore.Internal;
#else
using Microsoft.EntityFrameworkCore.Infrastructure;
#endif
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace MicroserviceFramework.Ef.Design
{
	public class ClearForeignKeysCSharpMigrationOperationGenerator : CSharpMigrationOperationGenerator
	{
		public ClearForeignKeysCSharpMigrationOperationGenerator(
			CSharpMigrationOperationGeneratorDependencies dependencies) : base(
			dependencies)
		{
		}

		public override void Generate(string builderName, IReadOnlyList<MigrationOperation> operations,
			IndentedStringBuilder builder)
		{
			foreach (var operation in operations)
			{
				if (operation is not CreateTableOperation createTableOperation)
				{
					continue;
				}

				createTableOperation.ForeignKeys.Clear();
			}

			base.Generate(builderName, operations, builder);
		}
	}
}