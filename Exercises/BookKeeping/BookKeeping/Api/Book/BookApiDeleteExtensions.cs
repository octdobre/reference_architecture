using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Book;

/// <summary>
/// Deletion extensions cover:
/// - Delete with attached only entity
/// - Delete with state changed
/// - Delete with ExecuteDelete() method
/// </summary>
public static class BookApiDeleteExtensions
{
    public static void AddBookApiDeleteExtensions(this RouteGroupBuilder group)
    {
        group.MapDelete("/attachedDelete/{id:guid}", DeleteWithAttachHandler)
            .WithName("DeleteWithAttached")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/entityStateChangedDelete/{id:guid}", DeleteWithEntityStateChangedHandler)
            .WithName("DeleteWithEntityStateChanged")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/executeDelete/{id:guid}", DeleteWithExecuteDelete)
            .WithName("DeleteWithExecute")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);
    }

    /*
     * SET IMPLICIT_TRANSACTIONS OFF;
       SET NOCOUNT ON;
       DELETE FROM [Books]
       OUTPUT 1
       WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteWithAttachHandler
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                //No entity is retrieved
                var bookToTryToDelete = new Domain.Book { Identity = id };
                //Attach first
                bookKeepingContext.Books.Attach(bookToTryToDelete);
                //Remove
                bookKeepingContext.Remove(bookToTryToDelete);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }
            catch
            {
                //Could fail then maybe doesn't exist
                return TypedResults.NotFound();
            }

        };

    /*
       SET IMPLICIT_TRANSACTIONS OFF;
       SET NOCOUNT ON;
       DELETE FROM [Books]
       OUTPUT 1
       WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteWithEntityStateChangedHandler
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                //No entity is retrieved
                var bookToTryToDelete = new Domain.Book { Identity = id };
                //Set entity state
                bookKeepingContext.Entry(bookToTryToDelete).State = EntityState.Deleted;
                //Remove, no need to call delete
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }
            catch
            {
                //Could fail then maybe doesn't exist
                return TypedResults.NotFound();
            }
        };

    /*
        SET IMPLICIT_TRANSACTIONS OFF;
        SET NOCOUNT ON;
        DELETE FROM [b]
        FROM [Books] AS [b]
        WHERE [b].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteWithExecuteDelete
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                //Deletes entities based on a query
                await bookKeepingContext.Books.Where(b => b.Identity == id).ExecuteDeleteAsync(token);

                return TypedResults.Ok(id);
            }
            catch
            {
                //Could fail then maybe doesn't exist
                return TypedResults.NotFound();
            }
        };
}