using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using UfcComScraper.ScrapySharp;

namespace UfcComScraper.ConsoleExample
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var result = 
                //new ScrapySharpUfcScraper()
                await new UfcScraper()
            //.GetTitleHolders();
            //.GetRankings();
            .Scrape();
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}