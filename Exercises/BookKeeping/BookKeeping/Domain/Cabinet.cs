namespace BookKeeping.Domain;

public class Cabinet
{
    public Guid Identity { get; init; }

    public string Category { get; set; }

    #region Relationships

    public List<Shelf> Shelves { get; init; } = new ();

    #endregion

    public Cabinet()
    {

    }

    public Cabinet(string category)
    {
        Identity = Guid.NewGuid();

        Category = !string.IsNullOrEmpty(category) ? category : throw new ArgumentException("Category cannot be empty.", nameof(category));
    }

    public void AddShelf(Shelf shelf)
    {
        if (Shelves.Any(s => s.RowNumber == shelf.RowNumber))
        {
            throw new ArgumentException("Shelf with row number already exists.", nameof(shelf.RowNumber));
        }

        Shelves.Add(shelf);
    }

    public void RemoveShelf(Shelf shelf)
    {
        if (Shelves.SingleOrDefault(s => s.RowNumber == shelf.RowNumber) is { } existingShelf)
        {
            Shelves.Remove(existingShelf);
        }
        else
        {
            throw new ArgumentException("Shelf with row number does not exist.", nameof(shelf.RowNumber));
        }
    }
}