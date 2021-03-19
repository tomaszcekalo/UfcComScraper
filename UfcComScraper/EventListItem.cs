namespace UfcComScraper
{
    public class EventListItem
    {
        public long DataMainCardTimestamp { get; set; }
        public long DataPrelimsCardTimestamp { get; set; }
        public string DataMainCard { get; set; }
        public string DataPrelimsCard { get; set; }
        public EventAddress Address { get; set; }
        public string Headline { get; set; }
    }
}