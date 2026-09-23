namespace DocumentManagementSystem;
using DAL;
using Microsoft.EntityFrameworkCore;
using Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        
        var app = builder.Build();
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocumentManagementSystem.API v1");
        });
        MigrationExtension.MigrationApplication(app);
        app.MapControllers();
        app.Run();
    }
}