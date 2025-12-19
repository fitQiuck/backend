using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Service.DTOs.RolesDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface IRoleService
{
    Task<List<RoleForViewDto>> GetAllAsync(Expression<Func<Role, bool>> filter = null, string[] includes = null);
    Task<RoleForViewGetDto> GetAsync(Expression<Func<Role, bool>> filter = null, string[] includes = null);
    Task<RoleForViewDto> CreateAsync(RoleForCreationDto dto);
    Task<bool> DeleteAsync(Expression<Func<Role, bool>> filter);
    Task<RoleForViewDto> UpdateAsync(Guid id, RoleForUpdateDto dto);
}
