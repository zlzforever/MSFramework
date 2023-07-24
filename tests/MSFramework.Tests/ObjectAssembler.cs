using AutoMapper;
using MicroserviceFramework;
using MicroserviceFramework.AutoMapper;
using MicroserviceFramework.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MSFramework.Tests;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Obj, Dto>();
    }
}

public class Obj
{
    public string Name { get; set; }
}

public class Dto
{
    public string Name { get; set; }
}

public class ObjectAssembler
{
    [Fact]
    public void NullTo()
    {
        var service = new ServiceCollection();
        service.AddMicroserviceFramework(x =>
        {
            x.UseAutoMapper();
        });
        var provider = service.BuildServiceProvider();
        var assembler = provider.GetRequiredService<IObjectAssembler>();
        var result = assembler.To<Dto>(null);
        Assert.Null(result);
    }

    [Fact]
    public void To()
    {
        var service = new ServiceCollection();
        service.AddMicroserviceFramework(x =>
        {
            x.UseAutoMapper();
        });
        var provider = service.BuildServiceProvider();
        var assembler = provider.GetRequiredService<IObjectAssembler>();
        var result = assembler.To<Dto>(new Obj { Name = "test" });
        Assert.Equal("test", result.Name);

        var result1 = assembler.To<Obj, Dto>(new Obj { Name = "test" });
        Assert.Equal("test", result1.Name);

        var result2 = assembler.To<Obj, Dto>(new Obj { Name = "test" }, new Dto { Name = "dto" });
        Assert.Equal("test", result2.Name);
    }
}
