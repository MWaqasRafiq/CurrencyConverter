using static CurrencyConverter.Application.Common.Enumerator;

namespace CurrencyConverter.Application.Interfaces
{
    public interface ICurrencyProviderFactory
    {
        ICurrencyConverter GetProvider(Providers ProviderName);
    }
}
