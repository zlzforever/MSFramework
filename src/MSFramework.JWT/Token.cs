using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace MSFramework.IdentityServer4
{
	/// <summary>Models a token.</summary>
	public class Token
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdentityServer4.Models.Token" /> class.
		/// </summary>
		public Token()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:IdentityServer4.Models.Token" /> class.
		/// </summary>
		/// <param name="tokenType">Type of the token.</param>
		public Token(string tokenType)
		{
			this.Type = tokenType;
		}

		/// <summary>
		/// A list of allowed algorithm for signing the token. If null or empty, will use the default algorithm.
		/// </summary>
		public ICollection<string> AllowedSigningAlgorithms { get; set; } = (ICollection<string>) new HashSet<string>();

		/// <summary>
		/// Specifies the confirmation method of the token. This value, if set, will become the cnf claim.
		/// </summary>
		public string Confirmation { get; set; }

		/// <summary>Gets or sets the audiences.</summary>
		/// <value>The audiences.</value>
		public ICollection<string> Audiences { get; set; } = (ICollection<string>) new HashSet<string>();

		/// <summary>Gets or sets the issuer.</summary>
		/// <value>The issuer.</value>
		public string Issuer { get; set; }

		/// <summary>Gets or sets the creation time.</summary>
		/// <value>The creation time.</value>
		public DateTime CreationTime { get; set; }

		/// <summary>Gets or sets the lifetime.</summary>
		/// <value>The lifetime.</value>
		public int Lifetime { get; set; }

		/// <summary>Gets or sets the type.</summary>
		/// <value>The type.</value>
		public string Type { get; set; } = "access_token";

		/// <summary>Gets or sets the ID of the client.</summary>
		/// <value>The ID of the client.</value>
		public string ClientId { get; set; }

		/// <summary>Gets or sets the claims.</summary>
		/// <value>The claims.</value>
		public ICollection<Claim> Claims { get; set; } =
			(ICollection<Claim>) new HashSet<Claim>(new ClaimComparer());

		/// <summary>Gets or sets the version.</summary>
		/// <value>The version.</value>
		public int Version { get; set; } = 4;

		/// <summary>Gets the subject identifier.</summary>
		/// <value>The subject identifier.</value>
		public string SubjectId
		{
			get
			{
				return this.Claims.Where(x => x.Type == "sub")
					.Select(x => x.Value).SingleOrDefault();
			}
		}

		/// <summary>Gets the scopes.</summary>
		/// <value>The scopes.</value>
		public IEnumerable<string> Scopes
		{
			get
			{
				return this.Claims.Where(x => x.Type == "scope")
					.Select(x => x.Value);
			}
		}
	}
}