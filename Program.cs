namespace DocumentManagementSystem;
using DocumentManagementSystem.DAL;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.EntityFrameworkCore;
using DocumentManagementSystem.Endpoints;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        
        
        var app = builder.Build();
        app.UseRouting();
        
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "DocumentManagementSystem.API v1");
        });
        app.MapDocumentEndpoints();
        app.MapControllers();
        app.Run();
    }
}