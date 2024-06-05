using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly string secretKey;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration,
        UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _mapper = mapper;
        _userManager = userManager;
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
        _roleManager = roleManager;
    }

    public bool IsUniqueUser(string username)
    {
        var user = _db.ApplicationUsers.FirstOrDefault(x => x.UserName == username);
        if (user == null) return true;

        return false;
    }

    public async Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        // var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
        //                                               && u.Password == loginRequestDTO.Password);
        var user = _db.ApplicationUsers
            .FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower());

        bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);
        if (user == null || isValid == false)
            return new TokenDTO
            {
                AccessToken = ""
            };

        var jwtTokenId = $"JTI{Guid.NewGuid()}";
        var accessToken = await GetAccessToken(user, jwtTokenId);

        var refreshToken = await CreateNewRefreshToken(user.Id, jwtTokenId);
        // Tạo đối tượng LoginResponseDTO chứa token và thông tin người dùng, sau đó trả về đối tượng này.
        TokenDTO tokenDto = new TokenDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
        return tokenDto;
    }

    public async Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO)
    {
        ApplicationUser user = new()
        {
            UserName = registerationRequestDTO.UserName,
            Name = registerationRequestDTO.Name
        };

        try
        {
            var result = await _userManager.CreateAsync(user, registerationRequestDTO.Password);
            if (result.Succeeded)
            {
                if (!_roleManager.RoleExistsAsync(registerationRequestDTO.Role).GetAwaiter().GetResult())
                    await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));

                await _userManager.AddToRoleAsync(user, registerationRequestDTO.Role);
                var userToReturn = _db.ApplicationUsers
                    .FirstOrDefault(u => u.UserName == registerationRequestDTO.UserName);
                return _mapper.Map<UserDTO>(userToReturn);
            }
        }
        catch (Exception e)
        {
        }

        // return new UserDTO();
        return null;
    }


    public async Task<string> GetAccessToken(ApplicationUser user, string jwtTokenId)
    {
        var roles = await _userManager.GetRolesAsync(user);
        /* if user was found generate JWT Token */
        var tokenHandler = new JwtSecurityTokenHandler();
        // secretKey là khóa bí mật dùng để ký JWT token, được mã hóa thành byte array
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject: Chứa các thông tin xác nhận (claims) về người dùng,
            Subject = new ClaimsIdentity(new Claim[]
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.Role, roles.FirstOrDefault()),
                new(JwtRegisteredClaimNames.Jti, jwtTokenId),
                new(JwtRegisteredClaimNames.Sub, user.Id)
            }),
            // Expires: Thời hạn của token, ở đây là 7 ngày kể từ thời điểm tạo.
            // Expires = DateTime.UtcNow.AddDays(7),
            Expires = DateTime.UtcNow.AddMinutes(60),
            // SigningCredentials: Chứa thông tin về phương thức ký token, sử dụng thuật toán HMAC SHA256 với khóa đối xứng (SymmetricSecurityKey).
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Tạo JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var tokenStr = tokenHandler.WriteToken(token);
        return tokenStr;
    }

    public async Task<TokenDTO> RefreshAccessToken(TokenDTO tokenDTO)
    {
        /*Find an existing refresh token*/
        var existingRefreshToken =
            await _db.RefreshTokens.FirstOrDefaultAsync(u => u.Refresh_Token == tokenDTO.RefreshToken);
        if (existingRefreshToken == null)
        {
            return new TokenDTO();
        }

        /*Compare data from existing refresh and access token provided and if there is any missmatch then consider it as a fraud*/
        var accessTokenData = GetAccessTokenData(tokenDTO.AccessToken);
        if (!accessTokenData.isSuccessful || accessTokenData.userId != existingRefreshToken.UserId
                                          || accessTokenData.tokenId != existingRefreshToken.JwtTokenId)
        {
            existingRefreshToken.IsValid = false;
            _db.SaveChanges();
        }

        /*When someone tries to use not valid refresh token, fraud possible*/

        /*If just expired then mark as invalid and return empty*/
        if (existingRefreshToken.ExpiresAt < DateTime.UtcNow)
        {
            existingRefreshToken.IsValid = false;
            _db.SaveChanges();
        }

        /*replace old refresh with a new one with updated expire date*/
        var newRefreshToken = await CreateNewRefreshToken(existingRefreshToken.UserId, existingRefreshToken.JwtTokenId);

        /*revoke existing refresh token*/
        existingRefreshToken.IsValid = false;
        _db.SaveChanges();
        
        /*generate new access token*/
        var applicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == existingRefreshToken.UserId);
        if (applicationUser == null)
            return new TokenDTO();

        var newAccessToken = await GetAccessToken(applicationUser, existingRefreshToken.JwtTokenId);

        return new TokenDTO()
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
        };
    }

    private (bool isSuccessful, string userId, string tokenId) GetAccessTokenData(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.ReadJwtToken(accessToken);
            var jwtTokenId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti).Value;
            var userId = jwt.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Sub).Value;
            return (true, userId, jwtTokenId);
        }
        catch
        {
            return (false, null, null);
        }
    }

    private async Task<string> CreateNewRefreshToken(string userId, string tokenId)
    {
        RefreshToken refreshToken = new()
        {
            IsValid = true,
            UserId = userId,
            JwtTokenId = tokenId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            Refresh_Token = Guid.NewGuid() + "-" + Guid.NewGuid(),
        };

        await _db.RefreshTokens.AddAsync(refreshToken);
        await _db.SaveChangesAsync();
        return refreshToken.Refresh_Token;
    }
}