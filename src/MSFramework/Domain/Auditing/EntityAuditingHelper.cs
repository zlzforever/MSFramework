using System;

namespace MSFramework.Domain.Auditing
{
	public static class EntityAuditingHelper
	{
		public static void SetCreationAuditProperties(
			object entityAsObj,
			string userId)
		{
			if (entityAsObj is IHasCreationTime e1)
			{
				if (e1.CreationTime == default)
				{
					e1.CreationTime = DateTime.Now;
				}
			}

			if (entityAsObj is ICreationAudited e2)
			{
				e2.CreatorUserId = userId;
			}
		}

		public static void SetModificationAuditProperties(
			object entityAsObj,
			string userId)
		{
			if (entityAsObj is IHasModificationTime e1)
			{
				e1.LastModificationTime = DateTime.Now;
			}

			if (entityAsObj is IModificationAudited e2)
			{
				e2.LastModifierUserId = userId;
			}
		}
	}
}