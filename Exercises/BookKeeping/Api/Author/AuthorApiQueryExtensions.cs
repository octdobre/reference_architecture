using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Author;

/// <summary>
/// Query extensions cover:
/// - Get all entities with NoTracking()
/// - Get all entities with Projection (Non-entity types)
/// - Get multiple entities by multiple identifiers Where IN Clause
/// - Get first entity matching
/// - Get single entity matching
/// - Get all where property Like using Db functions
/// - Get projected entities from GROUP BY computed Server Side
/// </summary>
public static class AuthorApiQueryExtensions
{
    public const string PathName = "GetMultipleAuthors";

    public static void AddAuthorApiQueryExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/get");

        group.MapGet("/getAll", QueryAllAuthorsHandler)
            .WithName("GetAllAuthors")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/getAllProjected", GetAllAuthorsProjected)
            .WithName("GetAllAuthorsProjected")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/getMultipleAuthors", GetMultipleAuthorsHandler)
            .WithName(PathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/firstByLastName", GetFirstByLastNameHandler)
            .WithName("GetAuthorFirstByLastName")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/singleByFirstName", GetSingleByFirstNameHandler)
            .WithName("GetAuthorSingleByFirstName")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/getAllWhereFirstNameLike", GetFirstNameLikeHandler)
            .WithName("GetAuthorsWhereFirstNameLike")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


        group.MapGet("/getOldestByNationality", GetAuthorsOldestByNationalityHandler)
            .WithName("GetAuthorsOldestByNationalityHandler")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapGet("/getWithInclude", GetAuthorsIncludeAll)
            .WithName("GetAuthorsIncludeAll")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);
    }

    /*
     *  SELECT [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
        FROM [Authors] AS [a]
     */
    private static readonly Func<BookKeepingContext, CancellationToken, Task<IResult>> QueryAllAuthorsHandler
        = async (bookKeepingContext, token) =>
        {
            var authors = await bookKeepingContext.Authors
                .AsNoTracking()
                .ToListAsync(token);

            return TypedResults.Ok(authors);
        };

    /*
     * SELECT [a].[FirstName], [a].[LastName], [a].[Nationality]
       FROM [Authors] AS [a]
     */
    private static readonly Func<BookKeepingContext, CancellationToken, Task<IResult>> GetAllAuthorsProjected
        = async (bookKeepingContext, token) =>
        {
            var authors = await bookKeepingContext.Authors
                .AsNoTracking()
                .Select(a => new
                {
                    a.FirstName,
                    a.LastName,
                    a.Nationality
                })
                .ToListAsync(token);

            return TypedResults.Ok(authors);
        };

    /*
     * SELECT [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
       FROM [Authors] AS [a]
       WHERE [a].[Identity] IN ('bde018b3-51a4-4238-b079-08db952d9da1', 'bde018b3-51a4-4238-b079-08db952d9da1')
     */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetMultipleAuthorsHandler
            = async (ids, bookKeepingContext, token) =>
            {
                try
                {
                    var idList = ids.Split(',').Select(Guid.Parse);

                    var authors = await bookKeepingContext.Authors
                        .AsNoTracking()
                        .Where(a => idList.Contains(a.Identity))
                        .ToListAsync(token);

                    return TypedResults.Ok(authors);
                }
                catch
                {
                    return TypedResults.BadRequest();
                }
            };

    /*
     * SELECT TOP(1) [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
        FROM [Authors] AS [a]
        WHERE [a].[LastName] = @__lastName_0
     */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetFirstByLastNameHandler
        = async (lastName, bookKeepingContext, token) =>
        {
            try
            {
                var author = await bookKeepingContext.Authors
                    .AsNoTracking()
                    .FirstAsync(a => a.LastName == lastName, token);

                return TypedResults.Ok(author);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
     * SELECT TOP(2) [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
       FROM [Authors] AS [a]
       WHERE [a].[FirstName] = @__firstName_0
     */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetSingleByFirstNameHandler
        = async (firstName, bookKeepingContext, token) =>
        {
            try
            {
                var author = await bookKeepingContext.Authors
                    .AsNoTracking()
                    .SingleAsync(a => a.FirstName == firstName, token);

                return TypedResults.Ok(author);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
     * SELECT [a].[Identity], [a].[FirstName], [a].[LastName], [a].[Nationality], [a].[YearOfBirth]
       FROM [Authors] AS [a]
       WHERE [a].[FirstName] LIKE @__Format_1
     */
    private static readonly Func<string, BookKeepingContext, CancellationToken, Task<IResult>> GetFirstNameLikeHandler
        = async (firstName, bookKeepingContext, token) =>
        {
            try
            {
                var author = await bookKeepingContext.Authors
                    .AsNoTracking()
                    .Where(a => EF.Functions.Like(a.FirstName!, $"{firstName}%"))
                    .ToListAsync(token);

                return TypedResults.Ok(author);
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };


    /*
     * SELECT [a].[Nationality], MIN([a].[YearOfBirth]) AS [OldestAuthorAge]
       FROM [Authors] AS [a]
       GROUP BY [a].[Nationality]
     */
    private static readonly Func<BookKeepingContext, CancellationToken, Task<IResult>> GetAuthorsOldestByNationalityHandler
        = async (bookKeepingContext, token) =>
        {
            try
            {
                var authors = await bookKeepingContext.Authors
                    .AsNoTracking()
                    .GroupBy(a => a.Nationality)
                    .Select(group => new
                    {
                        Nationality = group.Key,
                        OldestAuthorAge = group.Select(g => g.YearOfBirth).Min()
                    })
                    .ToListAsync(token);

                return TypedResults.Ok(authors);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return TypedResults.BadRequest();
            }
        };

    /*
     *
     */
    private static readonly Func<BookKeepingContext, CancellationToken, Task<IResult>> GetAuthorsIncludeAll
        = async (bookKeepingContext, token) =>
        {
            try
            {
                var authors = await bookKeepingContext.Authors
                    .AsNoTracking()
                    .Include(a => a.Books)
                    .Include(a => a.Addresses)
                    .Include(a => a.Publishers)
                    .ToListAsync(token);

                return TypedResults.Ok(authors);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return TypedResults.BadRequest();
            }
        };
}
