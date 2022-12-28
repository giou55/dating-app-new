using Microsoft.EntityFrameworkCore;

namespace api.Helpers
{
    // we want PagedList to work with any type of object or class,
    // so we give it a generic type
    public class PagedList<T> : List<T>
    {
        public PagedList
        (
            IEnumerable<T> items,
            int count,
            int pageNumber,
            int pageSize
        )
        {
            CurrentPage = pageNumber;
            TotalPages = (int) Math.Ceiling(count / (double) pageSize);
            PageSize = pageSize;
            TotalCount = count;
            // when we return this PagedList, 
            // we're going to return it with a list of our items
            AddRange(items);
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // we'll use this method inside our repository
        public static async Task<PagedList<T>> CreateAsync
        (
            IQueryable<T> source,
            int pageNumber,
            int pageSize
        )
        {
            // we execute two queries against our database 
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}