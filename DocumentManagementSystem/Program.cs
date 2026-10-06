namespace DocumentManagementSystem;
using DAL;
using Microsoft.EntityFrameworkCore;
using Extensions;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        //For later
        /*builder.Services.AddCors(options =>
        {
            options.AddPolicy("SecurePolicy", policy =>
            {
                policy.WithOrigins("http://localhost:8080", "http://localhost:4200")
                    .WithMethods("GET", "POST")        // Only required methods
                    .AllowAnyHeader(); // Only necessary headers
            });
        });*/
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        
        var app = builder.Build();
        app.UseCors(options => options.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
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