using RenessansAPI.Domain.Common;

namespace RenessansAPI.Domain.Entities.News.CoursesEvents;

public class CourseEvent : Auditable
{
    // 🔤 Multilingual title
    public string? TitleUz { get; set; }
    public string? TitleRu { get; set; }
    public string? TitleEn { get; set; }

    // 📝 Multilingual description
    public string? DescriptionUz { get; set; }
    public string? DescriptionRu { get; set; }
    public string? DescriptionEn { get; set; }

    // 🖼 Image for the card/banner
    public string? ImagePath { get; set; }

    // 📅 Duration of event or course
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // 🔗 Linked applications (Apply button submissions)
    public ICollection<CourseEventApplication>? Applications { get; set; }
}
