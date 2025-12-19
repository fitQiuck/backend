using RenessansAPI.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace RenessansAPI.Service.DTOs.UsersDto;

public class UserForCreationDto
{
    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }
    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }
    [Required]
    [MaxLength(30)]
    public string UserName { get; set; }
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [Phone]
    public string PhoneNumber { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    [Required]
    public UserStatus UserStatus { get; set; }
    public Guid? RolesId { get; set; }
}
