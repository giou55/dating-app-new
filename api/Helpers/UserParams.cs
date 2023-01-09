namespace api.Helpers
{
  // the API will receive from the client the UserParams as a query string
    public class UserParams : PaginationParams
    {
      // here we specify the properties that we need in addition to the PaginationParams
      
      public string CurrentUsername { get; set; }
      public string Gender { get; set; }
      public int MinAge { get; set; } = 18;
      public int MaxAge { get; set; } = 100;
      public string OrderBy { get; set; } = "lastActive";
    }
}