using BookKeeping.Repo;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Author;

/// <summary>
///  Optimized atomic sql commands.
/// - Get using Find method by Identity
/// - Patch using Update method
/// - Delete using ExecuteDelete()
///
///  One-to-Many Relationships.
/// </summary>
public static class AuthorApi
{
    private const string ResourceName = "Author";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddAuthorApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}").WithTags(ResourceName);

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

        group.AddAuthorApiCreationExtensions();
        group.AddAuthorApiQueryExtensions();
        group.AddAuthorApiRelationshipExtensions();
        group.AddAuthorApiDeleteExtensions();
    }

    /*
     *INSERT INTO [Authors] ([Identity], ...)
      VALUES (@p0, @p1, @p2);

      Or when adding books also

      INSERT INTO [Authors] ([Identity], [FirstName], [LastName], [Nationality], [YearOfBirth])
      VALUES (@p0, @p1, @p2, @p3, @p4);
      INSERT INTO [Books] ([Identity], [Afterword], [AuthorId], [Title])
      VALUES (@p5, @p6, @p7, @p8, @p9);
     */
    private static readonly Func<Domain.Author, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (author, bookKeepingContext, linker, token) =>
        {
            author.Identity = Guid.NewGuid();

            bookKeepingContext.Authors.Add(author);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{author.Identity}",
            new {
                author.Identity,
                Books = author.Books.Select(b => new
                {
                    b.Identity,
                    b.Title,
                    b.Afterword
                })
            });
        };

    /*
     *SELECT TOP(1) [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
      FROM [Authors] AS [a]
      WHERE [a].[Identity] = @__get_Item_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token)
            => await bookKeepingContext.Authors.FindAsync(id, token) is { } author
            ? TypedResults.Ok(author)
            : TypedResults.NotFound();

    /*
     *UPDATE [Authors] SET [FirstName] = @p0, [LastName] = @p1, [Nationality] = @p2, [YearOfBirth] = @p3
      OUTPUT 1
      WHERE [Identity] = @p4;
     */
    private static readonly Func<Guid, Domain.Author, BookKeepingContext, CancellationToken, Task<IResult>> UpdateHandler
        = async (id, updateAuthor, bookKeepingContext, token) =>
        {
            try
            {
                updateAuthor.Identity = id;
                bookKeepingContext.Update(updateAuthor);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(updateAuthor);
            }
            catch
            {
                return TypedResults.NotFound();
            }
        };

    /*
      DELETE FROM [a]
      FROM [Authors] AS [a]
      WHERE [a].[Identity] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                //Deletes entities based on a query
                await bookKeepingContext.Authors.Where(b => b.Identity == id).ExecuteDeleteAsync(token);

                return TypedResults.Ok(id);
            }
            catch (SqlException ex)
            {
                //catch if ForeignKey exception
                if (ex is { Number: 547 })
                {
                    Console.WriteLine(ex.Message);
                }
                return TypedResults.BadRequest();
            }
            catch
            {
                //Could fail then maybe doesn't exist
                return TypedResults.NotFound();
            }
        };
}
