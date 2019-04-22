using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.CQRS.EventSouring;

namespace MSFramework.EntityFrameworkCore
{
	internal class EventSourceEntryConfiguration : EntityTypeConfigurationBase<EventSourceEntry>
	{
		public override void Configure(EntityTypeBuilder<EventSourceEntry> builder)
		{
			builder.ToTable("EventSource");
			builder.Property("EventType");
			builder.Property("Event");
			builder.Property("AggregateId");
			builder.Property("Version");
			builder.Property("CreationTime");
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => new {x.AggregateId, x.Version}).IsUnique();
			builder.HasIndex(x => x.CreationTime);
		}
	}
}