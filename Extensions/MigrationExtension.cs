using Microsoft.EntityFrameworkCore;
namespace DocumentManagementSystem.Extensions;
using DocumentManagementSystem.DAL;

public static class MigrationExtension
{
    public static void MigrationApplication(IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        using AppDbContext context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
    }
}