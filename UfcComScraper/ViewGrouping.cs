using System.Collections.Generic;

namespace UfcComScraper
{
    public class ViewGrouping
    {
        public string WeightClass { get; set; }
        public IEnumerable<RankingItem> Rankers { get; set; }
    }
}