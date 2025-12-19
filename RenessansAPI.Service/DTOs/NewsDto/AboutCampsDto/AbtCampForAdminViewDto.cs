namespace RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;

public class AbtCampForAdminViewDto
{
    public Guid Id { get; set; }

    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public string? ImagePath { get; set; }

    // auditing info (optional)
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
