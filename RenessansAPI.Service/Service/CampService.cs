using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.AboutCamps;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class CampService : ICampService
{
    private readonly IGenericRepository<AbtCamp> repository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public CampService(IGenericRepository<AbtCamp> repository, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<PagedResult<AbtCampForClientViewDto>> GetAllAsync(
    PaginationParams @params,
    Expression<Func<AbtCamp, bool>> filter = null,
    string[] includes = null)
    {
        // Combine filter with IsDeleted = false
        Expression<Func<AbtCamp, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(AbtCamp));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(finalFilter, param)
            );
            finalFilter = Expression.Lambda<Func<AbtCamp, bool>>(body, param);
        }

        // Query from repository with pagination
        var query = repository.GetAll(finalFilter, includes);
        var pagedEntities = await query.ToPagedListAsync(@params);

        // Map entities to DTOs
        var mapped = mapper.Map<List<AbtCampForClientViewDto>>(pagedEntities.Data)
                           .OrderBy(x => x.Id)
                           .ToList();

        // Update image paths
        foreach (var camp in mapped)
        {
            if (!string.IsNullOrEmpty(camp.ImagePath))
            {
                camp.ImagePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://" +
                                 $"{httpContextAccessor.HttpContext.Request.Host}/{camp.ImagePath}";
            }
        }

        // Build paged result
        PagedResult<AbtCampForClientViewDto> result = new PagedResult<AbtCampForClientViewDto>()
        {
            Data = mapped,
            TotalItems = pagedEntities.TotalItems,
            TotalPages = pagedEntities.TotalPages,
            CurrentPage = pagedEntities.CurrentPage,
            PageSize = pagedEntities.PageSize
        };

        return result;
    }


    public async Task<AbtCampForClientViewDto> GetAsync(Expression<Func<AbtCamp, bool>> filter, string[] includes = null)
    {
        var combinedFilter = filter;
        Expression<Func<AbtCamp, bool>> notDeletedFilter = x => !x.IsDeleted;

        if (filter != null)
        {
            var param = Expression.Parameter(typeof(AbtCamp));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(notDeletedFilter, param)
            );
            combinedFilter = Expression.Lambda<Func<AbtCamp, bool>>(body, param);
        }
        else
        {
            combinedFilter = notDeletedFilter;
        }

        var camp = await repository.GetAsync(combinedFilter, includes);
        if (camp == null)
            throw new HttpStatusCodeException(404, "Camp not found");

        var result = mapper.Map<AbtCampForClientViewDto>(camp);
        if (!string.IsNullOrEmpty(result.ImagePath))
        {
            result.ImagePath = $"{httpContextAccessor.HttpContext.Request.Scheme}://" +
                               $"{httpContextAccessor.HttpContext.Request.Host}/{result.ImagePath}";
        }

        return result;
    }

    // ✅ CREATE
    public async Task<AbtCampForAdminViewDto> CreateAsync(AbtCampForCreationDto dto)
    {
        string? relativePath = null;

        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
            var filePath = Path.Combine("wwwroot/images/camps", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImageFile.CopyToAsync(stream);

            relativePath = $"images/camps/{fileName}";
        }

        var entity = mapper.Map<AbtCamp>(dto);
        entity.ImagePath = relativePath;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = HttpContextHelper.UserId;
        entity.IsDeleted = false;

        await repository.CreateAsync(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<AbtCampForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ UPDATE
    public async Task<AbtCampForAdminViewDto> UpdateAsync(Guid id, AbtCampForUpdateDto dto)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Camp not found");

        // Map only non-null fields (AutoMapper configured with Condition)
        mapper.Map(dto, entity);

        // Handle image replacement
        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                var oldFilePath = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
            var filePath = Path.Combine("wwwroot/images/camps", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImageFile.CopyToAsync(stream);

            entity.ImagePath = $"images/camps/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<AbtCampForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ DELETE (Soft Delete)
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Camp not found");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        return true;
    }

    // ✅ GET ALL (Admin — all languages)
    public async Task<PagedResult<AbtCampForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<AbtCamp, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<AbtCamp, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(AbtCamp));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(finalFilter, param)
            );
            finalFilter = Expression.Lambda<Func<AbtCamp, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<AbtCampForAdminViewDto>>(paged.Data)
                           .OrderBy(x => x.Id)
                           .ToList();

        foreach (var camp in mapped)
            camp.ImagePath = MakeAbsoluteImageUrl(camp.ImagePath);

        return new PagedResult<AbtCampForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET ALL (Client — single language)
    public async Task<PagedResult<AbtCampForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<AbtCamp, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<AbtCamp, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(AbtCamp));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(finalFilter, param)
            );
            finalFilter = Expression.Lambda<Func<AbtCamp, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var data = paged.Data
            .Select(e => MapToClientView(e, lang))
            .OrderBy(x => x.Id)
            .ToList();

        foreach (var d in data)
            d.ImagePath = MakeAbsoluteImageUrl(d.ImagePath);

        return new PagedResult<AbtCampForClientViewDto>
        {
            Data = data,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET BY ID (Client)
    public async Task<AbtCampForClientViewDto> GetByIdForClientAsync(Guid id, Language lang)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Camp not found");

        var dto = MapToClientView(entity, lang);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // ✅ GET BY ID (Admin)
    public async Task<AbtCampForAdminViewDto> GetByIdForAdminAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Camp not found");

        var dto = mapper.Map<AbtCampForAdminViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    private AbtCampForClientViewDto MapToClientView(AbtCamp e, Language lang)
    {
        string? title = lang switch
        {
            Language.Uzbek => e.TitleUz,
            Language.Russian => e.TitleRu,
            Language.English => e.TitleEn,
            _ => e.TitleUz
        };

        string? desc = lang switch
        {
            Language.Uzbek => e.DescriptionUz,
            Language.Russian => e.DescriptionRu,
            Language.English => e.DescriptionEn,
            _ => e.DescriptionUz
        };

        // 🔹 Fallback order: Uzbek -> Russian -> English
        title ??= e.TitleUz ?? e.TitleRu ?? e.TitleEn ?? string.Empty;
        desc ??= e.DescriptionUz ?? e.DescriptionRu ?? e.DescriptionEn ?? string.Empty;

        return new AbtCampForClientViewDto
        {
            Id = e.Id,
            Title = title!,
            Description = desc!,
            ImagePath = e.ImagePath
        };
    }


    // 🌍 Helper — Build absolute URL for image
    private string? MakeAbsoluteImageUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var request = httpContextAccessor.HttpContext?.Request;
        return request == null ? path : $"{request.Scheme}://{request.Host}/{path.TrimStart('/')}";
    }
}
