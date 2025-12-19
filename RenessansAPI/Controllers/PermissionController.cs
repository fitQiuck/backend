using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PermissionController : ControllerBase
{
    private readonly IPermissionService _permissionService;
    private readonly ILogger<PermissionController> _logger;

    public PermissionController(IPermissionService permissionService, ILogger<PermissionController> logger)
    {
        _permissionService = permissionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllAsync()
    {
        var userId = HttpContextHelper.UserId;
        _logger.LogInformation("User {UserId} requested all permissions", userId);

        var hasPermission = HttpContextHelper.UserPermission.FirstOrDefault(item => item == "Permission_Get");

        if (hasPermission is not null)
        {
            var permissions = await _permissionService.GetAllAsync();
            return Ok(permissions);
        }
        return BadRequest("Don't have permission.");
    }

    [HttpGet("{permissionBy}")]
    public async Task<IActionResult> GetById(Guid permissionBy)
    {
        var userId = HttpContextHelper.UserId;
        _logger.LogInformation("User {UserId} requested permission by key: {Key}", userId, permissionBy);

        var hasPermission = HttpContextHelper.UserPermission.FirstOrDefault(item => item == "Permission_Get");

        if (hasPermission is not null)
        {
            var permission = await _permissionService.GetAsync(user => user.Id == permissionBy);
            if (permission == null)
            {
                return NotFound($"Permission with key '{permissionBy}' not found.");
            }
            return Ok(permission);
        }
        return BadRequest("Don't have permission.");
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync(PermissionForCreationDto groupForCreationDto)
    {
        var userId = HttpContextHelper.UserId;
        _logger.LogInformation("User {UserId} is creating a permission", userId);

        var hasPermission = HttpContextHelper.UserPermission.FirstOrDefault(item => item == "Permission_Create");

        if (hasPermission is not null)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var permissionDto = await _permissionService.CreateAsync(groupForCreationDto);

            return Ok(permissionDto);
        }
        return BadRequest("Don't have permission.");
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAsync(Guid permissionId)
    {
        var currentUserId = HttpContextHelper.UserId;
        _logger.LogInformation("User {UserId} is deleting permission with ID: {TargetId}", currentUserId, permissionId);

        var hasPermission = HttpContextHelper.UserPermission.FirstOrDefault(item => item == "Permission_Delete");

        if (hasPermission is not null)
        {
            var isDeleted = await _permissionService.DeleteAsync(user => user.Id == permissionId);
            if (!isDeleted)
            {
                return NotFound($"Permission with ID {permissionId} not found.");
            }
            return NoContent();
        }
        return BadRequest("Don't have permission.");
    }

    [HttpPatch("{permissionId}")]
    public async Task<IActionResult> UpdateAsync(Guid permissionId, [FromBody] PermissionForUpdateDto permissionForUpdateDto)
    {
        var userId = HttpContextHelper.UserId;
        _logger.LogInformation("User {UserId} is updating permission with ID: {PermissionId}", userId, permissionId);

        var hasPermission = HttpContextHelper.UserPermission.FirstOrDefault(item => item == "Permission_Update");

        if (hasPermission is not null)
        {
            if (permissionForUpdateDto == null)
            {
                return BadRequest("Permission data is required.");
            }

            var updatedPermission = await _permissionService.UpdateAsync(permissionId, permissionForUpdateDto);
            if (updatedPermission == null)
            {
                return NotFound($"Permission with ID {permissionId} not found.");
            }
            return Ok(updatedPermission);
        }
        return BadRequest("Don't have permission.");
    }
}
