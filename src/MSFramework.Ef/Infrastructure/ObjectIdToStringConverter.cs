using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Infrastructure
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