using BankLink.Dtos;

namespace BankLink.interfaces
{
    public interface IAuthService
    {
        string CreateToken(CreateTokenDto createTokenDto);
        string Login(LoginDto loginDto);
        Task<bool> ValidateApiKeyAsync(string apiKey);
    }
}