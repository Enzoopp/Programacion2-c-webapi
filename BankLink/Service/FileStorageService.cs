using BankLink.interfaces;

namespace BankLink.Service
{
    public class FileStorageService : IFileStorageService
    {
        public string Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"El archivo no existe. Creando uno nuevo en: {filePath}");
                File.WriteAllText(filePath, "[]");
            }

            return File.ReadAllText(filePath);
        }

        public void Write(string filePath, string fileContent)
        {
            try
            {
                File.WriteAllText(filePath, fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en el archivo: {filePath}. Error: {ex.Message}");
                throw;
            }
        }
    }
}
