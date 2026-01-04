using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace MicroserviceFramework.Extensions.Options;

/// <summary>
/// 配置模型标志
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class AutoOptionsAttribute : Attribute
{
    /// <summary>
    ///
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///
    /// </summary>
    public string Section { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether the binder attempts to set all properties or only public properties.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if the binder attempts to set all non-read-only properties; <see langword="false" /> if only public properties are set.
    /// </value>
    public bool BindNonPublicProperties { get; set; }

    /// <summary>
    /// Gets or sets a value that indicates whether exceptions are thrown when converting a value or when a configuration
    /// key is found for which the provided model object doesn't have an appropriate property that matches the key's name.
    /// </summary>
    /// <value>
    /// <see langword="true" /> if an <see cref="T:System.InvalidOperationException" /> is thrown with a description; <see langword="false" /> if no exceptions are thrown. The default is <see langword="false" />.
    /// </value>
    public bool ErrorOnUnknownConfiguration { get; set; }

    // /// <summary>
    // /// 配置模型标志
    // /// </summary>
    // /// <param name="name">定位对应的配置子节点</param>
    // /// <param name="section"></param>
    // public AutoOptionsAttribute( )
    // {
    //     Name = name;
    //     Section = section;
    // }
}
