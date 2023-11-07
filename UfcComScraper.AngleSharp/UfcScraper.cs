using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UfcComScraper.AngleSharp
{
    public class UfcScraper : IUfcScraper
    {
        private readonly char[] _whiteSpace = new char[] { '\n', ' ' };
        private readonly FightResultEqualityComparer _equalityComparer = new FightResultEqualityComparer();
        private readonly string _fightListing = ".c-listing-fight";
        private IConfiguration config;
        private IBrowsingContext context;

        public UfcScraper()
        {
            config = Configuration.Default.WithDefaultLoader();
            context = BrowsingContext.New(config);
        }

        public async Task<IEnumerable<string>> GetEventLinks(string url = Consts.UfcComUrlEvents)
        {
            var document = await context.OpenAsync(url);
            return GetEventLinks(document);
        }

        public IEnumerable<string> GetEventLinks(IDocument document)
        {
            var headlines = document.QuerySelectorAll(".c-card-event--result__headline");
            return headlines.Select(x => x.FirstElementChild.GetAttribute("href"));
        }

        public async Task<IEnumerable<EventListItem>> GetEventListItems(string url = Consts.UfcComUrlEvents)
        {
            var document = await context.OpenAsync(url);
            var headlines = document.QuerySelectorAll(".c-card-event--result__info");
            var result = new List<EventListItem>();
            foreach (var line in headlines)
            {
                var attributes = line.QuerySelector(".c-card-event--result__date")
                    .Attributes;
                var item = new EventListItem
                {
                    DataMainCardTimestamp = long.Parse(
                        attributes.First(x => x.Name == "data-main-card-timestamp").Value),
                    DataPrelimsCardTimestamp = long.Parse(
                        attributes.First(x => x.Name == "data-prelims-card-timestamp").Value),
                    DataMainCard = attributes.FirstOrDefault(x => x.Name == "data-main-card")?.Value,
                    DataPrelimsCard = attributes.FirstOrDefault(x => x.Name == "data-prelims-card")?.Value,
                    Headline = line.QuerySelector(".c-card-event--result__headline")?.TextContent
                };
                var location = line.QuerySelector(".c-card-event--result__location");
                if (location.HasChildNodes)
                {
                    var address = location.QuerySelector(".address");

                    item.Address = new EventAddress
                    {
                        Country = address?.QuerySelector(".country")?.TextContent,
                        AdministrativeArea = address?.QuerySelector(".administrative-area").TextContent,
                        Locality = address?.QuerySelector(".locality")?.TextContent
                    };
                }
                result.Add(item);
            }
            return result;
        }

        public async Task<IEnumerable<ViewGrouping>> GetRankings(string url = Consts.UfcComUrlRankings)
        {
            var document = await context.OpenAsync(url);
            return GetRankings(document);
        }

        private IEnumerable<ViewGrouping> GetRankings(IDocument document)
        {
            var result = document.QuerySelectorAll(".view-grouping")
                .Select(x => ParseViewGrouping(x));
            return result;
        }

        private ViewGrouping ParseViewGrouping(IElement node)
        {
            var champion = node.QuerySelector(".rankings--athlete--champion");
            var rankers = node.QuerySelectorAll("table tbody tr")
                    .Select(x => ParseRanked(x))
                    .ToList();
            rankers.Insert(0, ParseChampion(champion));

            var result = new ViewGrouping()
            {
                WeightClass = node.QuerySelector(".view-grouping-header")?.FirstChild.TextContent,
                Rankers = rankers
            };
            return result;
        }

        public RankingItem ParseChampion(IElement champion)
        {
            var result = new RankingItem()
            {
                Href = champion.QuerySelector(".view-athletes a")?.Attributes["href"].Value,
                Name = champion.QuerySelector(".view-athletes")?.TextContent.Trim(),
                Rank = (champion.QuerySelector(".view-grouping-header span")?.TextContent ?? "")
                    + (champion.QuerySelector(".div.info h6 span.text")?.TextContent ?? "")
            };
            return result;
        }

        public RankingItem ParseRanked(IElement node)
        {
            var result = new RankingItem()
            {
                Rank = node.QuerySelector(".views-field-weight-class-rank")
                    ?.TextContent
                    .Trim(),
                Name = node.QuerySelector(".view-id-athletes")
                    ?.TextContent
                    .Trim(),
                Href = node.QuerySelector(".view-id-athletes a")
                    ?.Attributes["href"]
                    .Value
            };
            return result;
        }

        public async Task<IEnumerable<TitleHolder>> GetTitleHolders(string url = Consts.UfcComUrlAthletes)
        {
            var document = await context.OpenAsync(url);
            return GetTitleHolders(document);
        }

        private IEnumerable<TitleHolder> GetTitleHolders(IDocument document)
        {
            var headlines = document.QuerySelectorAll(".athlete-titleholder-content");
            return headlines.Select(x => ParseTitleHolder(x));
        }

        private TitleHolder ParseTitleHolder(IElement node)
        {
            var link = node.QuerySelector(".ath-n__name a");

            var result = new TitleHolder()
            {
                Weight = node.QuerySelector(".ath-weight")
                    ?.TextContent,
                WeightClass = node.QuerySelector(".ath-wlcass")
                    ?.TextContent,
                Thumbnail = node.QuerySelector(".atm-thumbnail img")
                    ?.Attributes["src"]
                    .Value,
                Nickname = node.QuerySelector(".field--name-nickname")
                    ?.TextContent
                    .Trim(),
                Href = link?.Attributes["href"].Value,
                Name = link?.TextContent,
                Record = node.QuerySelector(".c-ath--record")
                    ?.TextContent
                    .Trim(),
                LastFight = node.QuerySelector(".view-fighter-last-fight")
                    ?.TextContent
                    .Trim(),
                Socials = node.QuerySelectorAll(".c-menu-social .c-menu-social__item a.c-menu-social__link")
                    .Select(x => ParseTitleHolderSocial(x))
                //.ToList()
            };
            return result;
        }

        public TitleHolderSocial ParseTitleHolderSocial(IElement node)
        {
            var result = new TitleHolderSocial()
            {
                Title = node.QuerySelector("title")
                    ?.TextContent,
                Href = node.Attributes["href"].Value
            };
            return result;
        }

        public async Task<IEnumerable<EventItem>> Scrape()
        {
            var url = "";
            var document = await context.OpenAsync(url);
            throw new NotImplementedException();
        }

        public async Task<EventItem> ScrapeEvent(string linkHref)
        {
            var url = "";
            var document = await context.OpenAsync(url);
            throw new NotImplementedException();
        }
    }
}