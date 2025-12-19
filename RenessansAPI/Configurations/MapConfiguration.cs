using AutoMapper;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Domain.Entities.News.AboutCamps;
using RenessansAPI.Domain.Entities.News.CampPossiblities;
using RenessansAPI.Domain.Entities.News.CoursesEvents;
using RenessansAPI.Domain.Entities.News.OverallImages;
using RenessansAPI.Domain.Entities.News.Tidings;
using RenessansAPI.Domain.Entities.Users;
using RenessansAPI.Service.DTOs.NewsDto.AboutCampsDto;
using RenessansAPI.Service.DTOs.NewsDto.CourseEventApplicationsDto;
using RenessansAPI.Service.DTOs.NewsDto.CoursesEventsDto;
using RenessansAPI.Service.DTOs.NewsDto.ImagesDto;
using RenessansAPI.Service.DTOs.NewsDto.PossibilitiesDto;
using RenessansAPI.Service.DTOs.NewsDto.TidingsDto;
using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.DTOs.RolesDto;
using RenessansAPI.Service.DTOs.TokensDto;
using RenessansAPI.Service.DTOs.UsersDto;

namespace RenessansAPI.Configurations;

public class MapConfiguration : Profile
{
    public MapConfiguration()
    {
        //Users
        CreateMap<UserForCreationDto, User>().ReverseMap();
        CreateMap<UserForUpdateDto, User>().ReverseMap();
        CreateMap<UserForViewDto, User>().ReverseMap();

        //Permissions
        CreateMap<PermissionForCreationDto, Permission>().ReverseMap();
        CreateMap<PermissionForUpdateDto, Permission>().ReverseMap();
        CreateMap<string, PermissionForViewDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src));
        CreateMap<PermissionForViewDto, Permission>().ReverseMap();



        //Roles
        CreateMap<RoleForCreationDto, Role>().ReverseMap();
        CreateMap<RoleForUpdateDto, Role>().ReverseMap();
        CreateMap<Role, RoleForViewDto>()
            .ForMember(d => d.Permissions, opt => opt.MapFrom(s =>
                (s.Permissions ?? new List<Permission>())
                .Select(p => p.Name)
                .ToList()
            ));
        CreateMap<RoleForViewGetDto, Role>().ReverseMap();

        //Token
        CreateMap<TokenForCreationDto, Token>().ReverseMap();
        CreateMap<TokenForUpdateDto, Token>().ReverseMap();
        CreateMap<TokenForViewDto, Token>().ReverseMap();

        //AboutCamp
        // Create -> Entity
        CreateMap<AbtCampForCreationDto, AbtCamp>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore()); // handled manually when uploading
        // Update -> Entity
        CreateMap<AbtCampForUpdateDto, AbtCamp>()
            .ForMember(dest => dest.ImagePath, opt => opt.Ignore()); // handled manually in service
        // Entity -> Admin View (all languages)
        CreateMap<AbtCamp, AbtCampForAdminViewDto>();
        // Entity -> Client View (filtered by language — mapping handled in service)
        CreateMap<AbtCamp, AbtCampForClientViewDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore());

        //CourseEvent
        CreateMap<CourseEventForCreationDto, CourseEvent>().ReverseMap();
        CreateMap<CourseEventForUpdateDto, CourseEvent>().ReverseMap();
        CreateMap<CourseEvent, CourseEventForClientViewDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore());
        CreateMap<CourseEvent, CourseEventForAdminViewDto>();   


        //CourseEventApplication
        CreateMap<CourseEventApplicationForCreationDto, CourseEventApplication>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsHandled, opt => opt.Ignore())
            .ForMember(dest => dest.HandledAt, opt => opt.Ignore())
    .        ForMember(dest => dest.HandledBy, opt => opt.Ignore());

        CreateMap<CourseEventApplication, CourseEventApplicationForViewDto>();
        CreateMap<CourseEventApplication, CourseEventApplicationForAdminViewDto>()
            .ForMember(dest => dest.EventTitle, opt => opt.MapFrom(src => src.CourseEvent.TitleEn)); // admin default title

        //Tiding
        CreateMap<TidingForCreationDto, Tiding>().ReverseMap();
        CreateMap<TidingForUpdateDto, Tiding>().ReverseMap();
        CreateMap<Tiding, TidingForClientViewDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Briefly, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore());
        CreateMap<Tiding, TidingForAdminViewDto>();

        //Possibility
        CreateMap<PossibilityForCreationDto, Possibilities>().ReverseMap();
        CreateMap<PossibilityForUpdateDto, Possibilities>().ReverseMap();
        CreateMap<Possibilities, PossibilityForClientViewDto>()
            .ForMember(dest => dest.Title, opt => opt.Ignore())
            .ForMember(dest => dest.Briefly, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.Ignore());
        CreateMap<Possibilities, PossibilityForAdminViewDto>();

        CreateMap<ImageForCreationDto, Images>().ReverseMap();
        CreateMap<ImageForUpdateDto, Images>().ReverseMap();
        CreateMap<Images, ImageForClientViewDto>();
        CreateMap<Images, ImageForAdminViewDto>();

    }
}
