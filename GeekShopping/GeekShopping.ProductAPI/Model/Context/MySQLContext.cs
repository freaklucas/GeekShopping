using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context
{
    public class MySQLContext : DbContext
    {
        public MySQLContext(DbContextOptions options) : base(options) { }
        public DbSet<Product> Products { get; set; }
    }
}
