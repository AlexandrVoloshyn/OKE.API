using Microsoft.EntityFrameworkCore;
using OKE.Database;

namespace OKE.API.Extensions;

public static class StartupExtensions
{
    public static async Task MigrateAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<Context>();

            if (context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            {
                await context.Database.GetDbConnection().OpenAsync();
            }

            await context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while migrating the database.");
        }
    }
}
