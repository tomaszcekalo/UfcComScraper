using System.Collections.Generic;

namespace UfcComScraper
{
    public class TitleHolder
    {
        public string Weight { get; set; }
        public string WeightClass { get; set; }
        public string Thumbnail { get; set; }
        public string Nickname { get; set; }
        public string Href { get; set; }
        public string Name { get; set; }
        public string Record { get; set; }
        public string LastFight { get; set; }
        public IEnumerable<TitleHolderSocial> Socials { get; set; }
    }

    public class TitleHolderSocial
    {
        public string Title { get; set; }
        public string Href { get; set; }
    }
}