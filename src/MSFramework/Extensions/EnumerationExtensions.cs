namespace MSFramework.Extensions
{
	public static class EnumerationExtensions
	{
		public static T ToEnumeration<T>(this string value)
			where T : Domain.Enumeration
		{
			return Domain.Enumeration.FromDisplayName<T>(value);
		}
	}
}