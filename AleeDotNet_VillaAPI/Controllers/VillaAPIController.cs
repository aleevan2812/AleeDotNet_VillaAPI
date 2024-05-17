using Alee_VillaAPI.Data;
using Alee_VillaAPI.Models;
using Alee_VillaAPI.Models.Dto;
using AleeDotNet_VillaAPI.Logging;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;


namespace Alee_VillaAPI.Controllers;

// [Route("api/[controller]")] can use this
[Route("api/VillaAPI")] // fix err: Action 'Alee_VillaAPI.Controllers.VillaAPIController.GetVillas (Alee_VillaAPI)' does not have an attribute route. Action methods on controllers annotated with ApiControllerAttribute must be attribute routed.
[ApiController]
public class VillaAPIController : ControllerBase // dont need Controller Class
{
    // private readonly ILogger<VillaAPIController> _logger;
    // // logger for logging in the console windows
    // public VillaAPIController(ILogger<VillaAPIController> logger)
    // {
    //     _logger = logger;
    // }

    private readonly ILogging _logger;

    public VillaAPIController(ILogging logger)
    {
        _logger = logger;
    }
    
    
    [HttpGet] // fix err: Failed to load API definition
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<VillaDTO>> GetVillas()
    {
        _logger.Log(("Getting all villas"), "");
        return Ok(VillaStore.villaList);
    }

    [HttpGet("{id:int}", Name = "GetVilla")] // if dont define HTTP Verb, it defaults to "HttpGet"
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // [ProducesResponseType(200, Type = typeof(VillaDTO))] // for Ok
    // [ProducesResponseType(404)] // NotFound
    // [ProducesResponseType(400)] // BadRequest
    public ActionResult<VillaDTO> GetVilla(int id)
    {
        if (id == 0)
        {
            _logger.Log("Get Villa Error with Id: = " + id, "error");
            return BadRequest();
        }
        var villas = VillaStore.villaList.FirstOrDefault(u => u.Id == id);

        if (villas == null)
            return NotFound();
        return Ok(villas);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
    {
        // The [ApiController] attribute makes model validation errors automatically trigger an HTTP 400 response. Consequently, the following code is unnecessary
        // if (!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }

        if (VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
        {
            ModelState.AddModelError("CustomError", "Villa  already Exists!");
            return BadRequest(ModelState);
        }

        if (villaDTO == null)
            return BadRequest(villaDTO);
        if (villaDTO.Id > 0)
            return StatusCode(StatusCodes.Status500InternalServerError);

        villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
        VillaStore.villaList.Add(villaDTO);

        return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    public IActionResult DeleteVilla(int id) // use IActionResult since don't need to define return TYPE
    {
        if (id == 0)
            return BadRequest();
        var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
        if (villa == null)
            return NotFound();
        VillaStore.villaList.Remove(villa);
        return NoContent();
    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult DeleteVilla(int id, [FromBody] VillaDTO villaDTO)
    {
        if (villaDTO == null || id != villaDTO.Id)
            return BadRequest();

        var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
        villa.Name = villaDTO.Name;
        villa.Occupancy = villaDTO.Occupancy;
        villa.Sqft = villaDTO.Sqft;

        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
    {
        if (patchDTO == null || id == 0)
            return BadRequest();
        var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
        if (villa == null)
            return BadRequest();
        patchDTO.ApplyTo(villa, ModelState);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return NoContent();
    }
}