namespace Biblioteca.Services
{
    public interface IFileStorageService
    {
        string Read(string filePath);
        void Write(string filePath, string fileContent);
    }
}
