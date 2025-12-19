using RenessansAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RenessansAPI.Service.DTOs.UsersDto;

public class UserForUpdateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public UserStatus? UserStatus { get; set; }
    public Guid? RolesId { get; set; }
}
