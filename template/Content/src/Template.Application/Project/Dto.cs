using System;

namespace Template.Application.Project
{
	public static class Dto
	{
		public static class V10
		{
			public class CreateProductOut
			{
				public string Name { get; set; }
				public string Id { get; set; }
				public DateTimeOffset CreationTime { get; set; }
			}

			public class ProductOut
			{
				public string Name { get; set; }
				public string Id { get; set; }
				public DateTimeOffset CreationTime { get; set; }
				public string CreationUserName { get; set; }
			}
		}
	}
}