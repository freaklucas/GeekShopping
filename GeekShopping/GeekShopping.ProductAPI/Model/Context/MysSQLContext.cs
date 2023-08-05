using Microsoft.EntityFrameworkCore;

namespace GeekShopping.ProductAPI.Model.Context
{
    public class MysSQLContext : DbContext
    {
        public MysSQLContext() {}
        public MysSQLContext(DbContextOptions <MysSQLContext> options) : base(options) {}
        public DbSet<Product> Products { get; set; }
    }
}
