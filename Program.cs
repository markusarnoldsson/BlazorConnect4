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

            //AIModels.QAgent EasyYellowAi = AIModels.QAgent.ConstructFromFile("Data/EasyYellowAi.bin");
            //AIModels.QAgent EasyRedAi = AIModels.QAgent.ConstructFromFile("Data/EasyRedAi.bin");

            AIModels.QAgent MediumYellowAi = new AIModels.QAgent(Model.CellColor.Yellow);
            AIModels.QAgent MediumRedAi = new AIModels.QAgent(Model.CellColor.Red);
            AIModels.RandomAI randomAI = new AIModels.RandomAI();


            Console.WriteLine("AI's Loaded");

            //EasyYellowAi.TrainAgent(EasyRedAi, 10000);
            //EasyYellowAi.TrainAgent(randomAI, 10000);

            //EasyRedAi.TrainAgent(randomAI, 10000);
            //EasyRedAi.TrainAgent(EasyYellowAi, 10000);


            Console.WriteLine("Time for some R&R!");
            //EasyYellowAi.ToFile("Data/EasyYellowAi.bin");
            //EasyRedAi.ToFile("Data/EasyRedAi.bin");
        }
    }
}