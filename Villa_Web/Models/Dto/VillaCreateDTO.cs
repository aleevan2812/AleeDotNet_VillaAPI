using System.ComponentModel.DataAnnotations;

namespace AleeDotNet_VillaWeb.Models.Dto;

// DTOs provide a wrapper between the entity or the database model and what is being exposed from the API
public class VillaCreateDTO
{
    [Required] [MaxLength(30)] public string Name { get; set; }
    public string Details { get; set; }
    public int Occupancy { get; set; }
    public int Sqft { get; set; } // square feet^2
    [Required] public double Rate { get; set; }
    public string ImageUrl { get; set; }
    public string Amenity { get; set; }
}