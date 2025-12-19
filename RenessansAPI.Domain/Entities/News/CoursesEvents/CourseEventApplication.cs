using RenessansAPI.Domain.Common;

namespace RenessansAPI.Domain.Entities.News.CoursesEvents;

public class CourseEventApplication :Auditable
{
    public Guid CourseEventId { get; set; }
    public CourseEvent CourseEvent { get; set; } = null!;

    public string FullName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!; // normalized (+998...)
    public string? Note { get; set; }

    // Admin workflow
    public bool IsHandled { get; set; } = false;
    public DateTime? HandledAt { get; set; }
    public Guid? HandledBy { get; set; } // admin user id who processed the application

    // Optionally add AdminNote for internal comments
    public string? AdminNote { get; set; }

    public string IpAddress { get; set; } = null!;
}
