using System.ComponentModel.DataAnnotations.Schema;

namespace api.Entities
{
    [Table("Photos")]
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public bool IsMain { get; set; }
        public bool IsApproved { get; set; }
        public string PublicId { get; set; }
        public int AppUserId { get; set; } // so every photo must related to a user
        public AppUser AppUser { get; set; } // so every photo must related to a user
    }
}
