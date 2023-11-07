using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UfcComScraper
{
    public interface IUfcScraper
    {
        Task<IEnumerable<string>> GetEventLinks(string url);

        Task<IEnumerable<EventListItem>> GetEventListItems(string url);

        Task<EventItem> ScrapeEvent(string linkHref);

        Task<IEnumerable<TitleHolder>> GetTitleHolders(string url);

        Task<IEnumerable<ViewGrouping>> GetRankings(string url);

        Task<IEnumerable<EventItem>> Scrape();
    }
}