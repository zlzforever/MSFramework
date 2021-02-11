using System;
using System.Linq.Expressions;
using MicroserviceFramework.Shared;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MicroserviceFramework.Ef.Internal
{
	internal class ObjectIdToStringConverter
		: ValueConverter<ObjectId, string>
	{
		private new static Expression<Func<ObjectId, string>> ToString()
			=> v => v.ToString();

		private static Expression<Func<string, ObjectId>> ToObjectId()
			=> v => v == null ? default : new ObjectId(v);

		private static readonly ConverterMappingHints DefaultHints = new(36,
			null, null, null,
			(_, _) => new ObjectIdValueGenerator());
		
		public ObjectIdToStringConverter(ConverterMappingHints mappingHints = null)
			: base(ToString(), ToObjectId(), mappingHints ?? DefaultHints)
		{
		}
	}
}