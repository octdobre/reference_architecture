using BookKeeping.Common;
using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Book;

/// <summary>
/// Query extensions cover:
///  - Paginated query.
/// - Lazy loading using ILazyLoading interface;
/// </summary>
public static class BookApiQueryExtensions
{
    public static void AddBookApiQueryExtensions(this RouteGroupBuilder group)
    {
        group.MapGet("/byTitle", GetSortedByTitleHandler)
            .WithName("GetBooksSortedByTitle")
            .WithOpenApi()
            .Produces<Page<Domain.Book>>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);


        group.MapGet("{bookId:}/withContent", GetBookWithContentLazyLoaded)
            .WithName("GetBookWithContentLazyLoaded")
            .WithOpenApi()
            .Produces<Page<Domain.Book>>()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /*
       SELECT [b].[Identity], [b].[Content], [b].[Title]
       FROM [Books] AS [b]
       ORDER BY [b].[Title]
       OFFSET @__p_0 ROWS FETCH NEXT @__p_1 ROWS ONLY
     */
    private static readonly Func<int?, int?, bool?, BookKeepingContext, CancellationToken, Task<IResult>>
        GetSortedByTitleHandler
            = async (pageNumber,
                pageSize,
                sortByTitle,
                bookKeepingContext,
                token) =>
            {
                //default values
                pageNumber ??= 1;

                pageSize = pageSize is { }
                    ? pageSize is > 10 or < 0
                        ? 10
                        : pageSize
                    : 10;

                sortByTitle ??= true;
                var skip = (pageNumber - 1) * pageSize;

                var query = bookKeepingContext.Books.AsNoTracking();

                //Order first
                var orderedQuery = sortByTitle.Value
                    ? query.OrderBy(b => b.Title)
                    : query.OrderByDescending(b => b.Title);

                //Then query
                var paginatedQuery = orderedQuery
                    .Skip(skip.Value)
                    .Take(pageSize.Value);

                var items = await paginatedQuery.ToListAsync(token);

                var count = items.Count;

                return items.Any()
                    ? TypedResults.Ok(new Page<Domain.Book>(items, count, pageNumber.Value))
                : TypedResults.NoContent();
            };

    /**
     *SELECT TOP(1) [b].[Identity], [b].[Afterword], [b].[AuthorId], [b].[Title]
      FROM [Books] AS [b]
      WHERE [b].[Identity] = @__bookId_0

      Executed DbCommand (1ms) [Parameters=[@__get_Item_0='3c0e2031-6893-4c7a-bda3-6e9ff2910e0d' (Nullable = true)], CommandType='Text', CommandTimeout='30']
      SELECT [b].[Identity], [b].[BookId], [b].[Content]
      FROM [BookContents] AS [b]
      WHERE [b].[BookId] = @__get_Item_0
     */
    private static readonly Func<Guid, bool?, BookKeepingContext, Action<object, string>, CancellationToken, Task<IResult>>
        GetBookWithContentLazyLoaded
            = async (bookId,
                load,
                bookKeepingContext,
                lazyLoaderService,
                token) =>
            {
                var book = await bookKeepingContext.Books
                    .FirstOrDefaultAsync(b => b.Identity == bookId, token);

                if (book is { })
                {
                    if (load.HasValue && load.Value)
                    {
                        lazyLoaderService(book, nameof(book.Content));
                    }

                    return TypedResults.Ok(book);
                }
                else
                {
                    return TypedResults.NotFound();
                }
            };
}
