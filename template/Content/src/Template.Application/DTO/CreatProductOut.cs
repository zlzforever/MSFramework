using System;

namespace Template.Application.DTO
{
	public class CreatProductOut
	{
		public string Name { get; set; }
		public Guid Id { get; set; }
		public DateTimeOffset CreationTime { get; set; }
	}
}