using Microsoft.ML.Data;

namespace CryptoTrader.App.ML;

public class TradeData
{
    public float RSIAtEntry { get; set; }
    public float VolumeAtEntry { get; set; }
    public float MACD_Line { get; set; }
    public float MACD_Signal { get; set; }
    public float MACD_Histogram { get; set; }
    public float Histogram_Slope { get; set; }
    
    [ColumnName("Label")]
    public bool WasProfit { get; set; } 
}

public class TradePrediction
{
    [ColumnName("PredictedLabel")]
    public bool PredictedLabel { get; set; }

    public float Probability { get; set; }
    public float Score { get; set; }
}
