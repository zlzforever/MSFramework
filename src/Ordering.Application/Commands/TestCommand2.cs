using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public class TestCommand2
    : IRequest
{
    /// <summary>
    ///
    /// </summary>
    [Required, StringLength(32)]
    public string Value { get; set; }
}
