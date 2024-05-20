using Alee_VillaAPI.Data;
using Alee_VillaAPI.Models;
using Villa_API.Repository;
using Villa_API.Repository.IRepository;

namespace AleeDotNet_VillaNumberAPI.Repository;

public class VillaNumberRepository : Repository<VillaNumber>, IVillaNumberRepository
{
    private readonly ApplicationDbContext _db;

    public VillaNumberRepository(ApplicationDbContext db) : base(db)
    {
        _db = db;
    }


    public async Task<VillaNumber> UpdateAsync(VillaNumber entity)
    {
        entity.UpdatedDate = DateTime.Now;
        _db.VillaNumbers.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }
}