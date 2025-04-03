using CurrencyConverter.Application.Models;

namespace CurrencyConverter.Application.Interfaces
{
    public interface ICurrencyConverter
    {
        Task<Dictionary<string, decimal>> GetLatestRates(string baseCurrency);
        Task<decimal> ConvertCurrency(CurrencyConvertReq request);
        Task<Dictionary<DateTime, Dictionary<string, decimal>>> GetHistoricalRates(string baseCurrency, DateTime startDate, DateTime endDate, int page, int pageSize);

    }
}
