using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.KeyValue;
using Couchbase.Management.Buckets;
using Couchbase.Management.Collections;
using Couchbase.Management.Query;

namespace DocumentDbComparison.Couchbase;

public class BugCouchbaseDbRepo
{
    public record BugDocument(
        Guid Id,
        string Title,
        string Description,
        DateTime ReportTime);

    private IBucket _bucket;
    public ICouchbaseCollection BugCollection { get; private set; }
    public ICluster BugCluster { get; }

    public string BucketName => "bugsStore";
    public string ScopeName => "bugsScope";
    public string CollectionName => "bugsCollection";

    public BugCouchbaseDbRepo(IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        var conn = configuration.GetConnectionString("couchbase");

        //extract here uname and pswd from conn
        var uri = new Uri(conn);
        if (uri.UserInfo.Contains(":"))
        {
            var up = uri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            //create new connection string without user info
            uri = new UriBuilder(uri)
            {
                UserName = "",
                Password = ""
            }.Uri;

            var opts = new ClusterOptions().WithCredentials(up[0], up[1]).WithLogging(loggerFactory);
            opts.HttpIgnoreRemoteCertificateMismatch = true;
            opts.KvIgnoreRemoteCertificateNameMismatch = true;

            //trim connection string otherwise couchbase will not connect
            var cluster = Cluster.ConnectAsync(uri.ToString().TrimEnd('/').TrimEnd('/'), opts).Result;

            BugCluster = cluster;
            // checks if databases and collections exist
            VerifyAndSetupDatabase(cluster);
        }
    }

    private void VerifyAndSetupDatabase(ICluster cluster)
    {
        //this section is not perfectly handled and may throw errors
        //due to the eventual consistent nature of couchbase



        // Check if the bucket exists; if not, create it
        var bucketExists = cluster.Buckets.GetAllBucketsAsync().Result.ContainsKey(BucketName);
        if (!bucketExists)
        {
            var bucketSettings = new BucketSettings
            {
                Name = BucketName,
                RamQuotaMB = 100,
                BucketType = BucketType.Couchbase
            };
            cluster.Buckets.CreateBucketAsync(bucketSettings).Wait();
        }

        _bucket = cluster.BucketAsync(BucketName).Result;

        ICouchbaseCollectionManager collectionMgr = _bucket.Collections;

        // Check if the scope exists; if not, create it
        var scopeExists = _bucket.Collections.GetAllScopesAsync().Result.FirstOrDefault(s => s.Name == ScopeName);
        if (scopeExists is not { })
        {
            _bucket.Collections.CreateScopeAsync(ScopeName).Wait();
        }

        var scope = _bucket.Scope(ScopeName);

        // Check if the collection exists; if not, create it
        var collectionSpec = new CollectionSpec(ScopeName, CollectionName);

        try
        {
            collectionMgr.CreateCollectionAsync(collectionSpec);
        }
        catch (CollectionExistsException)
        {
            Console.WriteLine("Collection already exists");
        }
        catch (ScopeNotFoundException)
        {
            Console.WriteLine("The specified parent scope doesn't exist");
        }

        BugCollection = scope.Collection(CollectionName);

        //create primary index on bugs
        try
        {
            cluster.QueryIndexes.CreatePrimaryIndexAsync($"`{BucketName}`.`{ScopeName}`.`{CollectionName}`", options =>
            {
                options.IgnoreIfExists(true);
                options.IndexName("bugs-primary");
            }).Wait();
        }
        catch (InternalServerFailureException)
        {
            Console.WriteLine("Index already exists");
        }

        try
        {
            cluster.QueryIndexes.CreateIndexAsync($"`{BucketName}`.`{ScopeName}`.`{CollectionName}`", "bugs-title-secondary", new[] { "title" }, options => options.IgnoreIfExists(true)).Wait();
        }
        catch (InternalServerFailureException)
        {
            Console.WriteLine("Index already exists");
        }
    }
}