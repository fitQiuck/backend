namespace RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;

public class CourseEventForClientViewDto
{
    public Guid Id { get; set; }

    // 🌐 Only one language displayed at a time
    public string? Title { get; set; }
    public string? Description { get; set; }

    // 📅 Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // 🖼 Image path
    public string? ImagePath { get; set; }
}
