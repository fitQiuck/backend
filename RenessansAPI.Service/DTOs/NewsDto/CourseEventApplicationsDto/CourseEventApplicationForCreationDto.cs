namespace RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;

public class CourseEventApplicationForCreationDto
{
    public Guid CourseEventId { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string? Note { get; set; }

    // Optional, service orqali set qilinadi, userdan olinmaydi
    public string? IpAddress { get; set; }
}
