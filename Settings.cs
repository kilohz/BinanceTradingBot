using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BinanceAutoTrader
{
    public class Settings
    {
        private static readonly Lazy<Settings> lazy = new Lazy<Settings>(() => new Settings());

        public static Settings Instance { get { return lazy.Value; } }

        private Settings()
        {
            //Hardcoded settings
            ScoutTransactionFee = 0.001M;
            BridgeCurrency = "BUSD";
            BinanceApiKey = "neverputhere";
            BinanceApiSecret = "theywillhackyou";
            ScoutMultiplier = 5;
            ScoutSleepTime = 2000;//Milis
            ValidCoins = new String[] {
                "XLM",
                "TRX",
                "ICX",
                "EOS",
                "IOTA",
                "ONT",
                "QTUM",
                "ETC",
                "ADA",
                "XMR",
                "DASH",
                "NEO",
                "ATOM",
                "DOGE",
                "VET",
                "BAT",
                "OMG",
                "BTT",
                "HBAR"
            };
        }


        public string[] ValidCoins { get; set; }

        public string BinanceApiKey { get; set; }

        public string BinanceApiSecret { get; set; }

        public decimal ScoutTransactionFee { get; set; }

        public int ScoutMultiplier { get; set; }

        public int ScoutSleepTime { get; set; }

        public string BridgeCurrency { get; set; }
    }
}
