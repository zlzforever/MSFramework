using MicroserviceFramework.Domain;

namespace MSFramework.AspNetCore.Test.DataModel;

public class ExternalUser(string id) : ExternalEntity<string>(id);
