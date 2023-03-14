using Company_API.Data;
using Company_API.Models;
using Company_API.Repository.IRepository;

namespace Company_API.Repository
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        private readonly DatabaseContext _db;
        public CompanyRepository(DatabaseContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Company> UpdateAsync(Company entity)
        {
            _db.Companies.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}