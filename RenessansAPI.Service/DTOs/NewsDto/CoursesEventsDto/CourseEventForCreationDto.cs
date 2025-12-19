using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;

public class CourseEventForCreationDto
{
    // 🔤 Multilingual titles
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    // 📝 Multilingual descriptions
    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    // 📅 Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // 🖼 Image file
    public IFormFile? Image { get; set; }
}
