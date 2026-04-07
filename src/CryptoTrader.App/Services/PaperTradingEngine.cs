using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoTrader.App.Models;
using Newtonsoft.Json;

namespace CryptoTrader.App.Services;

public class PaperTradingEngine
{
    private readonly string _historyFilePath = "TradeHistory.json";
    public decimal VirtualBalance { get; private set; } = 10000m;
    public decimal Holdings { get; private set; } = 0m;
    public bool IsInPosition => Holdings > 0;

    public List<TradeRecord> TradeHistory { get; private set; } = new();

    private TradeRecord? _activeTrade;

    public PaperTradingEngine()
    {
        LoadHistory();
    }

    public decimal EvaluatePortfolioValue(decimal currentPrice)
    {
        return VirtualBalance + (Holdings * currentPrice);
    }

    public decimal CalculateNetProfit(decimal currentPrice)
    {
        return EvaluatePortfolioValue(currentPrice) - 10000m;
    }

    public void ExecuteBuy(decimal currentPrice, double rsi, decimal volume, double macdLine, double macdSignal, double macdHistogram, double histogramSlope, DateTime timestamp)
    {
        if (IsInPosition) return;

        Holdings = VirtualBalance / currentPrice;
        VirtualBalance = 0;

        _activeTrade = new TradeRecord
        {
            TradeType = "Buy",
            EntryPrice = currentPrice,
            RSIAtEntry = rsi,
            VolumeAtEntry = volume,
            MACD_Line = macdLine,
            MACD_Signal = macdSignal,
            MACD_Histogram = macdHistogram,
            Histogram_Slope = histogramSlope,
            EntryTimestamp = timestamp,
            WasProfit = false
        };
    }

    public void ExecuteSell(decimal currentPrice, DateTime timestamp)
    {
        if (!IsInPosition || _activeTrade == null) return;

        decimal newBalance = Holdings * currentPrice;
        
        _activeTrade.ExitPrice = currentPrice;
        _activeTrade.ExitTimestamp = timestamp;
        _activeTrade.ProfitLoss = newBalance - (Holdings * _activeTrade.EntryPrice);
        _activeTrade.WasProfit = _activeTrade.ProfitLoss > 0;
        
        Holdings = 0;
        VirtualBalance = newBalance;

        TradeHistory.Add(_activeTrade);
        SaveHistory();
        _activeTrade = null;
    }

    private void LoadHistory()
    {
        if (File.Exists(_historyFilePath))
        {
            try
            {
                var json = File.ReadAllText(_historyFilePath);
                TradeHistory = JsonConvert.DeserializeObject<List<TradeRecord>>(json) ?? new List<TradeRecord>();
            }
            catch
            {
                TradeHistory = new List<TradeRecord>();
            }
        }
    }

    private void SaveHistory()
    {
        var json = JsonConvert.SerializeObject(TradeHistory, Formatting.Indented);
        File.WriteAllText(_historyFilePath, json);
    }
}
