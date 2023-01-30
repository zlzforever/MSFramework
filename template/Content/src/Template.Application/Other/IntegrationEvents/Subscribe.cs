using MongoDB.Bson;

namespace Template.Application.Other.IntegrationEvents;

public static class Subscribe
{
	public class ProjectCreatedEvent 
	{
		public ObjectId Id { get; set; }
	}
}