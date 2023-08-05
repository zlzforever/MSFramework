using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public record TestCommand1 : Request<string>
{
    /// <summary>
    ///
    /// </summary>
    [Required, StringLength(10)]
    public string Value { get; set; }
}
