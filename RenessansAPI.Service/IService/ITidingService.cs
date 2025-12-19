using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.Tidings;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.TidingsDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface ITidingService
{
    Task<TidingForAdminViewDto> CreateAsync(TidingForCreationDto dto);
    Task<TidingForAdminViewDto> UpdateAsync(Guid id, TidingForUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<TidingForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Tiding, bool>> filter = null,
        string[] includes = null);

    Task<PagedResult<TidingForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Tiding, bool>> filter = null,
        string[] includes = null);

    Task<TidingForClientViewDto> GetByIdForClientAsync(Guid id, Language lang);
    Task<TidingForAdminViewDto> GetByIdForAdminAsync(Guid id);
}
