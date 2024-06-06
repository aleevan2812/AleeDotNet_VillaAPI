using System.Net;
using Microsoft.AspNetCore.Mvc;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Villa_API.Controllers;

[Route("api/v{version:apiVersion}/UsersAuth")]
// [ApiVersion("1.0")]
[ApiVersionNeutral] // API trung lập
[ApiController]
public class UsersController : Controller
{
    private readonly IUserRepository _userRepo;
    protected APIResponse _response;

    public UsersController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
        _response = new APIResponse();
    }

    [HttpGet("error")]
    public async Task<IActionResult> Error()
    {
        throw new FileNotFoundException();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
    {
        var tokenDto = await _userRepo.Login(model);
        if (tokenDto == null || string.IsNullOrEmpty(tokenDto.AccessToken))
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Username or password is incorrect");
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        _response.Result = tokenDto;
        return Ok(_response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterationRequestDTO model)
    {
        bool ifUserNameUnique = _userRepo.IsUniqueUser(model.UserName);
        if (!ifUserNameUnique)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Username already exists");
            return BadRequest(_response);
        }

        var user = await _userRepo.Register(model);
        if (user == null)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error while registering");
            return BadRequest(_response);
        }

        _response.StatusCode = HttpStatusCode.OK;
        _response.IsSuccess = true;
        return Ok(_response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> GetNewTokenFromRefreshToken([FromBody] TokenDTO tokenDTO)
    {
        if (ModelState.IsValid)
        {
            var tokenDTOResponse = await _userRepo.RefreshAccessToken(tokenDTO);
            if (tokenDTOResponse == null || string.IsNullOrEmpty(tokenDTOResponse.AccessToken))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Token Invalid");
                return BadRequest(_response);
            }

            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = tokenDTOResponse;
            return Ok(_response);
        }

        _response.IsSuccess = false;
        _response.Result = "Invalid Input";
        return BadRequest(_response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> RevokeRefreshToken([FromBody] TokenDTO tokenDTO)
    {
        if (ModelState.IsValid)
        {
            await _userRepo.RevokeRefreshToken(tokenDTO);
            _response.IsSuccess = true;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }

        _response.IsSuccess = false;
        _response.Result = "Invalid Input";
        return BadRequest(_response);
    }
}