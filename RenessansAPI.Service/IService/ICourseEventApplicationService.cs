using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;
using System.Linq.Expressions;

namespace RenessansAPI.Service.IService;

public interface ICourseEventApplicationService
{
    Task<CourseEventApplicationForViewDto> CreateAsync(CourseEventApplicationForCreationDto dto, string requesterIp = null);
    Task<PagedResult<CourseEventApplicationForAdminViewDto>> GetAllAdminAsync(PaginationParams @params, Expression<Func<CourseEventApplication, bool>> filter = null);
    Task<CourseEventApplicationForAdminViewDto> GetByIdAdminAsync(Guid id);
    Task<bool> MarkHandledAsync(Guid applicationId, Guid adminUserId, string? adminNote = null);
}
