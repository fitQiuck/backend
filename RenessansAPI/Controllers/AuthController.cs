using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Service.DTOs.UsersDto;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        this.authService = authService;
        _userService = userService;
    }


    /// <summary>
    /// Authorization
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    public async ValueTask<IActionResult> Login(UserLoginDto dto)
    {
        var token = await authService.GenerateToken(dto.Email, dto.Password);
        return Ok(new
        {
            token
        });
    }

    [HttpPost("refreshToken")]
    public async ValueTask<IActionResult> restartToken([FromForm] string refreshToken)
    {
        var token = await authService.RestartToken(refreshToken);
        return Ok(new
        {
            token
        });
    }

    [HttpGet("GetPermissionWithToken")]
    public async ValueTask<IActionResult> GetPermissionWithToken()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authorizationHeader))
        {
            return Unauthorized(new { message = "Authorization header is missing." });
        }

        var bearerToken = authorizationHeader.ToString();
        if (!bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Unauthorized(new { message = "Invalid authorization header format." });
        }

        var accessToken = bearerToken.Substring("Bearer ".Length).Trim();

        var token = await authService.GetPermissinWithToken(accessToken);
        if (token == null)
        {
            return Unauthorized(new { message = "Invalid or expired token." });
        }

        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<ActionResult<UserForViewDto>> CreateUser(UserForCreationDto userForCreationDto)
    {
        userForCreationDto.RolesId = null;
        var userDto = await _userService.CreateAsync(userForCreationDto);
        return CreatedAtAction(nameof(CreateUser), new { id = userDto }, userDto);
    }
}
