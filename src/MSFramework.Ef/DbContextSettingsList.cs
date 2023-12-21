using System;
using System.Collections.Generic;
using MicroserviceFramework.Extensions.Options;

namespace MicroserviceFramework.Ef;

[OptionsType("DbContexts")]
public class DbContextSettingsList : List<DbContextSettings>
{
    public DbContextSettings Get(Type contextType)
    {
        foreach (var value in this)
        {
            if (value.GetDbContextType() == contextType)
            {
                return value;
            }
        }

        return null;
    }
}
