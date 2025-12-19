namespace RenessansAPI.Service.DTOs.RolesDto;

public class RoleForUpdateDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<Guid> RolePermissions { get; set; }
}
