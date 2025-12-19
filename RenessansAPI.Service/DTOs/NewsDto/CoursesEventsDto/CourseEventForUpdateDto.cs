using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;

public class CourseEventForUpdateDto
{
    // 🔤 Optional multilingual updates
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    // 📅 Optional date updates
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    // 🖼 Optional new image
    public IFormFile? Image { get; set; }
}
