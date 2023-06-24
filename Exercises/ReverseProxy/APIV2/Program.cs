var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/api/v2/version", () =>
    {
        return "This API is version 2!!!!!!! " + GenerateRandomString(10);
    })
    .WithName("GetVersion")
    .WithOpenApi();

app.MapGet("/api/v1/version", () =>
    {
        return "This API is version 1 Load balanced in API 2!!!!!!! " + GenerateRandomString(10);
    })
    .WithName("GetVersion2")
    .WithOpenApi();

app.Run();


static string GenerateRandomString(int length)
{
    Random random = new Random();
    string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

    return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
}