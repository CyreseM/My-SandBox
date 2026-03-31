using JWTDemo.DTOs;

namespace JWTDemo.Services
{
    public interface IUserService
    {
        Task<bool> RegisterUserAsync(UserRegisterDTO registerDto);
        Task<AuthResponseDTO?> AuthenticateUserAsync(UserLoginDTO loginDto, string ipAddress);
        Task<AuthResponseDTO?> RefreshTokenAsync(string refreshToken, string clientId, string ipAddress);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress);
    }
}
