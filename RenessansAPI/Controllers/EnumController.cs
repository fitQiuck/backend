using Microsoft.AspNetCore.Mvc;
using RenessansAPI.Domain.Enums;
using System.Reflection;

namespace RenessansAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnumController : ControllerBase
{
    [HttpGet("{enumName}")]
    public IActionResult GetEnumValues(string enumName)
    {
        // Auth.Domain.Enums namespace ichidan enum qidiradi
        var enumType = Assembly.GetAssembly(typeof(UserStatus))!
            .GetTypes()
            .FirstOrDefault(t => t.IsEnum && t.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase));

        if (enumType == null)
            return NotFound(new { Message = $"Enum '{enumName}' not found." });

        var values = Enum.GetValues(enumType)
            .Cast<object>()
            .Select(e => new
            {
                Id = (int)e,
                Name = e.ToString()
            });

        return Ok(values);
    }
}
