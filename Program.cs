using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorConnect4
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (!Directory.Exists("Data"))
            {
                Directory.CreateDirectory("./Data");
            }
            //CreateHostBuilder(args).Build().Run();
            Training();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        public static void Training()
        {
            Console.WriteLine("Initialize training protocol!");

            AIModels.QAgent EasyAi = new AIModels.QAgent(Model.CellColor.Yellow);
            AIModels.QAgent randomAi = new AIModels.QAgent(Model.CellColor.Red);


            Console.WriteLine("AI's Loaded");

            randomAi.TrainAgent(EasyAi, 1000);
            //Console.WriteLine("Victories: " + EasyAi.wins + "\n" + "Ties: " + EasyAi.ties + "\n" + "Defeats: " + EasyAi.losses + "\n" + "Games played: " + EasyAi.nrOfGames + "\n");
            Console.WriteLine("Time for some R&R!");
            //EasyYellowAi.ToFile("Data/EasyDifficulty.bin");
            //EasyRedAi
        }
    }
}
