using System;
using System.ComponentModel.DataAnnotations;
using MSFramework.Common;
using MSFramework.Domain;
using MSFramework.Serialization;

namespace MSFramework.Snapshot
{
	public class Snapshot
	{
		[Required]
		[StringLength(255)]
		public string AggregateRootType { get; }

		public string AggregateRoot { get; }

		/// <summary>
		/// 聚合根标识
		/// </summary>
		[Required]
		[StringLength(255)]
		public string AggregateRootId { get; }

		/// <summary>
		/// 版本号
		/// </summary>
		[Required]
		public int Version { get; }

		/// <summary>
		/// 
		/// </summary>
		[Required]
		public DateTime Timestamp { get; }

		public Snapshot(IAggregateRoot aggregateRoot)
		{
			AggregateRootType = aggregateRoot.GetType().AssemblyQualifiedName;
			Version = aggregateRoot.Version;
			AggregateRoot = Singleton<IJsonConvert>.Instance.SerializeObject(aggregateRoot);
			AggregateRootId = aggregateRoot.GetId();
			Timestamp = DateTime.UtcNow;
		}

		public IAggregateRoot ToAggregateRoot()
		{
			return (IAggregateRoot) Singleton<IJsonConvert>.Instance.DeserializeObject(AggregateRoot,
				Type.GetType(AggregateRootType));
		}
	}
}