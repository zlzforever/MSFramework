using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Dapr;
using MicroserviceFramework.AspNetCore;
using MicroserviceFramework.Common;
using MicroserviceFramework.Domain;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Ordering.Application.Dto;
using Ordering.Domain.AggregateRoots;
using Ordering.Domain.AggregateRoots.Order;

namespace Ordering.API.Controllers;

[Route("[controller]")]
[ApiController]
public class ValueController(IObjectAssembler objectAssembler) : ApiControllerBase
{
    [HttpGet("int")]
    public int Get([FromQuery, Required, StringLength(4, ErrorMessage = "长度不能超过 4")] string a)
    {
        return new Random().Next(0, 10);
    }

    [HttpPost]
    public M ModelValid(M m)
    {
        return m;
    }


    [HttpGet("OrderDto3")]
    public Task<List<OrderDto>> GetOrderDto3()
    {
        var dto = GetOrderDto();
        return Task.FromResult(new List<OrderDto> { dto, null, null });
    }

    [HttpGet("OrderDto2")]
    public Task<OrderDto> GetOrderDto2()
    {
        var dto = GetOrderDto();
        return Task.FromResult(dto);
    }

    [HttpGet("OrderDto")]
    public OrderDto GetOrderDto()
    {
        var address = new Address
        {
            Street = "Street",
            City = "City",
            State = "State",
            Country = "Country",
            ZipCode = "ZipCode"
        };

        var order = Order.Create(
            new UserInfo("1") { Name = "lewis" },
            address,
            "Description");
        order.AddItem("100049450275",
            "英特尔(Intel) i5-13400F 13代 酷睿 处理器 10核16线程 睿频至高可达4.6Ghz 20M三级缓存 台式机CPU",
            "https://img10.360buyimg.com/n1/s450x450_jfs/t1/125974/18/29337/184045/63ae90f3F3d8b8b8a/b6cef93bb9b3b2c1.jpgl",
            149900);
        order.AddItem("100041994142",
            "ROG ROG STRIX Z790-A GAMING WIFI吹雪主板 支持DDR5 CPU 13900K/13700K（Intel Z790/LGA 1700）",
            "https://img12.360buyimg.com/n1/s450x450_jfs/t1/177676/26/33690/186079/63f71ca3F72878ea9/54e9c6c564a1d4e1.jpg",
            28400);
        order.SetList(new[] { "hi1", "hi2" });
        order.AddExtra("质保", "3 年");
        order.AddExtra("RGB", "ARGB");
        order.AddKeyValuePair("test1", "value1");
        order.AddKeyValuePair("test2", "value2");
        var output = objectAssembler.To<OrderDto>(order);
        return output;
    }


    [Topic("rabbitmq-pubsub", "test")]
    [HttpGet("string")]
    public string GetString()
    {
        return Guid.NewGuid().ToString();
    }

    // [HttpGet("ok")]
    // public ApiResult GetOk()
    // {
    //     return ApiResult.Ok;
    // }
    //
    // [HttpGet("error")]
    // public ApiResult GetError()
    // {
    //     return ApiResult.Error;
    // }

    [HttpGet("noData")]
    public Task GetNoRes()
    {
        return Task.CompletedTask;
    }

    [HttpGet("list")]
    public IEnumerable<int> Get1()
    {
        return new List<int> { 1, 2, 3 };
    }

    [HttpGet("apiResult")]
    public List<int> Get2()
    {
        return [1, 2, 3];
    }

    [HttpGet("file")]
    public IActionResult Get3()
    {
        var stream = System.IO.File.ReadAllBytes("1.csv");
        return new FileStreamResult(new MemoryStream(stream), "text/csv");
    }

    [HttpGet("actionResult")]
    public IActionResult Get4()
    {
        return new ObjectResult(null);
    }

    [HttpGet("pagedResult")]
    public PaginationResult<int> Get5()
    {
        return new PaginationResult<int>(1, 10, 10, [1, 2, 3]);
    }

    [HttpGet("emptyResult")]
    public IActionResult Get6()
    {
        return new EmptyResult();
    }
}

public class M
{
    public DateTimeOffset FullDateTimeOffset { get; set; }
    public DateTimeOffset PartDateTimeOffset { get; set; }
    public ObjectId ObjectId { get; set; }
    public int Usage { get; set; }
}
