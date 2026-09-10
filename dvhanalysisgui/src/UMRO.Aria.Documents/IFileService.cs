namespace UMRO.Aria.Documents
{
    public interface IFileService
    {
        byte[] ReadAllBytes(string path);

        string GetExtension(string path);
    }
}
