using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using MSFramework.EntityFrameworkCore;

namespace MSFramework.EventSource.EntityFrameworkCore
{
	//public class EventEntryConfiguration : EntityTypeConfigurationBase<EventEntry>
	//{
	//	public override Type DbContextType => typeof(EventSourceDbContext);

	//	public override void Configure(EntityTypeBuilder<EventEntry> builder)
	//	{
	//		builder.ToTable("EventEntry");

	//		builder.HasKey(e => e.EventId);

	//		builder.Property(e => e.EventId)
	//			.IsRequired();
	//		builder.HasIndex(e => e.EventId).IsUnique();
			
	//		builder.Property(e => e.Content)
	//			.IsRequired();

	//		builder.Property(e => e.CreationTime)
	//			.IsRequired();

	//		builder.HasIndex(e => e.CreationTime).IsUnique(false);

	//		builder.Property(e => e.Status)
	//			.IsRequired();

	//		builder.Property(e => e.SentTimes)
	//			.IsRequired();

	//		builder.Property(e => e.EventTypeName)
	//			.IsRequired();
	//	}
	//}
}