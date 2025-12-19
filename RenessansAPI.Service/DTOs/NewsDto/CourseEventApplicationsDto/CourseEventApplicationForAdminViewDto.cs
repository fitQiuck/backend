namespace RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;

public class CourseEventApplicationForAdminViewDto : CourseEventApplicationForViewDto
{
    public string? EventTitle { get; set; } // useful for admin list
    public bool IsHandled { get; set; }
    public DateTime? HandledAt { get; set; }
    public Guid? HandledBy { get; set; }
    public string? AdminNote { get; set; }

    // admin uchun IP address
    public string? IpAddress { get; set; }
}
