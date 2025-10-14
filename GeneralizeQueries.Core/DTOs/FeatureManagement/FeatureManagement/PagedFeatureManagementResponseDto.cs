using System.Text.Json.Serialization;
using GeneralizeQueries.Core.Models;

namespace GeneralizeQueries.Api.DTOs.FeatureManagement;

/// <summary>
///     Paginated response DTO for feature management
/// </summary>
public class PagedFeatureManagementResponseDto<T>
{
    [JsonPropertyName("items")] public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

    [JsonPropertyName("page")] public int Page { get; set; }

    [JsonPropertyName("pageSize")] public int PageSize { get; set; }

    [JsonPropertyName("totalCount")] public long TotalCount { get; set; }

    [JsonPropertyName("totalPages")] public int TotalPages { get; set; }

    [JsonPropertyName("hasPreviousPage")] public bool HasPreviousPage { get; set; }

    [JsonPropertyName("hasNextPage")] public bool HasNextPage { get; set; }

    [JsonPropertyName("count")] public int Count { get; set; }

    /// <summary>
    ///     Creates a paginated response from a PagedResult
    /// </summary>
    public static PagedFeatureManagementResponseDto<T> FromPagedResult(PagedResult<T> pagedResult)
    {
        return new PagedFeatureManagementResponseDto<T>
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