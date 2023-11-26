using BookKeeping.Repo;

namespace BookKeeping.Api.Publisher;

/// <summary>
/// One-To-One Relationships.
/// - Linking entities in One-to-One relationship
/// - Un-linking entities in One-to-One relationship without deleting the entities
/// - Changing link between different one-to-one entities
///
/// Many-To-Many Relationships.
/// - Create row in relationship table for many-to-many relationship
/// https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one
/// </summary>
public static class PublisherApiRelationshipExtensions
{
    public static void AddPublisherApiRelationshipExtensions(this RouteGroupBuilder group)
    {
        group = group.MapGroup("/{publisherId:guid}/relationships");

        group.MapPost("/editor/startAssociation/{editorId:guid}", StartAssociationHandler)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/editor/endAssociation/{editorId:guid}", EndAssociationHandler)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapPost("/editor/{editorId:guid}/changePublisher/{newPublisherId:guid}", ChangePublisherHandler)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);

        group.MapPost("authors/startPublications/{authorId:guid}", StartPublicationsForAuthorHandler)
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK);
    }

    /* Single command
      UPDATE [Editors] SET [PublisherId] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> StartAssociationHandler
        = async (publisherId, editorId, bookKeepingContext, token) =>
        {
            try
            {
                var publisher = new Domain.Publisher { Identity = publisherId };

                var editor = new Domain.Editor { Identity = editorId };

                bookKeepingContext.Attach(publisher);
                bookKeepingContext.Attach(editor);

                publisher.Editor = editor;

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(new
                {
                    publisher.Identity,
                    Editor = new
                    {
                        publisher.Editor.Identity,
                    }
                });
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
      UPDATE [Editors] SET [PublisherId] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> EndAssociationHandler
        = async (publisherId, editorId, bookKeepingContext, token) =>
        {
            try
            {
                var editor = new Domain.Editor
                {
                    Identity = editorId,
                    PublisherId = null
                };

                bookKeepingContext.Attach(editor);

                bookKeepingContext.Entry(editor).Property("PublisherId").IsModified = true;

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(new
                {
                    PublisherId = publisherId,
                    EditorId = editorId
                });
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
      UPDATE [Editors] SET [PublisherId] = @p0
      OUTPUT 1
      WHERE [Identity] = @p1;
     */
    private static readonly Func<Guid, Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> ChangePublisherHandler
        = async (publisherId, editorId, newPublisherId, bookKeepingContext, token) =>
        {
            try
            {
                var editor = new Domain.Editor
                {
                    Identity = editorId,
                    PublisherId = newPublisherId
                };

                bookKeepingContext.Attach(editor);

                bookKeepingContext.Entry(editor).Property("PublisherId").IsModified = true;

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(new
                {
                    OldPublisherId = publisherId,
                    NewPublisherId = newPublisherId,
                    EditorId = editorId
                });
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };

    /*
     * INSERT INTO [AuthorPublisher] ([AuthorIdentity], [PublisherIdentity])
       VALUES (@p0, @p1);
     */
    private static readonly Func<Guid, Guid, BookKeepingContext, CancellationToken, Task<IResult>> StartPublicationsForAuthorHandler
        = async (publisherId, authorId, bookKeepingContext, token) =>
        {
            try
            {
                var publisher = new Domain.Publisher { Identity = publisherId };

                var author = new Domain.Author { Identity = authorId };

                bookKeepingContext.Attach(publisher);
                bookKeepingContext.Attach(author);

                publisher.Authors.Add(author);

                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(new
                {
                    publisher.Identity,
                    authorIdentity = authorId
                });
            }
            catch
            {
                return TypedResults.BadRequest();
            }
        };
}