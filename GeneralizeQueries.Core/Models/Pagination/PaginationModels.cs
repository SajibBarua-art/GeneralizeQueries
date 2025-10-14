namespace GeneralizeQueries.Core.Models;

/// <summary>
///     Represents pagination parameters for querying data
/// </summary>
public class PaginationParameters
{
    /// <summary>
    ///     Page number (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    ///     Number of items per page
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    ///     Field to sort by
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    ///     Sort direction (asc/desc)
    /// </summary>
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    ///     Validates pagination parameters and sets defaults
    /// </summary>
    public void ValidateAndSetDefaults(int maxPageSize = 100)
    {
        if (Page < 1) Page = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > maxPageSize) PageSize = maxPageSize;
        if (string.IsNullOrWhiteSpace(SortDirection) ||
            (!SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase) &&
             !SortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase)))
            SortDirection = "asc";
    }
}

/// <summary>
///     Represents a paged result with metadata
/// </summary>
/// <typeparam name="T">Type of items in the result</typeparam>
public class PagedResult<T>
{
    /// <summary>
    ///     The items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    ///     Current page number (1-based)
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    ///     Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    ///     Total number of items across all pages
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    ///     Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    ///     Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    ///     Whether there is a next page
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    ///     Number of items in the current page
    /// </summary>
    public int Count => Items.Count();

    /// <summary>
    ///     Creates a new PagedResult instance
    /// </summary>
    public static PagedResult<T> Create(
        IEnumerable<T> items,
        int page,
        int pageSize,
        long totalCount)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}