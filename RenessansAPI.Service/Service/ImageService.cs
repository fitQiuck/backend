using AutoMapper;
using Microsoft.AspNetCore.Http;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.OverallImages;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.NewsDto.ImagesDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class ImageService : IImageService
{
    private readonly IGenericRepository<Images> repository;
    private readonly IMapper mapper;
    private readonly IHttpContextAccessor httpContextAccessor;

    public ImageService(IGenericRepository<Images> repository,
        IMapper mapper,
        IHttpContextAccessor httpContextAccessor)
    {
        this.repository = repository;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    // ✅ CREATE
    public async Task<ImageForAdminViewDto> CreateAsync(ImageForCreationDto dto)
    {
        if (dto.Image == null || dto.Image.Length == 0)
            throw new HttpStatusCodeException(400, "Image is required");

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
        var folderPath = Path.Combine("wwwroot/images/overallImages");
        Directory.CreateDirectory(folderPath);

        var filePath = Path.Combine(folderPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await dto.Image.CopyToAsync(stream);

        var entity = new Images
        {
            ImagePath = $"images/overallImages/{fileName}",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = HttpContextHelper.UserId,
            IsDeleted = false
        };

        await repository.CreateAsync(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<ImageForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ UPDATE
    public async Task<ImageForAdminViewDto> UpdateAsync(Guid id, ImageForUpdateDto dto)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
                     ?? throw new HttpStatusCodeException(404, "Image not found");

        if (dto.Image != null && dto.Image.Length > 0)
        {
            if (!string.IsNullOrEmpty(entity.ImagePath))
            {
                var oldFile = Path.Combine("wwwroot", entity.ImagePath);
                if (File.Exists(oldFile))
                    File.Delete(oldFile);
            }

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine("wwwroot/images/overallImages");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            entity.ImagePath = $"images/overallImages/{fileName}";
        }

        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        var result = mapper.Map<ImageForAdminViewDto>(entity);
        result.ImagePath = MakeAbsoluteImageUrl(result.ImagePath);
        return result;
    }

    // ✅ DELETE
    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
                     ?? throw new HttpStatusCodeException(404, "Image not found");

        if (!string.IsNullOrEmpty(entity.ImagePath))
        {
            var filePath = Path.Combine("wwwroot", entity.ImagePath);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = HttpContextHelper.UserId;

        repository.Update(entity);
        await repository.SaveChangesAsync();

        return true;
    }

    // ✅ GET ALL (Admin)
    public async Task<PagedResult<ImageForAdminViewDto>> GetAllForAdminAsync(
        PaginationParams @params,
        Expression<Func<Images, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Images, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Images));
            var body = Expression.AndAlso(Expression.Invoke(filter, param), Expression.Invoke(finalFilter, param));
            finalFilter = Expression.Lambda<Func<Images, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<ImageForAdminViewDto>>(paged.Data);

        foreach (var m in mapped)
            m.ImagePath = MakeAbsoluteImageUrl(m.ImagePath);

        return new PagedResult<ImageForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET ALL (Client)
    public async Task<PagedResult<ImageForClientViewDto>> GetAllForClientAsync(
        PaginationParams @params,
        Language lang,
        Expression<Func<Images, bool>> filter = null,
        string[] includes = null)
    {
        Expression<Func<Images, bool>> finalFilter = x => !x.IsDeleted;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(Images));
            var body = Expression.AndAlso(Expression.Invoke(filter, param), Expression.Invoke(finalFilter, param));
            finalFilter = Expression.Lambda<Func<Images, bool>>(body, param);
        }

        var query = repository.GetAll(finalFilter, includes);
        var paged = await query.ToPagedListAsync(@params);

        var mapped = mapper.Map<List<ImageForClientViewDto>>(paged.Data);
        foreach (var m in mapped)
            m.ImagePath = MakeAbsoluteImageUrl(m.ImagePath);

        return new PagedResult<ImageForClientViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    // ✅ GET BY ID (Client)
    public async Task<ImageForClientViewDto> GetByIdForClientAsync(Guid id, Language lang)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
                     ?? throw new HttpStatusCodeException(404, "Image not found");

        var dto = mapper.Map<ImageForClientViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // ✅ GET BY ID (Admin)
    public async Task<ImageForAdminViewDto> GetByIdForAdminAsync(Guid id)
    {
        var entity = await repository.GetAsync(x => x.Id == id && !x.IsDeleted)
                     ?? throw new HttpStatusCodeException(404, "Image not found");

        var dto = mapper.Map<ImageForAdminViewDto>(entity);
        dto.ImagePath = MakeAbsoluteImageUrl(dto.ImagePath);
        return dto;
    }

    // 🌍 Helper — Absolute URL
    private string MakeAbsoluteImageUrl(string path)
    {
        if (string.IsNullOrEmpty(path)) return path;
        var request = httpContextAccessor.HttpContext?.Request;
        return request == null ? path : $"{request.Scheme}://{request.Host}/{path.TrimStart('/')}";
    }
}
