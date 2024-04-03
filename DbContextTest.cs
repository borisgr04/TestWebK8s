using Microsoft.EntityFrameworkCore;

namespace TestWebK8s
{
    public class DbContextTest : DbContext 
    {
        public DbSet<Category> Category { get;set; }

        public DbContextTest(DbContextOptions<DbContextTest> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema(schema: "Admin");

        }
    }
}
