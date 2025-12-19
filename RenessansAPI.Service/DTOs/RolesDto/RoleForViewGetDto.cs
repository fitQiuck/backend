using RenessansAPI.Domain.Entities.Auth;

namespace RenessansAPI.Service.DTOs.RolesDto;

public class RoleForViewGetDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    //public Object? Permissions { get; set; }
    public string? CreatedBy { get; set; }
    public List<Permission> Permissions { get; set; }
}
