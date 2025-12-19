namespace RenessansAPI.Service.DTOs.RolesDto;

public class RoleForCreationDto
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<Guid> RolePermissions { get; set; }
}
