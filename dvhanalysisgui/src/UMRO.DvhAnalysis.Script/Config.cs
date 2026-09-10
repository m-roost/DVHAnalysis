using System.IO;
using System.Reflection;

namespace UMRO.DvhAnalysis.Script
{
    public class Config
    {
        public static string GetConfigFileName()
        {
            return GetAssemblyPath() + ".config";
        }

        public static string GetAssemblyPath()
        {
            return Assembly.GetExecutingAssembly().Location;
        }

        public static string GetAssemblyDirectory()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }
    }
}
