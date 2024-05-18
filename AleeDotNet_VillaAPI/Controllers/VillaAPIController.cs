using Alee_VillaAPI.Data;
using Alee_VillaAPI.Models;
using Alee_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    private readonly ApplicationDbContext _db;

    public VillaAPIController(ApplicationDbContext db)
    {
        _db = db;
    }


    [HttpGet] // fix err: Failed to load API definition
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<VillaDTO>> GetVillas()
    {
        return Ok(_db.Villas.ToList());
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
        if (id == 0) return BadRequest();

        var villas = _db.Villas.FirstOrDefault(u => u.Id == id);

        if (villas == null)
            return NotFound();
        return Ok(villas);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<VillaDTO> CreateVilla([FromBody] VillaCreateDTO villaDTO)
    {
        // The [ApiController] attribute makes model validation errors automatically trigger an HTTP 400 response. Consequently, the following code is unnecessary
        // if (!ModelState.IsValid)
        // {
        //     return BadRequest(ModelState);
        // }

        if (_db.Villas.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
        {
            ModelState.AddModelError("CustomError", "Villa  already Exists!");
            return BadRequest(ModelState);
        }

        if (villaDTO == null)
            return BadRequest(villaDTO);
        // if (villaDTO.Id > 0)
        //     return StatusCode(StatusCodes.Status500InternalServerError);

        var model = new Villa
        {
            // Id = villaDTO.Id,
            Name = villaDTO.Name,
            Details = villaDTO.Details,
            Rate = villaDTO.Rate,
            Sqft = villaDTO.Sqft,
            Occupancy = villaDTO.Occupancy,
            ImageUrl = villaDTO.ImageUrl,
            Amenity = villaDTO.Amenity
        };

        _db.Villas.Add(model);
        _db.SaveChanges();

        return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
    }

    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    public IActionResult DeleteVilla(int id) // use IActionResult since don't need to define return TYPE
    {
        if (id == 0)
            return BadRequest();
        var villa = _db.Villas.FirstOrDefault(u => u.Id == id);
        if (villa == null)
            return NotFound();
        _db.Villas.Remove(villa);
        return NoContent();
    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateVilla(int id, [FromBody] VillaUpdateDTO villaDTO)
    {
        if (villaDTO == null || id != villaDTO.Id)
            return BadRequest();

        // var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
        // villa.Name = villaDTO.Name;
        // villa.Occupancy = villaDTO.Occupancy;
        // villa.Sqft = villaDTO.Sqft;

        var villa = new Villa
        {
            Id = villaDTO.Id,
            Name = villaDTO.Name,
            Details = villaDTO.Details,
            Rate = villaDTO.Rate,
            Sqft = villaDTO.Sqft,
            Occupancy = villaDTO.Occupancy,
            ImageUrl = villaDTO.ImageUrl,
            Amenity = villaDTO.Amenity
        };
        _db.Villas.Update(villa);
        _db.SaveChanges();
        return NoContent();
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
    {
        if (patchDTO == null || id == 0)
            return BadRequest();
        var villa = _db.Villas.AsNoTracking().FirstOrDefault(u => u.Id == id);

        var villaDTO = new VillaUpdateDTO()
        {
            Id = villa.Id, Name = villa.Name,
            Details = villa.Details,
            Rate = villa.Rate, Sqft = villa.Sqft,
            Occupancy = villa.Occupancy,
            ImageUrl = villa.ImageUrl,
            Amenity = villa.Amenity
        };

        if (villa == null)
            return BadRequest();
        patchDTO.ApplyTo(villaDTO, ModelState);

        var model = new Villa
        {
            Id = villaDTO.Id,
            Name = villaDTO.Name,
            Details = villaDTO.Details,
            Rate = villaDTO.Rate,
            Sqft = villaDTO.Sqft,
            Occupancy = villaDTO.Occupancy,
            ImageUrl = villaDTO.ImageUrl,
            Amenity = villaDTO.Amenity
        };

        _db.Villas.Update(model);
        _db.SaveChanges();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        return NoContent();
    }
}