using Villa_API.Models;
using Villa_API.Models.Dto;

namespace Villa_API.Repository.IRepository;

public interface IUserRepository
{
    bool IsUniqueUser(string username);
    Task<LoginResponseDTO> Login(LoginRequestDTO loginRequestDTO);
    Task<LocalUser> Register(RegisterationRequestDTO registerationRequestDTO);
}