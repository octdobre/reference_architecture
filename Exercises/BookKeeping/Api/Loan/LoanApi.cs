using BookKeeping.Repo;
using Microsoft.EntityFrameworkCore;

namespace BookKeeping.Api.Loan;

/// <summary>
/// Example of Temporal Tables.
/// The history table contains only old rows and not the current row.
///
/// - Query for all history -> TemporalAll()
/// - Query for loan at specific time point -> TemporalAsOf()
/// - Query for all loans in the time span without upper bound-> TemporalFromTo()
/// - Query for all loans that where active in the time span -> TemporalBetween()
/// - Query for all loans that started being active and ended being active between -> TemporalContainedIn()
///     Active will have time set to 9999 so be careful using this with active rows.
/// </summary>
public static class LoanApi
{
    private const string ResourceName = "Loan";

    private const string CreatePathName = $"Create{ResourceName}";

    public static void AddLoanApi(this WebApplication app)
    {
        var group = app.MapGroup($"/{ResourceName.ToLower()}")
            //groups endpoints like controllers
            .WithTags(ResourceName);

        group.MapPost("/", CreateHandler)
            .WithName(CreatePathName)
            .WithOpenApi()
            .Produces(StatusCodes.Status201Created);

        group.MapGet("/current/byReader/{id:guid}", GetByReader)
            .WithName($"Get{ResourceName}ByReader")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/current/byBook/{id:guid}", GetByBook)
            .WithName($"Get{ResourceName}ByBook")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}", PassOn)
            .WithName($"PassOn{ResourceName}")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        //get history of loans  TemporalAll()
        group.MapPut("/history/byBook", HistoryOfLoansByBook)
            .WithName($"HistoryOfLoans")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        //get who loaned at what time point  TemporalAsOf()
        group.MapPut("/history/whoLoanedWhen/{whenLoaned:datetime}", WhoLoanedWhen)
            .WithName($"WhoLoanedWhen")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        //get inside timespan
        group.MapPut("/{bookId:guid}/history/nobounds/start/{start:datetime}/end{end:datetime}", LoansInTimeSpanDontIncludeBounds)
            .WithName($"LoansHistoryBoundsNotIncluded")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        //get inside timespan
        group.MapPut("/{bookId:guid}/history/active/start/{start:datetime}/end{end:datetime}", LoadsActiveInTimeSpan)
            .WithName($"LoadsActiveInTimeSpan")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);


        group.MapPut("/{bookId:guid}/history/exact/start/{start:datetime}/end{end:datetime}", LoansExactlyInTimeRange)
            .WithName($"LoansExactlyInTimeRange")
            .WithOpenApi()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    /*
     *INSERT INTO [Loans] ([Identity], [BookId], [ReaderId])
      VALUES (@p0, @p1, @p2);
     */
    private static readonly Func<Domain.Loan, BookKeepingContext, LinkGenerator, CancellationToken, Task<IResult>> CreateHandler
        = async (loan, bookKeepingContext, linker, token) =>
        {
            bookKeepingContext.Loans.Add(loan);

            await bookKeepingContext.SaveChangesAsync(token);

            return TypedResults.Created($"{linker.GetPathByName(CreatePathName)}/{loan.Identity}", loan);
        };

    /*
     *SELECT TOP(2) [l].[Identity], [l].[BookId], [l].[ReaderId]
      FROM [Loans] AS [l]
      WHERE [l].[ReaderId] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByReader
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Loans
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.ReaderId == id, token) is { } loan
                ? TypedResults.Ok(loan)
                : TypedResults.NotFound();
        };

    /*
    *SELECT TOP(2) [l].[Identity], [l].[BookId], [l].[ReaderId]
     FROM [Loans] AS [l]
     WHERE [l].[BookId] = @__id_0
    */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> GetByBook
        = async (id, bookKeepingContext, token) =>
        {
            return await bookKeepingContext.Loans
                .AsNoTracking()
                .SingleOrDefaultAsync(b => b.BookId == id, token) is { } loan
                ? TypedResults.Ok(loan)
                : TypedResults.NotFound();
        };

    /*

     */
    private static readonly Func<Guid, Domain.Loan, BookKeepingContext, CancellationToken, Task<IResult>> PassOn
        = async (id, updateLoan, bookKeepingContext, token) =>
        {
            if (await bookKeepingContext.Loans.SingleOrDefaultAsync(b => b.Identity == id, token) is { } loan)
            {
                loan.ReaderId = updateLoan.ReaderId;
                await bookKeepingContext.SaveChangesAsync(token);

                return TypedResults.Ok(loan);
            }

            return TypedResults.NotFound();
        };

    /*
     * SELECT [l].[BookId], [l].[ReaderId], [l].[PeriodStart] AS [LoanStart], [l].[PeriodEnd] AS [LoanEnd], [r].[FirstName] AS [ReaderFirstName]
       FROM [Loans] FOR SYSTEM_TIME ALL AS [l]
       INNER JOIN [Readers] AS [r] ON [l].[ReaderId] = [r].[Identity]
       WHERE [l].[BookId] = @__id_0
     */
    private static readonly Func<Guid, BookKeepingContext, CancellationToken, Task<IResult>> HistoryOfLoansByBook
        = async (id, bookKeepingContext, token) =>
        {
            var loansHistory = await bookKeepingContext
                .Loans
                .TemporalAll() //warning! rows are returned not tracked because all entities have the same primary key
                .Join(
                    bookKeepingContext.Readers,
                    loan => loan.ReaderId,
                    reader => reader.Identity,
                    (loan, reader) => new
                    {
                        BookId = loan.BookId,
                        ReaderId = loan.ReaderId,
                        LoanStart = EF.Property<DateTime>(loan, "PeriodStart"),
                        LoanEnd = EF.Property<DateTime>(loan, "PeriodEnd"),
                        ReaderFirstName = reader.FirstName
                    }
                )
                .Where(b => b.BookId == id)
                .OrderByDescending(l => l.LoanStart)  // Order where the last loaner is first on top
                .ToListAsync(token);

            return TypedResults.Ok(loansHistory);
        };

    /*
     * SELECT TOP(2) [l].[BookId], [l].[ReaderId], [l].[PeriodStart] AS [LoanStart], [l].[PeriodEnd] AS [LoanEnd], [r].[FirstName] AS [ReaderFirstName]
      FROM [Loans] FOR SYSTEM_TIME AS OF '2023-11-11T15:59:39.2817663' AS [l]
      INNER JOIN [Readers] AS [r] ON [l].[ReaderId] = [r].[Identity]
     */
    private static readonly Func<DateTime, BookKeepingContext, CancellationToken, Task<IResult>> WhoLoanedWhen
        = async (whenLoaned, bookKeepingContext, token) =>
        {
            var loansHistory = await bookKeepingContext
                .Loans
                .TemporalAsOf(whenLoaned)
                .Join(
                    bookKeepingContext.Readers,
                    loan => loan.ReaderId,
                    reader => reader.Identity,
                    (loan,
                        reader) => new
                        {
                            BookId = loan.BookId,
                            ReaderId = loan.ReaderId,
                            LoanStart = EF.Property<DateTime>(loan, "PeriodStart"),
                            LoanEnd = EF.Property<DateTime>(loan, "PeriodEnd"),
                            ReaderFirstName = reader.FirstName
                        }
                )
                .SingleOrDefaultAsync(token);

            return TypedResults.Ok(loansHistory);
        };

    /*
     * SELECT [l].[BookId], [l].[ReaderId], [l].[PeriodStart] AS [LoanStart], [l].[PeriodEnd] AS [LoanEnd], [r].[FirstName] AS [ReaderFirstName]
      FROM [Loans] FOR SYSTEM_TIME FROM '2023-11-11T16:07:32.1220365' TO '2023-11-11T16:07:32.1426566' AS [l]
      INNER JOIN [Readers] AS [r] ON [l].[ReaderId] = [r].[Identity]
      WHERE [l].[BookId] = @__id_0
      ORDER BY [l].[PeriodStart] DESC
     */
    private static readonly Func<Guid, DateTime, DateTime, BookKeepingContext, CancellationToken, Task<IResult>> LoansInTimeSpanDontIncludeBounds
        = async (bookId, start, end, bookKeepingContext, token) =>
        {
            var loansHistory = await bookKeepingContext
                .Loans
                .TemporalFromTo(start, end)
                .Join(
                    bookKeepingContext.Readers,
                    loan => loan.ReaderId,
                    reader => reader.Identity,
                    (loan,
                        reader) => new
                        {
                            BookId = loan.BookId,
                            ReaderId = loan.ReaderId,
                            LoanStart = EF.Property<DateTime>(loan, "PeriodStart"),
                            LoanEnd = EF.Property<DateTime>(loan, "PeriodEnd"),
                            ReaderFirstName = reader.FirstName
                        }
                )
                .Where(b => b.BookId == bookId)
                .OrderByDescending(l => l.LoanStart)  // Order where the last loaner is first on top
                .ToListAsync(token);

            return TypedResults.Ok(loansHistory);
        };

    /***
     * SELECT [l].[BookId], [l].[ReaderId], [l].[PeriodStart] AS [LoanStart], [l].[PeriodEnd] AS [LoanEnd], [r].[FirstName] AS [ReaderFirstName]
      FROM [Loans] FOR SYSTEM_TIME BETWEEN '2023-11-11T16:25:49.4153651' AND '2023-11-11T16:25:49.4316909' AS [l]
      INNER JOIN [Readers] AS [r] ON [l].[ReaderId] = [r].[Identity]
      WHERE [l].[BookId] = @__bookId_0
      ORDER BY [l].[PeriodStart] DESC
     */
    private static readonly Func<Guid, DateTime, DateTime, BookKeepingContext, CancellationToken, Task<IResult>> LoadsActiveInTimeSpan
        = async (bookId, start, end, bookKeepingContext, token) =>
        {
            var loansHistory = await bookKeepingContext
                .Loans
                .TemporalBetween(start, end)
                .Join(
                    bookKeepingContext.Readers,
                    loan => loan.ReaderId,
                    reader => reader.Identity,
                    (loan,
                        reader) => new
                    {
                        BookId = loan.BookId,
                        ReaderId = loan.ReaderId,
                        LoanStart = EF.Property<DateTime>(loan, "PeriodStart"),
                        LoanEnd = EF.Property<DateTime>(loan, "PeriodEnd"),
                        ReaderFirstName = reader.FirstName
                    }
                )
                .Where(b => b.BookId == bookId)
                .OrderByDescending(l => l.LoanStart)  // Order where the last loaner is first on top
                .ToListAsync(token);

            return TypedResults.Ok(loansHistory);
        };

    /*
     * SELECT [l].[BookId], [l].[ReaderId], [l].[PeriodStart] AS [LoanStart], [l].[PeriodEnd] AS [LoanEnd], [r].[FirstName] AS [ReaderFirstName]
      FROM [Loans] FOR SYSTEM_TIME CONTAINED IN ('2023-11-11T16:25:49.4153651', '2023-12-11T17:27:49.4316909') AS [l]
      INNER JOIN [Readers] AS [r] ON [l].[ReaderId] = [r].[Identity]
      WHERE [l].[BookId] = @__bookId_0
      ORDER BY [l].[PeriodStart] DESC
     */
    private static readonly Func<Guid, DateTime, DateTime, BookKeepingContext, CancellationToken, Task<IResult>> LoansExactlyInTimeRange
        = async (bookId, start, end, bookKeepingContext, token) =>
        {
            var loansHistory = await bookKeepingContext
                .Loans
                .TemporalContainedIn(start, end)
                .Join(
                    bookKeepingContext.Readers,
                    loan => loan.ReaderId,
                    reader => reader.Identity,
                    (loan,
                        reader) => new
                    {
                        BookId = loan.BookId,
                        ReaderId = loan.ReaderId,
                        LoanStart = EF.Property<DateTime>(loan, "PeriodStart"),
                        LoanEnd = EF.Property<DateTime>(loan, "PeriodEnd"),
                        ReaderFirstName = reader.FirstName
                    }
                )
                .Where(b => b.BookId == bookId)
                .OrderByDescending(l => l.LoanStart)  // Order where the last loaner is first on top
                .ToListAsync(token);

            return TypedResults.Ok(loansHistory);
        };
}
