namespace RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;

public class AbtCampForClientViewDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? ImagePath { get; set; }
}
