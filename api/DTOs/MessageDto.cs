namespace api.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public string SenderUsername { get; set; }

        // we cannot expect automapper to workout with SenderPhotoUrl on our behalf,
        // so we need to add configuration in AutoMapperProfiles.cs file
        public string SenderPhotoUrl { get; set; }

        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }

        // we cannot expect automapper to workout with RecipientPhotoUrl on our behalf,
        // so we need to add configuration in AutoMapperProfiles.cs file
        public string RecipientPhotoUrl { get; set; }

        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; }
    }
}