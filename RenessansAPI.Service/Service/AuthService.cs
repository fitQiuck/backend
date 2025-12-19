using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RenessansAPI.DataAccess.IRepository;
using RenessansAPI.Domain.Entities.Auth;
using RenessansAPI.Domain.Entities.Users;
using RenessansAPI.Domain.Enums;
using RenessansAPI.Service.DTOs.PermissionsDto;
using RenessansAPI.Service.DTOs.TokensDto;
using RenessansAPI.Service.Exceptions;
using RenessansAPI.Service.IService;
using RenessansAPI.Service.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace RenessansAPI.Service.Service;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<Token> _sessionRepository;
    private readonly IGenericRepository<User> _userRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private IAuthService _authServiceImplementation;

    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(IGenericRepository<Token> sessionRepository,
        IGenericRepository<User> userRepository,
        IGenericRepository<Role> roleRepository,
        IConfiguration configuration,
        IMapper mapper)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async ValueTask<TokenForViewDto> GenerateToken(string Username, string password)
    {
        var user = await _userRepository.GetAsync(p => p.Email == Username && p.UserStatus == UserStatus.Active)
                   ?? throw new HttpStatusCodeException(400, "Login or Password is incorrect");

        var res = await _roleRepository.GetAsync(p => p.Id == user.RolesId, includes: ["Permissions"]);


        var res1 = new List<string>();
        foreach (var role in res.Permissions)
        {
            res1.Add(role.Name);
        }

        var jsonPermissin = JsonSerializer.Serialize(res1);

        if (!SecurePasswordHasher.Verify(password, user.Password))
            throw new HttpStatusCodeException(400, "Login or Password is incorrect");


        if (user is null)
            throw new HttpStatusCodeException(400, "Login or Password is incorrect");

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

        Token newSesstion = new Token()
        {
            UsersId = user.Id,
            Users = user,
        };

        var token = new JwtSecurityToken(
             issuer: _configuration["JWT:ValidIssuer"],
             expires: DateTime.Now.AddDays(int.Parse(_configuration["JWT:Expire"])),
             claims: new List<Claim>
             {
                    new Claim("Permissions", jsonPermissin),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, res.Name.ToString())
             },
             signingCredentials: new SigningCredentials(
             key: authSigningKey,
             algorithm: SecurityAlgorithms.HmacSha256)
        );

        var resToken = new JwtSecurityToken(
             issuer: _configuration["JWT:ValidIssuer"],
             expires: DateTime.Now.AddDays(int.Parse(_configuration["JWT:ResExpire"])),
              claims: new List<Claim>
             {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    //new Claim(JwtRegisteredClaimNames.Jti, token1.Key.ToString()),
             },
             signingCredentials: new SigningCredentials(
             key: authSigningKey,
             algorithm: SecurityAlgorithms.HmacSha256)
        );

        newSesstion.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
        newSesstion.RefreshToken = new JwtSecurityTokenHandler().WriteToken(resToken);

        var tokenHandler = new JwtSecurityTokenHandler();
        var AccessToken = tokenHandler.ReadJwtToken(newSesstion.AccessToken);
        var refreshToken = tokenHandler.ReadJwtToken(newSesstion.RefreshToken);

        newSesstion.ExpiredAccessTokenDate = AccessToken.ValidTo;
        newSesstion.ExpiredRefreshTokenDate = refreshToken.ValidTo;


        if (await _sessionRepository.GetAsync(p => p.UsersId == user.Id) == null)
        {
            await _sessionRepository.CreateAsync(newSesstion);
            await _sessionRepository.SaveChangesAsync();
        }
        else
        {
            var getToken = await _sessionRepository.GetAsync(p => p.UsersId == user.Id);

            getToken.AccessToken = newSesstion.AccessToken;
            getToken.RefreshToken = newSesstion.RefreshToken;
            getToken.ExpiredAccessTokenDate = newSesstion.ExpiredAccessTokenDate;
            getToken.ExpiredRefreshTokenDate = newSesstion.ExpiredRefreshTokenDate;

            _sessionRepository.Update(getToken);
            await _sessionRepository.SaveChangesAsync();
        }

        return _mapper.Map<TokenForViewDto>(newSesstion);
    }

    public async ValueTask<string> RestartToken(string token)
    {
        bool resToken = IsTokenExpired(token);
        if (!resToken)
        {
            var getToken = await _sessionRepository.GetAsync(p => p.RefreshToken == token);

            if (getToken == null)
            {
                throw new HttpStatusCodeException(400, "Token not found");
            }

            var user = await _userRepository.GetAsync(p => p.Id == getToken.UsersId && p.UserStatus == UserStatus.Active)
                   ?? throw new HttpStatusCodeException(400, "User not found");

            var res = await _roleRepository.GetAsync(p => p.Id == user.RolesId, includes: ["Permissions"])
                       ?? throw new HttpStatusCodeException(400, "Role note fount");

            var res1 = new List<string>();
            foreach (var role in res.Permissions)
            {
                res1.Add(role.Name);
            }

            var jsonPermissin = JsonSerializer.Serialize(res1);


            var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var newToken = new JwtSecurityToken(
                 issuer: _configuration["JWT:ValidIssuer"],
                 expires: DateTime.Now.AddDays(int.Parse(_configuration["JWT:Expire"])),
                 claims: new List<Claim>
                 {
                    new Claim("Permissions", jsonPermissin),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, res.Name.ToString())
                 },
                 signingCredentials: new SigningCredentials(
                 key: authSigningKey,
                 algorithm: SecurityAlgorithms.HmacSha256)
            );



            getToken.AccessToken = new JwtSecurityTokenHandler().WriteToken(newToken);
            getToken.ExpiredAccessTokenDate = DateTime.UtcNow.AddHours(5);

            _sessionRepository.Update(getToken);
            await _sessionRepository.SaveChangesAsync();

            return new JwtSecurityTokenHandler().WriteToken(newToken);

        }

        throw new HttpStatusCodeException(400, "Token is expired");
    }

    public async ValueTask<List<PermissionForViewDto>> GetPermissinWithToken(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            DateTime expirationTime = jwtToken.ValidTo;
            bool isExpired = expirationTime < DateTime.UtcNow;

            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new Exception("User ID not found in the token.");
            }

            var user = await _userRepository.GetAsync(p => p.Id == Guid.Parse(userId), includes: ["Roles"])
                       ?? throw new HttpStatusCodeException(400, "User not found");

            var res = await _roleRepository.GetAsync(p => p.Id == user.RolesId, includes: ["Permissions"])
                      ?? throw new HttpStatusCodeException(400, "Role note fount");

            var res1 = res.Permissions;

            return _mapper.Map<List<PermissionForViewDto>>(res1);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing token: {ex.Message}");
            throw;
        }
    }

    private bool IsTokenExpired(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            throw new ArgumentException("Token cannot be null or empty.", nameof(token));
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            DateTime expirationTime = jwtToken.ValidTo;

            bool isExpired = expirationTime < DateTime.UtcNow;

            return isExpired;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking token expiration: {ex.Message}");
            throw;
        }
    }
}
