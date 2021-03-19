using System.Collections.Generic;

namespace UfcComScraper
{
    public class FightListItem
    {
        public EventFighter RedCorner { get; set; }
        public EventFighter BlueCorner { get; set; }
        public string FMID { get; set; }
        public string WeightClass { get; set; }
        public FightOdds Odds { get; set; }
        public IEnumerable<FightResult> Results { get; set; }
    }
}