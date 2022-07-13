using System.Text.Json;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Newtonsoft;
using MicroserviceFramework.Serialization;
using MongoDB.Bson;
using Newtonsoft.Json;
using Xunit;

namespace MSFramework.Tests
{
	public class SerializationTests
	{
		class Obj
		{
			public ObjectId Id { get; set; }
			public Enum1 Enum { get; set; }
		}

		class Enum1 : Enumeration
		{
			public static Enum1 Graph = new Enum1(nameof(Graph), nameof(Graph));
			public static Enum1 Property = new Enum1(nameof(Property), nameof(Property));

			public Enum1(string id, string name) : base(id, name)
			{
			}
		}

		[Fact]
		public void DeserializeNullToObjectId1()
		{
			var options = new JsonSerializerOptions();
			// options.Converters.Add(new ObjectIdJsonConverter());
			var serializer = new DefaultSerializer(options);

			serializer.Serialize(new { id = ObjectId.GenerateNewId() });

			var json = "{\"id\":null}";
			var obj = serializer.Deserialize<Obj>(json);

			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{\"id\":\"\"}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{\"enum\":\"Graph\"}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);
		}

		[Fact]
		public void DeserializeNullToObjectId2()
		{
			var options = new JsonSerializerSettings();
			options.Converters.Add(new MicroserviceFramework.Newtonsoft.Converters.ObjectIdConverter());
			options.Converters.Add(new MicroserviceFramework.Newtonsoft.Converters.EnumerationConverter());
			JsonConvert.DefaultSettings = () => options;
			var serializer = new NewtonsoftSerializer();

			serializer.Serialize(new { id = ObjectId.GenerateNewId() });

			var json = "{\"id\":null}";
			var obj = serializer.Deserialize<Obj>(json);

			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{\"id\":\"\"}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);

			json = "{\"enum\":\"Graph\"}";
			obj = serializer.Deserialize<Obj>(json);
			Assert.Equal(ObjectId.Empty, obj.Id);
		}
	}
}