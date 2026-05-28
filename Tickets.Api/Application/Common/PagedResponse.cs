namespace Tickets.Api.Application.Common;

public class PagedResponse<T>
{
    public IReadOnlyCollection<T> Items { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalItems { get; init; }

    public int TotalPages { get; init; }

    public bool HasPreviousPage => Page > 1;

    public bool HasNextPage => Page < TotalPages;

    public static PagedResponse<T> Create(IReadOnlyCollection<T> items, int page, int pageSize, int totalItems)
    {
        var safePageSize = pageSize <= 0 ? 10 : pageSize;
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)safePageSize);

        return new PagedResponse<T>
        {
            Items = items,
            Page = page,
            PageSize = safePageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
