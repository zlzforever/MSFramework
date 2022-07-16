namespace Ordering.Domain.AggregateRoots;

public class ExtraInfo
{
	public string Name { get; set; }
	public string Age { get; set; }

	public ExtraInfo(string name, string age)
	{
		Name = name;
		Age = age;
	}
}