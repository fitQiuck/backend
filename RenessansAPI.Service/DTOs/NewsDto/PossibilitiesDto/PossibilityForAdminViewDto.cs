namespace RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;

public class PossibilityForAdminViewDto
{
    public Guid Id { get; set; }

    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? BrieflyUz { get; set; }
    public string? BrieflyRu { get; set; }
    public string? BrieflyEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public string? ImagePath { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
