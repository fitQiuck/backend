namespace RenessansAPI.Service.Helpers;

public static class PhoneHelper
{
    public static string Normalize(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return string.Empty;
        var digits = new string(phone.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("998")) return $"+{digits}";
        if (digits.StartsWith("00")) return $"+{digits.Substring(2)}";

        // local formats:
        if (digits.Length == 9) return $"+998{digits}";
        if (digits.Length == 10 && digits.StartsWith("8")) return $"+998{digits.Substring(1)}";

        // fallback: prepend +
        return digits.StartsWith("+") ? digits : $"+{digits}";
    }

    public static bool IsValidUzbekPhone(string normalized)
    {
        if (string.IsNullOrWhiteSpace(normalized)) return false;
        // E.164 for Uzbekistan: +998 and 9 more digits => length 13
        return normalized.StartsWith("+998") && normalized.Length == 13;
    }
}
