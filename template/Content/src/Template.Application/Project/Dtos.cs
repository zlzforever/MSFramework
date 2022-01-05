using System;

namespace Template.Application.Project
{
	public static class Dtos
	{
		public static class V10
		{
			public class CreatProductOut
			{
				public string Name { get; set; }
				public Guid Id { get; set; }
				public DateTimeOffset CreationTime { get; set; }
			}

			public class ProductOut
			{
				public string Name { get; set; }
				public Guid Id { get; set; }
				public DateTimeOffset CreationTime { get; set; }
				public string CreationUserName { get; set; }
			}
			
	 
		}
	}
}