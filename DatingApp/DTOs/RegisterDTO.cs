namespace DatingApp.DTOs
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string KnownAs { get; set; }
        public string Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Password { get; set; }

    }
}
