using BinanceAutoTrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceAutoTrader.Services
{
    public class TradeService
    {
        Dictionary<string, decimal> ScoutRatios;

        public BinanceService _binanceService { get; set; }
        public TradeAsset CurrentAsset { get; set; }

        public TradeService(BinanceService binanceService)
        {
            _binanceService = binanceService;
            //todo: find starting asset, for now hardcoding
            CurrentAsset = new TradeAsset()
            {
                OpenCoin = "ADA",
                OpenQty = 100,
                OpenPrice = 0, // ????
                CurrentCoin = "ADA",
                CurrentQty = 100,
                CurrentPrice = 0, // ????
            };
        }

        private void ScoreItems()
        {
            //TODO:
        }

        public void Scout()
        {
            Program.log.Info("Scouting...");
            ScoutRatios = new Dictionary<string, decimal>();
            var bridgeCoin = Settings.Instance.BridgeCurrency;
            var currentCoin = CurrentAsset.CurrentCoin;

            Thread.Sleep(Settings.Instance.ScoutSleepTime);
            // Find low prices for buy trade until we can buy back at a win

            var validCoins = Settings.Instance.ValidCoins;

            foreach (var optionalCoin in validCoins)
            {
                if (optionalCoin == currentCoin) continue;

                CurrentAsset.CurrentPrice = _binanceService.GetAskPrice(currentCoin,bridgeCoin);
                var optionCoinPrice = _binanceService.GetAskPrice(optionalCoin, bridgeCoin);

                if (optionCoinPrice == 0) continue;

                var usdRatio = CurrentAsset.CurrentPrice / optionCoinPrice; 
                //Pair ratio right way around ??
                var pairRatio = FindPairRatio(currentCoin, optionalCoin);

                if (pairRatio == 0) continue;

                var weightedRatio = (usdRatio - Settings.Instance.ScoutTransactionFee * Settings.Instance.ScoutMultiplier * usdRatio) - pairRatio;

                // Add or Update to list of scouted coins
                if (ScoutRatios.ContainsKey(optionalCoin))
                {
                    ScoutRatios[optionalCoin] = weightedRatio;
                }
                else
                {
                    ScoutRatios.Add(optionalCoin, weightedRatio);
                }
            }

            //only keep ratios bigger than 0
            ScoutRatios = ScoutRatios.Where(sr => sr.Value > 0).ToDictionary(sr => sr.Key, sr => sr.Value);

            //find the best trade, if any
            if (ScoutRatios.Any()) {
                //find best ratio
                KeyValuePair<string, decimal> max = FindMax(ScoutRatios);

                Trade(currentCoin, max.Key);
            }
        }

        private decimal FindPairRatio(string from , string to)
        {
            decimal ratio = 0;
            var bridgeCoin = Settings.Instance.BridgeCurrency;
            var pairs = _binanceService.GetPairs(from);
            var buyPair = pairs.Where(p => p.BaseAsset == to && p.QuoteAsset == from).FirstOrDefault();
            var sellPair = pairs.Where(p => p.BaseAsset == from && p.QuoteAsset == to).FirstOrDefault();

            if (buyPair != null)
            {
                ratio = _binanceService.GetAskPrice(to, from);
            }
            else if (sellPair != null)
            {
                ratio = _binanceService.GetBidPrice(from, to );
            }
            else //Bridge
            {
                var currentPrice = _binanceService.GetBidPrice(from, bridgeCoin);
                var optionCoinPrice = _binanceService.GetAskPrice(to, bridgeCoin);

                ratio = currentPrice / optionCoinPrice;
            }

            return ratio;
        }


        public void Trade(string from, string to)
        {
            var pairs = _binanceService.GetPairs(from);
            var bridgeCoin = Settings.Instance.BridgeCurrency;
            var tradefee = Settings.Instance.ScoutTransactionFee;

            //BUY
            var buyPair = pairs.Where(p => p.BaseAsset == to && p.QuoteAsset == from).FirstOrDefault();
            var sellPair = pairs.Where(p => p.BaseAsset == from && p.QuoteAsset == to).FirstOrDefault();
            if (buyPair != null)
            {
                Program.log.Info($" Trading {from} for {to} with BUY[{buyPair.Name}]...");
                var buyPrice = _binanceService.GetAskPrice(to, from);
                decimal newQty = CurrentAsset.CurrentQty / buyPrice;
                var bridgePrice = _binanceService.GetAskPrice(to, bridgeCoin);
                //deduct fee
                newQty = newQty - (tradefee * bridgePrice * newQty);

                CurrentAsset.CurrentCoin = to;
                CurrentAsset.CurrentQty = newQty;
                CurrentAsset.CurrentPrice = bridgePrice;
            }
            //SELL
            else if (sellPair != null)
            {
                Program.log.Info($" Trading {from} for {to} with SELL[{sellPair.Name}]...");
                
                var sellPrice = _binanceService.GetBidPrice(from,to);
                decimal newQty = CurrentAsset.CurrentQty * sellPrice;
                var bridgePrice = _binanceService.GetAskPrice(to, bridgeCoin);
                //deduct fee
                newQty = newQty - (tradefee * bridgePrice * newQty);

                CurrentAsset.CurrentCoin = to;
                CurrentAsset.CurrentQty = newQty;
                CurrentAsset.CurrentPrice = bridgePrice;
            }
            //Bridge Trade
            else
            {
                Program.log.Info($" Trading {from} for {to} with Bridge...");

                //sell from-coin
                var sellPrice = _binanceService.GetAskPrice(from,bridgeCoin);
                var soldValue = CurrentAsset.CurrentQty * sellPrice;
                
                //deduct fee
                soldValue = soldValue - (tradefee * sellPrice);

                //buy to-coin
                var buyPrice = _binanceService.GetBidPrice(to,bridgeCoin);
                decimal newQty = soldValue / buyPrice;

                //deduct fee
                newQty = newQty - (newQty * tradefee * buyPrice);

                CurrentAsset.CurrentCoin = to;
                CurrentAsset.CurrentQty = newQty;
                CurrentAsset.CurrentPrice = buyPrice;
            }

            Program.log.Info($"Total Asset Value from {CurrentAsset.OpenValue} to {CurrentAsset.CurrentValue}");
        }



        private KeyValuePair<string, decimal> FindMax(Dictionary<string, decimal> input)
        {
            KeyValuePair<string, decimal> max = new KeyValuePair<string, decimal>();
            foreach (var kvp in input)
            {
                if (kvp.Value > max.Value) max = kvp;
            }
            return max;
        }
    }
}
