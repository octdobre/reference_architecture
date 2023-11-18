namespace BookKeeping.Domain;

public class BankDetails
{
    public string Name { get; set; }

    public string IBAN { get; set; }

    public string BIC { get; set; }

    public ICollection<CreditCard> CreditCards { get; set; }
}