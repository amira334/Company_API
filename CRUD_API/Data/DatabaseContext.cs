using Company_API.Models;
using Company_API.Models.DTO;
using Microsoft.EntityFrameworkCore;

namespace Company_API.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); //
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    Id = 1,
                    Name = "Super Admin",
                    IsActive = true,
                },
              new UserRole
              {
                  Id = 2,
                  Name = "Admin",
                  IsActive = true,
              },
              new UserRole
              {
                  Id = 3,
                  Name = "User",
                  IsActive = true,
              }
              
              );
        }
        }
}
