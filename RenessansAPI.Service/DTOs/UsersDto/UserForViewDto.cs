using RenessansAPI.Domain.Enums;

namespace RenessansAPI.Service.DTOs.UsersDto;

public class UserForViewDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string AvatarUrl { get; set; }
    public UserStatus UserStatus { get; set; }
    public Guid RolesId { get; set; }
    public string RoleName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
