using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.ImagesDto;

public class ImageForCreationDto
{
    public IFormFile Image { get; set; } = null!;
}
