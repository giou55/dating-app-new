using Microsoft.Extensions.Localization;

namespace api.Entities
{
    public class Message
    {
        public int Id { get; set; }

        // automapper is smart enough to know how to populate the SenderId
        // based on AppUser Sender
        public int SenderId { get; set; }

        // automapper is also smart enough to know how to populate the SenderUsername 
        // based on AppUser Sender
        public string SenderUsername { get; set; }

        public AppUser Sender { get; set; }
        public int RecipientId { get; set; }
        public string RecipientUsername { get; set; }
        public AppUser Recipient{ get; set; }
        public string Content { get; set; }
        public DateTime? DateRead { get; set; }
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;

        // check if a sender user has deleted the message
        public bool SenderDeleted { get; set; }

        // check if a recipient user has deleted the message
        public bool RecipientDeleted { get; set; }

        // only when both of above are true, we will actually delete the message from database
    }
}