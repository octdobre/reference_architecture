using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Publisher;

/// <summary>
/// One-To-One Relationships.
/// Many-To-Many Relationships.
///
/// Lazy loading of entity using Find() and then .Entry(entity).Reference(p => p.NavProperty).LoadAsync();
/// </summary>
public static class PublisherApi
{
    private const string ResourceName = "Publisher";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddPublisherApi(this WebApplication app)
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

        group.MapDelete("/{id:guid}", DeleteWithExecuteDelete)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.AddPublisherApiRelationshipExtensions();
        group.AddPublisherApiQueryExtensions();
    }

    /*
     *INSERT INTO [Publishers] ([Identity], ...)
      VALUES (@p0, ...);
     */
    private static readonly Func<Domain.Publisher, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (publisher, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Publishers.Add(publisher);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{publisher.Identity}", publisher);
        };

    /*
      SELECT TOP(1) [p].[Identity], [p].[FirstName]
      FROM [Publishers] AS [p]
      WHERE [p].[Identity] = @__get_Item_0

      SELECT [e].[Identity], [e].[FirstName], [e].[LastName], [e].[PublisherId]
      FROM [Editors] AS [e]
      WHERE [e].[PublisherId] = @__get_Item_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            if (await bookKeepingContext.Publishers.FindAsync(id, token) is { } publisher)
            {
                await bookKeepingContext.Entry(publisher).Reference(p => p.Editor).LoadAsync();

                return TypedResults.Ok(new
                {
                    publisher.Identity,
                    publisher.Name,
                    Editor = publisher.Editor is null ? null : new
                    {
                        publisher.Editor.Identity,
                        publisher.Editor.FirstName,
                        publisher.Editor.LastName
                    }
                });
            }

            return TypedResults.NotFound();
        };

    /*
     * Will only delete a publisher if no editor is linked to it.
     * Even if the PublisherId field is null on the Editor, it must be set to null before deleting the publisher.
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteWithExecuteDelete
        = async (id, bookKeepingContext, token) =>
        {
            try
            {
                //Deletes entities based on a query
                await bookKeepingContext.Publishers.Where(b => b.Identity == id).ExecuteDeleteAsync(token);

                return TypedResults.Ok(id);
            }
            catch(Exception ex)
            {
                //Could fail then maybe doesn't exist
                return TypedResults.NotFound();
            }
        };
}