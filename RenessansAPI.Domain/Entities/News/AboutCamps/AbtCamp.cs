using RenessansAPI.Domain.Common;

namespace RenessansAPI.Domain.Entities.News.AboutCamps;

public class AbtCamp : Auditable
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public string? ImagePath { get; set; }
}
