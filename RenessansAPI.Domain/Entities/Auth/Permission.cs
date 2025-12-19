using RenessansAPI.Domain.Common;

namespace RenessansAPI.Domain.Entities.Auth;

public class Permission : Auditable
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Role>? Roles { get; set; }

    public Permission()
    {
        Roles = new List<Role>();
    }
}
