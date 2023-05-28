﻿using Laboratorium2.Interfejsy;
using Laboratorium3.Interfaces;
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
                ProcessTodayCurrencyRate();
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
}
