using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Infrastructure
{
	public class StringToObjectIdConverter : StringObjectIdConverter<string, ObjectId>
	{
		public StringToObjectIdConverter(ConverterMappingHints mappingHints = null)
			: base(ToObjectId(), ToString(), mappingHints ?? DefaultHints)
		{
		}
	}
}