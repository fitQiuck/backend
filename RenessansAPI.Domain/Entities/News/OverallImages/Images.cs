using RenessansAPI.Domain.Common;

namespace RenessansAPI.Domain.Entities.News.OverallImages;

public class Images : Auditable
{
    public string ImagePath { get; set; } = null!;
}
