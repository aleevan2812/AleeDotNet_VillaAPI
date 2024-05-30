using System.Net;
using Villa_API.Data;
using Villa_API.Models;
using Villa_API.Models.Dto;
using Villa_API.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Villa_API.Controllers;

// [Route("api/[controller]")] can use this
[Route("api/VillaAPI")] // fix err: Action 'Villa_API.Controllers.VillaAPIController.GetVillas (Villa_API)' does not have an attribute route. Action methods on controllers annotated with ApiControllerAttribute must be attribute routed.
[ApiController]
public class VillaAPIController : ControllerBase // dont need Controller Class
{
    // private readonly ILogger<VillaAPIController> _logger;
    // // logger for logging in the console windows
    // public VillaAPIController(ILogger<VillaAPIController> logger)
    // {
    //     _logger = logger;
    // }

    // private readonly ApplicationDbContext _db;
    protected APIResponse _response;
    private readonly IVillaRepository _dbVilla;
    private readonly IMapper _mapper;

    public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
    {
        _dbVilla = dbVilla;
        _mapper = mapper;
        _response = new APIResponse();
    }

    [HttpGet] // fix err: Failed to load API definition
    [Authorize]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<APIResponse>> GetVillas()
    {
        try
        {
            IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
            _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [HttpGet("{id:int}", Name = "GetVilla")] // if dont define HTTP Verb, it defaults to "HttpGet"
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> GetVilla(int id)
    {
        try
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var villa = await _dbVilla.GetAsync(u => u.Id == id);
            if (villa == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            _response.Result = _mapper.Map<VillaDTO>(villa);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
    {
        // The [ApiController] attribute makes model validation errors automatically trigger an HTTP 400 response. Consequently, the following code is unnecessary
        // if (!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }
        try
        {
            if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("ErrorMessages", "Villa  already Exists!");
                return BadRequest(ModelState);
            }

            if (createDTO == null)
                return BadRequest(createDTO);
            // if (villaDTO.Id > 0)
            //     return StatusCode(StatusCodes.Status500InternalServerError);

            // var model = new Villa
            // {
            //     // Id = villaDTO.Id,
            //     Name = createDTO.Name,
            //     Details = createDTO.Details,
            //     Rate = createDTO.Rate,
            //     Sqft = createDTO.Sqft,
            //     Occupancy = createDTO.Occupancy,
            //     ImageUrl = createDTO.ImageUrl,
            //     Amenity = createDTO.Amenity
            // };
            Villa model = _mapper.Map<Villa>(createDTO);

            await _dbVilla.CreateAsync(model);
            await _dbVilla.SaveAsync();
            _response.Result = _mapper.Map<VillaDTO>(model);
            _response.StatusCode = HttpStatusCode.Created;
            return CreatedAtRoute("GetVilla", new { id = model.Id }, _response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
        // return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
    }

    [Authorize(Roles = "CUSTOM")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    public async Task<ActionResult<APIResponse>>
        DeleteVilla(int id) // use IActionResult since don't need to define return TYPE
    {
        // if (id == 0)
        //     return BadRequest();
        // var villa = await _dbVilla.GetAsync(u => u.Id == id);
        // if (villa == null)
        //     return NotFound();
        // await _dbVilla.RemoveAsync(villa);
        // await _dbVilla.SaveAsync();
        // return NoContent();

        try
        {
            if (id == 0) return BadRequest();

            var villa = await _dbVilla.GetAsync(u => u.Id == id);
            if (villa == null) return NotFound();

            await _dbVilla.RemoveAsync(villa);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
    {
        try
        {
            if (updateDTO == null || id != updateDTO.Id)
                return BadRequest();

            // var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            // villa.Name = villaDTO.Name;
            // villa.Occupancy = villaDTO.Occupancy;
            // villa.Sqft = villaDTO.Sqft;

            // var villa = new Villa
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
            var villa = _mapper.Map<Villa>(updateDTO);

            await _dbVilla.UpdateAsync(villa);
            await _dbVilla.SaveAsync();
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
            // return NoContent();
        }
        catch (Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages
                = new List<string>() { ex.ToString() };
        }

        return _response; // Implicit conversion of 'response' from 'APIResponse' to 'ActionResult<APIResponse>'
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
    {
        if (patchDTO == null || id == 0)
            return BadRequest();
        var villa = await _dbVilla.GetAsync(u => u.Id == id, false);

        // var villaDTO = new VillaUpdateDTO()
        // {
        //     Id = villa.Id, Name = villa.Name,
        //     Details = villa.Details,
        //     Rate = villa.Rate, Sqft = villa.Sqft,
        //     Occupancy = villa.Occupancy,
        //     ImageUrl = villa.ImageUrl,
        //     Amenity = villa.Amenity
        // };
        var villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

        if (villa == null)
            return BadRequest();
        patchDTO.ApplyTo(villaDTO, ModelState);

        // var model = new Villa
        // {
        //     Id = villaDTO.Id,
        //     Name = villaDTO.Name,
        //     Details = villaDTO.Details,
        //     Rate = villaDTO.Rate,
        //     Sqft = villaDTO.Sqft,
        //     Occupancy = villaDTO.Occupancy,
        //     ImageUrl = villaDTO.ImageUrl,
        //     Amenity = villaDTO.Amenity
        // };
        var model = _mapper.Map<Villa>(villaDTO);

        await _dbVilla.UpdateAsync(model);
        await _dbVilla.SaveAsync();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return NoContent();
    }
}