using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace MSFramework.DependencyInjection
{
    public interface IDependencyTypeFinder
    {
        Dictionary<ServiceLifetime, Type[]> GetDependencyTypeDict();
    }
}