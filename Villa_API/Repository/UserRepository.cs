using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool IsUniqueUser(string username)
    {
        throw new NotImplementedException();
    }

    public Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO)
    {
        throw new NotImplementedException();
    }

    public Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO)
    {
        throw new NotImplementedException();
    }
}