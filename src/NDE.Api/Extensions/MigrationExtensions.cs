using Microsoft.EntityFrameworkCore;

using NDE.Data.Context;

namespace NDE.Api.Extensions;

public static class MigrationExtensions
{
  public static async Task ApplyMigrationsAsync(this IHost app)
  {
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    try
    {
      var dbContext = serviceProvider.GetRequiredService<AppDbContext>();
      await dbContext.Database.MigrateAsync();
    }
    catch (InvalidOperationException ex)
    {
      Console.WriteLine($"Erro ao aplicar migrations: {ex.Message}");
    }
  }
}

