using System;
using Skender.Stock.Indicators;

namespace CryptoTrader.App.Models;

// Implements Skender's IQuote for easy indicator calculations
public class KlineData : IQuote
{
    public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}
