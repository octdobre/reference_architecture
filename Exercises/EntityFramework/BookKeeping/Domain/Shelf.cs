namespace BookKeeping.Domain;

/// <summary>
/// Owned type does not require a primary key.
/// You cannot create a DbSet<T> for an owned type.
/// You cannot call Entity<T>() with an owned type on ModelBuilder.
/// </summary>
public class Shelf
{
    public Shelf(int rowNumber)
    {
        RowNumber = rowNumber > 0 ? rowNumber : throw new ArgumentException("Row number cannot be lower thant 0;");
    }

    public int RowNumber { get; init; }
}