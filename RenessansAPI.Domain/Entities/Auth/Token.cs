using RenessansAPI.Domain.Common;
using RenessansAPI.Domain.Entities.Users;

namespace RenessansAPI.Domain.Entities.Auth;

public class Token : Auditable
{
    public Guid UsersId { get; set; }
    public User Users { get; set; }
    public string RefreshToken { get; set; }
    public string AccessToken { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiredRefreshTokenDate { get; set; }
    public DateTime ExpiredAccessTokenDate { get; set; }
}
