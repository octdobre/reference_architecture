namespace BookKeeping.Domain;

public class Reservation
{
    public Guid Identity { get; set; }

    public Guid BookId { get; set; }

    public Guid ReaderId { get; set; }
}