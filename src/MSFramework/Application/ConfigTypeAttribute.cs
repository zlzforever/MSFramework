using System;

namespace MSFramework.Application
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
		/// 是否可选
		/// </summary>
		public bool Optional { get; }

		/// <summary>
		/// 是否允许重载
		/// </summary>
		public bool ReloadOnChange { get; }

		/// <summary>
		/// 配置模型标志
		/// </summary>
		/// <param name="sectionName">不配置，默认为类名称</param>
		/// <param name="optional">是否可选</param>
		/// <param name="reloadOnChange">是否允许重载</param>
		public ConfigTypeAttribute(string sectionName = "", bool optional = true, bool reloadOnChange = false)
		{
			SectionName = sectionName;
			ReloadOnChange = reloadOnChange;
			Optional = optional;
		}
	}
}