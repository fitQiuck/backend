using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.AboutCamps;
using RenessansAPI.Domain.Entities.News.OverallImages;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;
using RenessansAPI.Service.DTOs.NewsDto.ImagesDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface IImageService
{
    Task<ImageForAdminViewDto> CreateAsync(ImageForCreationDto dto);
    Task<ImageForAdminViewDto> UpdateAsync(Guid id, ImageForUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<ImageForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Images, bool>> filter = null,
        string[] includes = null);

    Task<PagedResult<ImageForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Images, bool>> filter = null,
        string[] includes = null);

    Task<ImageForClientViewDto> GetByIdForClientAsync(Guid id, Language lang);
    Task<ImageForAdminViewDto> GetByIdForAdminAsync(Guid id);
}
