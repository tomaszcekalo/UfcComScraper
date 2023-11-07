namespace UfcComScraper
{
    public class FightDetails
    {
        public bool? IsLiveNow { get; set; }
        public string WeightClass { get; set; }
        public string[] Ranks { get; set; }
        public CornerDetails BlueCorner { get; set; }
        public CornerDetails RedCorner { get; set; }
        public string[] Awards { get; set; }
    }
}