# Crypto Paper-Trading Bot

A .NET 8 console application that fetches real-time crypto data, generates technical indicators (RSI and MACD), makes mock trades using an internal virtual wallet, and employs ML.NET to learn from its past trade history to avoid bad trades!

## How it Works 

Imagine you are running a small trading company, and you've hired a team of four different "robots" to work together round the clock. Here is what each part of the project does:

### 1. The Scout: `BinanceClient.cs`
This component's only job is to run to the stock exchange (Binance) every 60 seconds and ask: *"What happened to the price of the coin in the last hour?"* 
It grabs the open price, close price, highest price, and the trading volume, then brings that raw data (`KlineData.cs`) back to the application.

### 2. The Rulebook: `Program.cs` Indicators
Once the data is fetched, the bot calculates the "momentum" of the market using a technical indicator called **RSI (Relative Strength Index)**. 
Think of this as a thermometer:
* If it gets below 35, the market is "too cold" (people sold too much, so the price might bounce up).
* If it goes above 65, it's "too hot" (people bought too much, so it might crash). 
Whenever RSI goes below 35, the bot triggers a potential **BUY** signal.

### 3. The Veteran Advisor: `TradeLearner.cs` (ML.NET)
Before the bot actually buys, the ML.NET model steps in. The model evaluates a different trend indicator called the **MACD**. 
The model checks its journal of all your past trades (`TradeHistory.json`) and evaluates the current trend: 
*"Let me look at the MACD trend right now compared to every other time you've ever bought. Based on what happened in the past, I think there is a 40% chance you will win this trade, and a 60% chance you will lose."* 
If the win probability is above 60%, the Machine Learning model gives the green light! 🚦

### 4. The Accountant: `PaperTradingEngine.cs`
Once the ML model gives the green light, we tell the engine to make the trade. 
The engine manages a "fake" $10,000 wallet. When it buys, it takes virtual USD and converts it into the virtual coin. When the RSI gets "too hot" (above 65), the engine sells the coin back to USD. 
Most importantly, it writes down exactly what happened in `TradeHistory.json` so the ML model can learn from the success or failure of that trade!

### 5. The Command Center: The Live Dashboard
While all of this is happening in the background, the console draws a live, persistent dashboard right in your terminal window using `Spectre.Console` and `AsciiChart.Sharp`. It displays an ASCII graph of the latest prices, tells you how much money the virtual wallet has, and logs the current decisions in real-time.

---

## How to Run

1. Open your terminal or command prompt.
2. Navigate to the project source directory:
   ```bash
   cd src/CryptoTrader.App
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

**A note on the first run:**
When you first run the program, the "Veteran Advisor" (ML.NET) has an empty journal—it has no past trades to look at. For the first few trades, it simply steps aside and lets the bot trade blindly based on the RSI Rulebook. But as the system logs your wins and losses, the `TradeLearner` begins training automatically. As time goes on, the ML model will get better and better at vetoing bad trades!

---

## How to Change the Coin

By default, the bot is set to trade Bitcoin (`BTCUSDT`). If you want to switch to another coin like Dogecoin (`DOGEUSDT`) or Shiba Inu (`SHIBUSDT`), you just need to update three lines in **`Program.cs`**:

1. **The Data Fetcher** (around line 32):
   ```csharp
   // Change "BTCUSDT" to your desired coin, e.g., "DOGEUSDT"
   var klines = await binanceClient.GetHistoricalDataAsync("DOGEUSDT", "1h", 150);
   ```

2. **The Price Label** (around line 96):
   ```csharp
   // Update the label text 
   [bold]Current DOGE Price[/]: {currentPrice:C}
   ```

3. **The Chart Title** (around line 114):
   ```csharp
   // Update the chart header title
   new Panel(chart).Header("DOGEUSDT (Last 60h)").Expand()
   ```

> [!WARNING]
> **Important Note on Machine Learning:** 
> The ML Model trains on the math based on your trade history. Since every coin has unique price momentum characteristics, you should **delete the `TradeHistory.json` file** anytime you switch to a totally new coin. This allows the Model to learn the new coin's unique behaviors from scratch!
