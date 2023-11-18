using BookKeeping.Domain;

namespace BookKeeping.Repo;

public class DataSeeder
{
    public static void AddSeed(BookKeepingContext context)
    {
        var authJVerne = new Author
        {
            Identity = Guid.NewGuid(),
            FirstName = "Jules",
            LastName = "Verne",
            Nationality = "French",
            YearOfBirth = 1828,
            Books = new List<Book>
            {
                new()
                {
                    Identity = Guid.NewGuid(),
                    Title = "Journey To The Centre Of The Earth",
                    Afterword = "Widely known"
                },
                new()
                {
                    Identity = Guid.NewGuid(),
                    Title = "Twenty eight leagues under the sea",
                    Afterword = "Widely known"
                }
            }
        };

        context.Authors.Add(authJVerne);

        var addrBlvdLong = new Address
        {
            Identity = Guid.NewGuid(),
            City = "Amiens",
            Street = "Boulevard Longueville",
            Number = "44",
            AuthorId = authJVerne.Identity
        };

        var addrRueOlivier = new Address
        {
            Identity = Guid.NewGuid(),
            City = "Nantes",
            Street = "Rue Olivier-de-Clisson",
            Number = "4",
            AuthorId = authJVerne.Identity
        };

        context.Addresses.AddRange(addrBlvdLong, addrRueOlivier);

        var publshGas = new Publisher
        {
            Identity = Guid.NewGuid(),
            Name = "Gaslight works"
        };

        publshGas.Authors.Add(authJVerne);

        var edtMark = new Editor
        {
            Identity = Guid.NewGuid(),
            FirstName = "Mark",
            LastName = "Bill",
            PublisherId = publshGas.Identity
        };

        context.Publishers.Add(publshGas);
        context.Editors.Add(edtMark);

        var rdrJack = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Jack",
            LastName = "Black"
        };

        var rdrJohn = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Johnson",
            LastName = "Harry"
        };

        var rdrMarr = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Mary",
            LastName = "Jane"
        };

        context.Readers.AddRange(rdrJack, rdrJohn, rdrMarr);

        var resJverne = new Reservation
        {
            Identity = Guid.NewGuid(),
            BookId = authJVerne.Books.First().Identity!.Value,
            ReaderId = rdrJack.Identity
        };

        context.Reservations.AddRange(resJverne);


        var bookContentJToCenter = new BookContent
        {
            Identity = Guid.NewGuid(),
            Content =
                "Go down into the crater of Snaefells Jökull, which Scartaris's shadow caresses " +
                "just before the calends of July, O daring traveler, and you'll make it to the " +
                "center of the earth. I've done so. Arne Saknussemm"
        };

        var journeyBookRef = authJVerne.Books.First();

        context.BookContents.Add(bookContentJToCenter);
        context.Entry(bookContentJToCenter).Property("BookId").CurrentValue = journeyBookRef.Identity;

        var readerMark = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Mark",
            LastName = "Baker"
        };

        var readerPaula = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Paula",
            LastName = "Tris"
        };

        var readerShani = new Reader
        {
            Identity = Guid.NewGuid(),
            FirstName = "Shani",
            LastName = "Vince"
        };

        context.Readers.AddRange(readerMark, readerPaula, readerShani);

        var loan = new Loan
        {
            Identity = Guid.NewGuid(),
            BookId = journeyBookRef.Identity!.Value,
            ReaderId = readerShani.Identity
        };

        context.SaveChanges();

        context.Loans.Add(loan);
        context.SaveChanges();

        loan.ReaderId = readerPaula.Identity;
        context.Update(loan);
        context.SaveChanges();

        loan.ReaderId = readerMark.Identity;
        context.Update(loan);
        context.SaveChanges();

    }
}