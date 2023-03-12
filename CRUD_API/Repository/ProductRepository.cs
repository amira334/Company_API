using Company_API.Data;
using Company_API.Models;
using Company_API.Repository.IRepository;

namespace Company_API.Repository
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private readonly DatabaseContext _db;
        public ProductRepository(DatabaseContext db) : base(db)
        {
            _db = db;
        }
        public async Task<Product> UpdateAsync(Product entity)
        {
            _db.Products.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
        // 
    }
}
