using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace MSFramework.IdentityServer4
{
	public class ClaimComparer : EqualityComparer<Claim>
	{
		private readonly Options _options = new Options();

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdentityModel.ClaimComparer" /> class with default options.
		/// </summary>
		public ClaimComparer()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdentityModel.ClaimComparer" /> class with given comparison options.
		/// </summary>
		/// <param name="options">Comparison options.</param>
		public ClaimComparer(Options options)
		{
			Options options1 = options;
			_options = options1 ?? throw new ArgumentNullException(nameof(options));
		}

		/// <inheritdoc />
		public override bool Equals(Claim x, Claim y)
		{
			if (x == null && y == null)
				return true;
			if (x == null && y != null || x != null && y == null)
				return false;
			StringComparison comparisonType = StringComparison.Ordinal;
			if (_options.IgnoreValueCase)
				comparisonType = StringComparison.OrdinalIgnoreCase;
			bool flag = string.Equals(x.Type, y.Type, StringComparison.OrdinalIgnoreCase) &&
			            string.Equals(x.Value, y.Value, comparisonType) &&
			            string.Equals(x.ValueType, y.ValueType, StringComparison.Ordinal);
			if (_options.IgnoreIssuer)
				return flag;
			return flag && string.Equals(x.Issuer, y.Issuer, comparisonType);
		}

		/// <inheritdoc />
		public override int GetHashCode(Claim claim)
		{
			if (claim == null)
				return 0;
			string type = claim.Type;
			int num1;
			if (type == null)
			{
				int? hashCode = claim.ValueType?.GetHashCode();
				num1 = (hashCode.HasValue ? new int?(0 ^ hashCode.GetValueOrDefault()) : new int?())
					.GetValueOrDefault();
			}
			else
				num1 = type.ToLowerInvariant().GetHashCode();

			int num2 = num1;
			int num3;
			int num4;
			if (_options.IgnoreValueCase)
			{
				string str = claim.Value;
				num3 = str != null ? str.ToLowerInvariant().GetHashCode() : 0;
				string issuer = claim.Issuer;
				num4 = issuer != null ? issuer.ToLowerInvariant().GetHashCode() : 0;
			}
			else
			{
				string str = claim.Value;
				num3 = str != null ? str.GetHashCode() : 0;
				string issuer = claim.Issuer;
				num4 = issuer != null ? issuer.GetHashCode() : 0;
			}

			return _options.IgnoreIssuer ? num2 ^ num3 : num2 ^ num3 ^ num4;
		}

		/// <summary>Claim comparison options</summary>
		public class Options
		{
			/// <summary>
			/// Specifies if the issuer value is being taken into account
			/// </summary>
			public bool IgnoreIssuer { get; set; }

			/// <summary>
			/// Specifies if claim and issuer value comparison should be case-sensitive
			/// </summary>
			public bool IgnoreValueCase { get; set; }
		}
	}
}