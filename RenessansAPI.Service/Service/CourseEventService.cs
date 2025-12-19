using AutoMapper;
using Microsoft.AspNetCore.Http;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class CourseEventService : ICourseEventService
{
    private readonly IGenericRepository<CourseEvent> repository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public CourseEventService(IGenericRepository<CourseEvent> repository,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    // ✅ CREATE
    public async Task<CourseEventForAdminViewDto> CreateAsync(CourseEventForCreationDto dto)
    {
        string? relativePath = null;

        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var filePath = Path.Combine("wwwroot/images/events", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            relativePath = $"images/events/{fileName}";
        }

        var entity = mapper.Map<CourseEvent>(dto);
        entity.ImagePath = relativePath;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = HttpContextHelper.UserId;
        entity.IsDeleted = false;

        await repository.CreateAsync(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<CourseEventForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ UPDATE
    public async Task<CourseEventForAdminViewDto> UpdateAsync(Guid id, CourseEventForUpdateDto dto)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Course event not found");

        mapper.Map(dto, entity);

        if (dto.Image != null && dto.Image.Length > 0)
        {
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                var oldPath = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldPath))
                    File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var newPath = Path.Combine("wwwroot/images/events", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);

            using var stream = new FileStream(newPath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            entity.ImagePath = $"images/events/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<CourseEventForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ DELETE (Soft Delete)
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Course event not found");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        return true;
    }

    // ✅ GET ALL (Admin)
    public async Task<PagedResult<CourseEventForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<CourseEvent, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<CourseEvent, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(CourseEvent));
            var body = Expression.AndAlso(Expression.Invoke(filter, param), Expression.Invoke(finalFilter, param));
            finalFilter = Expression.Lambda<Func<CourseEvent, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<CourseEventForAdminViewDto>>(paged.Data)
            .OrderBy(x => x.Id)
            .ToList();

        foreach (var ev in mapped)
            ev.ImagePath = MakeAbsoluteImageUrl(ev.ImagePath);

        return new PagedResult<CourseEventForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET ALL (Client)
    public async Task<PagedResult<CourseEventForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<CourseEvent, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<CourseEvent, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(CourseEvent));
            var body = Expression.AndAlso(Expression.Invoke(filter, param), Expression.Invoke(finalFilter, param));
            finalFilter = Expression.Lambda<Func<CourseEvent, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var data = paged.Data
            .Select(e => MapToClientView(e, lang))
            .OrderBy(x => x.Id)
            .ToList();

        foreach (var d in data)
            d.ImagePath = MakeAbsoluteImageUrl(d.ImagePath);

        return new PagedResult<CourseEventForClientViewDto>
        {
            Data = data,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET BY ID (Client)
    public async Task<CourseEventForClientViewDto> GetByIdForClientAsync(Guid id, Language lang)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Course event not found");

        var dto = MapToClientView(entity, lang);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // ✅ GET BY ID (Admin)
    public async Task<CourseEventForAdminViewDto> GetByIdForAdminAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Course event not found");

        var dto = mapper.Map<CourseEventForAdminViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // 🌐 Helper — Map entity to client DTO with language fallback
    private CourseEventForClientViewDto MapToClientView(CourseEvent e, Language lang)
    {
        string? title = lang switch
        {
            Language.Uzbek => e.TitleUz,
            Language.Russian => e.TitleRu,
            Language.English => e.TitleEn,
            _ => e.TitleEn
        };

        string? desc = lang switch
        {
            Language.Uzbek => e.DescriptionUz,
            Language.Russian => e.DescriptionRu,
            Language.English => e.DescriptionEn,
            _ => e.DescriptionEn
        };

        title ??= e.TitleEn ?? e.TitleUz ?? e.TitleRu ?? string.Empty;
        desc ??= e.DescriptionEn ?? e.DescriptionUz ?? e.DescriptionRu ?? string.Empty;

        return new CourseEventForClientViewDto
        {
            Id = e.Id,
            Title = title!,
            Description = desc!,
            StartDate = e.StartDate,
            EndDate = e.EndDate,
            ImagePath = e.ImagePath
        };
    }

    // 🌍 Helper — Build absolute image URL
    private string? MakeAbsoluteImageUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var request = httpContextAccessor.HttpContext?.Request;
        return request == null ? path : $"{request.Scheme}://{request.Host}/{path.TrimStart('/')}";
    }
}
