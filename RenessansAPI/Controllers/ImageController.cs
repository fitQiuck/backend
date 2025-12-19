using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.ImagesDto;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImageController : ControllerBase
{
    private readonly IImageService service;

    public ImageController(IImageService service)
    {
        this.service = service;
    }

    // ===========================
    // 🔹 CLIENT ENDPOINTS
    // ===========================

    /// <summary>
    /// Get all images (client-side)
    /// Example: /api/Images/public?pageIndex=1&pageSize=10
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllForClientAsync([FromQuery] PaginationParams @params)
    {
        var result = await service.GetAllForClientAsync(@params, Language.Uzbek);
        return Ok(result);
    }

    /// <summary>
    /// Get one image by Id (client-side)
    /// Example: /api/Images/public/{id}
    /// </summary>
    [HttpGet("public/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdForClientAsync(Guid id)
    {
        var result = await service.GetByIdForClientAsync(id, Language.Uzbek);
        return Ok(result);
    }

    // ===========================
    // 🔹 ADMIN ENDPOINTS
    // ===========================

    /// <summary>
    /// Get all images (admin-side)
    /// Example: /api/Images/admin?pageIndex=1&pageSize=10
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllForAdminAsync([FromQuery] PaginationParams @params)
    {
        var result = await service.GetAllForAdminAsync(@params);
        return Ok(result);
    }

    /// <summary>
    /// Get one image (admin-side)
    /// </summary>
    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByIdForAdminAsync(Guid id)
    {
        var result = await service.GetByIdForAdminAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Upload new image
    /// </summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateAsync([FromForm] ImageForCreationDto dto)
    {
        var result = await service.CreateAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Update existing image (supports file replacement)
    /// </summary>
    [HttpPatch("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] ImageForUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete an image by Id
    /// </summary>
    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var isDeleted = await service.DeleteAsync(id);
        return Ok(new { success = isDeleted });
    }
}
