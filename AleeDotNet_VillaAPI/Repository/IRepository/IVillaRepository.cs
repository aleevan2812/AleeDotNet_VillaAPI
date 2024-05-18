using System.Linq.Expressions;
using Alee_VillaAPI.Models;

namespace AleeDotNet_VillaAPI.Repository.IRepository;

public interface IVillaRepository
{
    Task<List<Villa>> GetAll(Expression<Func<Villa>> filter = null);
    Task<Villa> Get(Expression<Func<Villa>> filter = null, bool tracked = true);
    Task Create(Villa entity);
    Task Remove(Villa entity);
    Task Save();
}