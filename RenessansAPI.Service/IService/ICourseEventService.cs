using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface ICourseEventService
{
    Task<CourseEventForAdminViewDto> CreateAsync(CourseEventForCreationDto dto);
    Task<CourseEventForAdminViewDto> UpdateAsync(Guid id, CourseEventForUpdateDto dto);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<CourseEventForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<CourseEvent, bool>> filter = null,
        string[] includes = null);

    Task<PagedResult<CourseEventForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<CourseEvent, bool>> filter = null,
        string[] includes = null);

    Task<CourseEventForClientViewDto> GetByIdForClientAsync(Guid id, Language lang);
    Task<CourseEventForAdminViewDto> GetByIdForAdminAsync(Guid id);
}
