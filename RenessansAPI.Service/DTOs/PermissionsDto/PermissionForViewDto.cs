using RenessansAPI.Domain.Entities.Auth;

namespace RenessansAPI.Service.DTOs.PermissionsDto;

public class PermissionForViewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<Role> Role { get; set; } = new List<Role>();
}
