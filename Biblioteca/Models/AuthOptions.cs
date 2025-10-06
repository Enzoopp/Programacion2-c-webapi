namespace Biblioteca.Models;

public class AuthOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Key { get; set; }
    public int ExpMinutes { get; set; }

}