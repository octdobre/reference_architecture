namespace BookKeeping.Domain;

public class Customer
{
    public Guid Identity { get; set; }

    public string? Name { get; set; }

    public ContactDetails? ContactAndPayment { get; set; }
}