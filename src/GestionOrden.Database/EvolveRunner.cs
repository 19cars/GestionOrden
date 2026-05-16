using CampaignCatalog.Database;
using EvolveDb;
using EvolveDb.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Reflection;

namespace GestionOrden.Database;

public class EvolveRunner
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EvolveRunner> _logger;

    public EvolveRunner(IConfiguration configuration, ILogger<EvolveRunner> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }
    private string GetProjectDirectory(string projectFolderName)
    {
        // Start from assembly location
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();

        // 1) Walk up looking for a directory with the given project folder name
        var dirInfo = new DirectoryInfo(assemblyDir);
        while (dirInfo != null)
        {
            if (string.Equals(dirInfo.Name, projectFolderName, StringComparison.OrdinalIgnoreCase))
            {
                return dirInfo.FullName;
            }

            // If solution root detected, try to find the project folder inside it
            if (dirInfo.GetFiles("*.sln").Any() || dirInfo.GetDirectories("src").Any() || dirInfo.GetDirectories(".git").Any())
            {
                var candidate = Path.Combine(dirInfo.FullName, projectFolderName);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                // Try to find the csproj by name anywhere under the solution root
                try
                {
                    var matches = Directory.EnumerateFiles(dirInfo.FullName, projectFolderName + ".csproj", SearchOption.AllDirectories);
                    var first = matches.FirstOrDefault();
                    if (!string.IsNullOrEmpty(first))
                    {
                        return Path.GetDirectoryName(first)!;
                    }
                }
                catch
                {
                    // ignore IO errors and continue walking up
                }
            }

            dirInfo = dirInfo.Parent;
        }

        // 2) Fallback: try relative path a few levels up (common when running from bin)
        try
        {
            var fallback = Path.GetFullPath(Path.Combine(assemblyDir, "..", "..", "..", projectFolderName));
            if (Directory.Exists(fallback)) return fallback;
        }
        catch { }

        // Final fallback: return assembly directory
        return assemblyDir;
    }
    private TransactionKind GetTransactionKind(string strategyText)
    {
        return strategyText switch
        {
            "each" => TransactionKind.CommitEach,
            "rollback" => TransactionKind.RollbackAll,
            _ => TransactionKind.CommitAll
        };
    }
    private string GetApplicationRootDirectory()
    {
        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Directory.GetCurrentDirectory();
        var dirInfo = new DirectoryInfo(assemblyDir);

        while (dirInfo != null)
        {
            // If we find a solution file, a src folder or a .git folder we assume this is the repository/project root
            if (dirInfo.GetFiles("*.sln").Any() || dirInfo.GetDirectories("src").Any() || dirInfo.GetDirectories(".git").Any())
            {
                return dirInfo.FullName;
            }

            dirInfo = dirInfo.Parent;
        }

        // Fallback: go up three levels from the assembly location (common when running from bin/Debug/netX)
        try
        {
            return Path.GetFullPath(Path.Combine(assemblyDir, "..", ".."));
        }
        catch
        {
            return assemblyDir;
        }
    }
    public void RunEvolve()
    {
        //var connectionString = configuration.GetConnectionString("Default");
        var connectionString = _configuration[MigrationConstantes.ConnectionStringKey];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.WriteLine("No se encontró una connection string 'Default' para ejecutar migraciones Evolve.");
            return;
        }

        try
        {
            var dir = GetProjectDirectory("GestionOrden.Database");
            /*var migrationsFolder = Path.Combine(dir, "..", "..", "..", "src", "GestionOrden.Database", "Migrations");
            migrationsFolder = Path.GetFullPath(migrationsFolder);

            using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
            var evolve = new Evolve(conn, msg => Console.WriteLine(msg))
            {
                Locations = new[] { migrationsFolder },
                IsEraseDisabled = true
            };
            evolve.Migrate();*/
            foreach (var source in MigrationConstantes.Sources)
            {
                var sourceDirectory = Path.Combine(dir, source);

                _logger.LogInformation("Procesando migraciones desde el directorio: {sourceDirectory}", sourceDirectory);

                var evolve = new Evolve(new NpgsqlConnection(connectionString))
                {
                    Locations = new string[1] { sourceDirectory },
                    IsEraseDisabled = true,
                    SqlMigrationPrefix = MigrationConstantes.ScriptPrefix,
                    SqlRepeatableMigrationPrefix = MigrationConstantes.RepeatableScriptPrefix,
                    CommandTimeout = 60,
                    TransactionMode = GetTransactionKind(MigrationConstantes.Strategy)
                };

                foreach (var command in MigrationConstantes.Commands)
                {
                    _logger.LogInformation("Ejecutando comando: {command}", command);

                    switch (command)
                    {
                        case "migrate":
                            evolve.Migrate();
                            _logger.LogInformation("Migración completada exitosamente.");
                            break;
                        case "repair":
                            evolve.Repair();
                            _logger.LogInformation("Reparación completada exitosamente.");
                            break;
                        case "info":
                            evolve.Info();
                            _logger.LogInformation("Información completada exitosamente.");
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Evolve migración falló: {ex.Message}");
            throw;
        }
    }
}
