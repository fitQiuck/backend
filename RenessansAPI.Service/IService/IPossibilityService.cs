using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CampPossiblities;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface IPossibilityService
{
    Task<PossibilityForAdminViewDto> CreateAsync(PossibilityForCreationDto dto);
    Task<PossibilityForAdminViewDto> UpdateAsync(Guid id, PossibilityForUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<PossibilityForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Possibilities, bool>> filter = null,
        string[] includes = null);

    Task<PagedResult<PossibilityForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Possibilities, bool>> filter = null,
        string[] includes = null);

    Task<PossibilityForClientViewDto> GetByIdForClientAsync(Guid id, Language lang);
    Task<PossibilityForAdminViewDto> GetByIdForAdminAsync(Guid id);
}
