using System.Linq.Expressions;
using Alee_VillaAPI.Models;

namespace AleeDotNet_VillaAPI.Repository.IRepository;

public interface IVillaRepository : IRepository<Villa>
{
    Task<Villa> UpdateAsync(Villa entity);
}