using BookKeeping.Repo;

namespace BookKeeping.Api.Editor;

/// <summary>
/// Optional One-To-One Relationships.
/// </summary>
public static class EditorApi
{
    private const string ResourceName = "Editor";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddEditorApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}").WithTags(ResourceName);

        group.MapPost("/", CreateHandler)
            .WithName(CreatePathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);
    }

    private static readonly Func<Domain.Editor, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (entity, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Editors.Add(entity);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{entity.Identity}", entity);
        };
}