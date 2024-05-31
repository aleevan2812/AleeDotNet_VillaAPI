using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;

namespace Alee_VillaNumberAPI.Controllers;

// [Route("api/[controller]")] can use this
[Route("api/VillaNumberAPI")] // fix err: Action 'Alee_VillaNumberAPI.Controllers.VillaNumberAPIController.GetVillaNumbers (Alee_VillaNumberAPI)' does not have an attribute route. Action methods on controllers annotated with ApiControllerAttribute must be attribute routed.
[ApiController]
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

    [HttpGet] // fix err: Failed to load API definition
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<APIResponse>> GetVillaNumbers()
    {
        try
        {
            IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync(includeProperties: "Villa");
            _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string> { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [HttpGet("{id:int}", Name = "GetVillaNumber")] // if dont define HTTP Verb, it defaults to "HttpGet"
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var VillaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
            if (VillaNumber == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            _response.Result = _mapper.Map<VillaNumberDTO>(VillaNumber);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string> { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
    {
        // The [ApiController] attribute makes model validation errors automatically trigger an HTTP 400 response. Consequently, the following code is unnecessary
        // if (!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }
        try
        {
            if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
            {
                ModelState.AddModelError("ErrorMessages", "VillaNumber  already Exists!");
                return BadRequest(ModelState);
            }

            if (await _dbVilla.GetAsync(u => u.Id == createDTO.VillaID) == null)
            {
                ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
                return BadRequest(ModelState);
            }

            if (createDTO == null)
                return BadRequest(createDTO);
            // if (VillaNumberDTO.Id > 0)
            //     return StatusCode(StatusCodes.Status500InternalServerError);

            // var model = new VillaNumber
            // {
            //     // Id = VillaNumberDTO.Id,
            //     Name = createDTO.Name,
            //     Details = createDTO.Details,
            //     Rate = createDTO.Rate,
            //     Sqft = createDTO.Sqft,
            //     Occupancy = createDTO.Occupancy,
            //     ImageUrl = createDTO.ImageUrl,
            //     Amenity = createDTO.Amenity
            // };
            VillaNumber model = _mapper.Map<VillaNumber>(createDTO);

            await _dbVillaNumber.CreateAsync(model);
            await _dbVillaNumber.SaveAsync();
            _response.Result = _mapper.Map<VillaNumberDTO>(model);
            _response.StatusCode = HttpStatusCode.Created;
            return CreatedAtRoute("GetVillaNumber", new { id = model.VillaNo }, _response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string> { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
        // return CreatedAtRoute("GetVillaNumber", new { id = model.Id }, model);
    }

    [Authorize(Roles = "admin")]
    [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>>
        DeleteVillaNumber(int id) // use IActionResult since don't need to define return TYPE
    {
        // if (id == 0)
        //     return BadRequest();
        // var VillaNumber = await _dbVillaNumber.GetAsync(u => u.Id == id);
        // if (VillaNumber == null)
        //     return NotFound();
        // await _dbVillaNumber.RemoveAsync(VillaNumber);
        // await _dbVillaNumber.SaveAsync();
        // return NoContent();

        try
        {
            if (id == 0) return BadRequest();

            var VillaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
            if (VillaNumber == null) return NotFound();

            await _dbVillaNumber.RemoveAsync(VillaNumber);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string> { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [Authorize(Roles = "admin")]
    [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
    {
        try
        {
            if (updateDTO == null || id != updateDTO.VillaNo)
                return BadRequest();

            if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaID) == null)
            {
                ModelState.AddModelError("ErrorMessages", "Villa ID is Invalid!");
                return BadRequest(ModelState);
            }

            // var VillaNumber = VillaNumberStore.VillaNumberList.FirstOrDefault(u => u.Id == id);
            // VillaNumber.Name = VillaNumberDTO.Name;
            // VillaNumber.Occupancy = VillaNumberDTO.Occupancy;
            // VillaNumber.Sqft = VillaNumberDTO.Sqft;

            // var VillaNumber = new VillaNumber
            // {
            //     Id = updateDTO.Id,
            //     Name = updateDTO.Name,
            //     Details = updateDTO.Details,
            //     Rate = updateDTO.Rate,
            //     Sqft = updateDTO.Sqft,
            //     Occupancy = updateDTO.Occupancy,
            //     ImageUrl = updateDTO.ImageUrl,
            //     Amenity = updateDTO.Amenity
            // };
            var VillaNumber = _mapper.Map<VillaNumber>(updateDTO);

            await _dbVillaNumber.UpdateAsync(VillaNumber);
            await _dbVillaNumber.SaveAsync();
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
            // return NoContent();
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string> { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }
}