using Microsoft.EntityFrameworkCore;

namespace CualiVy_CC.Models;

public class CualiVyContext : DbContext
{
    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     if (!optionsBuilder.IsConfigured)
    //     {
    //         optionsBuilder.UseMySQL("server=localhost:8088;port=3306;user=root;password=;database=cualivydb");
    //     }
    // }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasKey(u => u.guid);

        // Other entity configurations...

        base.OnModelCreating(modelBuilder);
    }

    public CualiVyContext(DbContextOptions<CualiVyContext> options)
        : base(options)
    {
    }

    public DbSet<User> User { get; set; }
}