using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace NoSqlComparison.MSSQL;

public class BugSqlDb : DbContext
{
    public record BugRecord(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public BugSqlDb(DbContextOptions<BugSqlDb> options) : base(options) { }

    public DbSet<BugRecord> Bugs => Set<BugRecord>();

    public static void SetupSqlDb(IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("mssql");
        serviceCollection.AddDbContext<BugSqlDb>(opt => opt.UseSqlServer(conn));
    }

    public static void EnsureDatabaseAndTableCreated(IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("mssql");

        var databaseName = new SqlConnectionStringBuilder(conn).InitialCatalog;

        // Change the initial catalog to 'master' to create the database
        var masterConnectionStringBuilder = new SqlConnectionStringBuilder(conn)
        {
            InitialCatalog = "master"
        };

        using (var connection = new SqlConnection(masterConnectionStringBuilder.ToString()))
        {
            connection.Open();

            var createDatabaseCommandText = @$"
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Bug')
BEGIN
    CREATE DATABASE {databaseName};
END
";
            using (var command = new SqlCommand(createDatabaseCommandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Change the initial catalog to 'BugTracker' to create the table
        var bugTrackerConnectionStringBuilder = new SqlConnectionStringBuilder(conn)
        {
            InitialCatalog = databaseName
        };

        // Create the table if it doesn't exist
        using (var connection = new SqlConnection(bugTrackerConnectionStringBuilder.ToString()))
        {
            connection.Open();

            var createTableCommandText = @"
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bugs')
BEGIN
    CREATE TABLE Bugs (
        Id UNIQUEIDENTIFIER PRIMARY KEY,
        Title NVARCHAR(255) NOT NULL,
        Description NVARCHAR(MAX),
        ReportTime DATETIME2 NOT NULL
    );
END
";
            using (var command = new SqlCommand(createTableCommandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    
}