using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class PermissionService : IPermissionService
{
    private readonly IGenericRepository<Permission> _repository;
    private readonly IMapper _mapper;

    public PermissionService(IGenericRepository<Permission> _repository,
        IMapper mapper)
    {
        this._repository = _repository;
        _mapper = mapper;
    }

    public async Task<List<PermissionForViewDto>> GetAllAsync(Expression<Func<Permission, bool>> filter = null, string[] includes = null)
    {
        // Fetch all permissions from the repository
        var permissions = await _repository.GetAll(filter, includes).ToListAsync();

        return _mapper.Map<List<PermissionForViewDto>>(permissions).ToList();
    }



    public async Task<PermissionForViewDto> GetAsync(Expression<Func<Permission, bool>> filter, string[] includes = null)
    {
        var permission = await _repository.GetAsync(filter, includes);
        if (permission == null)
            throw new HttpStatusCodeException(404, "Permission not found");

        return _mapper.Map<PermissionForViewDto>(permission);
    }

    public async Task<PermissionForViewDto> CreateAsync(PermissionForCreationDto dto)
    {
        var res = await _repository.GetAsync(p => p.Name == dto.Name);
        if (res != null)
            throw new HttpStatusCodeException(400, "Permission already exists");

        var permission = _mapper.Map<Permission>(dto);
        permission = await _repository.CreateAsync(permission);
        permission.CreatedAt = DateTime.UtcNow;
        permission.CreatedBy = HttpContextHelper.UserId;
        await _repository.SaveChangesAsync();

        return _mapper.Map<PermissionForViewDto>(permission);
    }

    public async Task<bool> DeleteAsync(Expression<Func<Permission, bool>> filter)
    {
        var permission = await _repository.GetAsync(filter, includes: ["Roles"]);
        if (permission == null)
            throw new HttpStatusCodeException(404, "Permission not found");

        if (permission.Roles != null && permission.Roles.Count > 0)
            throw new HttpStatusCodeException(400, "Permission cannot be deleted because it is assigned to roles");

        permission.DeletedBy = HttpContextHelper.UserId;
        permission.DeletedAt = DateTime.UtcNow;
        await _repository.DeleteAsync(permission);
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task<PermissionForViewDto> UpdateAsync(Guid id, PermissionForUpdateDto dto)
    {
        var permission = await _repository.GetAsync(p => p.Id == id);
        if (permission == null)
            throw new HttpStatusCodeException(404, "Permission not found");

        _mapper.Map(dto, permission);
        permission.UpdatedAt = DateTime.UtcNow;
        permission.UpdatedBy = HttpContextHelper.UserId;
        _repository.Update(permission);
        await _repository.SaveChangesAsync();

        return _mapper.Map<PermissionForViewDto>(permission);
    }


    public async Task<PermissionForViewDto> GetPermissionsAsync(Expression<Func<Permission, bool>> filter, string[] includes = null)
    {
        var allPermissions = await _repository.GetAsync(filter, includes: ["Roles"]);

        return _mapper.Map<PermissionForViewDto>(allPermissions);
    }
}
