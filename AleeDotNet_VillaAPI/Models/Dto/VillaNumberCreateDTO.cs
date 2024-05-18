using System.ComponentModel.DataAnnotations;

namespace Alee_VillaAPI.Models.Dto;

public class VillaNumberCreateDTO
{
    [Required]
    public int VillaNo { get; set; }

    public string SpecialDetails { get; set; }
}