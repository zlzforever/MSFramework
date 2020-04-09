using System.ComponentModel;
using MSFramework.Domain;

namespace Template.Domain.AggregateRoot
{
	[Description("表1")]
	public class Class1 : ModificationAuditedAggregateRoot
	{
		/// <summary>
		/// 
		/// </summary>
		[Description("列1")]
		public string Name { get; private set; }

		private Class1()
		{
		}

		public Class1(string name)
		{
			Name = name;
		}

		public void ChangeName(string name)
		{
			if (!Name.Equals(name))
			{
				Name = name;
			}
		}
	}
}