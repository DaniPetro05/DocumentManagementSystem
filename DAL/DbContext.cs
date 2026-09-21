using Microsoft.EntityFrameworkCore;
using DocumentManagementSystem.Models;
namespace DocumentManagementSystem.DAL;

public class AppDbContext : DbContext //Inheritance
{
    protected readonly IConfiguration Configuration;
    public AppDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
    }
    
    public DbSet<Document> Documents { get; set; }
}