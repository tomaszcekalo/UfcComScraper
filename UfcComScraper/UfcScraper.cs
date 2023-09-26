using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UfcComScraper
{
    public interface IUfcScraper
    {
        IEnumerable<string> GetEventLinks(string url);

        IEnumerable<EventListItem> GetEventListItems(string url);

        EventItem ScrapeEvent(string linkHref);

        IEnumerable<TitleHolder> GetTitleHolders(string url);

        IEnumerable<ViewGrouping> GetRankings(string url);
    }

    public class UfcScraper : IUfcScraper
    {
        private readonly ScrapingBrowser _browser;
        private readonly char[] _whiteSpace = new char[] { '\n', ' ' };
        private readonly FightResultEqualityComparer _equalityComparer = new FightResultEqualityComparer();
        private readonly string _fightListing = ".c-listing-fight";

        public UfcScraper()
        {
            _browser = new ScrapingBrowser();
        }

        public UfcScraper(ScrapingBrowser browser)
        {
            _browser = browser;
        }

        public IEnumerable<string> GetEventLinks(string url = Consts.UfcComUrlEvents)
        {
            WebPage homePage = _browser.NavigateToPage(new Uri(url));
            return GetEventLinks(homePage.Html);
        }

        public IEnumerable<string> GetEventLinks(HtmlNode node)
        {
            var headlines = node.CssSelect(".c-card-event--result__headline");
            return headlines.Select(x => x.FirstChild.GetAttributeValue("href", string.Empty));
        }

        public IEnumerable<EventItem> Scrape()
        {
            return GetEventLinks().Select(new Func<string, EventItem>(ScrapeEvent)).ToList();
        }

        //add tests like:
        //string htmlString = 'Your html string here...';
        //HtmlAgilityPack.HtmlDocument htmlDocument = new HtmlAgilityPack.HtmlDocument();
        //htmlDocument.LoadHtml(htmlString);
        // Do whatever with htmlDocument here

        // add string selectors to constants

        //add https://welcome.ufcfightpass.com/schedule parser

        public IEnumerable<EventListItem> GetEventListItems(string url = Consts.UfcComUrlEvents)
        {
            WebPage homePage = _browser.NavigateToPage(new Uri(url));
            var headlines = homePage.Html.CssSelect(".c-card-event--result__info");
            var result = new List<EventListItem>();
            foreach (var line in headlines)
            {
                var attributes = line.CssSelect(".c-card-event--result__date")
                    .First()
                    .Attributes;
                var item = new EventListItem
                {
                    DataMainCardTimestamp = long.Parse(
                        attributes.First(x => x.Name == "data-main-card-timestamp").Value),
                    DataPrelimsCardTimestamp = long.Parse(
                        attributes.First(x => x.Name == "data-prelims-card-timestamp").Value),
                    DataMainCard = attributes.FirstOrDefault(x => x.Name == "data-main-card")?.Value,
                    DataPrelimsCard = attributes.FirstOrDefault(x => x.Name == "data-prelims-card")?.Value,
                    Headline = line.CssSelect(".c-card-event--result__headline").FirstOrDefault()?.InnerText
                };
                var location = line.CssSelect(".c-card-event--result__location").First();
                if (location.HasChildNodes)
                {
                    var address = location.CssSelect(".address").FirstOrDefault();

                    item.Address = new EventAddress
                    {
                        Country = address?.CssSelect(".country").FirstOrDefault()?.InnerText,
                        AdministrativeArea = address?.CssSelect(".administrative-area").First().InnerText,
                        Locality = address?.CssSelect(".locality").FirstOrDefault()?.InnerText
                    };
                }
                result.Add(item);
            }

            return result;
        }

        public EventFighter ParseFighter(HtmlNode node)
        {
            var result = new EventFighter
            {
                Rank = node.CssSelect(".c-listing-fight__corner-rank").FirstOrDefault()?.InnerText.Trim(),
                GivenName = node.CssSelect(".c-listing-fight__corner-given-name")
                    .FirstOrDefault()
                    ?.InnerText,
                FamilyName = node.CssSelect(".c-listing-fight__corner-family-name")
                .FirstOrDefault()
                ?.InnerText,
                FightOutcome =
                    node.CssSelect(".c-listing-fight__outcome-wrapper ").FirstOrDefault()?.InnerText.Trim(),
                Image = node.CssSelect("img").FirstOrDefault()?.Attributes["src"].Value,
            };
            return result;
        }

        public string ParseCountry(HtmlNode node)
        {
            var result=node.CssSelect(".c-listing-fight__country-text").FirstOrDefault()?.InnerText;
            return result;
        }

        public FightResult ParseFightResult(HtmlNode node)
        {
            var divs = node.CssSelect("div").ToArray();
            return new FightResult
            {
                Label = divs[0].InnerText,
                Text = divs[1].InnerText
            };
        }

        public IEnumerable<FightResult> ParseFightResults(HtmlNode node)
        {
            return node.CssSelect(".c-listing-fight__result")
                .Select(ParseFightResult)
                .Distinct(_equalityComparer)
                .ToList();
        }

        public FightDetails ParseFightDetails(HtmlNode node)
        {
            var result = new FightDetails()
            {
                WeightClass = node.CssSelect("div.c-listing-fight__class-text").FirstOrDefault()?.InnerText,
                Ranks = node.CssSelect(".c-listing-fight__ranks-row .c-listing-fight__corner-rank").Select(x => x.InnerText.Trim()).ToArray(),
                IsLiveNow = !node.CssSelect("div.c-listing-fight__banner--live").FirstOrDefault()?.HasClass("hidden"),
                BlueCorner = ParseCornerDetails(node.CssSelect(".c-listing-fight__corner-name--blue").FirstOrDefault()),
                RedCorner = ParseCornerDetails(node.CssSelect(".c-listing-fight__corner-name--red").FirstOrDefault()),
                Awards= node.CssSelect(".c-listing-fight__awards span.text").Select(x => x.InnerText).ToArray()
            };
            return result;
        }

        public CornerDetails ParseCornerDetails(HtmlNode node)
        {
            var anchor = node.CssSelect("a").FirstOrDefault();
            var result = new CornerDetails()
            {
                Url = anchor?.Attributes["href"].Value,
                GivenName = anchor?.CssSelect(".c-listing-fight__corner-given-name").FirstOrDefault()?.InnerText,
                FamilyName = anchor?.CssSelect(".c-listing-fight__corner-family-name").FirstOrDefault()?.InnerText,
            };
            return result;
        }
        public FightCorner ParseFightCorner(HtmlNode node)
        {
            var anchor= node.CssSelect("a").FirstOrDefault();

            var result = new FightCorner()
            {
                Image = anchor.CssSelect("img").FirstOrDefault()?.Attributes["src"].Value,
                Url = anchor?.Attributes["href"].Value,
                Outcome = node.CssSelect(".c-listing-fight__outcome-wrapper").FirstOrDefault()?.InnerText.Trim(),
            };
            return result;
        }

        public FightOdds ParseFightOdds(HtmlNode node)
        {
            var result = new FightOdds()
            {
                Red = node.CssSelect(".c-listing-fight__odds-amount").FirstOrDefault()?.InnerText,
                Blue = node.CssSelect(".c-listing-fight__odds-amount").LastOrDefault()?.InnerText,
            };
            return result;
        }

        public FightListItem ParseFight(HtmlNode node)
        {
            var fightDetails = node.CssSelect("div.c-listing-fight__details ")
                .Select(x => ParseFightDetails(x))
                .FirstOrDefault();
            var redCorner = node.CssSelect(".c-listing-fight__corner--red").Select(ParseFightCorner).FirstOrDefault();
            var blueCorner = node.CssSelect(".c-listing-fight__corner--blue").Select(ParseFightCorner).FirstOrDefault();
            var fightOddsRow= node.CssSelect(".c-listing-fight__odds-row").FirstOrDefault();
            var redCountry = node.CssSelect(".c-listing-fight__country--red").Select(ParseCountry).FirstOrDefault();
            var blueCountry = node.CssSelect(".c-listing-fight__country--blue").Select(ParseCountry).FirstOrDefault();

            var result = new FightListItem
            {
                FMID = node.Attributes["data-fmid"]?.Value,
                WeightClass = node.CssSelect(".c-listing-fight__class-text").FirstOrDefault()?.InnerText.Trim(),
                RedCorner = new EventFighter()
                {
                    FamilyName = fightDetails.RedCorner.FamilyName,
                    GivenName = fightDetails.RedCorner.GivenName,
                    Rank = fightDetails.Ranks[0],
                    Country = redCountry,
                    Image = redCorner?.Image,
                    FightOutcome=redCorner.Outcome
                },
                BlueCorner = new EventFighter()
                {
                    FamilyName=fightDetails.BlueCorner.FamilyName,
                    GivenName=fightDetails.BlueCorner.GivenName,
                    Rank = fightDetails.Ranks[1],
                    Country = blueCountry,
                    Image = blueCorner?.Image,
                    FightOutcome = redCorner.Outcome
                },
                Odds = ParseFightOdds(node.CssSelect(".c-listing-fight__odds-wrapper").FirstOrDefault()),
                Results = ParseFightResults(node)
            };
            return result;
        }

        public FightCard ParseFightCard(HtmlNode node)
        {
            if (node == null)
                return new FightCard();

            var broadcasterTime = node.CssSelect(".c-event-fight-card-broadcaster__time").FirstOrDefault();

            var result = new FightCard()
            {
                BroadcasterTime = broadcasterTime?.InnerText.Trim(),
                BroadcasterTimestamp = broadcasterTime?.Attributes["data-timestamp"].Value,
                Fights = node.CssSelect(_fightListing)
                    .Select(ParseFight)
                    .ToList()
            };
            if(!string.IsNullOrWhiteSpace(result.BroadcasterTimestamp))
            {
                if(int.TryParse(result.BroadcasterTimestamp, out var unixTimestamp))
                {
                    result.BroadcasterTimestampDateTimeUtc = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp).UtcDateTime;
                }
                
            }
            return result;
        }

        public EventItem ParseEvent(HtmlNode node)
        {
            var headlineSuffix = node.CssSelect(".c-hero__headline-suffix").FirstOrDefault();
            var mainCard = node.CssSelect(".main-card").FirstOrDefault();
            var fightCardPrelims = node.CssSelect(".fight-card-prelims").FirstOrDefault();
            var fightCardPrelimsEarly = node.CssSelect(".fight-card-prelims-early").FirstOrDefault();

            var result = new EventItem
            {
                HeadlinePrefix = node.CssSelect(".c-hero__headline-prefix").FirstOrDefault()?.InnerText.Trim(),
                Headline = string.Join(" ", node.CssSelect(".e-divider")
                    .First()
                    .InnerText
                    .Split(_whiteSpace, StringSplitOptions.RemoveEmptyEntries)),
                HeadlineSuffixText = headlineSuffix?.InnerText.Trim(),
                HeadlineSuffixTimestamp = headlineSuffix?.Attributes["data-timestamp"].Value,
                Venue = node.CssSelect(".field--name-venue").FirstOrDefault()?.InnerText.Trim(),
                EarlyPrelims = ParseFightCard(fightCardPrelimsEarly),
                Prelims = ParseFightCard(fightCardPrelims),
                Main = ParseFightCard(mainCard),
                Fights = (fightCardPrelimsEarly == null && fightCardPrelims == null && mainCard == null) ?
                    node.CssSelect(_fightListing)
                    .Select(ParseFight)
                    .ToList()
                    : null
            };

            return result;
        }

        public EventItem ScrapeEvent(string linkHref)
        {
            WebPage homePage = _browser.NavigateToPage(new Uri(Consts.UfcComUrlBase + linkHref));
            return ParseEvent(homePage.Html);
        }

        public IEnumerable<TitleHolder> GetTitleHolders(string url = Consts.UfcComUrlAthletes)
        {
            WebPage homePage = _browser.NavigateToPage(new Uri(url));
            return GetTitleHolders(homePage.Html);
        }

        public IEnumerable<TitleHolder> GetTitleHolders(HtmlNode node)
        {
            var headlines = node.CssSelect(".athlete-titleholder-content");
            return headlines.Select(x => ParseTitleHolder(x));
        }

        public TitleHolder ParseTitleHolder(HtmlNode node)
        {
            var link = node.CssSelect(".ath-n__name a")
                .FirstOrDefault();

            var result = new TitleHolder()
            {
                Weight = node.CssSelect(".ath-weight")
                    .FirstOrDefault()
                    ?.InnerText,
                WeightClass = node.CssSelect(".ath-wlcass")
                    .FirstOrDefault()
                    ?.InnerText,
                Thumbnail = node.CssSelect(".atm-thumbnail img")
                    .FirstOrDefault()
                    ?.Attributes["src"]
                    .Value,
                Nickname = node.CssSelect(".field--name-nickname")
                    .FirstOrDefault()
                    ?.InnerText
                    .Trim(),
                Href = link?.Attributes["href"].Value,
                Name = link?.InnerText,
                Record = node.CssSelect(".c-ath--record")
                    .FirstOrDefault()
                    ?.InnerText
                    .Trim(),
                LastFight = node.CssSelect(".view-fighter-last-fight")
                    .FirstOrDefault()
                    ?.InnerText
                    .Trim(),
                Socials = node.CssSelect(".c-menu-social .c-menu-social__item a.c-menu-social__link")
                    .Select(x => ParseTitleHolderSocial(x))
                //.ToList()
            };
            return result;
        }

        public TitleHolderSocial ParseTitleHolderSocial(HtmlNode node)
        {
            var result = new TitleHolderSocial()
            {
                Title = node.CssSelect("title")
                    .FirstOrDefault()
                    ?.InnerText,
                Href = node.Attributes["href"].Value
            };
            return result;
        }

        public IEnumerable<ViewGrouping> GetRankings(string url = Consts.UfcComUrlRankings)
        {
            WebPage homePage = _browser.NavigateToPage(new Uri(url));
            return GetRankings(homePage.Html);
        }

        public IEnumerable<ViewGrouping> GetRankings(HtmlNode node)
        {
            var result = node.CssSelect(".view-grouping")
                .Select(x => ParseViewGrouping(x));
            return result;
        }

        public ViewGrouping ParseViewGrouping(HtmlNode node)
        {
            var champion = node.CssSelect(".rankings--athlete--champion")
                .FirstOrDefault();
            var rankers = node.CssSelect("table tbody tr")
                    .Select(x => ParseRanked(x))
                    .ToList();
            rankers.Insert(0, ParseChampion(champion));

            var result = new ViewGrouping()
            {
                WeightClass = node.CssSelect(".view-grouping-header")
                    .FirstOrDefault()?.FirstChild.InnerText,
                Rankers = rankers
            };
            return result;
        }

        public RankingItem ParseChampion(HtmlNode champion)
        {
            var result = new RankingItem()
            {
                Href = champion.CssSelect(".view-athletes a").FirstOrDefault()?.Attributes["href"].Value,
                Name = champion.CssSelect(".view-athletes").FirstOrDefault()?.InnerText.Trim(),
                Rank = (champion.CssSelect(".view-grouping-header span").FirstOrDefault()?.InnerText ?? "")
                    + (champion.CssSelect(".div.info h6 span.text").FirstOrDefault()?.InnerText ?? "")
            };
            return result;
        }

        public RankingItem ParseRanked(HtmlNode node)
        {
            var result = new RankingItem()
            {
                Rank = node.CssSelect(".views-field-weight-class-rank")
                    .FirstOrDefault()
                    ?.InnerText
                    .Trim(),
                Name = node.CssSelect(".view-id-athletes")
                    .FirstOrDefault()
                    ?.InnerText
                    .Trim(),
                Href = node.CssSelect(".view-id-athletes a")
                    .FirstOrDefault()
                    ?.Attributes["href"]
                    .Value
            };
            return result;
        }
    }
}