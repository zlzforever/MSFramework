using System;

namespace MicroserviceFramework.Configuration
{
	/// <summary>
	/// 配置模型标志
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class OptionsTypeAttribute : Attribute
	{
		/// <summary>
		/// 配置节点的名词，默认为类名称
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// 配置模型标志
		/// </summary>
		/// <param name="sectionName">不配置，默认为类名称</param>
		public OptionsTypeAttribute(string sectionName = "")
		{
			SectionName = sectionName;
		}
	}
}