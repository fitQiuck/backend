using AutoMapper;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Service.DTOs.TokensDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.Helpers;
using RenessansAPI.Service.IService;
using System.Linq.Expressions;

namespace RenessansAPI.Service.Service;

public class TokenService : ITokenService
{
    private readonly IGenericRepository<Token> tokenRepository;
    private readonly IMapper mapper;

    public TokenService(IGenericRepository<Token> repository, IMapper mapper)
    {
        tokenRepository = repository;
        this.mapper = mapper;
    }

    public async Task<IEnumerable<TokenForViewDto>> GetAllAsync(
        Expression<Func<Token, bool>> filter = null,
        string[] includes = null)
    {
        var token = tokenRepository.GetAll(filter, includes).ToList();
        return mapper.Map<IEnumerable<TokenForViewDto>>(token);
    }

    public async Task<TokenForViewDto> GetAsync(
        Expression<Func<Token, bool>> filter,
        string[] includes = null)
    {
        var token = await tokenRepository.GetAsync(filter, includes);
        if (token is null)
            throw new HttpStatusCodeException(404, "Token not found");

        return mapper.Map<TokenForViewDto>(token);
    }

    public async Task<TokenForViewDto> CreateAsync(TokenForCreationDto dto)
    {
        // Example: Check if the same refresh token already exists
        var existingToken = await tokenRepository.GetAsync(s => s.RefreshToken == dto.RefreshToken);
        if (existingToken is not null)
            throw new HttpStatusCodeException(409, "Token with this refresh token already exists");

        var token = mapper.Map<Token>(dto);
        await tokenRepository.CreateAsync(token);
        await tokenRepository.SaveChangesAsync();

        // Re-fetch with includes if needed
        var createdToken = await tokenRepository.GetAsync(s => s.Id == token.Id);
        return mapper.Map<TokenForViewDto>(createdToken);
    }

    public async Task<TokenForViewDto> UpdateAsync(Guid id, TokenForUpdateDto dto)
    {
        var token = await tokenRepository.GetAsync(s => s.Id == id);
        if (token is null)
            throw new HttpStatusCodeException(404, "Token not found");

        // PATCH: only update provided fields
        if (dto.UsersId.HasValue)
            token.UsersId = dto.UsersId.Value;

        if (!string.IsNullOrWhiteSpace(dto.RefreshToken))
            token.RefreshToken = dto.RefreshToken;

        if (!string.IsNullOrWhiteSpace(dto.AccessToken))
            token.AccessToken = dto.AccessToken;

        if (!string.IsNullOrWhiteSpace(dto.IpAddress))
            token.IpAddress = dto.IpAddress;

        if (dto.ExpiredRefreshTokenDate.HasValue)
            token.ExpiredRefreshTokenDate = dto.ExpiredRefreshTokenDate.Value;

        if (dto.ExpiredAccessTokenDate.HasValue)
            token.ExpiredAccessTokenDate = dto.ExpiredAccessTokenDate.Value;

        token.UpdatedAt = DateTime.UtcNow;

        tokenRepository.Update(token);
        await tokenRepository.SaveChangesAsync();

        return mapper.Map<TokenForViewDto>(token);
    }

    public async Task<bool> DeleteAsync(Expression<Func<Token, bool>> filter)
    {
        var token = await tokenRepository.GetAsync(filter)
            ?? throw new HttpStatusCodeException(404, "Token not found");

        token.DeletedBy = HttpContextHelper.UserId ?? Guid.Empty;
        token.DeletedAt = DateTime.UtcNow;

        await tokenRepository.DeleteAsync(token);
        await tokenRepository.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CheckTokenExistsAsync(string token)
    {
        var tokens = await tokenRepository.GetAsync(p => p.AccessToken == token);
        return tokens != null;
    }
}
