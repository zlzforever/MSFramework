using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MSFramework.IdentityServer4;

namespace MSFramework.JWT
{
	public static class ServiceCollectionExtensions
	{
		public static MSFrameworkBuilder AddJwtService(this MSFrameworkBuilder builder,
			RsaSecurityKey key, RsaSigningAlgorithm signingAlgorithm = RsaSigningAlgorithm.RS256)
		{
			builder.Services.AddScoped<IdentityServerOptions>();
			builder.Services.AddScoped<ITokenCreationService, DefaultTokenCreationService>();

			var credential = new SigningCredentials(key, CryptoHelper.GetRsaSigningAlgorithmValue(signingAlgorithm));
			
			builder.Services.AddSingleton<ISigningCredentialProvider>(new InMemorySigningCredentialProvider(credential));

			// var keyInfo = new SecurityKeyInfo
			// {
			// 	Key = credential.Key,
			// 	SigningAlgorithm = credential.Algorithm
			// };
			//
			// builder.Services.AddSingleton<IValidationKeysStore>(new InMemoryValidationKeysStore(new[] { keyInfo }));
			
			return builder;
		}


	}
}