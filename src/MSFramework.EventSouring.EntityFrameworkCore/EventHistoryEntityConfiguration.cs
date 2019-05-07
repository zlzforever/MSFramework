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
			builder.Property(x => x.EventType);
			builder.Property(x => x.Event);
			builder.Property(x => x.AggregateRootId);
			builder.Property(x => x.Version);
			builder.Property(x => x.Creator);
			builder.Property(x => x.CreatorId);
			builder.Property(x => x.Timestamp);
			builder.HasKey(x => x.Id);
			builder.HasIndex(x => new {AggregateId = x.AggregateRootId, x.Version}).IsUnique();
			builder.HasIndex(x => x.Timestamp);
		}
	}
}