using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.ImagesDto;

public class ImageForAdminViewDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public Guid? UpdatedBy { get; set; }
}
