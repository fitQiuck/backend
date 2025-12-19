using RenessansAPI.Domain.Entities.Users;
using RenessansAPI.Service.DTOs.UsersDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface IUserService
{
    Task<List<UserForViewDto>> GetAllAsync(Expression<Func<User, bool>> filter = null, string[] includes = null);
    Task<UserForViewDto> GetAsync(Expression<Func<User, bool>> filter, string[] includes = null);
    Task<List<UserForViewDto>> GetGestUsersAsync();
    Task<UserForViewDto> CreateAsync(UserForCreationDto dto);
    Task<bool> DeleteAsync(Expression<Func<User, bool>> filter);
    Task<UserForViewDto> UpdateAsync(Guid id, UserForUpdateDto dto);
    Task<bool> ChangePassword(string email, string password);
}
