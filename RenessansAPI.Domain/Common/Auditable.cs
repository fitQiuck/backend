namespace RenessansAPI.Domain.Common;

public class Auditable
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; } = DateTime.UtcNow;
    public Guid? UpdatedBy { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? DeletedBy { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
}
