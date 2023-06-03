using Laboratorium2.Interfejsy;
using Laboratorium3.Interfaces;
using Laboratorium3.Models;
using ProjektFinalny.Constants;
using ProjektFinalny.Extensions;
using ProjektFinalny.Interfaces;

namespace ProjektFinalny.Processors;

public class CurrencyProcessor : ICurrencyProcessor
{
    private readonly IUserInputHandler _userInputHandler = default!;
    private readonly ICurrencyWebClient _currencyWebClient = default!;
    private readonly IUserLogger _userLogger = default!;
    private readonly List<string> _currencies = new List<string>();

    public CurrencyProcessor(IUserInputHandler userInputHandler,
        ICurrencyWebClient currencyWebClient,
        IUserLogger userLogger)
    {
        _userInputHandler = userInputHandler;
        _currencyWebClient = currencyWebClient;
        _userLogger = userLogger;
        _currencies = _currencyWebClient.GetAllCurrencies();
    }

    public void Process()
    {
        var input = _userInputHandler.GetUserInput(Messages.CurrencyOptions);

        switch (input)
        {
            case "1":
                ProcessLastCurrencyRate();
                break;
            case "2":
                ProcessCurrencyRates();
                break;
            case "3":
                GetCurrencyRateFromImput();
                break;
            case "4":
                GetCurrencyTopRateFromLast30Days();
                break;
            case "5":
                ProcessCurrencyExchangeRate();
                break;
            default:
                break;
        }
    }

    private void ProcessLastCurrencyRate()
    {
        var currencyCode = _userInputHandler.GetUserInput(Messages.GetCurrencyCode).ToUpper();
        var formattedMessage = string.Empty;

        if (_currencies.Contains(currencyCode))
        {
            var currency = _currencyWebClient.GetCurrencyByCode(currencyCode);
            var actualCurrencyRate = currency.Rates.First();
            formattedMessage = string.Format(Messages.ActualCurrencyRate, currencyCode, actualCurrencyRate.Price,
                actualCurrencyRate.Date.ToString(ConstantValues.DateFormat));
        }
        else
        {
            formattedMessage = string.Format(Messages.NoSuchCurrency, currencyCode);
        }

        _userLogger.Log(formattedMessage);
    }

    private CurrencyDTO? GetCurrencyRateFromImput()
    {
        var currencyCode = _userInputHandler.GetUserInput(Messages.GetCurrencyCode).ToUpper();
        var formattedMessage = string.Empty;
        CurrencyDTO? actualCurrencyRate = null;

        if (_currencies.Contains(currencyCode))
        {
            actualCurrencyRate = _currencyWebClient.GetCurrencyByCode(currencyCode);
            formattedMessage = string.Format(Messages.ActualCurrencyRate, currencyCode, actualCurrencyRate.Rates.First().Price,
                actualCurrencyRate.Rates.First().Date.ToString(ConstantValues.DateFormat));
        }
        else if (currencyCode == "PLN")
        {
            actualCurrencyRate = new CurrencyDTO
            {
                Code = "PLN",
                Rates = new List<RateDTO>
                {
                    new RateDTO
                    {
                        Date = DateTime.Today,
                        Price = 1
                    }
                }
            };
            formattedMessage = string.Format(Messages.ActualCurrencyRate, currencyCode, actualCurrencyRate.Rates.First().Price,
                actualCurrencyRate.Rates.First().Date.ToString(ConstantValues.DateFormat));
        }
        else
        {
            formattedMessage = string.Format(Messages.NoSuchCurrency, currencyCode);
        }
        _userLogger.Log(formattedMessage);
        return actualCurrencyRate;
    }

    private void ProcessCurrencyExchangeRate()
    {
        CurrencyDTO? firstCurrency = null;
        CurrencyDTO? secondCurrency = null;
        while (firstCurrency == null)
        {
            firstCurrency = GetCurrencyRateFromImput();
        }
        var firstCurrencyPrice = firstCurrency.Rates.First().Price;
        var firstCurrencyCode = firstCurrency.Code;

        var firstCurrencyAmount = _userInputHandler.GetUserInput(Messages.GetAmount);

        while (secondCurrency == null)
        {
            secondCurrency = GetCurrencyRateFromImput();
        }
        var secondCurrencyPrice = secondCurrency.Rates.First().Price;
        var secondCurrencyCode = secondCurrency.Code;

        var secondCurrencyAmount = firstCurrencyPrice / secondCurrencyPrice * firstCurrencyAmount.ToInt();
        decimal roundedSecondCurrencyAmount = Math.Round(secondCurrencyAmount, 2);

        _userLogger.Log(firstCurrencyAmount + " " + firstCurrencyCode + " = " + 
            roundedSecondCurrencyAmount + " " + secondCurrencyCode);
    }

    private void ProcessCurrencyRates()
    {
        var currencyCode = _userInputHandler.GetUserInput(Messages.GetCurrencyCode).ToUpper();
        var formattedMessage = string.Empty;

        if (_currencies.Contains(currencyCode))
        {
            var currencyCount = _userInputHandler.GetUserInput(Messages.CurrencyRatesCount);

            var currencies = _currencyWebClient.GetLastCurrencyRates(currencyCode, currencyCount.ToInt());

            var ratesWithDate = currencies.Rates.Select(_ => $"data: {_.Date.ToString(ConstantValues.DateFormat)} " +
            $"cena: {_.Price}");
            //var tempList = new List<string>();
            //foreach (var rate in currencies.Rates)
            //{
            //    tempList.Add($"data: {rate.Date.ToString(ConstantValues.DateFormat)} cena: {rate.Price}");
            //}

            var result = string.Join("\n", ratesWithDate);
            formattedMessage = string.Format(Messages.LastCurrencyRates, result);
        }
        else
        {
            formattedMessage = string.Format(Messages.NoSuchCurrency, currencyCode);
        }

        _userLogger.Log(formattedMessage);
    }

    private void ProcessTodayCurrencyRate()
    {
        var currencyCode = _userInputHandler.GetUserInput(Messages.GetCurrencyCode).ToUpper();
        var formattedMessage = string.Empty;

        if(_currencies.Contains(currencyCode))
        {
            var currencyRate = _currencyWebClient.GetTodayCurrencyRate(currencyCode);
            if(currencyRate != null)
            {
                var actualCurrencyRate = currencyRate.Rates.First();
                formattedMessage = string.Format(Messages.ActualCurrencyRate, currencyCode, actualCurrencyRate.Price,
                    actualCurrencyRate.Date.ToString(ConstantValues.DateFormat));
            }
            else
            {
                formattedMessage = Messages.NoRatesToday;
            }
        }
        else
        {
            formattedMessage = string.Format(Messages.NoSuchCurrency, currencyCode);
        }
        _userLogger.Log(formattedMessage);
    }

    private void GetCurrencyTopRateFromLast30Days()
    {

        var currencyCode = _userInputHandler.GetUserInput(Messages.GetCurrencyCode).ToUpper();
        var formattedMessage = string.Empty;

        if (_currencies.Contains(currencyCode))
        {
            var response = _currencyWebClient.GetLastCurrencyRates(currencyCode, 30);
            var rates = response.Rates.OrderByDescending(_ => _.Price).ToArray();
            var topRate = rates.First();

            formattedMessage = string.Format(Messages.TopCurrencyRate, currencyCode, topRate.Price, topRate.Date.ToString(ConstantValues.DateFormat));
        }
        else
        {
            formattedMessage = string.Format(Messages.NoSuchCurrency, currencyCode);
        }
        _userLogger.Log(formattedMessage);
    }
}
