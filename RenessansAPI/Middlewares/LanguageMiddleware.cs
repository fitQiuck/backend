using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.Extensions;
using System.Linq;

namespace RenessansAPI.Middlewares;

public class LanguageMiddleware
{
    private readonly RequestDelegate _next;

    public LanguageMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Default uz
        Language languageEnum = Language.Uzbek;

        // Headerni olish
        var headerLang = context.Request.Headers["Accept-Language"].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(headerLang))
        {
            languageEnum = headerLang.ToLanguageEnum();
        }

        // Middleware orqali contextga saqlash
        context.Items["Language"] = languageEnum;

        await _next(context);
    }
}
