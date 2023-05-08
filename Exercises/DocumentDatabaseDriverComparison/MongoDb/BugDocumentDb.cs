using MongoDB.Bson;
using MongoDB.Driver;

namespace DocumentDatabaseDriverComparison.MongoDb;

public class BugDocumentDb
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public IMongoCollection<BugDocument> BugCollection { get; private set; }

    public BugDocumentDb(IConfiguration configuration)
    {
        //temp until the framework switches completely
        MongoDefaults.GuidRepresentation = GuidRepresentation.Standard;

        var conn = configuration.GetConnectionString("mongodb");
        var client = new MongoClient(conn);

        // checks if databases and collections exist
        VerifyAndSetupDatabase(client);
    }

    private void VerifyAndSetupDatabase(MongoClient client)
    {
        //if db does not exist, it creates it silently
        var database = client.GetDatabase("Squash");

        //if collection does not exist, it creates it silently
        BugCollection = database.GetCollection<BugDocument>("bugs");
    }
}