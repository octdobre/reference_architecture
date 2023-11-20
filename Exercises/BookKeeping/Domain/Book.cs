namespace BookKeeping.Domain;

public class Book
{
    public Guid? Identity { get; set; }

    public string? Title { get; set; }

    public string? Afterword { get; set; }

    #region Relationships

    /// <summary>
    /// Foreign key.
    /// Optional.
    /// If not present replaced with shadow property.
    /// One-to-Many.
    ///
    /// This defines a fully formed relationship.
    /// A book cannot be created without an Author.
    /// https://www.learnentityframeworkcore.com/conventions/one-to-many-relationship
    /// </summary>
    public Guid AuthorId { get; set; }

    /// <summary>
    /// Reference navigation property
    /// or
    /// Inverse navigation property
    ///
    /// Optional.
    /// One-to-Many.
    /// The relationship is considered to be fully defined.
    /// The referential constraint will be specified as Cascade on delete.
    /// https://www.learnentityframeworkcore.com/conventions/one-to-many-relationship
    /// </summary>
    public Author Author { get; set; }

    public Reservation Reservation { get; set; }

    public BookContent? Content { get; set; }

    public Loan? Loan { get; set; }

    #endregion
}
