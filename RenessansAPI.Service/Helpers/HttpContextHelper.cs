using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace RenessansAPI.Service.Helpers;

public class HttpContextHelper
{
    public static IHttpContextAccessor Accessor { get; set; }
    public static HttpContext HttpContext => Accessor?.HttpContext;
    public static IHeaderDictionary ResponseHeaders => HttpContext?.Response?.Headers;
    public static Guid? UserId => GetUserId();
    public static string UserRole => GetUserRoul();
    public static List<string> UserPermission => GetUserPermission();

    private static Guid? GetUserId()
    {
        string value = HttpContext?.User?.Claims
            .FirstOrDefault(p => p.Type == ClaimTypes.NameIdentifier)?.Value;

        bool canParse = Guid.TryParse(value, out Guid id);
        return canParse ? id : null;
    }

    private static string GetUserRoul()
    {
        string value = HttpContext?.User?.Claims.FirstOrDefault(p => p.Type == ClaimTypes.Role)?.Value;
        return value;
    }

    private static List<string> GetUserPermission()
    {
        var permissionsJson = HttpContext?.User?.Claims
            .FirstOrDefault(p => p.Type == "Permissions")?.Value;

        if (string.IsNullOrEmpty(permissionsJson))
        {
            return new List<string>();
        }

        // Agar JSON formatida bo'lmasa, vergul bilan ajratamiz
        if (!permissionsJson.StartsWith("["))
        {
            return permissionsJson.Split(',').ToList();
        }

        var permissions = JsonSerializer.Deserialize<List<string>>(permissionsJson);
        return permissions ?? new List<string>();
    }
}
