using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.TidingsDto;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.IService;

namespace RenessansAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TidingController : ControllerBase
{
    private readonly ITidingService service;

    public TidingController(ITidingService service)
    {
        this.service = service;
    }

    /// <summary>
    /// Get all news (client-side, one language)
    /// Example: /api/Tiding/public?lang=English&pageIndex=1&pageSize=10
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
    /// Get all news (admin-side, all languages)
    /// Example: /api/Tiding/admin?pageIndex=1&pageSize=10
    /// </summary>
    [HttpGet("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetAllForAdminAsync([FromQuery] PaginationParams @params)
    {
        var result = await service.GetAllForAdminAsync(@params);
        return Ok(result);
    }

    /// <summary>
    /// Get one news item (admin-side, full language data)
    /// </summary>
    [HttpGet("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetByIdForAdminAsync(Guid id)
    {
        var result = await service.GetByIdForAdminAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Create a news item (with optional image upload)
    /// </summary>
    [HttpPost("admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CreateAsync([FromForm] TidingForCreationDto dto)
    {
        var result = await service.CreateAsync(dto);

        // You can return Created(...) but Ok is fine too
        return Ok(result);
    }

    /// <summary>
    /// Update news (partial update, supports image replace)
    /// </summary>
    [HttpPatch("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromForm] TidingForUpdateDto dto)
    {
        var result = await service.UpdateAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Soft delete a news item
    /// </summary>
    [HttpDelete("admin/{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var deleted = await service.DeleteAsync(id);
        return Ok(new { success = deleted });
    }
}
