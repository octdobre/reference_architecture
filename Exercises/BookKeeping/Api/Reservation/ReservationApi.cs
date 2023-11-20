using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Reservation;

/// <summary>
/// Example of OptimisticConcurrency using RowVersion.
/// The rowVersion column will lock the entire row record from being updated.
/// Again the same as the ConcurrencyToken, the first to call SaveChanges() wins the race.
/// The second work-flow to call SaveChanges() will fail, with the error: "0 rows have been affected".
///
/// When performing an update or delete operation, EF includes these concurrency tokens in the SQL WHERE clause.
/// </summary>
public static class ReservationApi
{
    private const string ResourceName = "Reservation";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddReservationApi(this WebApplication app)
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
     *INSERT INTO [Reservation] ([Identity], [BookId], [ReaderId])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Domain.Reservation, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (reservation, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Reservations.Add(reservation);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{reservation.Identity}", reservation);
        };

    /*
     *SELECT TOP(2) [r].[Identity], [r].[BookId], [r].[ReaderId]
      FROM [Reservation] AS [r]
      WHERE [r].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
    = async (id, bookKeepingContext, token) =>
       {
           return await bookKeepingContext.Reservations
               .AsNoTracking()
               .SingleOrDefaultAsync(b => b.Identity == id, token) is { } book
               ? TypedResults.Ok(book)
               : TypedResults.NotFound();
       };

    /*
     *SELECT TOP(2) [r].[Identity], [r].[BookId], [r].[ReaderId]
      FROM [Reservation] AS [r]
      WHERE [r].[Identity] = @__id_0

      UPDATE [reservation] SET [ReaderId] = @p0
      OUTPUT 1
      WHERE [Identity] = @p2;
     */
    private static readonly Func<Guid, Domain.Reservation,  bool, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateReservation, shouldWait, bookKeepingContext, token) =>
        {
            try
            {
                if (await bookKeepingContext.Reservations.SingleOrDefaultAsync(b => b.Identity == id, token) is { } reservation)
                {
                    reservation.ReaderId = updateReservation.ReaderId;

                    if (shouldWait)
                    {
                        await Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(10)));
                    }

                    await bookKeepingContext.SaveChangesAsync(token);

                    return TypedResults.Ok(reservation);
                }

                return TypedResults.NotFound();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return TypedResults.BadRequest(ex.Message);
            }
        };

    /*
      SELECT TOP(2) [r].[Identity], [r].[BookId], [r].[ReaderId]
      FROM [reservation] AS [r]
      WHERE [r].[Identity] = @__id_0

     *DELETE FROM [reservation]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingReservation = await bookKeepingContext.Reservations.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingReservation is { })
            {
                bookKeepingContext.Remove(existingReservation);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}