using System.Collections.Generic;

namespace CryptoTrader.App.Models;

public class BotState
{
    public decimal PortfolioValue { get; set; }
    public decimal NetProfit { get; set; }
    public decimal CurrentPrice { get; set; }
    public bool IsInPosition { get; set; }
    
    public double Rsi14 { get; set; }
    public double MacdLine { get; set; }
    public double MacdSignal { get; set; }
    public double MacdHistogram { get; set; }
    
    public string LastAction { get; set; } = "Initializing...";
    public int LoggedTradesCount { get; set; }

    public string TradingPair { get; set; } = "DOGEUSDT";
    public string StatusError { get; set; } = "";

    // Detailed historical tracking for the Chart
    public List<double> RecentPrices { get; set; } = new();
    public List<string> RecentTimestamps { get; set; } = new();
}
