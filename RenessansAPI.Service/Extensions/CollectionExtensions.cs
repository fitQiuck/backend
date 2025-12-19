using Microsoft.EntityFrameworkCore;
using RenessansAPI.Domain.Configurations;

namespace RenessansAPI.Service.Extensions;

public static class CollectionExtensions
{
    public static async Task<PagedResult<T>> ToPagedListAsync<T>(
            this IQueryable<T> source,
            PaginationParams @params)
    {
        if (@params.PageIndex <= 0) @params.PageIndex = 1;
        if (@params.PageSize <= 0) @params.PageSize = 10;

        var totalItems = await source.CountAsync();

        var items = await source
            .Skip((@params.PageIndex - 1) * @params.PageSize)
            .Take(@params.PageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)@params.PageSize);

        return new PagedResult<T>
        {
            Data = items,
            TotalItems = totalItems,
            TotalPages = totalPages,
            CurrentPage = @params.PageIndex,
            PageSize = @params.PageSize
        };
    }
}
