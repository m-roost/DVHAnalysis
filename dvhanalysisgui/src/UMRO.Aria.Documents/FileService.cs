using System.IO;

namespace UMRO.Aria.Documents
{
    internal class FileService : IFileService
    {
        public byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }
    }
}
