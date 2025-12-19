namespace RenessansAPI.Domain.Configurations;

public class EmailOptions
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; } = 587;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public bool UseSsl { get; set; } = true;
    public string From { get; set; } = null!;     // e.g. "no-reply@renessans.uz"
    public string DisplayName { get; set; } = "Renessans";
}
