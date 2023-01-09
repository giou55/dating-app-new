namespace api.Helpers
{
    public class LikesParams : PaginationParams
    {
       // here we specify the properties that we need in addition to the PaginationParams

       public int UserId { get; set; }
       public string Predicate { get; set; }
        
    }
}