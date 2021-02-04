using System;
using MicroserviceFramework.Domain;
using MicroserviceFramework.Shared;

namespace MicroserviceFramework.FeatureManagement
{
	public class Feature : AggregateRoot
	{
		public bool Enabled { get; private set; }

		/// <summary>
		/// 功能名称，必须是唯一的
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// 功能描述
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// 是否过期
		/// </summary>
		public bool Expired { get; private set; }

		/// <summary>
		/// 创建时间
		/// </summary>
		public DateTimeOffset CreationTime { get; private set; }

		/// <summary>
		/// The last modified time for this entity.
		/// </summary>
		public DateTimeOffset? ModificationTime { get; private set; }

		public Feature(string name, string description) : base(ObjectId.NewId())
		{
			Name = name;
			Description = description;
			CreationTime = DateTimeOffset.Now;
			Enabled = true;
		}

		public void Expire()
		{
			Expired = true;
			ModificationTime = DateTimeOffset.Now;
		}

		public void Renewal()
		{
			Expired = false;
			ModificationTime = DateTimeOffset.Now;
		}

		public override string ToString()
		{
			return
				$"[ENTITY: {GetType().Name}] Id = {Id}; {{ 'Name': {Name}, 'Enabled': {Enabled}, 'Expired': {Expired}, 'Description': {Description} }}";
		}
	}
}