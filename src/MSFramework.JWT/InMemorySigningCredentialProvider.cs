using Microsoft.IdentityModel.Tokens;
using MSFramework.Data;

namespace MSFramework.IdentityServer4
{
	public class InMemorySigningCredentialProvider : ISigningCredentialProvider
	{
		private readonly SigningCredentials _credentials;

		public InMemorySigningCredentialProvider(SigningCredentials credentials)
		{
			credentials.NotNull(nameof(credentials));
			_credentials = credentials;
		}

		public SigningCredentials Get()
		{
			return _credentials;
		}
	}
}