using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Squash.Api.Infrastructure;

public class BugDocumentDb
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    public IMongoCollection<BugDocument> BugCollection { get; }

    public BugDocumentDb(IConfiguration configuration)
    {
        //Registers GuidSerialization so that the field appears as a guid value in the document in mongodb instead of a binary data field
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        var conn = configuration.GetConnectionString("mongodb");
        var client = new MongoClient(conn);
        var database = client.GetDatabase("Squash");
        BugCollection = database.GetCollection<BugDocument>("bugs");
    }
}