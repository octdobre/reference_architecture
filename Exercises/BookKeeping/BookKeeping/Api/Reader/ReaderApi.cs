using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Reader;

/// <summary>
/// Example of Last In Wins Concurrency.
/// For the update endpoint, the last one to call .SaveChanges() is the one that would commit the final change.
///
/// Example for Optimistic Concurrency -> ConcurrencyToken
/// When 2 parallel work-flows start
///     and they both update the same row
///     and both start with the same value
/// the first one that calls SaveChanges() wins.
/// Only the configured columns with ConcurrencyTokens will be taken into consideration and now the entire row.
/// </summary>
public static class ReaderApi
{
    private const string ResourceName = "Reader";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddReaderApi(this WebApplication app)
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
    }

    /*
     *INSERT INTO [Reader] ([Identity], [FirstName], [LastName])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Domain.Reader, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (reader, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Readers.Add(reader);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{reader.Identity}", reader);
        };

    /*
     *SELECT TOP(2) [r].[Identity], [r].[FirstName], [r].[LastName]
      FROM [Reader] AS [r]
      WHERE [r].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
    = async (id, bookKeepingContext, token) =>
       {
           return await bookKeepingContext.Readers
               .AsNoTracking()
               .SingleOrDefaultAsync(b => b.Identity == id, token) is { } book
               ? TypedResults.Ok(book)
               : TypedResults.NotFound();
       };

    /*
     *SELECT TOP(2) [r].[Identity], [r].[FirstName], [r].[LastName]
      FROM [Reader] AS [r]
      WHERE [r].[Identity] = @__id_0

      UPDATE [Reader] SET [FirstName] = @p0, [LastName] = @p1
      OUTPUT 1
      WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Reader, bool, bool, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateReader, updateLastName, shouldWait, bookKeepingContext, token) =>
        {
            if (await bookKeepingContext.Readers.SingleOrDefaultAsync(b => b.Identity == id, token) is { } reader)
            {
                if (!updateLastName)
                {
                    reader.FirstName = updateReader.FirstName;
                }
                else
                {
                    reader.LastName = updateReader.LastName;
                }

                if (shouldWait)
                {
                    await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(10)));
                }

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(reader);
            }

            return TypedResults.NotFound();
        };

    /*
      SELECT TOP(2) [r].[Identity], [r].[FirstName], [r].[LastName]
      FROM [Reader] AS [r]
      WHERE [r].[Identity] = @__id_0

     *DELETE FROM [Reader]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingReader = await bookKeepingContext.Readers.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingReader is { })
            {
                bookKeepingContext.Remove(existingReader);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}