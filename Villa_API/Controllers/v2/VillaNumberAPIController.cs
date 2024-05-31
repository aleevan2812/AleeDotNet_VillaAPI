using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Alee_VillaNumberAPI.Controllers.v2;

// [Route("api/[controller]")] can use this
// [Route("api/VillaNumberAPI")] // fix err: Action 'Alee_VillaNumberAPI.Controllers.VillaNumberAPIController.GetVillaNumbers (Alee_VillaNumberAPI)' does not have an attribute route. Action methods on controllers annotated with ApiControllerAttribute must be attribute routed.
[Route("api/v{version:apiVersion}/VillaNumberAPI")]
[ApiController]
[ApiVersion("2.0", Deprecated = true)] // lỗi thời :)
public class VillaNumberAPIController : ControllerBase // dont need Controller Class
{
    private readonly IVillaRepository _dbVilla;
    private readonly IVillaNumberRepository _dbVillaNumber;

    private readonly IMapper _mapper;
    // private readonly ILogger<VillaNumberAPIController> _logger;
    // // logger for logging in the console windows
    // public VillaNumberAPIController(ILogger<VillaNumberAPIController> logger)
    // {
    //     _logger = logger;
    // }

    // private readonly ApplicationDbContext _db;
    protected APIResponse _response;

    public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IMapper mapper, IVillaRepository dbVilla)
    {
        _dbVillaNumber = dbVillaNumber;
        _mapper = mapper;
        _response = new APIResponse();
        _dbVilla = dbVilla;
    }

    [HttpGet("GetString")]
    // [MapToApiVersion("2.0")]
    public IEnumerable<string> Get()
    {
        return new string[] { "GetString in V2 version", "This is V2" };
    }
}