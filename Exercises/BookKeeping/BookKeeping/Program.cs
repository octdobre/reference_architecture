using System.Text.Json.Serialization;
using BookKeeping.Api.Address;
using BookKeeping.Api.Author;
using BookKeeping.Api.Book;
using BookKeeping.Api.BookContent;
using BookKeeping.Api.Cabinet;
using BookKeeping.Api.Customer;
using BookKeeping.Api.Editor;
using BookKeeping.Api.Loan;
using BookKeeping.Api.Publisher;
using BookKeeping.Api.Reader;
using BookKeeping.Api.Reservation;
using BookKeeping.Api.Writing;
using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BookKeepingContext>(
    options =>
    {
        options
            //log sql commands
            .UseLoggerFactory(LoggerFactory.Create(lBuilder => { lBuilder.AddConsole(); }))
            //show values in sql commands
            .EnableSensitiveDataLogging()
            .UseSqlServer(
                builder.Configuration.GetConnectionString("LocalConnection"),
                sqlOptions =>
                {
                    //set assembly for migrations
                    sqlOptions.MigrationsAssembly(typeof(Program).Assembly.GetName().Name);
                });
    });

//generic delegate to load navigation properties
builder.Services.AddTransient<Action<object, string>>(provider =>
{
    var dbContext = provider.GetRequiredService<BookKeepingContext>();

    return (entity, navigationProperty) =>
    {
        dbContext.Entry(entity).Reference(navigationProperty).Load();

        //await bookKeepingContext.Entry(loan).Reference(p => p.Reader).LoadAsync();
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        c =>
        {
            c.DisplayRequestDuration();
        });
}

app.UseHttpsRedirection();

app.AddBookApi();
app.AddAuthorApi();
app.AddAddressApi();
app.AddPublisherApi();
app.AddEditorApi();
app.AddReaderApi();
app.AddReservationApi();
app.AddBookContentApi();
app.AddLoanApi();
app.AddCabinetApi();
app.AddCustomerApi();
app.AddWritingApi();

using var scopedServices = app.Services.CreateScope();

var context = scopedServices.ServiceProvider.GetRequiredService<BookKeepingContext>();

context.Database.EnsureDeleted();

context.Database.EnsureCreated();

DataSeeder.AddSeed(context);

app.Run();