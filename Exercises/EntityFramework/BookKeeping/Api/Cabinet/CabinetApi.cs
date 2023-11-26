using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Cabinet;

/// <summary>
/// Owned entity types.
/// Owned types are retrieved also when retrieving the aggregate.
/// Owned types do not need an .Include() call to be retrieved.
///
/// Split query support.
///     Entities are queries in multiple queries rather than with a join.
///     In certain scenarios this is faster.
/// </summary>
public static class CabinetApi
{
    private const string ResourceName = "Cabinet";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddCabinetApi(this WebApplication app)
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
     *INSERT INTO [Cabinets] ([Identity], [Title])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Domain.Cabinet, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (cabinet, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Cabinets.Add(cabinet);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{cabinet.Identity}", cabinet);
        };

    /*
     *SELECT TOP(2) [c].[Identity], [c].[Category]
      FROM [Cabinets] AS [c]
      WHERE [c].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Cabinets
                .AsNoTracking()
                .AsSplitQuery()
                .SingleOrDefaultAsync(b => b.Identity == id, token) is { } book
                ? TypedResults.Ok(book)
                : TypedResults.NotFound();
        };

    /*
     *
      DELETE FROM [Shelves]
      OUTPUT 1
      WHERE [CabinetIdentity] = @p0 AND [Id] = @p1;
      MERGE [Shelves] USING (
      VALUES (@p2, @p3, 0),
      (@p4, @p5, 1),
      (@p6, @p7, 2)) AS i ([CabinetIdentity], [RowNumber], _Position) ON 1=0
      WHEN NOT MATCHED THEN
      INSERT ([CabinetIdentity], [RowNumber])
      VALUES (i.[CabinetIdentity], i.[RowNumber])
      OUTPUT INSERTED.[Id], i._Position;
     */
    private static readonly Func<Guid, Domain.Cabinet, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updatedCabinet, bookKeepingContext, token) =>
        {
            try
            {
                var existingCabinet = bookKeepingContext.Cabinets
                    .AsSplitQuery()
                    .Single(c => c.Identity == id);

                existingCabinet.Category = updatedCabinet.Category;

                // Add new shelves
                foreach (var addedShelf in updatedCabinet.Shelves.Where(s => existingCabinet.Shelves.All(us => us.RowNumber != s.RowNumber)).ToList())
                {
                    // Add the new shelf to the existing cabinet
                    existingCabinet.AddShelf(addedShelf);
                }

                // Remove shelves
                foreach (var removedShelf in existingCabinet.Shelves.Where(s => updatedCabinet.Shelves.All(us => us.RowNumber != s.RowNumber)).ToList())
                {
                    // Remove the shelf from the existing cabinet
                    existingCabinet.RemoveShelf(removedShelf);
                }

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(existingCabinet);
            }
            catch
            {
                return TypedResults.NotFound();

            }
        };

    /*
      SELECT TOP(2) [c].[Identity], [c].[Category]
      FROM [Cabinets] AS [c]
      WHERE [c].[Identity] = @__id_0

     *DELETE FROM [Cabinets]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingCabinet = await bookKeepingContext
                .Cabinets
                .AsSplitQuery()
                .SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingCabinet is { })
            {
                bookKeepingContext.Remove(existingCabinet);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}
