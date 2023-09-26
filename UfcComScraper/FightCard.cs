using System;
using System.Collections.Generic;

namespace UfcComScraper
{
    public class FightCard
    {
        public string BroadcasterTime { get; set; }
        public string BroadcasterTimestamp { get; set; }
        public IEnumerable<FightListItem> Fights { get; set; } = new List<FightListItem>();
        public DateTime BroadcasterTimestampDateTimeUtc { get; internal set; }
    }
}