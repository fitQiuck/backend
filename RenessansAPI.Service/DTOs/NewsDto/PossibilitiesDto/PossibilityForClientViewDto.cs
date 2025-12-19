namespace RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;

public class PossibilityForClientViewDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;       // frontend uchun tanlangan language
    public string Briefly { get; set; } = null!;
    public string Description { get; set; } = null!;

    public string? ImagePath { get; set; }
}
