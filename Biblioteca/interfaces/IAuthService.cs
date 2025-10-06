using Biblioteca.Dtos;
namespace Biblioteca.interfaces;

public interface IAuthService
{
    string CreateToken(CreateTokenDto createTokenDto);
    string Login(LoginDto loginDto);
}