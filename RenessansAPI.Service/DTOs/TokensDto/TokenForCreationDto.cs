namespace RenessansAPI.Service.DTOs.TokensDto;

public class TokenForCreationDto
{
    public Guid UsersId { get; set; }
    public string RefreshToken { get; set; }
    public string AccessToken { get; set; }
    public string? IpAddress { get; set; }
    public DateTime ExpiredRefreshTokenDate { get; set; }
    public DateTime ExpiredAccessTokenDate { get; set; }
}
