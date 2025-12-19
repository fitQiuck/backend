namespace RenessansAPI.Service.DTOs.NewsDto.TidingsDto;

public class TidingForClientViewDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Location { get; set; } = null!;
    public string Briefly { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime? Date { get; set; }
    public string? ImagePath { get; set; }
}
