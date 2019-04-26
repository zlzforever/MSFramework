using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class AttachmentType : Enumeration
	{
		public static AttachmentType Report = new AttachmentType(1, nameof(Report).ToLowerInvariant());
		public static AttachmentType Invitation = new AttachmentType(2, nameof(Invitation).ToLowerInvariant());
		public static AttachmentType Other = new AttachmentType(3, nameof(Other).ToLowerInvariant());
		
		public AttachmentType(int id, string name) : base(id, name)
		{
		}
	}
}