using System.Linq.Expressions;
using Villa_API.Models;

namespace Villa_API.Repository.IRepository;

public interface IVillaNumberRepository : IRepository<VillaNumber>
{
    Task<VillaNumber> UpdateAsync(VillaNumber entity);
}