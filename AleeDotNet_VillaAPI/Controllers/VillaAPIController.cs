using Alee_VillaAPI.Data;
using Alee_VillaAPI.Models;
using Alee_VillaAPI.Models.Dto;
using AutoMapper;
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
	private readonly IMapper _mapper;

	public VillaAPIController(ApplicationDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	[HttpGet] // fix err: Failed to load API definition
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas()
	{
		var villas = await _db.Villas.ToListAsync();
		return Ok(_mapper.Map<IEnumerable<VillaDTO>>(villas));
	}

	[HttpGet("{id:int}", Name = "GetVilla")] // if dont define HTTP Verb, it defaults to "HttpGet"
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<VillaDTO>> GetVilla(int id)
	{
		if (id == 0) return BadRequest();

		var villas = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);

		if (villas == null)
			return NotFound();
		return Ok(villas);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
	{
		// The [ApiController] attribute makes model validation errors automatically trigger an HTTP 400 response. Consequently, the following code is unnecessary
		// if (!ModelState.IsValid)
		// {
		//     return BadRequest(ModelState);
		// }

		if (await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
		{
			ModelState.AddModelError("CustomError", "Villa  already Exists!");
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
		var model = _mapper.Map<Villa>(createDTO);

		await _db.Villas.AddAsync(model);
		await _db.SaveChangesAsync();

		return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
	}

	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[HttpDelete("{id:int}", Name = "DeleteVilla")]
	public async Task<IActionResult> DeleteVilla(int id) // use IActionResult since don't need to define return TYPE
	{
		if (id == 0)
			return BadRequest();
		var villa = await _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
		if (villa == null)
			return NotFound();
		_db.Villas.Remove(villa);
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPut("{id:int}", Name = "UpdateVilla")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
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

		_db.Villas.Update(villa);
		await _db.SaveChangesAsync();
		return NoContent();
	}

	[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
	{
		if (patchDTO == null || id == 0)
			return BadRequest();
		var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);

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

		_db.Villas.Update(model);
		await _db.SaveChangesAsync();

		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		return NoContent();
	}
}