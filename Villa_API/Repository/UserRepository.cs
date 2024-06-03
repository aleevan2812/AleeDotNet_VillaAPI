using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
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
                AccessToken = "",
            };

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
                new(ClaimTypes.Role, roles.FirstOrDefault())
            }),
            // Expires: Thời hạn của token, ở đây là 7 ngày kể từ thời điểm tạo.
            Expires = DateTime.UtcNow.AddDays(7),
            // SigningCredentials: Chứa thông tin về phương thức ký token, sử dụng thuật toán HMAC SHA256 với khóa đối xứng (SymmetricSecurityKey).
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Tạo JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Tạo đối tượng LoginResponseDTO chứa token và thông tin người dùng, sau đó trả về đối tượng này.
        TokenDTO tokenDto = new TokenDTO
        {
            // Viết token thành chuỗi (tokenHandler.WriteToken(token)).
            AccessToken = tokenHandler.WriteToken(token),
            // Role = roles.FirstOrDefault()
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
                {
                    await _roleManager.CreateAsync(new IdentityRole(registerationRequestDTO.Role));
                }

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
}