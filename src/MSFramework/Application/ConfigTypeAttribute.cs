using System;

namespace MicroserviceFramework.Application
{
	/// <summary>
	/// 配置模型标志
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ConfigTypeAttribute : Attribute
	{
		/// <summary>
		/// 配置节点的名词，默认为类名称
		/// </summary>
		public string SectionName { get; }

		/// <summary>
		/// 是否允许重载
		/// </summary>
		public bool ReloadOnChange { get; }

		/// <summary>
		/// 配置模型标志
		/// </summary>
		/// <param name="sectionName">不配置，默认为类名称</param>
		/// <param name="reloadOnChange">是否允许重载</param>
		public ConfigTypeAttribute(string sectionName = null, bool reloadOnChange = false)
		{
			SectionName = sectionName;
			ReloadOnChange = reloadOnChange;
		}
	}
}