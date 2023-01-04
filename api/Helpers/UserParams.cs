namespace api.Helpers
{
  // the API will receive from the client the UserParams as a query string
    public class UserParams
    {
      private const int MaxPageSize = 50;  

      // if the client want to get users with no query string,
      // then our API is going to use these default values 
      public int PageNumber { get; set; } = 1;
      private int _pageSize = 10;
      public int PageSize
      {
        // get { return _pageSize; }
        // set { 
        //     if (value > MaxPageSize) {
        //         _pageSize = MaxPageSize;
        //     } else {
        //         _pageSize = value;
        //     }
        // }

        // we use arrows
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
      }

      public string CurrentUsername { get; set; }
      public string Gender { get; set; }
      public int MinAge { get; set; } = 18;
      public int MaxAge { get; set; } = 100;
    }
}