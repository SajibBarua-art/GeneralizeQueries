using System.Text.Json.Serialization;
using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Api.DTOs.QueryTemplate;

/// <summary>
///     DTO for paged query template response
/// </summary>
/// <typeparam name="T">Type of items in the response</typeparam>
public class PagedQueryTemplateResponseDto<T>
{
    /// <summary>
    ///     The items for the current page
    /// </summary>
    [JsonPropertyName("items")]
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    /// <summary>
    ///     Current page number (1-based)
    /// </summary>
    [JsonPropertyName("page")]
    public int Page { get; set; }

    /// <summary>
    ///     Number of items per page
    /// </summary>
    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    /// <summary>
    ///     Total number of items across all pages
    /// </summary>
    [JsonPropertyName("totalCount")]
    public long TotalCount { get; set; }

    /// <summary>
    ///     Total number of pages
    /// </summary>
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    /// <summary>
    ///     Whether there is a previous page
    /// </summary>
    [JsonPropertyName("hasPreviousPage")]
    public bool HasPreviousPage { get; set; }

    /// <summary>
    ///     Whether there is a next page
    /// </summary>
    [JsonPropertyName("hasNextPage")]
    public bool HasNextPage { get; set; }

    /// <summary>
    ///     Number of items in the current page
    /// </summary>
    [JsonPropertyName("count")]
    public int Count { get; set; }

    /// <summary>
    ///     Creates a PagedQueryTemplateResponseDto from a PagedResult
    /// </summary>
    public static PagedQueryTemplateResponseDto<T> FromPagedResult(PagedResult<T> pagedResult)
    {
        return new PagedQueryTemplateResponseDto<T>
        {
            Items = pagedResult.Items,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize,
            TotalCount = pagedResult.TotalCount,
            TotalPages = pagedResult.TotalPages,
            HasPreviousPage = pagedResult.HasPreviousPage,
            HasNextPage = pagedResult.HasNextPage,
            Count = pagedResult.Count
        };
    }
}