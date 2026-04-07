using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CryptoTrader.App.ML;
using CryptoTrader.App.Models;
using Microsoft.Extensions.Hosting;
using Skender.Stock.Indicators;

namespace CryptoTrader.App.Services;

public class BotRunnerService : BackgroundService
{
    private readonly BotState _state;
    private readonly BinanceClient _binanceClient;
    private readonly PaperTradingEngine _engine;
    private readonly TradeLearner _learner;

    public BotRunnerService(BotState state, BinanceClient binanceClient, PaperTradingEngine engine, TradeLearner learner)
    {
        _state = state;
        _binanceClient = binanceClient;
        _engine = engine;
        _learner = learner;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _learner.TrainModel();
        double lastHistogram = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var klines = await _binanceClient.GetHistoricalDataAsync(_state.TradingPair, "1h", 150);
                if (!klines.Any())
                {
                    _state.StatusError = "Failed to fetch data, retrying...";
                    await Task.Delay(10000, stoppingToken);
                    continue;
                }
                _state.StatusError = "";

                var currentPrice = klines.Last().Close;
                var currentTimestamp = klines.Last().Date;

                // Indicators
                var rsiResults = klines.GetRsi(14).ToList();
                var macdResults = klines.GetMacd(12, 26, 9).ToList();

                var currentRsi = rsiResults.Last().Rsi ?? 50;
                var currentMacd = macdResults.Last();

                var macdLine = currentMacd.Macd ?? 0;
                var macdSignal = currentMacd.Signal ?? 0;
                var macdHistogram = currentMacd.Histogram ?? 0;
                var histogramSlope = macdHistogram - lastHistogram;
                lastHistogram = macdHistogram;

                // Decision Logic
                string actionTaken = "HOLD";

                if (_engine.IsInPosition)
                {
                    if (currentRsi > 65)
                    {
                        _engine.ExecuteSell(currentPrice, currentTimestamp);
                        _learner.TrainModel();
                        actionTaken = "SELL";
                    }
                }
                else
                {
                    if (currentRsi < 35)
                    {
                        var winProb = _learner.PredictWinProbability(currentRsi, klines.Last().Volume, macdLine, macdSignal, macdHistogram, histogramSlope);

                        if (winProb > 0.6f)
                        {
                            _engine.ExecuteBuy(currentPrice, currentRsi, klines.Last().Volume, macdLine, macdSignal, macdHistogram, histogramSlope, currentTimestamp);
                            actionTaken = $"BUY (Win Prob: {winProb:P0})";
                        }
                        else
                        {
                            actionTaken = $"SKIPPED (Win Prob: {winProb:P0})";
                        }
                    }
                }

                // Update UI State
                _state.CurrentPrice = currentPrice;
                _state.PortfolioValue = _engine.EvaluatePortfolioValue(currentPrice);
                _state.NetProfit = _engine.CalculateNetProfit(currentPrice);
                _state.IsInPosition = _engine.IsInPosition;
                
                _state.Rsi14 = currentRsi;
                _state.MacdLine = macdLine;
                _state.MacdSignal = macdSignal;
                _state.MacdHistogram = macdHistogram;
                
                _state.LastAction = actionTaken;
                _state.LoggedTradesCount = _engine.TradeHistory.Count;

                // For chart: take last 60 prices
                _state.RecentPrices = klines.TakeLast(60).Select(k => (double)k.Close).ToList();
                _state.RecentTimestamps = klines.TakeLast(60).Select(k => k.Date.ToString("MM/dd HH:mm")).ToList();

            }
            catch (Exception ex)
            {
                _state.StatusError = ex.Message;
            }

            await Task.Delay(60000, stoppingToken);
        }
    }
}
