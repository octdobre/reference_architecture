namespace BookKeeping.Domain;

public class Author
{
    public Guid Identity { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public uint? YearOfBirth { get; set; }

    public string? Nationality { get; set; }

    /*
     * Author - Books     - One to Many
     * Author - Address   - One to Many
     * Author - Publisher - Many to Many
     */
    #region Relationships

    /// <summary>
    /// Navigation property to Books.
    /// One-to-Many.
    /// Books cannot exist without Author.
    /// https://www.learnentityframeworkcore.com/conventions/one-to-many-relationship
    /// </summary>
    public ICollection<Book> Books { get; set; } = new List<Book>();

    /// <summary>
    /// Navigation property to Address.
    /// One-to-Many.
    /// Address can exist without Author.
    /// </summary>
    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    /// <summary>
    /// Navigation property to Publishers.
    /// Many-to-Many.
    /// Both Authors and Publishers can exist independently.
    /// </summary>
    public ICollection<Publisher> Publishers { get; set; } = new List<Publisher>();

    #endregion
}