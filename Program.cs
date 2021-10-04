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

            AIModels.QAgent EasyAi = AIModels.QAgent.ConstructFromFile("Data/TempDifficulty.bin");
            AIModels.QAgent randomAi = AIModels.QAgent.ConstructFromFile("Data/EasyDifficulty.bin");


            Console.WriteLine("AI's Loaded");

            EasyAi.TrainAgent(randomAi, 100000);

            //Console.WriteLine("Victories: " + EasyAi.wins + "\n" + "Ties: " + EasyAi.ties + "\n" + "Defeats: " + EasyAi.losses + "\n" + "Games played: " + EasyAi.nrOfGames + "\n");
            Console.WriteLine("Time for some R&R!");
            //EasyAi.ToFile("Data/EasyDifficulty.bin");
        }
    }
}
