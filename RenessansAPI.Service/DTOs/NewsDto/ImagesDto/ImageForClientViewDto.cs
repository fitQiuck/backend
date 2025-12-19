using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.ImagesDto;

public class ImageForClientViewDto
{
    public Guid Id { get; set; }
    public string ImagePath { get; set; } = null!;
}
