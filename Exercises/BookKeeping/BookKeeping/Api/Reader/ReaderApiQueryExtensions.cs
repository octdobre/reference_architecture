using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Reader;

/// <summary>
/// .FromSql() - with interpolation
/// Used DbSet.FromSql to execute raw sql and return mapped entities.
///     The FromSql method does not actually pass this sql statement to the db.
///
/// .FromSqlRaw() - with parametrization
/// Used DbSet.FromRawSql to execute raw sql and return mapped entities.
///     The FromSql method does not actually pass this sql statement to the db.
///
///.FromSql() - Non Entities and Queries
///
/// SqlQuery()
/// To query scalar types.
///
/// ExecuteSql()
/// To perform a sql query directly on the database. INSERT/UPDATE/DELETE
/// It returns the number of rows affected.
/// </summary>
public static class ReaderApiQueryExtensions
{
    public static void AddBookApiQueryExtensions(this RouteGroupBuilder group)
    {
        group.MapGet("/byFirstName", GetFirstByFirstNameHandler)
            .WithName("GetFirstReaderByFirstName")
            .WithOpenApi()
            .Produces<Domain.Reader>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/byLastName", GetFirstByLastNameHandler)
            .WithName("GetFirstReaderByLastName")
            .WithOpenApi()
            .Produces<Domain.Reader>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{id:guid}/reservation", GetFirstReservation)
            .WithName("GetFirstReservation")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/count", GetCount)
            .WithName("GetCount")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapPut("/deleteWithExecute", UpdateWithExecute)
            .WithName("UpdateWithExecute")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);
    }

    /*FromSql
    SELECT TOP(1) [b].[Identity], [b].[FirstName], [b].[LastName]
      FROM (
          SELECT * FROM Readers WHERE FirstName = @p0
      ) AS [b]
    */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetFirstByFirstNameHandler
        = async (firstName, bookKeepingContext, token) =>
        {
            var reader = await bookKeepingContext.Readers
                .FromSql($"SELECT * FROM Readers WHERE FirstName = {firstName}").FirstOrDefaultAsync(token);

            return reader is not null
                ? TypedResults.Ok(reader)
                : TypedResults.NotFound();
        };

    /*FromSqlRaw
     *  SELECT TOP(1) [b].[Identity], [b].[FirstName], [b].[LastName]
      FROM (
          SELECT * FROM Readers WHERE LastName = @p0
      ) AS [b]
     */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetFirstByLastNameHandler
        = async (lastName, bookKeepingContext, token) =>
        {
            var reader = await bookKeepingContext.Readers
                .FromSqlRaw("SELECT * FROM Readers WHERE LastName = {0}", lastName).FirstOrDefaultAsync(token);

            return reader is not null
                ? TypedResults.Ok(reader)
                : TypedResults.NotFound();
        };

    /*Non-Entity Types and Projections

     * SELECT TOP(1) [b].[Identity] AS [ReaderIdentity], [b].[FirstName] AS [ReaderFirstName],
     *                  [r].[Identity] AS [ReservationIdentity], [r].[BookId] AS [ReservationBookIdentity]
      FROM (
          SELECT * FROM Readers WHERE [Identity] = @p0
      ) AS [b]
      LEFT JOIN [Reservations] AS [r] ON [b].[Identity] = [r].[ReaderId]
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetFirstReservation
        = async (id, bookKeepingContext, token) =>
        {
            var reader = await bookKeepingContext.Readers
                .FromSql($"SELECT * FROM Readers WHERE [Identity] = {id}")
                .Select(r => new
                {
                    ReaderIdentity = r.Identity,
                    ReaderFirstName = r.FirstName,
                    ReservationIdentity = r.Reservation.Identity,
                    ReservationBookIdentity = r.Reservation.BookId
                })
                .FirstOrDefaultAsync(token);

            return reader is not null
                ? TypedResults.Ok(reader)
                : TypedResults.NotFound();
        };

    /*
     * SELECT COUNT(*) FROM Readers
     */
    private static readonly Func<BookKeepingContext, CancellationToken, Task<IResult>> GetCount
        = async (bookKeepingContext, token) =>
        {
            var count = await bookKeepingContext.Database.SqlQuery<int>($"SELECT COUNT(*) FROM Readers").ToListAsync(token);

            return  await Task.FromResult(TypedResults.Ok(count.First()));
        };

    /*
     * UPDATE Readers SET LastName='{newLastName}'" -> returns affected rows
     */
    private static readonly Func<string,BookKeepingContext, CancellationToken, Task<IResult>> UpdateWithExecute
        = async (newLastName, bookKeepingContext, token) =>
        {
            var rowsAffected = await bookKeepingContext.Database.ExecuteSqlAsync($"UPDATE Readers SET LastName={newLastName}", token);

            return await Task.FromResult(TypedResults.Ok(
                new
                {
                    AffectedRows_UpdatedRows = rowsAffected
                }));
        };
}