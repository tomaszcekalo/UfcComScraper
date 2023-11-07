using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using UfcComScraper.ScrapySharp;

namespace UfcComScraper.Tests
{
    [TestClass]
    public class LiveDataTests
    {
        //ScrapySharp
        [TestMethod]
        public async Task TestGetTitleHoldersScrapySharp()
        {
            var result = await new ScrapySharpUfcScraper()
                .GetTitleHolders();
        }

        [TestMethod]
        public async Task TestGetRankingsScrapySharp()
        {
            var result = await new ScrapySharpUfcScraper()
                .GetRankings();
        }

        [TestMethod]
        public async Task TestGetEventLinksScrapySharp()
        {
            var result = await new ScrapySharpUfcScraper()
                .GetEventLinks();
        }

        //AngleSharp
        [TestMethod]
        public async Task TestGetTitleHoldersAngleSharp()
        {
            var result = await new UfcScraper()
                .GetTitleHolders();
        }

        [TestMethod]
        public async Task TestGetRankingsAngleSharp()
        {
            var result = await new UfcScraper()
                .GetRankings();
        }

        [TestMethod]
        public async Task TestGetEventLinksAngleSharp()
        {
            var result = await new UfcScraper()
                .GetEventLinks();
        }

        //Compare both
        [TestMethod]
        public async Task TestGetTitleHolders()
        {
            var angle = await new UfcScraper()
                .GetTitleHolders();
            var scrapy = await new ScrapySharpUfcScraper()
                .GetTitleHolders();
            angle.Should().BeEquivalentTo(scrapy);
        }

        [TestMethod]
        public async Task TestGetRankings()
        {
            var angle = await new UfcScraper()
                .GetRankings();
            var scrapy = await new ScrapySharpUfcScraper()
                .GetRankings();
            angle.Should().BeEquivalentTo(scrapy);
        }

        [TestMethod]
        public async Task TestGetEventLinks()
        {
            var angle = await new UfcScraper()
                .GetEventLinks();
            var scrapy = await new ScrapySharpUfcScraper()
                .GetEventLinks();
            angle.Should().BeEquivalentTo(scrapy);
        }
    }
}