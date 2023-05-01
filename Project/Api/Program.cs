using Api.Features.Bug;
using Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BugDb>(opt => opt.UseInMemoryDatabase("BugDb"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var bugGroup = app.MapGroup("/bug");

bugGroup.AddEndpointFilter((context, next) =>
{
    app.Logger.LogInformation("Started action on a bug!");
    var result =  next(context);
    app.Logger.LogInformation("Finished action on a bug!");
    return result;
});

bugGroup.SetupCreateBug();
bugGroup.SetupGetByIdBug();
bugGroup.SetupUpdateBug();
bugGroup.SetupDeleteBug();


app.Run();