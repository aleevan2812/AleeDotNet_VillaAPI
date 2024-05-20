using System.ComponentModel.DataAnnotations;

namespace AleeDotNet_VillaWeb.Models.Dto;

// DTOs provide a wrapper between the entity or the database model and what is being exposed from the API
public class VillaUpdateDTO
{
    [Required] public int Id { get; set; }

    [Required] [MaxLength(30)] public string Name { get; set; }
    [Required] public string Details { get; set; }
    [Required] public int Occupancy { get; set; }
    [Required] public int Sqft { get; set; } // square feet^2
    [Required] public double Rate { get; set; }
    [Required] public string ImageUrl { get; set; }
    public string Amenity { get; set; }
}