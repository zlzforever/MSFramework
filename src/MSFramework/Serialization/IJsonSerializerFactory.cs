namespace MicroserviceFramework.Serialization;

public interface IJsonSerializerFactory
{
    IJsonSerializer Create();
}
