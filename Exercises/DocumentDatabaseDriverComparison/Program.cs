using DocumentDatabaseDriverComparison.MongoDb;
using DocumentDatabaseDriverComparison.MongoDb.Bug;
using DocumentDatabaseDriverComparison.MSSQL;
using DocumentDatabaseDriverComparison.MSSQL.Bug;

var builder = WebApplication.CreateBuilder(args);

var features = builder.Configuration.GetSection(nameof(Features)).Get<Features>() ?? new Features(false, false);

if (features.Mssql)
{
    BugSqlDb.SetupSqlDb(builder.Services, builder.Configuration);
}
else if (features.Mongodb)
{
    builder.Services.AddSingleton<BugDocumentDb>();
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

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

internal record Features(bool Mssql, bool Mongodb);