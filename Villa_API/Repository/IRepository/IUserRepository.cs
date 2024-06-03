using Villa_API.Models.Dto;

namespace Villa_API.Repository.IRepository;

public interface IUserRepository
{
    bool IsUniqueUser(string username);
    Task<TokenDTO> Login(LoginRequestDTO loginRequestDTO);
    Task<UserDTO> Register(RegisterationRequestDTO registerationRequestDTO);
}