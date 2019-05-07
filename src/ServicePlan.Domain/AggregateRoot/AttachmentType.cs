using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class AttachmentType : Enumeration
	{
		/// <summary>
		/// 报告
		/// </summary>
		public static AttachmentType Report = new AttachmentType(1, nameof(Report).ToLowerInvariant());
		
		/// <summary>
		/// 邀请
		/// </summary>
		public static AttachmentType Invitation = new AttachmentType(2, nameof(Invitation).ToLowerInvariant());
		
		/// <summary>
		/// 其他
		/// </summary>
		public static AttachmentType Other = new AttachmentType(3, nameof(Other).ToLowerInvariant());
		
		public AttachmentType(int id, string name) : base(id, name)
		{
		}
	}
}