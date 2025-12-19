using RenessansAPI.Domain.Enums;

namespace RenessansAPI.Service.Extensions;

public static class LanguageExtensions
{
    public static Language ToLanguageEnum(this string langCode)
    {
        if (string.IsNullOrWhiteSpace(langCode)) return Language.Uzbek;

        var code = langCode.Length >= 2 ? langCode.Substring(0, 2).ToLower() : langCode.ToLower();

        return code switch
        {
            "uz" => Language.Uzbek,
            "ru" => Language.Russian,
            "en" => Language.English,
            _ => Language.Uzbek
        };
    }

    public static string Normalize(this string langCode)
    {
        if (string.IsNullOrWhiteSpace(langCode)) return "uz";
        return langCode.Substring(0, 2).ToLower() switch
        {
            "uz" => "uz",
            "ru" => "ru",
            "en" => "en",
            _ => "uz"
        };
    }
}
