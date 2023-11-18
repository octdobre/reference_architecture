using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;
using static BookKeeping.Api.Book.BookApiUpdateExtensions;

namespace BookKeeping.Api.Customer;

/// <summary>
/// JSON Columns or Documents.
/// Complex properties of aggregates are saved as json string values in the database.
/// All dependent objects are are retrieved without include.
/// Json objects cannot have foreign keys to other tables they are just documents.
/// </summary>
public static class CustomerApi
{
    private const string ResourceName = "Customer";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddCustomerApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}")
            //groups endpoints like controllers
            .WithTags(ResourceName);

        group.MapPost("/", CreateHandler)
            .WithName(CreatePathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", GetByIdentity)
            .WithName($"Get{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", UpdateHandler)
            .WithName($"Update{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteHandler)
            .WithName($"Delete{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }

    /*

     */
    private static readonly Func<Domain.Customer, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (customer, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Customers.Add(customer);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{customer.Identity}", customer);
        };

    /*

     */
    private static readonly Func<Guid , BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Customers
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.Identity == id, token) is { } customer
                ? TypedResults.Ok(customer)
                : TypedResults.NotFound();
        };

    /*

     */
    private static readonly Func<Guid, Domain.Customer, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateCustomer, bookKeepingContext, token) =>
        {
            try
            {
                var existingCustomer = await bookKeepingContext.Customers
                   .SingleAsync(b => b.Identity == id, token);

                existingCustomer.Name = updateCustomer.Name;

                existingCustomer.ContactAndPayment = updateCustomer.ContactAndPayment;

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateCustomer);
            }
            catch(Exception ex)
            {
                return TypedResults.NotFound(ex.Message);
            }
        };

    /*

     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingCustomer = await bookKeepingContext.Customers.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingCustomer is {})
            {
                bookKeepingContext.Remove(existingCustomer);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}
