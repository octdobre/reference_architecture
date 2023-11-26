using BookKeeping.Api.BookContent;
using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Book;

/// <summary>
/// Typical first implementation of CRUD using EF.
/// - Add()
/// - Get with Single() and AsNoTracking()
/// - Update with Single() and SaveChanges()
/// - Delete with Single() and Delete()
///
/// Required One-to-Many relationship.
/// A book cannot exist without an author.
/// Books are deleted on cascade when an Author is deleted.
/// </summary>
public static class BookApi
{
    private const string ResourceName = "Book";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddBookApi(this WebApplication app)
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

        group.AddBookApiQueryExtensions();
        group.AddBookApiDeleteExtensions();
        group.AddBookApiUpdateExtensions();
    }

    /*
     *INSERT INTO [Books] ([Identity], [Title])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Domain.Book, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (book, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Books.Add(book);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{book.Identity}", book);
        };

    /*
     *SELECT TOP(2) [b].[Identity], [b].[Title]
      FROM [Books] AS [b]
      WHERE [b].[Identity] = @__id_0
     */
    private static readonly Func<Guid , BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Books
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.Identity == id, token) is { } book
                ? TypedResults.Ok(book)
                : TypedResults.NotFound();
        };

    /*
     *SELECT TOP(2) [b].[Identity], [b].[Title]
      FROM [Books] AS [b]
      WHERE [b].[Identity] = @__id_0

      UPDATE [Books] SET [Afterword] = @p0, [Content] = @p1
      OUTPUT 1
      WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            if (await bookKeepingContext.Books.SingleOrDefaultAsync(b => b.Identity == id, token) is { } book)
            {
                book.Title = updateBook.Title ?? book.Title;
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(book);
            }

            return TypedResults.NotFound();
        };

    /*
      SELECT TOP(2) [b].[Identity], [b].[Title]
      FROM [Books] AS [b]
      WHERE [b].[Identity] = @__id_0

     *DELETE FROM [Books]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingBook = await bookKeepingContext.Books.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingBook is {})
            {
                bookKeepingContext.Remove(existingBook);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}
