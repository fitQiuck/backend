namespace RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;

public class CourseEventApplicationForViewDto
{
    public Guid Id { get; set; }
    public Guid CourseEventId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
