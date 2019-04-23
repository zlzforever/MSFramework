using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MSFramework.EntityFrameworkCore;

namespace MSFramework.EventSouring.EntityFrameworkCore
{
	public class EventHistoryEntityConfiguration : EntityTypeConfigurationBase<EventHistory>
	{
		public override Type DbContextType => typeof(EventSouringDbContext);

		public override void Configure(EntityTypeBuilder<EventHistory> builder)
		{
			builder.ToTable("EventHistory");
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