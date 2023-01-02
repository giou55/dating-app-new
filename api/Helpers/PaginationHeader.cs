namespace api.Helpers
{
    // this is an object that we'll return inside the HTTP response headers
    public class PaginationHeader
    {
        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }

        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}

// Pagination header sent to client will have a value like this:
// {"currentPage":1,"itemsPerPage":10,"totalItems":24,"totalPages":3}