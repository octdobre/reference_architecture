namespace BookKeeping.Domain;

public class Address
{
    public Guid Identity { get; set; }

    public string City { get; set; } = string.Empty;

    public string Street { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    #region RelationShips

    /// <summary>
    /// Optional relationship.
    /// An address can exist without an author.
    ///
    /// Mandatory field.
    /// </summary>
    public Guid? AuthorId { get; set; }

    /// <summary>
    /// When there is no Foreign key or Inverse navigation property
    /// the Foreign Key is nullable
    /// An address can exist without an author.
    /// </summary>
    public Author? Author { get; set; }

    #endregion
}