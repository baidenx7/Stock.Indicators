﻿using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // CHANDELIER EXIT
        public static IEnumerable<ChandelierResult> GetChandelier(
            IEnumerable<Quote> history, int lookbackPeriod = 22, 
            decimal multiplier = 3.0m, ChandelierType type = ChandelierType.Long)
        {

            // clean quotes
            Cleaners.PrepareHistory(history);

            // validate inputs
            ValidateChandelier(history, lookbackPeriod, multiplier);

            // initialize
            List<ChandelierResult> results = new List<ChandelierResult>();
            IEnumerable<AtrResult> atrResult = GetAtr(history, lookbackPeriod);  // uses ATR

            // roll through history
            foreach (Quote h in history)
            {

                ChandelierResult result = new ChandelierResult
                {
                    Index = (int)h.Index,
                    Date = h.Date
                };

                // add exit values
                if (h.Index >= lookbackPeriod)
                {
                    List<Quote> period = history
                        .Where(x => x.Index <= h.Index && x.Index > (h.Index - lookbackPeriod))
                        .ToList();

                    decimal atr = (decimal)atrResult
                        .Where(x => x.Index == h.Index)
                        .FirstOrDefault()
                        .Atr;

                    switch (type)
                    {
                        case ChandelierType.Long:

                            decimal maxHigh = period.Select(x => x.High).Max();
                            result.ChandelierExit = maxHigh - atr * multiplier;
                            break;

                        case ChandelierType.Short:

                            decimal minLow = period.Select(x => x.Low).Min();
                            result.ChandelierExit = minLow + atr * multiplier;
                            break;

                        default:
                            break;
                    }
                }

                results.Add(result);
            }

            return results;
        }


        private static void ValidateChandelier(
            IEnumerable<Quote> history, int lookbackPeriod, decimal multiplier)
        {

            // check parameters
            if (lookbackPeriod <= 0)
            {
                throw new BadParameterException("Lookback period must be greater than 0 for Chandelier Exit.");
            }

            if (multiplier <= 0)
            {
                throw new BadParameterException("Multiplier must be greater than 0 for Chandelier Exit.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = lookbackPeriod + 1;
            if (qtyHistory < minHistory)
            {
                throw new BadHistoryException("Insufficient history provided for Chandelier Exit.  " +
                        string.Format(cultureProvider,
                        "You provided {0} periods of history when at least {1} is required.",
                        qtyHistory, minHistory));
            }
        }
    }

}
