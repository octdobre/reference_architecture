namespace BookKeeping.Domain;

public class Loan
{
    public Guid Identity { get; set; }

    public Guid BookId { get; set; }

    public Guid ReaderId { get; set; }
    public Reader Reader { get; set; }
}