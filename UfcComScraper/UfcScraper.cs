using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace UfcComScraper
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
            var links = await GetEventLinks();
            var tasks = links.Select(ScrapeEvent);
            var result = await Task.WhenAll(tasks);
            return result;
        }

        public async Task<EventItem> ScrapeEvent(string linkHref)
        {
            var url = Consts.UfcComUrlBase + linkHref;
            var document = await context.OpenAsync(url);
            var result = ParseEvent(document);
            return result;
        }

        public EventItem ParseEvent(IDocument node)
        {
            var headlineSuffix = node.QuerySelector(".c-hero__headline-suffix");
            var mainCard = node.QuerySelector(".main-card");
            var fightCardPrelims = node.QuerySelector(".fight-card-prelims");
            var fightCardPrelimsEarly = node.QuerySelector(".fight-card-prelims-early");

            var result = new EventItem
            {
                HeadlinePrefix = node.QuerySelector(".c-hero__headline-prefix")?.TextContent.Trim(),
                Headline = string.Join(" ", node.QuerySelector(".e-divider")
                    ?.TextContent
                    .Split(_whiteSpace, StringSplitOptions.RemoveEmptyEntries)),
                HeadlineSuffixText = headlineSuffix?.TextContent.Trim(),
                HeadlineSuffixTimestamp = headlineSuffix?.Attributes["data-timestamp"].Value,
                Venue = node.QuerySelector(".field--name-venue")?.TextContent.Trim(),
                EarlyPrelims = ParseFightCard(fightCardPrelimsEarly),
                Prelims = ParseFightCard(fightCardPrelims),
                Main = ParseFightCard(mainCard),
                Fights = (fightCardPrelimsEarly == null && fightCardPrelims == null && mainCard == null) ?
                    node.QuerySelectorAll(_fightListing)
                    .Select(ParseFight)
                    .ToList()
                    : null
            };

            return result;
        }
        public FightCard ParseFightCard(IElement node)
        {
            if (node == null)
                return new FightCard();

            var broadcasterTime = node.QuerySelector(".c-event-fight-card-broadcaster__time");

            var result = new FightCard()
            {
                BroadcasterTime = broadcasterTime?.TextContent.Trim(),
                BroadcasterTimestamp = broadcasterTime?.Attributes["data-timestamp"].Value,
                Fights = node.QuerySelectorAll(_fightListing)
                    .Select(ParseFight)
                    .ToList()
            };
            if (!string.IsNullOrWhiteSpace(result.BroadcasterTimestamp))
            {
                if (int.TryParse(result.BroadcasterTimestamp, out var unixTimestamp))
                {
                    result.BroadcasterTimestampDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
                }
            }
            return result;
        }
        public FightListItem ParseFight(IElement node)
        {
            var fightDetails = node.QuerySelectorAll("div.c-listing-fight__details ")
                .Select(x => ParseFightDetails(x))
                .FirstOrDefault();
            var redCorner = node.QuerySelectorAll(".c-listing-fight__corner--red").Select(ParseFightCorner).FirstOrDefault();
            var blueCorner = node.QuerySelectorAll(".c-listing-fight__corner--blue").Select(ParseFightCorner).FirstOrDefault();
            var fightOddsRow = node.QuerySelectorAll(".c-listing-fight__odds-row").FirstOrDefault();
            var redCountry = node.QuerySelectorAll(".c-listing-fight__country--red").Select(ParseCountry).FirstOrDefault();
            var blueCountry = node.QuerySelectorAll(".c-listing-fight__country--blue").Select(ParseCountry).FirstOrDefault();

            var result = new FightListItem
            {
                FMID = node.Attributes["data-fmid"]?.Value,
                WeightClass = node.QuerySelector(".c-listing-fight__class-text")?.TextContent.Trim()
                    .Replace("&#039;", "'"),
                RedCorner = new EventFighter()
                {
                    FamilyName = fightDetails.RedCorner.FamilyName,
                    GivenName = fightDetails.RedCorner.GivenName,
                    Rank = fightDetails.Ranks[0],
                    Country = redCountry,
                    Image = redCorner?.Image,
                    FightOutcome = redCorner.Outcome
                },
                BlueCorner = new EventFighter()
                {
                    FamilyName = fightDetails.BlueCorner.FamilyName,
                    GivenName = fightDetails.BlueCorner.GivenName,
                    Rank = fightDetails.Ranks[1],
                    Country = blueCountry,
                    Image = blueCorner?.Image,
                    FightOutcome = redCorner.Outcome
                },
                Odds = ParseFightOdds(node.QuerySelector(".c-listing-fight__odds-wrapper")),
                Results = ParseFightResults(node)
            };
            return result;
        }
        public FightResult ParseFightResult(IElement node)
        {
            var divs = node.QuerySelectorAll("div").ToArray();
            return new FightResult
            {
                Label = divs[0].TextContent,
                Text = divs[1].TextContent
            };
        }

        public IEnumerable<FightResult> ParseFightResults(IElement node)
        {
            return node.QuerySelectorAll(".c-listing-fight__result")
                .Select(ParseFightResult)
                .Distinct(_equalityComparer)
            .ToList();
        }
        public string ParseCountry(IElement node)
        {
            var result = node.QuerySelector(".c-listing-fight__country-text")?.TextContent;
            return result;
        }
        public FightOdds ParseFightOdds(IElement node)
        {
            var result = new FightOdds()
            {
                Red = node.QuerySelector(".c-listing-fight__odds-amount")?.TextContent,
                Blue = node.QuerySelector(".c-listing-fight__odds-amount")?.TextContent,
            };
            return result;
        }
        public FightDetails ParseFightDetails(IElement node)
        {
            var result = new FightDetails()
            {
                WeightClass = node
                .QuerySelector("div.c-listing-fight__class-text")
                    ?.TextContent
                    .Replace("&#039;", "'"),
                Ranks = node
                    .QuerySelectorAll(".c-listing-fight__ranks-row .c-listing-fight__corner-rank")
                    .Select(x => x.TextContent.Trim()).ToArray(),
                IsLiveNow = !node
                    .QuerySelector("div.c-listing-fight__banner--live")
                    ?.ClassList
                    .Contains("hidden"),
                BlueCorner = ParseCornerDetails(node.QuerySelector(".c-listing-fight__corner-name--blue")),
                RedCorner = ParseCornerDetails(node.QuerySelector(".c-listing-fight__corner-name--red")),
                Awards = node.QuerySelectorAll(".c-listing-fight__awards span.text").Select(x => x.TextContent).ToArray()
            };
            return result;
        }
        public CornerDetails ParseCornerDetails(IElement node)
        {
            var anchor = node.QuerySelector("a");
            var result = new CornerDetails()
            {
                Url = anchor?.Attributes["href"].Value,
                GivenName = anchor?.QuerySelector(".c-listing-fight__corner-given-name")?.TextContent,
                FamilyName = anchor?.QuerySelector(".c-listing-fight__corner-family-name")?.TextContent,
            };
            return result;
        }
        public FightCorner ParseFightCorner(IElement node)
        {
            var anchor = node.QuerySelector("a");

            var result = new FightCorner()
            {
                Image = anchor.QuerySelector("img")?.Attributes["src"].Value,
                Url = anchor?.Attributes["href"].Value,
                Outcome = node.QuerySelector(".c-listing-fight__outcome-wrapper")?.TextContent.Trim(),
            };
            return result;
        }
    }
}