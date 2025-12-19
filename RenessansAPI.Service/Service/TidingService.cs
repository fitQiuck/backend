using AutoMapper;
using Microsoft.AspNetCore.Http;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.Tidings;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.TidingsDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class TidingService : ITidingService
{
    private readonly IGenericRepository<Tiding> repository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public TidingService(
        IGenericRepository<Tiding> repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    // -------------------------------------------------------------
    // CREATE
    // -------------------------------------------------------------
    public async Task<TidingForAdminViewDto> CreateAsync(TidingForCreationDto dto)
    {
        string? relativePath = null;

        // Upload image if exists
        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
            var filePath = Path.Combine("wwwroot/images/news", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImageFile.CopyToAsync(stream);

            relativePath = $"images/news/{fileName}";
        }

        var entity = mapper.Map<Tiding>(dto);
        entity.ImagePath = relativePath;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = HttpContextHelper.UserId;
        entity.IsDeleted = false;

        await repository.CreateAsync(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<TidingForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // -------------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------------
    public async Task<TidingForAdminViewDto> UpdateAsync(Guid id, TidingForUpdateDto dto)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "News not found");

        // Map non-null fields
        mapper.Map(dto, entity);

        // Handle image replacement
        if (dto.ImageFile != null && dto.ImageFile.Length > 0)
        {
            // Delete old image
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                var oldFilePath = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldFilePath))
                    File.Delete(oldFilePath);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImageFile.FileName)}";
            var filePath = Path.Combine("wwwroot/images/news", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImageFile.CopyToAsync(stream);

            entity.ImagePath = $"images/news/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<TidingForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // -------------------------------------------------------------
    // DELETE (Soft Delete)
    // -------------------------------------------------------------
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "News not found");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        return true;
    }

    // -------------------------------------------------------------
    // ADMIN — GET ALL
    // -------------------------------------------------------------
    public async Task<PagedResult<TidingForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Tiding, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Tiding, bool>> baseFilter = x => !x.IsDeleted;

        // Combine filters
        Expression<Func<Tiding, bool>> finalFilter = baseFilter;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Tiding));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(baseFilter, param)
            );
            finalFilter = Expression.Lambda<Func<Tiding, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<TidingForAdminViewDto>>(paged.Data)
                           .OrderBy(x => x.Id)
                           .ToList();

        // Convert image to absolute URL
        foreach (var item in mapped)
            item.ImagePath = MakeAbsoluteImageUrl(item.ImagePath);

        return new PagedResult<TidingForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // -------------------------------------------------------------
    // CLIENT — GET ALL (Language Sensitive)
    // -------------------------------------------------------------
    public async Task<PagedResult<TidingForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Tiding, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Tiding, bool>> baseFilter = x => !x.IsDeleted;

        // Combine filters
        Expression<Func<Tiding, bool>> finalFilter = baseFilter;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Tiding));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(baseFilter, param)
            );
            finalFilter = Expression.Lambda<Func<Tiding, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var data = paged.Data
            .Select(e => MapToClientView(e, lang))
            .OrderBy(x => x.Id)
            .ToList();

        foreach (var d in data)
            d.ImagePath = MakeAbsoluteImageUrl(d.ImagePath);

        return new PagedResult<TidingForClientViewDto>
        {
            Data = data,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // -------------------------------------------------------------
    // CLIENT — GET BY ID
    // -------------------------------------------------------------
    public async Task<TidingForClientViewDto> GetByIdForClientAsync(Guid id, Language lang)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "News not found");

        var dto = MapToClientView(entity, lang);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // -------------------------------------------------------------
    // ADMIN — GET BY ID
    // -------------------------------------------------------------
    public async Task<TidingForAdminViewDto> GetByIdForAdminAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "News not found");

        var dto = mapper.Map<TidingForAdminViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // -------------------------------------------------------------
    // PRIVATE — CLIENT MAPPING WITH LANGUAGE SWITCH
    // -------------------------------------------------------------
    private TidingForClientViewDto MapToClientView(Tiding e, Language lang)
    {
        string? title = lang switch
        {
            Language.Uzbek => e.TitleUz,
            Language.Russian => e.TitleRu,
            Language.English => e.TitleEn,
            _ => e.TitleEn
        };

        string? location = lang switch
        {
            Language.Uzbek => e.LocationUz,
            Language.Russian => e.LocationRu,
            Language.English => e.LocationEn,
            _ => e.LocationEn
        };

        string? briefly = lang switch
        {
            Language.Uzbek => e.BrieflyUz,
            Language.Russian => e.BrieflyRu,
            Language.English => e.BrieflyEn,
            _ => e.BrieflyEn
        };

        string? desc = lang switch
        {
            Language.Uzbek => e.DescriptionUz,
            Language.Russian => e.DescriptionRu,
            Language.English => e.DescriptionEn,
            _ => e.DescriptionEn
        };

        // fallback sequence
        title ??= e.TitleEn ?? e.TitleUz ?? e.TitleRu ?? "";
        location ??= e.LocationEn ?? e.LocationUz ?? e.LocationRu ?? "";
        briefly ??= e.BrieflyEn ?? e.BrieflyUz ?? e.BrieflyRu ?? "";
        desc ??= e.DescriptionEn ?? e.DescriptionUz ?? e.DescriptionRu ?? "";

        return new TidingForClientViewDto
        {
            Id = e.Id,
            Title = title!,
            Location = location!,
            Briefly = briefly!,
            Description = desc!,
            Date = e.Date,
            ImagePath = e.ImagePath
        };
    }

    // -------------------------------------------------------------
    // PRIVATE — ABSOLUTE URL BUILDER
    // -------------------------------------------------------------
    private string? MakeAbsoluteImageUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return path;

        return $"{request.Scheme}://{request.Host}/{path.TrimStart('/')}";
    }
}
