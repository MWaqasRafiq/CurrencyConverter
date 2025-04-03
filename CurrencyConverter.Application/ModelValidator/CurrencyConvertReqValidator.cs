using FluentValidation;
using CurrencyConverter.Application.Models;

public class CurrencyConvertReqValidator : AbstractValidator<CurrencyConvertReq>
{
    public CurrencyConvertReqValidator()
    {
        RuleFor(x => x.FromCurrency)
            .NotEmpty().WithMessage("FromCurrency is required.")
            .Length(3).WithMessage("FromCurrency must be 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("FromCurrency must be an uppercase ISO 4217 currency code.");

        RuleFor(x => x.ToCurrency)
            .NotEmpty().WithMessage("ToCurrency is required.")
            .Length(3).WithMessage("ToCurrency must be 3 characters.")
            .Matches("^[A-Z]{3}$").WithMessage("ToCurrency must be an uppercase ISO 4217 currency code.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(1_000_000).WithMessage("Amount cannot exceed 1,000,000.");
    }
}