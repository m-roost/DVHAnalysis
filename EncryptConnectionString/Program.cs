using System;
using System.Security.Cryptography;
using System.Text;

namespace EncryptConnectionString
{
    class Program
    {
        static void Main(string[] args)
        {
            // Generate RSA keys
            GenerateRSAKeys(out RSAParameters publicKey, out string privateKeyString);


            // Define a connection string to encrypt
            string AriaConnString_raw = "data source=XXX_SQLServerName;initial catalog=VARIAN;user id=XXX_UserID;password=XXX_Password;App=DvhAnalysis";

            Console.WriteLine("\nOriginal AriaConnString: \n" + AriaConnString_raw);

            string AriaSfConnString_raw = "data source=XXX_SQLServerName;initial catalog=VarianSharedFrameworkDatabase;user id=XXX_UserID;password=XXX_Password;App=DvhAnalysis";

            Console.WriteLine("\nOriginal AriaSfConnString: \n" + AriaSfConnString_raw);



            Console.WriteLine("\n\n >>>>>>> Copy paste the following PrivateKey and two connection strings to DVHAnalysis-3.5.0.2.esapi.dll.config: <<<<<<<\n");


            // Print the keys (for demonstration purposes, normally you'd store them securely)
            Console.WriteLine("Private Key (String): \n\n" + privateKeyString);


            // Encrypt the connection string using the public key
            string AriaConnString = EncryptConnectionString(AriaConnString_raw, publicKey);
            Console.WriteLine("\nEncrypted AriaConnString: \n\n" + AriaConnString);


            // Encrypt the connection string using the public key
            string AriaSfConnString = EncryptConnectionString(AriaSfConnString_raw, publicKey);
            Console.WriteLine("\nEncrypted AriaSfConnString: \n\n" + AriaSfConnString);

            // Decrpty and print to test
            string AriaSfConnString_decrypted = DecryptConnectionString(AriaSfConnString, privateKeyString);
            Console.WriteLine("\nDecrypted AriaSfConnString: \n\n" + AriaSfConnString_decrypted);

            Console.WriteLine("\n\nProgram finished. You can press any key to close this window now.");

            Console.Read();
        }



        public static void GenerateRSAKeys(out RSAParameters publicKey, out string privateKeyString)
        {
            using (var rsa = new RSACryptoServiceProvider(2048))
            {
                publicKey = rsa.ExportParameters(false);
                var privateKey = rsa.ExportParameters(true);
                privateKeyString = RSAParametersToString(privateKey);
            }
        }

        public static string EncryptConnectionString(string connectionString, RSAParameters publicKey)
        {
            byte[] encryptedData;
            using (var rsa = new RSACryptoServiceProvider())
            {
                rsa.ImportParameters(publicKey);
                encryptedData = rsa.Encrypt(Encoding.UTF8.GetBytes(connectionString), true);
            }
            return Convert.ToBase64String(encryptedData);
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


        // Helper method to convert RSAParameters to a string
        public static string RSAParametersToString(RSAParameters key)
        {
            return Convert.ToBase64String(key.Modulus) + ";" +
                   Convert.ToBase64String(key.Exponent) + ";" +
                   Convert.ToBase64String(key.P) + ";" +
                   Convert.ToBase64String(key.Q) + ";" +
                   Convert.ToBase64String(key.DP) + ";" +
                   Convert.ToBase64String(key.DQ) + ";" +
                   Convert.ToBase64String(key.InverseQ) + ";" +
                   Convert.ToBase64String(key.D);
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
