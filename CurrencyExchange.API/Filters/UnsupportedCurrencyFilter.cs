using CurrencyExchange.API.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

public class UnsupportedCurrencyFilter : ActionFilterAttribute
{
   
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var baseCurrency = GetBaseCurrency(context);

        if (!string.IsNullOrEmpty(baseCurrency) && UnsupportedCurrencies.ExcludedCurrencies.Contains(baseCurrency.ToUpper()))
        {
            context.Result = new BadRequestObjectResult($"Currency '{baseCurrency}' is not supported.");
        }

        base.OnActionExecuting(context);
    }

    private static string GetBaseCurrency(ActionExecutingContext context)
    {
        if (context.HttpContext.Request.Query.TryGetValue("baseCurrency", out var currency))
        {
            return currency.ToString();
        }

        return null;
    }
}
