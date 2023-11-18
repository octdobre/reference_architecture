namespace BookKeeping.Domain;

public class Writing
{
    public Guid Identity { get; set; }

    public string Title { get; set; }

    public DateTime PublishedDate { get; set; }

    public string Content { get; set; }
}

public class NewsArticle : Writing
{
    public string Source { get; set; }
}

public class Ad : Writing
{
    public string Advertiser { get; set; }
}

public class JobOffer : Writing
{
    public string Company { get; set; }

}