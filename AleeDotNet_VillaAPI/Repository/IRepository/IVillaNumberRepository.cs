using System.Linq.Expressions;
using Alee_VillaAPI.Models;

namespace AleeDotNet_VillaAPI.Repository.IRepository;

public interface IVillaNumberRepository : IRepository<VillaNumber>
{
    Task<VillaNumber> UpdateAsync(VillaNumber entity);
}