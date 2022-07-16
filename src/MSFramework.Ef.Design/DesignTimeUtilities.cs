using System.Reflection;

namespace MicroserviceFramework.Ef.Design
{
    public static class DesignTimeUtilities
    {
        public static bool IsDesignTime()
        {
            return "ef" == Assembly.GetEntryAssembly()?.GetName().Name?.ToLower();
        }
    }
}