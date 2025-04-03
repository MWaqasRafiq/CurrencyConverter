using CurrencyConverter.Application.Interfaces;
using static CurrencyConverter.Application.Common.Enumerator;

namespace CurrencyConverter.Infrastructure.Services
{
    public class CurrencyProviderFactory : ICurrencyProviderFactory
    {
        private readonly ICurrencyConverter _currencyConverter;
        public CurrencyProviderFactory(ICurrencyConverter currencyConverter)
        {
            _currencyConverter = currencyConverter;
        }
        public ICurrencyConverter GetProvider(Providers ProviderName)
        {
            if (ProviderName == Providers.frankfurter)
            {
                return _currencyConverter;
            }
            throw new NotImplementedException() ;
        }
    }
}
