using MSFramework.Domain;

namespace Template.Domain.AggregateRoot
{
	public class Class1 : ModificationAuditedAggregateRoot
	{
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