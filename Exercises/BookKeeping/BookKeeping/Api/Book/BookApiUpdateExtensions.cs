using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Book;

/// <summary>
/// Update extensions cover:
/// - Updating specific fields with Entry.SetValues
/// - Updating using StateChanged
/// - Updating using Update() method
/// - Updating using Entity Attach
/// - Updating using the ChangeTracker API and
///   Updating individual fields with trimmed update statement
/// </summary>
public static class BookApiUpdateExtensions
{
    public static void AddBookApiUpdateExtensions(this RouteGroupBuilder group)
    {
        group.MapPut("/updateOnlySpecificFields/{id:guid}", UpdateOnlySpecificFieldsHandler)
            .WithName("UpdateOnlySpecificFields")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);


        group.MapPut("/entityStateChangedUpdate/{id:guid}", UpdateWithEntityStateHandler)
            .WithName("UpdateBookWithEntityChanged")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/updateWithUpdateMethod/{id:guid}", UpdateWithUpdateMethodHandler)
            .WithName("UpdateWithUpdateMethodHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/updateWithAttach/{id:guid}", UpdateMethodWithAttachHandler)
            .WithName("UpdateMethodWithAttachHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/updateNonNullWithChangeTracker/{id:guid}", UpdateNonNullWithChangeTracker)
            .WithName("UpdateNonNullWithChangeTracker")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }

    public record UpdateBook(string? Content,
        string? Afterword);

    /*
     *SELECT TOP(1) [b].[Identity], [b].[Afterword], [b].[Content], [b].[Title]
      FROM [Books] AS [b]
      WHERE [b].[Identity] = @__get_Item_0

      UPDATE [Books] SET [Afterword] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, UpdateBook, BookKeepingContext, CancellationToken, Task<IResult>> UpdateOnlySpecificFieldsHandler
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            try
            {
                //Has to first get the entity
                var existingEntity = await bookKeepingContext.Books.FindAsync(id);

                if (existingEntity == null)
                {
                    return TypedResults.NotFound();
                }

                // Update only values that have changed
                // SetValues will copy all the values of all matching properties even if NULL
                bookKeepingContext.Entry(existingEntity).CurrentValues.SetValues(updateBook);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateBook);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };



    /*
     * UPDATE [Books] SET [Afterword] = @p0, [Content] = @p1
       OUTPUT 1
       WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> UpdateWithEntityStateHandler
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            try
            {
                var book = new Domain.Book
                {
                    Identity = id,
                    Title = updateBook.Title
                };

                if (updateBook.Afterword is { })
                {
                    book.Afterword = updateBook.Afterword;
                }

                updateBook.Identity = id;

                bookKeepingContext.Entry(updateBook).State = EntityState.Modified;

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateBook);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };

    /*
     * UPDATE [Books] SET [Afterword] = @p0, [Content] = @p1
       OUTPUT 1
       WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> UpdateWithUpdateMethodHandler
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            try
            {
                updateBook.Identity = id;
                bookKeepingContext.Update(updateBook);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateBook);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };

    /*
      UPDATE [Books] SET [Afterword] = @p0, [Content] = @p1
      OUTPUT 1
      WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> UpdateMethodWithAttachHandler
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            try
            {
                updateBook.Identity = id;
                bookKeepingContext.Attach(updateBook);

                if (updateBook.Afterword is { })
                {
                    bookKeepingContext.Entry(updateBook).Property("Afterword").IsModified = true;
                }

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateBook);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };

    /*
       True selective updating of properties

     * UPDATE [Books] SET [Content] = @p0
       OUTPUT 1
       WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Domain.Book, BookKeepingContext, CancellationToken, Task<IResult>> UpdateNonNullWithChangeTracker
        = async (id, updateBook, bookKeepingContext, token) =>
        {
            try
            {
                updateBook.Identity = id;

                var returnDict = new Dictionary<string, object>
                {
                    ["Identity"] = id
                };

                bookKeepingContext.ChangeTracker.TrackGraph(updateBook, e =>
                {
                    foreach (var entryProperty in e.Entry.Properties)
                    {
                        if (entryProperty.Metadata.Name is "Identity" or "Title")
                            continue;

                        if (entryProperty.CurrentValue != null)
                        {
                            entryProperty.IsModified = true;
                            returnDict[entryProperty.Metadata.Name] = entryProperty.CurrentValue;
                        }
                        else
                        {
                            entryProperty.IsModified = false;
                        }
                    }
                });

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(returnDict);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };
}