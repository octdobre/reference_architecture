using BookKeeping.Repo;

namespace BookKeeping.Api.Address;

/// <summary>
/// Optional foreign key relationship (Optional One-to-Many, Authors-to-Address).
/// An address can exist without an author.
/// When an Author is deleted, the addresses can remain and have their field set to NULL again.
/// </summary>
public static class AddressApi
{
    private const string ResourceName = "Address";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddAddressApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}").WithTags(ResourceName);

        group.MapPost("/", CreateHandler)
            .WithName(CreatePathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);
    }

    private static readonly Func<Domain.Address, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (address, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Addresses.Add(address);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{address.Identity}", address);
        };
}