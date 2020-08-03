using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MSFramework.Common;

namespace MSFramework.Ef.Infrastructure
{
	public class StringObjectIdConverter<TModel, TProvider> : ValueConverter<TModel, TProvider>
	{
		/// <summary>
		///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
		///     the same compatibility standards as public APIs. It may be changed or removed without notice in
		///     any release. You should only use it directly in your code with extreme caution and knowing that
		///     doing so can result in application failures when updating to a new Entity Framework Core release.
		/// </summary>
		protected static readonly ConverterMappingHints DefaultHints = new ConverterMappingHints(36,
			null, null, null,
			(p, t) => (ValueGenerator) new ObjectIdValueGenerator());

		/// <summary>
		///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
		///     the same compatibility standards as public APIs. It may be changed or removed without notice in
		///     any release. You should only use it directly in your code with extreme caution and knowing that
		///     doing so can result in application failures when updating to a new Entity Framework Core release.
		/// </summary>
		public StringObjectIdConverter(
			Expression<Func<TModel, TProvider>> convertToProviderExpression,
			Expression<Func<TProvider, TModel>> convertFromProviderExpression,
			ConverterMappingHints mappingHints = null) : base(convertToProviderExpression,
			convertFromProviderExpression, mappingHints)
		{
		}

		/// <summary>
		///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
		///     the same compatibility standards as public APIs. It may be changed or removed without notice in
		///     any release. You should only use it directly in your code with extreme caution and knowing that
		///     doing so can result in application failures when updating to a new Entity Framework Core release.
		/// </summary>
		protected new static Expression<Func<ObjectId, string>> ToString()
			=> v => v.ToString();

		/// <summary>
		///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
		///     the same compatibility standards as public APIs. It may be changed or removed without notice in
		///     any release. You should only use it directly in your code with extreme caution and knowing that
		///     doing so can result in application failures when updating to a new Entity Framework Core release.
		/// </summary>
		protected static Expression<Func<string, ObjectId>> ToObjectId()
			=> v => v == null ? default : new ObjectId(v);
	}
}