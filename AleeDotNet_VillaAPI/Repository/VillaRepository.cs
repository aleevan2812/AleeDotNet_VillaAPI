using System.Linq.Expressions;
using Alee_VillaAPI.Data;
using Alee_VillaAPI.Models;
using AleeDotNet_VillaAPI.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

namespace AleeDotNet_VillaAPI.Repository;

public class VillaRepository : Repository<Villa>, IVillaRepository
{
    private readonly ApplicationDbContext _db;
    public VillaRepository(ApplicationDbContext db): base(db)
    {
        _db = db;
    }


    public async Task<Villa> UpdateAsync(Villa entity)
    {
        entity.UpdatedDate = DateTime.Now;
        _db.Villas.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
}