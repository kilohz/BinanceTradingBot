using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAutoTrader.Models
{
    public class PriceDataItem
    {
        public string BaseAsset { get; set; }

        public string QuoteAsset { get; set; }

        public decimal BestAskPrice { get; set; }

        public decimal BestBidPrice { get; set; }
    }
}
