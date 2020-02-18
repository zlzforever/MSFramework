namespace MSFramework.AspNetCore.Permission
{
	public class Permission
	{
		public string Id { get; set; }
		public bool Expired { get; set; }
		public string Module { get; set; }
		public string Name { get; set; }
		public string Identification { get; set; }
		public string Description { get; set; }
		public int Type { get; set; } = 3;
	}
}