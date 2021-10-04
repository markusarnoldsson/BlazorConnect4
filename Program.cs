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
            CreateHostBuilder(args).Build().Run();
            //Training();
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

            AIModels.RandomAI randomAI = new AIModels.RandomAI();
            AIModels.QAgent YellowAi = new AIModels.QAgent(Model.CellColor.Yellow);

            Console.WriteLine("AI's Loaded");

            YellowAi.TrainAgent(randomAI, 1000000);

            Console.WriteLine("Time for some R&R!");
            YellowAi.ToFile("Data/EasyDifficulty.bin");
        }
    }
}
