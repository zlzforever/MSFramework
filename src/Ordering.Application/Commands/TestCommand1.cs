using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public class TestCommand1 : IRequest<string>
{
    /// <summary>
    ///
    /// </summary>
    [Required, StringLength(10)]
    public string Value { get; set; }
}
