using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using KamaradaBot.Core;

namespace KamaradaBot.ConsoleApp
{
    class Program
    {

        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", true, true);

            var config = builder.Build();

            string botToken = config["Telegram:Token"];

            BotKamarada bot = new BotKamarada(botToken);
            var name = await bot.GetName();

            Console.Title = name;

            var cts = new CancellationTokenSource();

            bot.StartReceiving(cts);

            Console.WriteLine($"Start listening for @{name}");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
        }
    }
}
