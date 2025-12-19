using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Service.DTOs.RolesDto;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> _logger)
    {
        _roleService = roleService;
        this._logger = _logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<RoleForViewDto>>> GetAllAsync()
    {
        _logger.LogInformation("User {UserId} requested all roles", HttpContextHelper.UserId);

        if (!HasPermission("Role_Get"))
            return Forbid();

        var allRoles = await _roleService.GetAllAsync();
        return Ok(allRoles);
    }

    [HttpGet("{roleId:guid}")]
    public async Task<ActionResult<RoleForViewGetDto>> GetById(Guid roleId)
    {
        _logger.LogInformation("User {UserId} requested role {RoleId}", HttpContextHelper.UserId, roleId);

        if (!HasPermission("Role_Get"))
            return Forbid();

        var role = await _roleService.GetAsync(r => r.Id == roleId);
        return Ok(role); // service throws 404 if not found
    }

    [HttpPost]
    public async Task<ActionResult<RoleForViewDto>> CreateAsync([FromBody] RoleForCreationDto roleForCreationDto)
    {
        _logger.LogInformation("User {UserId} is creating a new role", HttpContextHelper.UserId);

        if (!HasPermission("Role_Create"))
            return Forbid();

        var roleDto = await _roleService.CreateAsync(roleForCreationDto);
        return CreatedAtAction(nameof(GetById), new { roleId = roleDto.Id }, roleDto);
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid roleId)
    {
        _logger.LogInformation("User {UserId} is deleting role {RoleId}", HttpContextHelper.UserId, roleId);

        if (!HasPermission("Role_Delete"))
            return Forbid();

        await _roleService.DeleteAsync(r => r.Id == roleId);
        return NoContent();
    }

    [HttpPatch("{roleId:guid}")]
    public async Task<ActionResult<RoleForViewDto>> UpdateAsync(Guid roleId, [FromBody] RoleForUpdateDto roleForUpdateDto)
    {
        _logger.LogInformation("User {UserId} is updating role {RoleId}", HttpContextHelper.UserId, roleId);

        if (!HasPermission("Role_Update"))
            return Forbid();

        var updatedRole = await _roleService.UpdateAsync(roleId, roleForUpdateDto);
        return Ok(updatedRole);
    }

    private static bool HasPermission(string perm) =>
        HttpContextHelper.UserPermission?.Contains(perm) == true;
}
