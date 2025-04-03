namespace CurrencyConverter.Application.Models
{
    public class CurrencyConvertReq
    {

        public string FromCurrency { get; set; }
        public string ToCurrency { get; set; }
        public decimal Amount { get; set; }
    }
}
