using BookKeeping.Domain;
using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Writing;

/// <summary>
/// Table per concrete type strategy.
/// .OfType() filters per concrete type
/// </summary>
public static class WritingApi
{
    private const string ResourceName = "Writing";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddWritingApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}")
            //groups endpoints like controllers
            .WithTags(ResourceName);

        group.MapPost("/newsArticle/", CreateNewsArticleHandler)
            .WithName(CreatePathName+ "newsArticle")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapPost("/ad/", CreateAdHandler)
            .WithName(CreatePathName + "Ad")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapPost("/jobOffer/", CreateJobOfferHandler)
            .WithName(CreatePathName + "jobOffer")
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/{id:guid}", GetByIdentity)
            .WithName($"Get{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/byType", GetByType)
            .WithName($"Get{ResourceName}ByType")
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

     */
    private static readonly Func<Domain.NewsArticle, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateNewsArticleHandler
        = async (writing, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Writings.Add(writing);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{writing.Identity}", writing);
        };

    private static readonly Func<Domain.Ad, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateAdHandler
        = async (writing, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Writings.Add(writing);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{writing.Identity}", writing);
        };

    private static readonly Func<Domain.JobOffer, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateJobOfferHandler
        = async (writing, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Writings.Add(writing);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{writing.Identity}", writing);
        };

    /*

     */
    private static readonly Func<Guid , BookKeepingContext, CancellationToken, Task<IResult>> GetByIdentity
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Writings
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.Identity == id, token) is { } book
                ? TypedResults.Ok(book)
                : TypedResults.NotFound();
        };

    private static readonly Func<Writings, BookKeepingContext, CancellationToken, Task<IResult>> GetByType
        = async (writingType, bookKeepingContext, token) =>
        {
            return writingType switch
            {
                Writings.NewsArticle => TypedResults.Ok(await bookKeepingContext.Writings.OfType<NewsArticle>().ToListAsync(token)),
                Writings.Ad => TypedResults.Ok(await bookKeepingContext.Writings.OfType<Ad>().ToListAsync(token)),
                Writings.JobOffer  => TypedResults.Ok(await bookKeepingContext.Writings.OfType<JobOffer>().ToListAsync(token)),
                _ => TypedResults.NotFound()
            };
        };

    /*

     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> DeleteHandler
        = async (id, bookKeepingContext, token) =>
        {
            var existingWriting = await bookKeepingContext.Writings.SingleOrDefaultAsync(b => b.Identity == id, token);

            if (existingWriting is {})
            {
                bookKeepingContext.Remove(existingWriting);
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(id);
            }

            return TypedResults.NotFound();
        };
}

public enum Writings
{
    NewsArticle = 1,
    Ad = 2,
    JobOffer = 3
}