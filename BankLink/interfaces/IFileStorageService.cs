namespace BankLink.interfaces
{
    public interface IFileStorageService
    {
        string Read(string filePath);
        void Write(string filePath, string fileContent);
    }
}
