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


            AIModels.QAgent testYellowAi = AIModels.QAgent.ConstructFromFile("Data/testYellowAi.bin");
            AIModels.QAgent testRedAi = AIModels.QAgent.ConstructFromFile("Data/testRedAi.bin");
            AIModels.RandomAI randomAI = new AIModels.RandomAI();


            Console.WriteLine("AI's Loaded");

            testYellowAi.TrainAgent(testRedAi, 100);

            //testRedAi.TrainAgent(testYellowAi, 100);


            Console.WriteLine("Time for some R&R!");
            //EasyYellowAi.ToFile("Data/EasyYellowAi.bin");
            //EasyRedAi.ToFile("Data/EasyRedAi.bin");

            testYellowAi.ToFile("Data/testYellowAi.bin");
            //testRedAi.ToFile("Data/testRedAi.bin");
        }
    }
}