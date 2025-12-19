using AutoMapper;
using Microsoft.AspNetCore.Http;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CampPossiblities;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class PossibilityService : IPossibilityService
{
    private readonly IGenericRepository<Possibilities> repository;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IMapper mapper;

    public PossibilityService(
        IGenericRepository<Possibilities> repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    // 📌 CREATE
    public async Task<PossibilityForAdminViewDto> CreateAsync(PossibilityForCreationDto dto)
    {
        string? relativePath = null;

        if (dto.ImagePath != null && dto.ImagePath.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImagePath.FileName)}";
            var filePath = Path.Combine("wwwroot/images/possibility", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImagePath.CopyToAsync(stream);

            relativePath = $"images/possibility/{fileName}";
        }

        var entity = mapper.Map<Possibilities>(dto);
        entity.ImagePath = relativePath;
        entity.CreatedAt = DateTime.UtcNow;
        entity.CreatedBy = HttpContextHelper.UserId;
        entity.IsDeleted = false;

        await repository.CreateAsync(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<PossibilityForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);

        return result;
    }

    // 🔄 UPDATE
    public async Task<PossibilityForAdminViewDto> UpdateAsync(Guid id, PossibilityForUpdateDto dto)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Possibility not found");

        mapper.Map(dto, entity);

        if (dto.ImagePath != null && dto.ImagePath.Length > 0)
        {
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                var oldFile = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldFile)) File.Delete(oldFile);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ImagePath.FileName)}";
            var filePath = Path.Combine("wwwroot/images/possibility", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.ImagePath.CopyToAsync(stream);

            entity.ImagePath = $"images/possibility/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<PossibilityForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);

        return result;
    }

    // 🗑 DELETE (soft delete)
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Possibility not found");

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();
        return true;
    }

    // 📋 GET ALL (Admin)
    public async Task<PagedResult<PossibilityForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Possibilities, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Possibilities, bool>> finalFilter = x => !x.IsDeleted;

        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Possibilities));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(finalFilter, param)
            );
            finalFilter = Expression.Lambda<Func<Possibilities, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<PossibilityForAdminViewDto>>(paged.Data)
                           .OrderBy(x => x.Id)
                           .ToList();

        foreach (var item in mapped)
            item.ImagePath = MakeAbsoluteImageUrl(item.ImagePath);

        return new PagedResult<PossibilityForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // 🌍 GET ALL (Client)
    public async Task<PagedResult<PossibilityForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Possibilities, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Possibilities, bool>> finalFilter = x => !x.IsDeleted;

        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Possibilities));
            var body = Expression.AndAlso(
                Expression.Invoke(filter, param),
                Expression.Invoke(finalFilter, param)
            );
            finalFilter = Expression.Lambda<Func<Possibilities, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var data = paged.Data.Select(e => MapToClientView(e, lang))
                             .OrderBy(x => x.Id)
                             .ToList();

        foreach (var d in data)
            d.ImagePath = MakeAbsoluteImageUrl(d.ImagePath);

        return new PagedResult<PossibilityForClientViewDto>
        {
            Data = data,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // 🆔 GET BY ID (Client)
    public async Task<PossibilityForClientViewDto> GetByIdForClientAsync(Guid id, Language lang)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Possibility not found");

        var dto = MapToClientView(entity, lang);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);

        return dto;
    }

    // 🆔 GET BY ID (Admin)
    public async Task<PossibilityForAdminViewDto> GetByIdForAdminAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
            ?? throw new HttpStatusCodeException(404, "Possibility not found");

        var dto = mapper.Map<PossibilityForAdminViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);

        return dto;
    }


    // 🌐 Build Full Image URL
    private string? MakeAbsoluteImageUrl(string? path)
    {
        if (string.IsNullOrEmpty(path)) return path;

        var request = httpContextAccessor.HttpContext?.Request;
        if (request == null) return path;

        return $"{request.Scheme}://{request.Host}/{path.TrimStart('/')}";
    }

    // 🎨 Client mapping (with language)
    private PossibilityForClientViewDto MapToClientView(Possibilities e, Language lang)
    {
        string? title = lang switch
        {
            Language.Uzbek => e.TitleUz,
            Language.Russian => e.TitleRu,
            Language.English => e.TitleEn,
            _ => e.TitleEn
        };

        string? briefly = lang switch
        {
            Language.Uzbek => e.BrieflyUz,
            Language.Russian => e.BrieflyRu,
            Language.English => e.BrieflyEn,
            _ => e.BrieflyEn
        };

        string? description = lang switch
        {
            Language.Uzbek => e.DescriptionUz,
            Language.Russian => e.DescriptionRu,
            Language.English => e.DescriptionEn,
            _ => e.DescriptionEn
        };

        title ??= e.TitleEn ?? e.TitleUz ?? e.TitleRu ?? "";
        briefly ??= e.BrieflyEn ?? e.BrieflyUz ?? e.BrieflyRu ?? "";
        description ??= e.DescriptionEn ?? e.DescriptionUz ?? e.DescriptionRu ?? "";

        return new PossibilityForClientViewDto
        {
            Id = e.Id,
            Title = title!,
            Briefly = briefly!,
            Description = description!,
            ImagePath = e.ImagePath
        };
    }
}
