using Microsoft.EntityFrameworkCore;

namespace Test.Common
{
    public class NorthwindDbContext : DbContext
    {
        public NorthwindDbContext()
        {

        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=ATASAGUNMONSTER\\SQLEXPRESS;Database=Northwind;User Id=sa; Password=12345; MultipleActiveResultSets=True;");
            //base.OnConfiguring(optionsBuilder);
        }
    }
}
