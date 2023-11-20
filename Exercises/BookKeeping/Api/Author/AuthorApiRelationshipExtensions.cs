using BookKeeping.Repo;

namespace BookKeeping.Api.Author;

/// <summary>
/// - Attach book to author using  2x Find() and Add() = 3 operations
/// - Attach book to author using Attach => 1 Update operation
/// - Insert book and attach => 1 Insert operation
/// </summary>
public static class AuthorApiRelationshipExtensions
{
    public static void AddAuthorApiRelationshipExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/{authorId:guid}/relationships");

        group.MapPost("/linkToAddress/{addressId:guid}", CreateAddressLinkHandler)
            .WithName("CreateAddressLinkHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/linkToAddressWithAttach/{addressId:guid}", CreateAddressLinkWithAttachHandler)
            .WithName("CreateAddressLinkWithAttachHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/create-and-linkToBookWithAttach", CreateBookAndLinkWithAttachHandler)
            .WithName("CreateBookAndLinkWithAttachHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /*
       Classic linking of a child to a parent entity.
       2 SELECTS and one Update
       UPDATE [Addresses] SET [AuthorIdentity] = @p0
       OUTPUT 1
       WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> CreateAddressLinkHandler
        = async (authorId, addressId, bookKeepingContext, token) =>
        {
            try
            {
                var author = await bookKeepingContext.Authors.FindAsync(authorId, token);

                var address = await bookKeepingContext.Addresses.FindAsync(addressId);

                author!.Addresses.Add(address!);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(author);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /* Single command
      UPDATE [Addresses] SET [AuthorIdentity] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> CreateAddressLinkWithAttachHandler
        = async (authorId, addressId, bookKeepingContext, token) =>
        {
            try
            {
                var author = new Domain.Author { Identity = authorId };

                var address = new Domain.Address { Identity = addressId };

                bookKeepingContext.Attach(author);
                bookKeepingContext.Attach(address);

                author.Addresses.Add(address);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(author);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
     * INSERT INTO [Books] ([Identity], [Afterword], [AuthorId], [Content], [Title])
       VALUES (@p0, @p1, @p2, @p3, @p4);
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> CreateBookAndLinkWithAttachHandler
        = async (authorId, book, bookKeepingContext, token) =>
        {
            try
            {
                book.Identity = Guid.NewGuid();

                var author = new Domain.Author { Identity = authorId };

                bookKeepingContext.Attach(author);

                author.Books.Add(book);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(book);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };
}

