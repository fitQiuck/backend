using Microsoft.AspNetCore.Http;

namespace RenessansAPI.Service.DTOs.NewsDto.ImagesDto;

public class ImageForUpdateDto
{
    public IFormFile Image { get; set; } = null!;
}
