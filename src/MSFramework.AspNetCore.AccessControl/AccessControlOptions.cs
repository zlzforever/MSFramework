using MSFramework.Application;

namespace MSFramework.AspNetCore.AccessControl
{
	[ConfigType("AccessControl")]
	public class AccessControlOptions
	{
		public string ServiceUrl { get; set; }

		public int CacheTTL { get; set; } = 5;
	}
}