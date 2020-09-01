using System;
using MicroserviceFramework.Shared;
using MicroserviceFramework.Utilities;
using Xunit;

namespace MSFramework.Tests
{
	public class NumberEncodingTests
	{
		[Fact]
		public void NumberEncodingTest()
		{
			var random = new Random();
			for (var i = 0; i < 10000; ++i)
			{
				var number = random.Next(0, int.MaxValue);
				var a = BaseX.ToString(number);
				var b = BaseX.ToInt32(a);
				Assert.Equal(number, b);
			}
		}
	}
}