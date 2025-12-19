using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.TidingsDto;

public class TidingForUpdateDto
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? LocationUz { get; set; }
    public string? LocationRu { get; set; }
    public string? LocationEn { get; set; }

    public string? BrieflyUz { get; set; }
    public string? BrieflyRu { get; set; }
    public string? BrieflyEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public DateTime? Date { get; set; }

    public IFormFile? ImageFile { get; set; }
}
