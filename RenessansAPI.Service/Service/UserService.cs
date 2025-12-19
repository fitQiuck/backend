using AutoMapper;
using Microsoft.Extensions.Logging;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Domain.Entities.Users;
using RenessansAPI.Service.DTOs.UsersDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using RenessansAPI.Service.Security;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class UserService : IUserService
{
    private readonly IGenericRepository<User> _repository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;

    public UserService(IGenericRepository<User> repository,
        IMapper mapper,
        IGenericRepository<Role> roleRepository,
        ILogger<UserService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _roleRepository = roleRepository;
        _logger = logger;
    }

    public async Task<List<UserForViewDto>> GetAllAsync(
        Expression<Func<User, bool>> filter = null,
        string[] includes = null)
    {
        _logger.LogInformation("Fetching all users...");

        var query = _repository.GetAll(filter, includes: new[] { "Roles" });
        var res = _mapper.Map<List<UserForViewDto>>(query).OrderBy(t => t.Id).ToList();

        _logger.LogInformation("Fetched {Count} users", res.Count);
        return res;
    }

    public async Task<UserForViewDto> GetAsync(Expression<Func<User, bool>> filter, string[] includes = null)
    {
        _logger.LogInformation("Getting single user with filter...");

        var res = await _repository.GetAsync(filter, includes: ["Roles"]);
        if (res == null)
        {
            _logger.LogWarning("User not found with given filter");
            throw new HttpStatusCodeException(404, "User not found");
        }

        _logger.LogInformation("User found: {Email}", res.Email);
        return _mapper.Map<UserForViewDto>(res);
    }

    public async Task<List<UserForViewDto>> GetGestUsersAsync()
    {
        _logger.LogInformation("Fetching users with 'Gest' role...");

        var usersQuery = _repository.GetAll(includes: new[] { "Roles" });

        var gestUsers = usersQuery
            .Where(u => u.Roles != null && u.Roles.Name == "Gest")
            .OrderBy(u => u.UserName)
            .ToList();

        var result = _mapper.Map<List<UserForViewDto>>(gestUsers);

        _logger.LogInformation("Fetched {Count} Gest users", result.Count);
        return result;
    }

    public async Task<UserForViewDto> CreateAsync(UserForCreationDto entity)
    {
        _logger.LogInformation("Creating user: {Email}", entity.Email);

        var existUser = await _repository.GetAsync(p => p.Email == entity.Email);
        if (existUser != null)
        {
            _logger.LogWarning("User already exists with email: {Email}", entity.Email);
            throw new HttpStatusCodeException(400, "User is already exist");
        }

        Role? roleRes = null;

        if (entity.RolesId is not null)
            roleRes = await _roleRepository.GetAsync(item => item.Id == entity.RolesId);

        if (roleRes == null)
            roleRes = await _roleRepository.GetAsync(item => item.Name == "Gest");

        var user = _mapper.Map<User>(entity);
        user.AvatarUrl = "";
        user.CreatedAt = DateTime.UtcNow;
        user.CreatedBy = HttpContextHelper.UserId;
        user.Password = SecurePasswordHasher.Hash(entity.Password);
        user.Roles = roleRes;

        await _repository.CreateAsync(user);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("User successfully created: {Email}", entity.Email);
        return _mapper.Map<UserForViewDto>(user);
    }

    public async Task<bool> DeleteAsync(Expression<Func<User, bool>> filter)
    {
        _logger.LogInformation("Deleting user with filter...");

        var res = await _repository.GetAsync(filter);
        if (res == null)
        {
            _logger.LogWarning("User not found for deletion");
            throw new HttpStatusCodeException(404, "User not found");
        }

        res.DeletedBy = HttpContextHelper.UserId;
        res.DeletedAt = DateTime.UtcNow;

        await _repository.DeleteAsync(res);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("User deleted: {UserId}", res.Id);
        return true;
    }

    public async Task<UserForViewDto> UpdateAsync(Guid id, UserForUpdateDto dto)
    {
        _logger.LogInformation("Updating user: {UserId}", id);

        var res = await _repository.GetAsync(item => item.Id == id, includes: new[] { "Roles" });
        if (res == null)
        {
            _logger.LogWarning("User not found for update: {UserId}", id);
            throw new HttpStatusCodeException(404, "User not found");
        }
        Role role = new Role();

        res = _mapper.Map(dto, res);

        if (dto.RolesId != null)
        {
            var roleRes = await _roleRepository.GetAsync(item => item.Id == dto.RolesId);
            res.Roles = roleRes;
            res.RolesId = roleRes.Id;
            if (roleRes == null)
            {
                _logger.LogWarning("Role not found for RoleId: {RoleId}", dto.RolesId);
                throw new HttpStatusCodeException(404, "Role is not exist");
            }

        }
        res.UpdatedAt = DateTime.UtcNow;
        res.UpdatedBy = HttpContextHelper.UserId;
        res.RolesId = res.Roles.Id;

        _repository.Update(res);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("User updated successfully: {UserId}", res.Id);
        return _mapper.Map<UserForViewDto>(res);
    }

    public async Task<bool> ChangePassword(string email, string password)
    {
        _logger.LogInformation("Changing password for user: {Email}", email);

        var user = await _repository.GetAsync(item => item.Email == email);
        if (user == null)
        {
            _logger.LogWarning("User not found for password change: {Email}", email);
            throw new KeyNotFoundException($"User with email {email} does not exist.");
        }

        user.Password = SecurePasswordHasher.Hash(password);
        _repository.Update(user);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Password changed for user: {Email}", email);
        return true;
    }
}
