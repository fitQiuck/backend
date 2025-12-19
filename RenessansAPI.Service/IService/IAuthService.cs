using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.DTOs.TokensDto;

namespace RenessansAPI.Service.IService;

public interface IAuthService
{
    ValueTask<TokenForViewDto> GenerateToken(string email, string password);
    ValueTask<string> RestartToken(string token);
    ValueTask<List<PermissionForViewDto>> GetPermissinWithToken(string token);
}
