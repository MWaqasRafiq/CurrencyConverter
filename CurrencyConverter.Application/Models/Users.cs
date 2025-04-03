using Newtonsoft.Json;

namespace CurrencyConverter.Application.Models
{
    public class Users
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }

        public List<Users> GetMockUsers()
        {
            return new List<Users> { new Users { Username = "admin", Password = "12345",Role="Admin" },
                                     new Users { Username = "guest", Password = "12345",Role="Guest" }};
        }
    }
}
