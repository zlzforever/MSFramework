using System;
using MSFramework.Domain;

namespace ServicePlan.Domain.AggregateRoot
{
	public class Attachment : EntityBase<Guid>
	{
		/// <summary>
		/// 附件类型
		/// </summary>
		private string _type;

		/// <summary>
		/// 附件名称
		/// </summary>
		private string _name;

		/// <summary>
		/// 路径
		/// </summary>
		private string _path;

		/// <summary>
		/// 创建时间
		/// </summary>
		private DateTime _creationTime;

		public Attachment(string type, string name, string path, DateTime creationTime)
		{
			_type = type;
			_name = name;
			_path = path;
			_creationTime = creationTime;
		}
	}
}