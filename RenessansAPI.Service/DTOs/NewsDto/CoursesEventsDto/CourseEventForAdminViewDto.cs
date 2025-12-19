namespace RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;

public class CourseEventForAdminViewDto
{
    public Guid Id { get; set; }

    // 🔤 All language versions
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    // 📅 Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // 🖼 Image path
    public string? ImagePath { get; set; }
}
