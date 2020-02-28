using Microsoft.IdentityModel.Tokens;

namespace MSFramework.IdentityServer4
{
	public interface ISigningCredentialProvider
	{
		SigningCredentials Get();
	}
}