using System.Linq.Expressions;
using Alee_VillaAPI.Models;

namespace Villa_API.Repository.IRepository;

public interface IVillaRepository : IRepository<Villa>
{
    Task<Villa> UpdateAsync(Villa entity);
}