using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;

public class PossibilityForCreationDto
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? BrieflyUz { get; set; }
    public string? BrieflyRu { get; set; }
    public string? BrieflyEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public IFormFile? ImagePath { get; set; }
}
