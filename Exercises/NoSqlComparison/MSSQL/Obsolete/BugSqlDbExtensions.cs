using Microsoft.EntityFrameworkCore;
using static NoSqlComparison.MSSQL.BugSqlDb;

namespace NoSqlComparison.MSSQL.Obsolete;

/// <summary>
/// This class represents an example of extension methods ready to use in controllers.
/// </summary>
[Obsolete]
public class BugSqlDbExtensions
{
    public static async Task<CrudActions.GetBug?>
    CreateBug(BugSqlDb bugsDb, Guid id, CrudActions.CreateBug createBug, CancellationToken token)
    {
        var newBug = new BugRecord(
            id,
            createBug.Title,
            createBug.Description,
            createBug.ReportTime);

        bugsDb.Bugs.Add(newBug);

        try
        {
            await bugsDb.SaveChangesAsync(token);
        }
        catch (Exception ex)
        {
            return null;
        }
        return new CrudActions.GetBug(newBug.Id, newBug.Title, newBug.Description, newBug.ReportTime);
    }

    public static async Task<CrudActions.GetBug?>
        GetBug(BugSqlDb bugsDb, Guid id, CancellationToken token)
    {
        return await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is { } bug
            ? new CrudActions.GetBug(bug.Id, bug.Title, bug.Description, bug.ReportTime)
            : null;
    }

    public static async Task<CrudActions.Page?>
        GetPagedBugs(BugSqlDb bugsDb, int skip, int take, bool sortByTitle, CancellationToken token)
    {
        IQueryable<BugRecord> query = bugsDb.Bugs;

        var totalItems = await query.CountAsync(token);

        query = sortByTitle
            ? query.OrderBy(b => b.Title)
            : query.OrderByDescending(b => b.Title);

        var items = await query.Skip(skip).Take(take).ToListAsync(token);

        return items.Any()
            ? new CrudActions.Page(
                items.Select(b => new CrudActions.GetBug(b.Id, b.Title, b.Description, b.ReportTime)),
                totalItems)
            : null;
    }

    public static async Task<CrudActions.GetBug?>
        UpdateBug(BugSqlDb bugsDb, Guid id, CrudActions.UpdateBug updateBug, CancellationToken token)
    {
        if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is not { } bug)
            return null;

        var updatedBug = bug with
        {
            Title = updateBug.Title ?? bug.Title,
            Description = updateBug.Description ?? bug.Description
        };

        bugsDb.Update(updatedBug);

        await bugsDb.SaveChangesAsync(token);

        return new CrudActions.GetBug(updatedBug.Id, updatedBug.Title, updatedBug.Description, updatedBug.ReportTime);
    }

    public static async Task<Guid?>
        DeleteBug(BugSqlDb bugsDb, Guid id, CancellationToken token)
    {

        if (await bugsDb.Bugs.SingleOrDefaultAsync(b => b.Id == id, token) is not { } bug)
            return null;

        var deletedResourceIdentity = bug.Id;

        bugsDb.Bugs.Remove(bug);

        await bugsDb.SaveChangesAsync(token);

        return deletedResourceIdentity;
    }
}

