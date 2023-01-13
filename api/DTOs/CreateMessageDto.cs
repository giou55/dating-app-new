namespace api.DTOs
{
    public class CreateMessageDto
    {
        // we've just got two properties,
        // so we don't need to create a mapping profile
        // to go from the CreateMessageDto to the Message
        public string RecipientUsername { get; set; }
        public string Content { get; set; }
    }
}