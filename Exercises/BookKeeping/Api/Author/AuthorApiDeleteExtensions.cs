using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Author;

/// <summary>
/// This class to to give an example of deleting an entity
/// with many types of relationships.
///
/// When removing a relationship that has cascade delete set in the database, the dependent entity
/// does not need to be loaded into the change tracker.
///
/// When removing a relationship that does not have cascading delete in the database
/// both parties must be loaded into the change tracked and then EF takes care of cascading deletes.
///
/// For dependents with cascade deletion set to SetNull
/// it is not required to load dependent entities into the change tracker,
/// making the deletion processor simpler.
/// </summary>
public static class AuthorApiDeleteExtensions
{
    public static void AddAuthorApiDeleteExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/delete");

        group.MapDelete("/deleteAndUnlink", DeleteAndUnlinkHandler)
            .WithName("DeleteAndUnlinkFromOtherEntities")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapDelete("/deleteAndUnlinkWithFullCascade", DeleteAndUnlinkFullCascadeHandler)
            .WithName("DeleteAndUnlinkFromOtherEntitiesFullCascade")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);
    }

    /*
      QUERY------------------------------------
      SELECT [t].[Identity], [t].[FirstName], [t].[LastName], [t].[Nationality], [t].[YearOfBirth],
    [a0].[Identity], [a0].[AuthorId], [a0].[City], [a0].[Number], [a0].[Street]
      FROM (
          SELECT TOP(2) [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
          FROM [Authors] AS [a]
          WHERE [a].[Identity] = @__id_0
      ) AS [t]
      LEFT JOIN [Addresses] AS [a0] ON [t].[Identity] = [a0].[AuthorId]
      ORDER BY [t].[Identity]

      DELETION------------------------------------
      SET NOCOUNT ON;
      UPDATE [Addresses] SET [AuthorId] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
      UPDATE [Addresses] SET [AuthorId] = @p2
      OUTPUT 1
      WHERE [Identity] = @p3;
      DELETE FROM [AuthorPublisher]
      OUTPUT 1
      WHERE [AuthorsIdentity] = @p4 AND [PublishersIdentity] = @p5;
      DELETE FROM [Authors]
      OUTPUT 1
      WHERE [Identity] = @p6;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteAndUnlinkHandler
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                var author = await bookKeepingContext.Authors
                    //.Include(a => a.Publishers)  //<- this does not need to be performed, it is done automatically by SQL using Cascade Delete
                    .Include(a => a.Addresses)   //must be queried to be tracked
                    .SingleOrDefaultAsync(a => a.Identity == id, token);

                if (author is null)
                    return TypedResults.BadRequest();

                //author.Publishers.Clear(); //<- this does not need to be performed, it is done automatically by SQL using Cascade Delete
                author.Addresses.Clear(); // for tracked entities cascading behaviors will be correctly applied regardless of how the database is configured. !!!!!!!!!!!!!!!

                bookKeepingContext.Authors.Remove(author);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok();
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
     *
     * Works only if Fluent API is configured in the Authors entity configuration.
     *
      SET IMPLICIT_TRANSACTIONS OFF;
      SET NOCOUNT ON;
      DELETE FROM [Authors]
      OUTPUT 1
      WHERE [Identity] = @p0;
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteAndUnlinkFullCascadeHandler
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                var author = new Domain.Author
                {
                    Identity = id
                };

                bookKeepingContext.Authors.Remove(author);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok();
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };
}