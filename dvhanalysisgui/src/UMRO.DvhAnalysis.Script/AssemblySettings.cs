using System;
using System.Configuration;
using System.Drawing.Text;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace UMRO.DvhAnalysis.Script
{
    internal class AssemblySettings
    {
        public static string AriaConnectionString => GetAppSetting("AriaConnString");
        public static string AriaSfConnectionString => GetAppSetting("AriaSfConnString");
        public static string AriaDocAddress => GetAppSetting("AriaDocAddress");
        public static string AriaDocUsername => GetAppSetting("AriaDocUsername");
        public static string AriaDocPassword => GetAppSetting("AriaDocPassword");
        public static string AriaDocApiKey => GetAppSetting("AriaDocApiKey");
        public static string DocumentPath => GetAppSetting("DocPath");
        public static string MetricsPath => GetAppSetting("MetricsPath");
        public static string TemplatesPath => GetAppSetting("TemplatesPath");

        // Optional and read during startup before the main try/catch, so it must never throw:
        // a missing key returns null and logging falls back to a local directory.
        public static string LogDir => GetAppSettingOrNull("LogDir");



        // ROAR "save metrics" DB connection string. Optional: when the RoarConnString app setting is
        // absent this returns null and callers fall back to the value compiled into MRoar_Database
        // (Settings.Designer.cs). Read from the plugin's own .dll.config (like the Aria settings),
        // which -- unlike Settings.Default -- is a config the deployer can actually edit at runtime.
        public static string RoarConnectionString => GetAppSettingOrNull("RoarConnString");

        private static string GetAppSetting(string key)
        {
            try
            {
                var asmPath = Assembly.GetExecutingAssembly().Location;
                var config = ConfigurationManager.OpenExeConfiguration(asmPath);
                var element = config.AppSettings.Settings[key];


                if (key == "AriaConnString" || key == "AriaSfConnString")
                {
                    string privateKey = GetAppSettingOrNull("PrivateKey");

                    if (string.IsNullOrEmpty(privateKey) || privateKey?.ToLower() == "none") { return element?.Value; }   // setting private key to "none" will bypass decryption

                    string rv = DecryptConnectionString(element?.Value, privateKey);  // private static string privateKey  will be left out before commit to Git.

                    return rv;
                }


                return element?.Value;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Unable to read configuration setting", e);
            }
        }


        // Like GetAppSetting but returns null when the key is absent instead of throwing, so an
        // unset optional setting cleanly falls back to a default rather than failing.
        private static string GetAppSettingOrNull(string key)
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().Location);
                return config.AppSettings.Settings[key]?.Value;
            }
            catch
            {
                return null;
            }
        }


        public static string DecryptConnectionString(string encryptedConnectionString, string privateKeyString)
        {
            byte[] decryptedData;
            using (var rsa = new RSACryptoServiceProvider())
            {
                RSAParameters privateKey = StringToRSAParameters(privateKeyString);
                rsa.ImportParameters(privateKey);
                decryptedData = rsa.Decrypt(Convert.FromBase64String(encryptedConnectionString), true);
            }
            return Encoding.UTF8.GetString(decryptedData);
        }


        // Helper method to convert a string back to RSAParameters
        public static RSAParameters StringToRSAParameters(string keyString)
        {
            string[] keyParts = keyString.Split(';');
            return new RSAParameters
            {
                Modulus = Convert.FromBase64String(keyParts[0]),
                Exponent = Convert.FromBase64String(keyParts[1]),
                P = Convert.FromBase64String(keyParts[2]),
                Q = Convert.FromBase64String(keyParts[3]),
                DP = Convert.FromBase64String(keyParts[4]),
                DQ = Convert.FromBase64String(keyParts[5]),
                InverseQ = Convert.FromBase64String(keyParts[6]),
                D = Convert.FromBase64String(keyParts[7])
            };
        }




    }
}
