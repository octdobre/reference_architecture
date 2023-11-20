namespace BookKeeping.Domain;

public class Editor
{
    public Guid Identity { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    #region Relationships

    /// <summary>
    /// One-to-One relationship.
    /// Mandatory for one-to-one relationships with conventions.
    /// https://www.learnentityframeworkcore.com/conventions/one-to-one-relationship
    /// </summary>
    public Guid? PublisherId { get; set; }

    /// <summary>
    /// One-to-One relationship.
    /// If only this field then FK is a shadow property.
    /// Mandatory field with entity configuration.
    /// Optional one to one.
    /// </summary>
    public Publisher? Publisher { get; set; }

    #endregion
}