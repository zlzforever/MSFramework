using System.Text.Json;
using MicroserviceFramework.Serialization;
using MicroserviceFramework.Serialization.Converters;
using MicroserviceFramework.Shared;
using Xunit;

namespace MSFramework.Tests
{
	public class SerializationTests
	{
		class Obj
		{
			public ObjectId Id { get; set; }
		}

		[Fact]
		public void DeserializeNullToObjectId()
		{
			var options = new JsonSerializerOptions();
			options.Converters.Add(new ObjectIdJsonConverter());
			var serializer = new DefaultSerializer(options);
			var json = "{\"id\":null}";
			var obj = serializer.Deserialize<Obj>(json);
			serializer.Serialize(new {id = ObjectId.NewId()});
			Assert.Equal(ObjectId.Empty, obj.Id);
		}
	}
}