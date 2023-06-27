using System.ComponentModel.DataAnnotations;
using MicroserviceFramework.Mediator;

namespace Ordering.Application.Commands;

public class TestCommand2
    : IRequest
{
    [Required]
    public string Name { get; set; }
}
