using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Service.DTOs.TokensDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface ITokenService
{
    Task<IEnumerable<TokenForViewDto>> GetAllAsync(Expression<Func<Token, bool>> filter = null, string[] includes = null);
    Task<TokenForViewDto> GetAsync(Expression<Func<Token, bool>> filter, string[] includes = null);
    Task<TokenForViewDto> CreateAsync(TokenForCreationDto dto);
    Task<TokenForViewDto> UpdateAsync(Guid id, TokenForUpdateDto dto); // PATCH style
    Task<bool> DeleteAsync(Expression<Func<Token, bool>> filter);
    Task<bool> CheckTokenExistsAsync(string token);
}
