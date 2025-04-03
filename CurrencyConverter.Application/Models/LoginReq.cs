using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Application.Models
{
    public class LoginReq
    {
        public string UserName { get; set; }
        public string Password { get; set; }

    }
}
