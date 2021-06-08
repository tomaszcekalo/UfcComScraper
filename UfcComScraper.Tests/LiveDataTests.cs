using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UfcComScraper.Tests
{
    [TestClass]
    public class LiveDataTests
    {
        [TestMethod]
        public void TestGetTitleHolders()
        {
            var result = new UfcScraper()
                .GetTitleHolders();
        }
        [TestMethod]
        public void TestGetRankings()
        {
            var result = new UfcScraper()
                .GetRankings();
        }
        [TestMethod]
        public void TestGetEventLinks()
        {
            var result = new UfcScraper()
                .GetEventLinks();
        }
    }
}