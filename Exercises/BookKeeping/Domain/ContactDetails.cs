namespace BookKeeping.Domain;

public class ContactDetails
{
    public BankDetails BankDetails { get; set; } = null!;

    public string? Phone { get; set; }

}