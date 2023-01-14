namespace api.Helpers
{
    public class MessageParams : PaginationParams
    {
        public string Username { get; set; }
        public string Container { get; set; } = "Unread"; // return the unread messages by default
    }
}