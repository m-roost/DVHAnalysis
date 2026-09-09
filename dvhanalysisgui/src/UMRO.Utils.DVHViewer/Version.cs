using System;
using System.Reflection;

namespace UMRO.Utils.DVHViewer
{
    public class version
    {
        public static string getVersion()
        {
            Version version2 = Assembly.GetExecutingAssembly().GetName().Version;
            return version2.Major + "." + version2.Minor;
        }
    }
}
