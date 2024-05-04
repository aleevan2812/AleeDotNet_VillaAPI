using Alee_VillaAPI.Models.Dto;

namespace Alee_VillaAPI.Data;

public static class VillaStore
{
    public static List<VillaDTO> villaList = new List<VillaDTO>()
    {
        new VillaDTO { Id = 1, Name = "Villa 1", Occupancy = 1, Sqft = 100},
        new VillaDTO { Id = 2, Name = "Villa 2", Occupancy = 2, Sqft = 200 }
    };
}