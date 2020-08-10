namespace MSFramework.AspNetCore.AccessControl
{
	public class ApiInfo
	{
		public string Id { get; set; }

		/// <summary>
		/// API 名称，在一个 Application 中必须是唯一的
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 分组
		/// </summary>
		public string Group { get; set; }

		/// <summary>
		/// 所属应用程序
		/// </summary>
		public string Application { get; set; }

		/// <summary>
		/// 描述
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// 是否过期
		/// </summary>
		public bool Obsoleted { get; set; }
	}
}