using NoSqlComparison;
using NoSqlComparison.MongoDb;
using NoSqlComparison.MongoDb.Bug;
using NoSqlComparison.MSSQL;
using NoSqlComparison.MSSQL.Bug;
using NoSqlComparison.RavenDb;
using NoSqlComparison.RavenDb.Bug;

var builder = WebApplication.CreateBuilder(args);

var features = builder.Configuration.GetSection(nameof(Features)).Get<Features>() ?? new Features(false, false, false);

if (features.Mssql)
{
    BugSqlDb.SetupSqlDb(builder.Services, builder.Configuration);
}

if (features.Mongodb)
{
    builder.Services.AddSingleton<BugMongoDbRepo>();
}

if (features.RavenDb)
{
    builder.Services.AddSingleton<BugRavenDbRepo>();
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

if (features.Mssql)
{
    app.MapGroup("/sql/bug")
        .PostBugWithSql()
        .GetByIdBugWithSql()
        .GetPaginatedBugWithSql()
        .UpdateBugWithSql()
        .DeleteBugWithSql();

    BugSqlDb.EnsureDatabaseAndTableCreated(app.Configuration);
}

if (features.Mongodb)
{
    app.MapGroup("/mongodb/bug")
        .PostBugWithMongoDb()
        .GetBugByIdWithMongoDb()
        .GetPaginatedBugsWithMongoDb()
        .UpdateBugWithMongoDb()
        .DeleteBugWithMongoDb();
}

if (features.RavenDb)
{
    app.MapGroup("/ravendb/bug")
        .PostBugWithRavenDb()
        .GetBugByIdWithRavenDb()
        .GetPaginatedBugsWithRavenDb()
        .UpdateBugWithRavenDb()
        .DeleteBugWithRavenDb();
}

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

namespace NoSqlComparison
{
    internal record Features(bool Mssql, bool Mongodb, bool RavenDb);
}