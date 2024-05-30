using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;
    private string secretKey;

    public UserRepository(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        secretKey = configuration.GetValue<string>("ApiSettings:Secret");
    }

    public bool IsUniqueUser(string username)
    {
        var user = _db.LocalUsers.FirstOrDefault(x => x.UserName == username);
        if (user == null) return true;

        return false;
    }

    public async Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        var user = _db.LocalUsers.FirstOrDefault(u => u.UserName.ToLower() == loginRequestDTO.UserName.ToLower()
                                                      && u.Password == loginRequestDTO.Password);

        if (user == null)
        {
            return new LoginResponseDTO()
            {
                Token = "",
                User = null
            };
        }

        /* if user was found generate JWT Token */
        var tokenHandler = new JwtSecurityTokenHandler();
        // secretKey là khóa bí mật dùng để ký JWT token, được mã hóa thành byte array
        var key = Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            // Subject: Chứa các thông tin xác nhận (claims) về người dùng,
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            }),
            // Expires: Thời hạn của token, ở đây là 7 ngày kể từ thời điểm tạo.
            Expires = DateTime.UtcNow.AddDays(7),
            // SigningCredentials: Chứa thông tin về phương thức ký token, sử dụng thuật toán HMAC SHA256 với khóa đối xứng (SymmetricSecurityKey).
            SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        // Tạo JWT token
        var token = tokenHandler.CreateToken(tokenDescriptor);
        // Tạo đối tượng LoginResponseDTO chứa token và thông tin người dùng, sau đó trả về đối tượng này.
        LoginResponseDTO loginResponseDTO = new LoginResponseDTO()
        {
            // Viết token thành chuỗi (tokenHandler.WriteToken(token)).
            Token = tokenHandler.WriteToken(token),
            User = user
        };
        return loginResponseDTO;
    }

    public async Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
    {
        LocalUser user = new()
        {
            UserName = registerationRequestDTO.UserName,
            Password = registerationRequestDTO.Password,
            Name = registerationRequestDTO.Name,
            Role = registerationRequestDTO.Role
        };

        _db.LocalUsers.Add(user);
        await _db.SaveChangesAsync();
        user.Password = "";
        return user;
    }
}