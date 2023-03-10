namespace api.DTOs
{
    public class UserDto
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string PhotoUrl { get; set; } // will be sent only on loggin
        public string KnownAs { get; set; }
        public string Gender { get; set; }
    }
}
