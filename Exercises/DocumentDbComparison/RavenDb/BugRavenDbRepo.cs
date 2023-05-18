using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace DocumentDbComparison.RavenDb;

public class BugRavenDbRepo
{
    public record BugDocument(
        string Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public IDocumentStore Store { get; private set; }

    public BugRavenDbRepo(IConfiguration configuration)
    {
        var databaseName = "BugTracker";
        var conn = configuration.GetConnectionString("ravendb");

        Store = new DocumentStore
        {
            Urls = new[] { conn },
            Database = databaseName
        };

        Store.Initialize();

        VerifyAndSetupDatabase(databaseName).Wait();
    }

    private async Task VerifyAndSetupDatabase(string databaseName)
    {
        var operation = new GetDatabaseNamesOperation(0, 25);
        string[] databaseNames = Store.Maintenance.Server.Send(operation);

        //creates database if it doesn't exist
        if (!databaseNames.Contains(databaseName))
        {
                var createDatabaseOperation = new CreateDatabaseOperation(new DatabaseRecord(databaseName));
                await Store.Maintenance.Server.SendAsync(createDatabaseOperation);
                Console.WriteLine($"Database '{databaseName}' created.");
        }
    }
}