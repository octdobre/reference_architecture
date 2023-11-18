namespace BookKeeping.Domain;

public class Reader
{
    public Guid Identity { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    #region Relationships

    public Reservation Reservation { get; set; }

    public Loan? Loan { get; set; }

    #endregion
}