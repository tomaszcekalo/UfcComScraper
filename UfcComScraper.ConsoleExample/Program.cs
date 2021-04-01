using Newtonsoft.Json;
using System;

namespace UfcComScraper.ConsoleExample
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var result = new UfcScraper()
                //.Scrape();
                //.GetTitleHolders();
                //.GetRankings();
                .ScrapeAthlete("https://www.ufc.com/athlete/jan-blachowicz");
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented));
            Console.ReadKey();
        }
    }
}