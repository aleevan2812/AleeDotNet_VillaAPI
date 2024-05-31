using Microsoft.AspNetCore.Identity;

namespace Villa_API.Models;

public class ApplicationUser : IdentityUser
{
    public string Name { get; set; }
}