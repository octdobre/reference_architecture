namespace BookKeeping.Domain;

public class CreditCard
{
    public string Serial { get; set; }
    public DateTime Expiration { get; set; }
    public string Name { get; set; }
    public string CVC { get; set; }

}