using System.Collections.Generic;

namespace UfcComScraper
{
    public class Athlete
    {
        public string HeadlinePrefix { get; set; }
        public string Headline { get; set; }
        public string HeadlineSuffix { get; set; }
        //public string Status { get; set; }
        //public string Hometown { get; set; }
        //public string TrainingCamp { get; set; }
        //public string Age { get; set; }
        //public string Height { get; set; }
        //public string Weight { get; set; }
        //public string OctagonDebut { get; set; }
        //public string Reach { get; set; }
        //public string LegReach { get; set; }

        public Dictionary<string, string> Bios { get; set; }
        public Dictionary<string, string> Socials { get; set; }
        public Dictionary<string, string> PromotedRecords { get; set; }
        public string QnA { get; set; }
    }
}