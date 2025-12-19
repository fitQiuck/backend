namespace RenessansAPI.Service.DTOs.RolesDto;

public class RoleForViewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public List<string>? Permissions { get; set; }

    public RoleForViewDto()
    {
        Permissions = new List<string>();
    }
}
