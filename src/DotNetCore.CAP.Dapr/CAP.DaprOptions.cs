namespace DotNetCore.CAP.Dapr;

public class DaprOptions
{
    public string GrpcEndpoint { get; set; }
    public string Pubsub { get; set; } = "pubsub";
}
