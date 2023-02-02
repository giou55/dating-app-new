namespace api.Entities
{
    public class Connection
    {
        // to satisfy Entity framework when it creates the schema for our database,
        // we have to give this an empty constructor with no parameters
        public Connection() {}

        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}