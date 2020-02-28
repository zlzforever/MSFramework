using System;
using Microsoft.IdentityModel.Tokens;

namespace MSFramework.IdentityServer4
{
	public static class CryptoHelper
	{
		internal static string GetRsaSigningAlgorithmValue(RsaSigningAlgorithm value)
		{
			return value switch
			{
				RsaSigningAlgorithm.RS256 => SecurityAlgorithms.RsaSha256,
				RsaSigningAlgorithm.RS384 => SecurityAlgorithms.RsaSha384,
				RsaSigningAlgorithm.RS512 => SecurityAlgorithms.RsaSha512,

				RsaSigningAlgorithm.PS256 => SecurityAlgorithms.RsaSsaPssSha256,
				RsaSigningAlgorithm.PS384 => SecurityAlgorithms.RsaSsaPssSha384,
				RsaSigningAlgorithm.PS512 => SecurityAlgorithms.RsaSsaPssSha512,
				_ => throw new ArgumentException("Invalid RSA signing algorithm value", nameof(value)),
			};
		}
	}
}