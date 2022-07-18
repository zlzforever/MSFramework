﻿namespace MicroserviceFramework.EventBus.RabbitMQ;

public class RabbitMQOptions
{
    public string Exchange { get; set; }

    public string HostName { get; set; }

    public int Port { get; set; }

    public string UserName { get; set; }

    public string Password { get; set; }

    public int RetryCount { get; set; } = 5;
    public string Queue { get; set; }
}
