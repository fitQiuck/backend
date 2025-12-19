using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Service.DTOs.PermissionsDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface IPermissionService
{
    Task<List<PermissionForViewDto>> GetAllAsync(Expression<Func<Permission, bool>> filter = null, string[] includes = null);
    Task<PermissionForViewDto> GetAsync(Expression<Func<Permission, bool>> filter = null, string[] includes = null);
    Task<PermissionForViewDto> CreateAsync(PermissionForCreationDto dto);
    Task<bool> DeleteAsync(Expression<Func<Permission, bool>> filter);
    Task<PermissionForViewDto> UpdateAsync(Guid id, PermissionForUpdateDto dto);
    Task<PermissionForViewDto> GetPermissionsAsync(Expression<Func<Permission, bool>> filter, string[] includes = null);
}
