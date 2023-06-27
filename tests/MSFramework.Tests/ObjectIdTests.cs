// using MicroserviceFramework.Shared;
// using MongoDB.Bson;
// using Xunit;
//
// namespace MSFramework.Tests
// {
// 	public class ObjectIdTests
// 	{
// 		[Fact]
// 		public void ObjectIdTest()
// 		{
// 			var empty1 = ObjectId.Empty;
// 			var empty2 = new ObjectId("000000000000000000000000");
// 			var a = empty1 == empty2;
// 			Assert.True(a);
// 			Assert.True(empty1.Equals(empty2));
//
// 			Assert.False(empty1.Equals(null));
//
// 			var id1 = ObjectId.NewId();
// 			var id2 = new ObjectId(id1.ToString());
// 			Assert.True(id1 == id2);
// 			Assert.True(id1.Equals(id2));
// 			Assert.True(id1 != default);
// 			Assert.False(id1 == default);
// 		}
// 	}
// }
