using Squash.Api.Features.Bug;
using Squash.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BugInMemoryDb>(opt => opt.UseInMemoryDatabase("BugDb"));
builder.Services.AddSingleton<BugDocumentDb>();

// Manual ssl certificate
//builder.WebHost.ConfigureKestrel(serverOptions =>
//{
//    serverOptions.ConfigureHttpsDefaults(listenOptions =>
//    {
//        listenOptions.ServerCertificate = new X509Certificate2("Path to your .pfx file", "your password");
//    });
//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHttpLogging();
}

/*
 *Endpoint group mapping helps reduce repetition of a collection path
 * Specific filters can be added only to this endpoint group.
 */
var bugGroup = app.MapGroup("/bug");

bugGroup.AddEndpointFilter((context, next) =>
{
    var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(Program).FullName);
    logger.LogInformation("Started action on a bug!");
    var result =  next(context);
    logger.LogInformation("Finished action on a bug!");
    return result;
});

bugGroup.SetupCreateBug();
bugGroup.SetupGetByIdBug();
bugGroup.SetupUpdateBug();
bugGroup.SetupDeleteBug();
bugGroup.SetupGetBugsPaginated();


app.Run();