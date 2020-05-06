namespace JWT_AuthenticationAndSwagger_NET.Core3._1.Models
{
#nullable disable
    public class UserModel
    {
        public long Id { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Region { get; set; }
        public string City { get; set; }
        public string[] Roles { get; set; }
    }
}