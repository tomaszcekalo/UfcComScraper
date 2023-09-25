namespace UfcComScraper
{
    public class FightDetails
    {
        public bool? IsLiveNow { get; internal set; }
        public string WeightClass { get; internal set; }
        public string[] Ranks { get; internal set; }
        public CornerDetails BlueCorner { get; internal set; }
        public CornerDetails RedCorner { get; internal set; }
        public string[] Awards { get; internal set; }
    }
}