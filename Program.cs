using BinanceAutoTrader.Services;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BinanceAutoTrader
{
    class Program
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        static void Main(string[] args)
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddSingleton<BinanceService, BinanceService>()
                .AddSingleton<TradeService, TradeService>()
                .BuildServiceProvider();

            // Load configuration
            var logRepository = LogManager.GetRepository(System.Reflection.Assembly.GetEntryAssembly());
            log4net.Config.XmlConfigurator.Configure(logRepository, new System.IO.FileInfo("log4net.config"));

            log.Info("Starting up...");

            TradeService trade = serviceProvider.GetService<TradeService>();
            
            Task.Run(() =>
            {

                while (true)
                {
                    trade.Scout();
                }
            });

            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }

    }
}
