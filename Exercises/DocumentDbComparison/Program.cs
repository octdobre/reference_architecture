using DocumentDbComparison;
using DocumentDbComparison.Couchbase;
using DocumentDbComparison.Couchbase.Bug;
using DocumentDbComparison.CouchDb;
using DocumentDbComparison.CouchDb.Bug;
using DocumentDbComparison.MongoDb;
using DocumentDbComparison.MongoDb.Bug;
using DocumentDbComparison.MSSQL;
using DocumentDbComparison.MSSQL.Bug;
using DocumentDbComparison.RavenDb;
using DocumentDbComparison.RavenDb.Bug;

var builder = WebApplication.CreateBuilder(args);

var features = builder.Configuration.GetSection(nameof(Features)).Get<Features>() ?? new Features(false, false, false, false, false, false);

if (features.Mssql)
{
    BugSqlDb.SetupSqlDb(builder.Services, builder.Configuration);
}

if (features.Mongodb || features.FerretDb)
{
    builder.Services.AddSingleton<BugMongoDbRepo>(
        sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var conn = string.Empty;

            if (features.Mongodb)
            {
                conn = config.GetConnectionString("mongodb");
            }
            else if (features.FerretDb)
            {
                conn = config.GetConnectionString("ferretdb");
            }

            return new BugMongoDbRepo(conn);
        });
}

if (features.RavenDb)
{
    builder.Services.AddSingleton<BugRavenDbRepo>();
}

if (features.CouchDb)
{
    builder.Services.AddSingleton<BugCouchDbRepo>();
}
if (features.CouchBase)
{
    builder.Services.AddSingleton<BugCouchbaseDbRepo>();
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (features.Mssql)
{
    app.MapGroup("/mssql/bug").PostBugWithSql().GetByIdBugWithSql()
        .GetPaginatedBugWithSql().UpdateBugWithSql().DeleteBugWithSql();
    BugSqlDb.EnsureDatabaseAndTableCreated(app.Configuration);
}

if (features.Mongodb || features.FerretDb)
{
    app.MapGroup("/mongodb/bug").PostBugWithMongoDb().GetBugByIdWithMongoDb()
        .GetPaginatedBugsWithMongoDb().UpdateBugWithMongoDb().DeleteBugWithMongoDb();
}

if (features.RavenDb)
{
    app.MapGroup("/ravendb/bug").PostBugWithRavenDb().GetBugByIdWithRavenDb()
        .GetPaginatedBugsWithRavenDb().UpdateBugWithRavenDb()
        .DeleteBugWithRavenDb();
}

if (features.CouchDb)
{
    app.MapGroup("/couchdb/bug").PostBugWithCouchDb().GetBugByIdWithCouchDb()
        .GetPaginatedBugsWithCouchDb().UpdateBugWithCouchDb().DeleteBugWithCouchDb();
}

if (features.CouchBase)
{
    app.MapGroup("/couchdb/bug").PostBugWithCouchbaseDb().GetBugByIdWithCouchbaseDb()
        .GetPaginatedBugsWithCouchbaseDb().UpdateBugWithCouchbaseDb().DeleteBugWithCouchbaseDb();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

namespace DocumentDbComparison
{
    internal record Features(bool Mssql, bool Mongodb, bool FerretDb, bool RavenDb, bool CouchDb, bool CouchBase);
}