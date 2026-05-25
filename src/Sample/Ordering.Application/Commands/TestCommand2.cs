using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record TestCommand2
    : Request
{
    /// <summary>
    ///
    /// </summary>
    [Required, StringLength(32)]
    public string Value { get; set; }
}
