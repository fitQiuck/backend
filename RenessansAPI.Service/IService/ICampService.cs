using RenessansAPI.Domain.Configurations; // Add this using if PaginationParams is defined here
using RenessansAPI.Domain.Entities.News.AboutCamps;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;
using RenessansAPI.Service.Helpers;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface ICampService
{
    Task<AbtCampForAdminViewDto> CreateAsync(AbtCampForCreationDto dto);
    Task<AbtCampForAdminViewDto> UpdateAsync(Guid id, AbtCampForUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<AbtCampForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<AbtCamp, bool>> filter = null,
        string[] includes = null);

    Task<PagedResult<AbtCampForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<AbtCamp, bool>> filter = null,
        string[] includes = null);

    Task<AbtCampForClientViewDto> GetByIdForClientAsync(Guid id, Language lang);
    Task<AbtCampForAdminViewDto> GetByIdForAdminAsync(Guid id);
}
