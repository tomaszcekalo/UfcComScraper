using Newtonsoft.Json;
using System;
using UfcComScraper.ScrapySharp;

namespace UfcComScraper.ConsoleExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = new ScrapySharpUfcScraper()
            //.GetTitleHolders();
            //.GetRankings();
            .Scrape();
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}