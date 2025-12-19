using AutoMapper;
using Microsoft.Extensions.Logging;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Domain.Entities.Users;
using RenessansAPI.Service.DTOs.RolesDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class RoleService : IRoleService
{
    private readonly IGenericRepository<Role> _repository;
    private readonly IGenericRepository<Permission> _permissionRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;
    private readonly ILogger<RoleService> _logger;

    public RoleService(IGenericRepository<Role> repository,
        IMapper mapper,
        IGenericRepository<Permission> permissionRepository,
        IPermissionService permissionService,
        IGenericRepository<User> userRepository,
        ILogger<RoleService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _permissionRepository = permissionRepository;
        _permissionService = permissionService;
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<List<RoleForViewDto>> GetAllAsync(
        Expression<Func<Role, bool>> filter = null,
        string[] includes = null)
    {
        _logger.LogInformation("Fetching roles...");

        var rolesQuery = _repository.GetAll(filter, includes: new[] { "Permissions" });
        // If repository returns IQueryable, materialize asynchronously if available
        var roles = rolesQuery.ToList(); // or await rolesQuery.ToListAsync();

        var dtos = _mapper.Map<List<RoleForViewDto>>(roles);
        _logger.LogInformation("{Count} roles fetched.", dtos.Count);
        return dtos;
    }

    public async Task<RoleForViewGetDto> GetAsync(
        Expression<Func<Role, bool>> filter = null,
        string[] includes = null)
    {
        _logger.LogInformation("Fetching role by filter...");
        var role = await _repository.GetAsync(filter, includes: new[] { "Permissions" });

        if (role is null)
            throw new HttpStatusCodeException(404, "Role not found");

        // Map explicitly (or configure AutoMapper)
        return new RoleForViewGetDto
        {
            Id = role.Id,
            Name = role.Name,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy.ToString()
            // Add projected permissions here if needed
        };
    }

    public async Task<RoleForViewDto> CreateAsync(RoleForCreationDto dto)
    {
        if (await _repository.GetAsync(r => r.Name == dto.Name) is not null)
            throw new HttpStatusCodeException(409, "Role already exists");

        // Deduplicate IDs & batch fetch
        var permIds = new HashSet<Guid>(dto.RolePermissions ?? Enumerable.Empty<Guid>());
        if (permIds.Count == 0)
            throw new HttpStatusCodeException(400, "At least one permission is required.");

        var perms = _permissionRepository.GetAll(p => permIds.Contains(p.Id)).ToList();
        if (perms.Count != permIds.Count)
        {
            var found = perms.Select(p => p.Id).ToHashSet();
            var missing = permIds.Where(id => !found.Contains(id));
            throw new HttpStatusCodeException(404, $"Missing permissions: {string.Join(", ", missing)}");
        }

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Permissions = perms,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = HttpContextHelper.UserId ?? Guid.Empty
        };

        await _repository.CreateAsync(role);
        await _repository.SaveChangesAsync();

        // Let AutoMapper ignore Permissions or map a summary
        return _mapper.Map<RoleForViewDto>(role);
    }

    public async Task<RoleForViewDto> UpdateAsync(Guid id, RoleForUpdateDto dto)
    {
        var role = await _repository.GetAsync(r => r.Id == id, includes: new[] { "Permissions" });
        if (role is null)
            throw new HttpStatusCodeException(404, "Role not found");

        var newIds = new HashSet<Guid>(dto.RolePermissions ?? Enumerable.Empty<Guid>());

        // Batch fetch new permissions
        var newPerms = _permissionRepository.GetAll(p => newIds.Contains(p.Id)).ToList();
        if (newPerms.Count != newIds.Count)
        {
            var found = newPerms.Select(p => p.Id).ToHashSet();
            var missing = newIds.Where(id => !found.Contains(id));
            throw new HttpStatusCodeException(404, $"Missing permissions: {string.Join(", ", missing)}");
        }

        // Map scalars
        _mapper.Map(dto, role);

        // Compute diffs for many-to-many to help EF track changes efficiently
        var current = role.Permissions?.ToDictionary(p => p.Id) ?? new Dictionary<Guid, Permission>();
        var target = newPerms.ToDictionary(p => p.Id);

        // removals
        foreach (var p in current.Values.Where(p => !target.ContainsKey(p.Id)).ToList())
            role.Permissions.Remove(p);

        // additions
        foreach (var p in target.Values.Where(p => !current.ContainsKey(p.Id)))
            role.Permissions.Add(p);

        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = HttpContextHelper.UserId ?? Guid.Empty;

        _repository.Update(role);
        await _repository.SaveChangesAsync();

        return _mapper.Map<RoleForViewDto>(role);
    }

    public async Task<bool> DeleteAsync(Expression<Func<Role, bool>> filter)
    {
        _logger.LogInformation("Attempting to delete role...");
        var role = await _repository.GetAsync(filter, includes: new[] { "Users" });

        if (role is null)
            throw new HttpStatusCodeException(404, "Role not found");

        if ((role.Users?.Count ?? 0) > 0)
            throw new HttpStatusCodeException(409, "Cannot delete a role that is assigned to users");

        role.DeletedBy = HttpContextHelper.UserId ?? Guid.Empty;
        role.DeletedAt = DateTime.UtcNow;

        await _repository.DeleteAsync(role);
        await _repository.SaveChangesAsync();
        _logger.LogInformation("Role deleted successfully. Role ID: {RoleId}", role.Id);
        return true;
    }
}
