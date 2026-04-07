using System;

namespace CryptoTrader.App.Models;

public class TradeRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TradeType { get; set; } = "Buy";
    public decimal EntryPrice { get; set; }
    public decimal ExitPrice { get; set; }
    public decimal ProfitLoss { get; set; }
    public double RSIAtEntry { get; set; }
    public decimal VolumeAtEntry { get; set; }
    
    // MACD Info at Entry
    public double MACD_Line { get; set; }
    public double MACD_Signal { get; set; }
    public double MACD_Histogram { get; set; }
    public double Histogram_Slope { get; set; }
    
    public DateTime EntryTimestamp { get; set; }
    public DateTime? ExitTimestamp { get; set; }

    // Label for ML model
    public bool WasProfit { get; set; }
}
