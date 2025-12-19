using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Service.DTOs.UsersDto;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<UserForViewDto>>> GetAllAsync([FromQuery] string? search)
    {
        _logger.LogInformation("User {UserId} is requesting all users. Search: {Search}", HttpContextHelper.UserId, search);

        if (!HasPermission("User_Get"))
            return Forbid();

        // If search is empty, don't filter; otherwise, filter by username contains search
        var users = string.IsNullOrWhiteSpace(search)
            ? await _userService.GetAllAsync()
            : await _userService.GetAllAsync(u => (u.UserName ?? "").ToLower().Contains(search!.ToLower()));

        return Ok(users);
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<UserForViewDto>> GetById([FromRoute] Guid userId)
    {
        _logger.LogInformation("User {UserId} is requesting user by ID: {TargetUserId}", HttpContextHelper.UserId, userId);

        if (!HasPermission("User_Get"))
            return Forbid();

        var user = await _userService.GetAsync(u => u.Id == userId);
        return Ok(user); // assuming service throws 404 if not found
    }

    [Authorize(Roles = "Admin,SuperAdmin")]
    [HttpGet("gest")]
    public async Task<IActionResult> GetGestUsers()
    {
        var result = await _userService.GetGestUsersAsync();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<UserForViewDto>> CreateUser([FromBody] UserForCreationDto dto)
    {
        _logger.LogInformation("User {UserId} is creating a new user.", HttpContextHelper.UserId);

        if (!HasPermission("User_Create"))
            return Forbid();

        var created = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { userId = created.Id }, created);
    }

    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid userId)
    {
        _logger.LogInformation("User {UserId} is deleting user with ID: {TargetUserId}", HttpContextHelper.UserId, userId);

        if (!HasPermission("User_Delete"))
            return Forbid();

        await _userService.DeleteAsync(u => u.Id == userId);
        return NoContent();
    }

    [HttpPatch("{userId:guid}")]
    public async Task<ActionResult<UserForViewDto>> UpdateUser([FromRoute] Guid userId, [FromBody] UserForUpdateDto dto)
    {
        _logger.LogInformation("User {UserId} is updating user with ID: {TargetUserId}", HttpContextHelper.UserId, userId);

        if (!HasPermission("User_Update"))
            return Forbid();

        if (dto is null)
            return BadRequest("User data is required.");

        var updated = await _userService.UpdateAsync(userId, dto);
        return Ok(updated);
    }

    [HttpPatch]
    public async Task<ActionResult<UserForViewDto>> ChangePasswordUser([FromBody] ChangePasswordDto request)
    {
        _logger.LogInformation("User {UserId} is updating password for user with email: {Email}",
            HttpContextHelper.UserId, request.Email);

        if (!HasPermission("User_Update"))
            return Forbid();

        var updated = await _userService.ChangePassword(request.Email, request.Password);
        return Ok(updated);
    }


    private static bool HasPermission(string perm) =>
        HttpContextHelper.UserPermission?.Contains(perm) == true;
}
