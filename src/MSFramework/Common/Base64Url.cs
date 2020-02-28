using System;

namespace MSFramework.Common
{
	/// <summary>Base64Url encoder/decoder</summary>
	public static class Base64Url
	{
		/// <summary>Encodes the specified byte array.</summary>
		/// <param name="arg">The argument.</param>
		/// <returns></returns>
		public static string Encode(byte[] arg)
		{
			return Convert.ToBase64String(arg).Split('=')[0].Replace('+', '-').Replace('/', '_');
		}

		/// <summary>Decodes the specified string.</summary>
		/// <param name="arg">The argument.</param>
		/// <returns></returns>
		/// <exception cref="T:System.Exception">Illegal base64url string!</exception>
		public static byte[] Decode(string arg)
		{
			string s = arg.Replace('-', '+').Replace('_', '/');
			switch (s.Length % 4)
			{
				case 0:
					return Convert.FromBase64String(s);
				case 2:
					s += "==";
					goto case 0;
				case 3:
					s += "=";
					goto case 0;
				default:
					throw new Exception("Illegal base64url string!");
			}
		}
	}
}