namespace BookKeeping.Domain;

public class Publisher
{
    public Guid? Identity { get; set; }

    public string? Name { get; set; }

    #region Relationships

    /// <summary>
    /// Navigation property to Authors.
    /// Many-to-Many relationship.
    /// </summary>
    public ICollection<Author> Authors { get; set; } = new List<Author>();

   /// <summary>
    /// One-to-One relationship.
    /// FK is a shadow property.
    /// Mandatory.
    /// </summary>
    public Editor? Editor { get; set; }

    #endregion
}