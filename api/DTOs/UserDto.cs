namespace api.DTOs
{
    // when the user logs in or register
    // these properties of UserDto will be sent by the API
    // and will be stored inside local storage of the browser
    public class UserDto
    {
        public string Username { get; set; }
        public string Token { get; set; }
        public string PhotoUrl { get; set; } // will be sent only on loggin
        public string KnownAs { get; set; }
        public string Gender { get; set; }
    }
}
