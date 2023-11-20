namespace BookKeeping.Common;

public record Page<T>(IEnumerable<T> Items, int Total, int PageNumber);
