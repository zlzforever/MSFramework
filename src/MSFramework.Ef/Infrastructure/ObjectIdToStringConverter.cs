using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MSFramework.Common;

namespace MSFramework.Ef.Infrastructure
{
	public class ObjectIdToStringConverter
		: StringObjectIdConverter<ObjectId, string>
	{
		public ObjectIdToStringConverter(ConverterMappingHints mappingHints = null)
			: base(ToString(), ToObjectId(), mappingHints ?? DefaultHints)
		{
		}
	}
}