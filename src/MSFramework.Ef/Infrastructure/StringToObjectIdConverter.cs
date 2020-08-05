using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MSFramework.Shared;

namespace MSFramework.Ef.Infrastructure
{
	public class StringToObjectIdConverter : StringObjectIdConverter<string, ObjectId>
	{
		public StringToObjectIdConverter(ConverterMappingHints mappingHints = null)
			: base(ToObjectId(), ToString(), mappingHints ?? DefaultHints)
		{
		}
	}
}