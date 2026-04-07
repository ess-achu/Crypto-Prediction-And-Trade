using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CryptoTrader.App.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Newtonsoft.Json;

namespace CryptoTrader.App.ML;

public class TradeLearner
{
    private readonly string _modelPath = "TradeModel.zip";
    private readonly string _historyFilePath = "TradeHistory.json";
    private readonly MLContext _mlContext;
    private ITransformer? _model;

    public TradeLearner()
    {
        _mlContext = new MLContext(seed: 1);
    }

    public void TrainModel()
    {
        if (!File.Exists(_historyFilePath)) return;

        var json = File.ReadAllText(_historyFilePath);
        var history = JsonConvert.DeserializeObject<List<TradeRecord>>(json) ?? new();

        // Need min sample size and both conditions to avoid ML exceptions
        if (history.Count < 5 || !history.Any(x => x.WasProfit) || !history.Any(x => !x.WasProfit))
        {
            return; 
        }

        var data = history.Select(h => new TradeData
        {
            RSIAtEntry = (float)h.RSIAtEntry,
            VolumeAtEntry = (float)h.VolumeAtEntry,
            MACD_Line = (float)h.MACD_Line,
            MACD_Signal = (float)h.MACD_Signal,
            MACD_Histogram = (float)h.MACD_Histogram,
            Histogram_Slope = (float)h.Histogram_Slope,
            WasProfit = h.WasProfit
        }).ToList();

        IDataView trainingData = _mlContext.Data.LoadFromEnumerable(data);

        var pipeline = _mlContext.Transforms.Concatenate("Features", 
                "RSIAtEntry", "VolumeAtEntry", "MACD_Line", "MACD_Signal", "MACD_Histogram", "Histogram_Slope")
            .Append(_mlContext.Transforms.NormalizeMinMax("Features"))
            .Append(_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(labelColumnName: "Label", featureColumnName: "Features"));

        _model = pipeline.Fit(trainingData);
        _mlContext.Model.Save(_model, trainingData.Schema, _modelPath);
    }

    public float PredictWinProbability(double rsi, decimal volume, double macdLine, double macdSignal, double macdHistogram, double histogramSlope)
    {
        if (_model == null)
        {
            if (File.Exists(_modelPath))
            {
                _model = _mlContext.Model.Load(_modelPath, out var modelSchema);
            }
            else
            {
                // Run blind initially if no model
                return 1.0f; 
            }
        }

        var predictionEngine = _mlContext.Model.CreatePredictionEngine<TradeData, TradePrediction>(_model);
        
        var input = new TradeData
        {
            RSIAtEntry = (float)rsi,
            VolumeAtEntry = (float)volume,
            MACD_Line = (float)macdLine,
            MACD_Signal = (float)macdSignal,
            MACD_Histogram = (float)macdHistogram,
            Histogram_Slope = (float)histogramSlope
        };

        var prediction = predictionEngine.Predict(input);
        return prediction.Probability;
    }
}
