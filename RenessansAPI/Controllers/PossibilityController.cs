using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PossibilityController : ControllerBase
{
    private readonly IPossibilityService service;

    public PossibilityController(IPossibilityService service)
    {
        this.service = service; 
    }

    // ===========================
    // 🌍 PUBLIC (CLIENT) ENDPOINTS
    // ===========================

    /// <summary>
    /// Get all possibilities (client-side, single language)
    /// Example: /api/Possibility/public?lang=English&pageIndex=1&pageSize=10
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllForClientAsync([FromQuery] PaginationParams @params, [FromQuery] string lang = null)
    {
        // Query param > Middleware > Default
        Language languageEnum = Language.Uzbek;

        if (!string.IsNullOrWhiteSpace(lang))
            languageEnum = lang.ToLanguageEnum();
        else if (HttpContext.Items.TryGetValue("Language", out var headerLang) && headerLang is Language hl)
            languageEnum = hl;

        var result = await service.GetAllForClientAsync(@params, languageEnum);
        return Ok(result);
    }

    [HttpGet("public/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdForClientAsync(Guid id, [FromQuery] string lang = null)
    {
        Language languageEnum = Language.Uzbek;

        if (!string.IsNullOrWhiteSpace(lang))
            languageEnum = lang.ToLanguageEnum();
        else if (HttpContext.Items.TryGetValue("Language", out var headerLang) && headerLang is Language hl)
            languageEnum = hl;

        var result = await service.GetByIdForClientAsync(id, languageEnum);
        return Ok(result);
    }

    // ===========================
    // 🔹 ADMIN ENDPOINTS
    // ===========================

    /// <summary>
    /// Get all possibilities (admin-side, all languages included)
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllForAdminAsync([FromQuery] PaginationParams @params)
    {
        var result = await service.GetAllForAdminAsync(@params);
        return Ok(result);
    }

    /// <summary>
    /// Get one possibility (admin-side, all languages)
    /// </summary>
    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByIdForAdminAsync(Guid id)
    {
        var result = await service.GetByIdForAdminAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Create new possibility (image upload supported)
    /// </summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateAsync([FromForm] PossibilityForCreationDto dto)
    {
        var result = await service.CreateAsync(dto);
        return Ok(result);
    }

    /// <summary>
    /// Update existing possibility (supports partial update + image upload)
    /// </summary>
    [HttpPatch("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] PossibilityForUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete possibility
    /// </summary>
    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var isDeleted = await service.DeleteAsync(id);
        return Ok(new { success = isDeleted });
    }
}
