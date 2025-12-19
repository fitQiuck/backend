namespace RenessansAPI.Service.DTOs.PermissionsDto;

public class PermissionForCreationDto
{
    private Guid _id = Guid.NewGuid(); // defaultda yangi GUID

    public Guid Id
    {
        get => _id;
        set => _id = value == Guid.Empty ? Guid.NewGuid() : value;
    }

    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
