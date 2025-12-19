namespace RenessansAPI.Service.IService;

public interface IEmailService
{
    /// <summary>
    /// Send an email asynchronously. Returns true if sent successfully.
    /// </summary>
    Task<bool> SendAsync(string to, string subject, string body, bool isHtml = true);
}
