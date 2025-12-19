using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;

public class AbtCampForUpdateDto
{
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    public IFormFile? ImageFile { get; set; }
}
