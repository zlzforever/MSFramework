using System;
using Xunit;

namespace MSFramework.Tests
{
	public class NumberEncodingTests
	{
		[Fact]
		public void NumberEncoding()
		{
			var random = new Random();
			for (var i = 0; i < 10000; ++i)
			{
				var number = random.Next(0, int.MaxValue);
				var a = Utilities.NumberEncoding.ToString(number);
				var b = Utilities.NumberEncoding.ToInt32(a);
				Assert.Equal(number, b);
			}
		}
	}
}