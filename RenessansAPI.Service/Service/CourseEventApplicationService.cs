using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Configurations;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Extensions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.Hubs;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class CourseEventApplicationService : ICourseEventApplicationService
{
    private readonly IGenericRepository<CourseEventApplication> _appRepo;
    private readonly IGenericRepository<CourseEvent> _eventRepo;
    private readonly IMapper _mapper;
    private readonly IHubContext<AdminHub> _hub; // SignalR
    private readonly IEmailService _emailService; // optional, inject if you have
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CourseEventApplicationService(
        IGenericRepository<CourseEventApplication> appRepo,
        IGenericRepository<CourseEvent> eventRepo,
        IMapper mapper,
        IHubContext<AdminHub> hub,
        IEmailService emailService, // can be null if not used; register null object if absent
        IHttpContextAccessor httpContextAccessor)
    {
        _appRepo = appRepo;
        _eventRepo = eventRepo;
        _mapper = mapper;
        _hub = hub;
        _emailService = emailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<CourseEventApplicationForViewDto> CreateAsync(CourseEventApplicationForCreationDto dto, string requesterIp = null)
    {
        if (dto == null) throw new HttpStatusCodeException(400, "Invalid payload.");
        if (string.IsNullOrWhiteSpace(dto.FullName) || string.IsNullOrWhiteSpace(dto.PhoneNumber))
            throw new HttpStatusCodeException(400, "FullName and PhoneNumber are required.");

        // Event exists
        var ev = await _eventRepo.GetAsync(e => e.Id == dto.CourseEventId && !e.IsDeleted);
        if (ev == null) throw new HttpStatusCodeException(404, "Event not found.");

        // Normalize & validate phone
        var normalizedPhone = PhoneHelper.Normalize(dto.PhoneNumber);
        if (!PhoneHelper.IsValidUzbekPhone(normalizedPhone))
            throw new HttpStatusCodeException(400, "Phone number is not valid. Use Uzbekistan format: +998XXXXXXXXX");

        // Get user IP
        var ip = requesterIp
                 ?? _httpContextAccessor.HttpContext?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString()
                 ?? "unknown";

        // Get all applications from this IP for this event
        var ipApplications = await _appRepo.GetAll(a =>
            a.CourseEventId == dto.CourseEventId &&
            a.IpAddress == ip &&
            !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        // 5-minutes anti-spam check
        var recent = ipApplications.FirstOrDefault();
        if (recent != null && recent.CreatedAt.AddMinutes(5) > DateTime.UtcNow)
            throw new HttpStatusCodeException(429, "You already applied recently. Please wait a few minutes.");

        // Max 8 applications per IP per event
        if (ipApplications.Count >= 8)
            throw new HttpStatusCodeException(429, "You have reached the maximum number of applications for this event. Your device/IP is temporarily blocked.");

        // Create entity
        var entity = new CourseEventApplication
        {
            CourseEventId = dto.CourseEventId,
            FullName = dto.FullName.Trim(),
            PhoneNumber = normalizedPhone,
            Note = dto.Note?.Trim(),
            CreatedAt = DateTime.UtcNow,
            IpAddress = ip,
            IsHandled = false
        };

        await _appRepo.CreateAsync(entity);
        await _appRepo.SaveChangesAsync();

        // SignalR notification
        try
        {
            await _hub.Clients.Group("admins").SendAsync("NewCourseEventApplication", new
            {
                ApplicationId = entity.Id,
                EventId = entity.CourseEventId,
                FullName = entity.FullName,
                PhoneNumber = entity.PhoneNumber,
                CreatedAt = entity.CreatedAt
            });
        }
        catch { }

        // Optional email notification
        try
        {
            if (_emailService != null)
            {
                var subject = $"New application for event {ev.TitleEn ?? ev.TitleUz ?? ev.TitleRu}";
                var body = $"Name: {entity.FullName}\nPhone: {entity.PhoneNumber}\nEventId: {entity.CourseEventId}\nCreatedAt: {entity.CreatedAt}";
                await _emailService.SendAsync("admin@yourdomain.uz", subject, body);
            }
        }
        catch { }

        return _mapper.Map<CourseEventApplicationForViewDto>(entity);
    }


    public async Task<PagedResult<CourseEventApplicationForAdminViewDto>> GetAllAdminAsync(PaginationParams @params, Expression<Func<CourseEventApplication, bool>> filter = null)
    {
        Expression<Func<CourseEventApplication, bool>> finalFilter = x => true;
        if (filter != null)
        {
            var param = Expression.Parameter(typeof(CourseEventApplication));
            var body = Expression.AndAlso(Expression.Invoke(filter, param), Expression.Invoke(finalFilter, param));
            finalFilter = Expression.Lambda<Func<CourseEventApplication, bool>>(body, param);
        }

        var query = _appRepo.GetAll(finalFilter, includes: new[] { "CourseEvent" });
        var paged = await query.ToPagedListAsync(@params);

        var mapped = paged.Data.Select(a =>
        {
            var m = _mapper.Map<CourseEventApplicationForAdminViewDto>(a);
            m.EventTitle = a.CourseEvent?.TitleEn ?? a.CourseEvent?.TitleUz ?? a.CourseEvent?.TitleRu;
            return m;
        }).ToList();

        return new PagedResult<CourseEventApplicationForAdminViewDto>
        {
            Data = mapped,
            TotalItems = paged.TotalItems,
            TotalPages = paged.TotalPages,
            CurrentPage = paged.CurrentPage,
            PageSize = paged.PageSize
        };
    }

    public async Task<CourseEventApplicationForAdminViewDto> GetByIdAdminAsync(Guid id)
    {
        var entity = await _appRepo.GetAsync(a => a.Id == id, includes: new[] { "CourseEvent" })
                     ?? throw new HttpStatusCodeException(404, "Application not found");

        var dto = _mapper.Map<CourseEventApplicationForAdminViewDto>(entity);
        dto.EventTitle = entity.CourseEvent?.TitleEn ?? entity.CourseEvent?.TitleUz ?? entity.CourseEvent?.TitleRu;
        return dto;
    }

    public async Task<bool> MarkHandledAsync(Guid applicationId, Guid adminUserId, string? adminNote = null)
    {
        var entity = await _appRepo.GetAsync(a => a.Id == applicationId)
                     ?? throw new HttpStatusCodeException(404, "Application not found");

        if (entity.IsHandled) return false;

        entity.IsHandled = true;
        entity.HandledAt = DateTime.UtcNow;
        entity.HandledBy = adminUserId;
        entity.AdminNote = adminNote;

        _appRepo.Update(entity);
        await _appRepo.SaveChangesAsync();

        await _hub.Clients.Group("admins").SendAsync("CourseEventApplicationHandled", new
        {
            ApplicationId = applicationId,
            HandledBy = adminUserId
        });

        return true;
    }
}
