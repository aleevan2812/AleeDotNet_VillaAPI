using System.Linq.Expressions;
using Alee_VillaAPI.Models;

namespace Villa_API.Repository.IRepository;

public interface IVillaNumberRepository : IRepository<VillaNumber>
{
    Task<VillaNumber> UpdateAsync(VillaNumber entity);
}