using Binance.Net;
using BinanceAutoTrader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Binance.Net.Objects.Spot.MarketData;

namespace BinanceAutoTrader
{
    public class BinanceService
    {
        private PriceDataItem[] _PriceData;
        private BinanceExchangeInfo _exchangeInfo;

        public List<PriceDataItem> PriceData { get { return _PriceData.ToList(); }  }

        public BinanceService()
        {

            BinanceClient client = new BinanceClient();
            _exchangeInfo = client.Spot.System.GetExchangeInfo()?.Data;

            var socketClient = new BinanceSocketClient();
            // subscribe to updates on the spot API
            var symbolsForPricing = new List<BinanceSymbol>();
            foreach (var coin in Settings.Instance.ValidCoins)
            {
                var pairs = GetPairs(coin);
                symbolsForPricing = symbolsForPricing.Union(pairs).ToList();
            }
            var distinctSymbols = symbolsForPricing.Select(s => s.Name).Distinct().ToList();
            _PriceData = new PriceDataItem[distinctSymbols.Count];
            foreach (var distinctSymbol in distinctSymbols)
            {
                var symbol = symbolsForPricing.Where(s => s.Name == distinctSymbol).FirstOrDefault();

                socketClient.Spot.SubscribeToBookTickerUpdates(symbol.BaseAsset+symbol.QuoteAsset, data =>
                {
                    UpdatePriceData(new PriceDataItem { BaseAsset = symbol.BaseAsset, QuoteAsset = symbol.QuoteAsset, BestBidPrice = data.BestBidPrice, BestAskPrice = data.BestAskPrice });
                });
            }


            //Task.Run(() =>
            //{

            //    while (true)
            //    {
            //        Thread.Sleep(10);
            //        this.PrintPriceUpdates();
            //    }
            //});
        }

        public List<BinanceSymbol> GetPairs(string coin)
        {
            var bridgeCoin = Settings.Instance.BridgeCurrency;
            var validCoins = Settings.Instance.ValidCoins;

            var result = new List<BinanceSymbol>();
            foreach(var pair in _exchangeInfo.Symbols.ToList())
            {
                if (pair.BaseAsset == coin) result.Add(pair);
                if (pair.QuoteAsset == coin) result.Add(pair);
            }

            //only return pairs for valid coins or bridge coin
            return result.Where(p =>
            validCoins.Any(oc => oc == p.QuoteAsset) && validCoins.Any(oc => oc == p.BaseAsset) 
            || (validCoins.Any(oc => oc == p.BaseAsset) && (p.QuoteAsset == bridgeCoin) || validCoins.Any(oc => oc == p.QuoteAsset) && ( p.BaseAsset == bridgeCoin))
            ).Distinct().ToList();
        }

        public Decimal GetBidPrice(string baseAsset, String quoteAsset )
        {
            return _PriceData.ToList().Where(p => p != null 
                && p.QuoteAsset == quoteAsset 
                && p.BaseAsset == baseAsset)
                .Select(p => p.BestBidPrice).FirstOrDefault();
        }

        public Decimal GetAskPrice(string baseAsset, String quoteAsset)
        {
            return _PriceData.ToList().Where(p => p != null 
                && p.QuoteAsset == quoteAsset 
                && p.BaseAsset == baseAsset)
                .Select(p => p.BestAskPrice).FirstOrDefault();
        }

        private void UpdatePriceData(PriceDataItem data) 
        {
            bool found = false;
            //update or add  price data
            for (int i = 0; i < _PriceData.Length; i++)
            {
                if (_PriceData[i] != null 
                    && _PriceData[i].BaseAsset == data.BaseAsset && _PriceData[i].QuoteAsset == data.QuoteAsset)
                {
                    found = true;
                    _PriceData[i].BestAskPrice =  data.BestAskPrice;
                    _PriceData[i].BestBidPrice = data.BestBidPrice;

                    break;
                }
            }

            if (!found)
            {
                for (int i = 0; i < _PriceData.Length; i++)
                {
                    if (_PriceData[i] == null)
                    {
                        _PriceData[i] = data;
                        break;
                    }
                }
            }
        }

        public void PrintPriceUpdates()
        {
            Console.Clear();
            foreach(var item in _PriceData.ToList())
            {
                Console.WriteLine($" {item?.BaseAsset} {item?.BestAskPrice} {item?.BestBidPrice}");
            }
        }
    }
}
