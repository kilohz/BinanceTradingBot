using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAutoTrader.Models
{
    public class TradeAsset
    {
        public TradeAsset()
        {

        }

        public decimal OpenValue {
            get  { return OpenPrice* OpenQty; } 
        }

        public decimal OpenPrice { get; set; }

        public decimal OpenQty { get; set; }

        public string OpenCoin { get; set; }


        public decimal CurrentValue
        {
            get { return CurrentPrice * CurrentQty; }
        }

        //remove prices from this object ?  could go stale ??
        public decimal CurrentPrice { get; set; }

        public decimal CurrentQty { get; set; }

        public string CurrentCoin { get; set; }

    }
}
