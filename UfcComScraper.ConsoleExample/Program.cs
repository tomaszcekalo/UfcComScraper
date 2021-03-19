using Newtonsoft.Json;
using System;

namespace UfcComScraper.ConsoleExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = new UfcScraper().Scrape();
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}