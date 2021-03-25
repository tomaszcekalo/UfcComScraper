using System.Collections.Generic;

namespace UfcComScraper
{
    public class EventItem
    {
        public string Headline { get; set; }

        public string HeadlinePrefix { get; set; }
        public FightCard Main { get; set; }
        public FightCard Prelims { get; set; }
        public FightCard EarlyPrelims { get; set; }
        public string HeadlineSuffixText { get; set; }
        public string HeadlineSuffixTimestamp { get; set; }
        public string Venue { get; set; }
        public IEnumerable<FightListItem> Fights { get; set; }
    }
}