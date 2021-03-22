using System.Collections.Generic;

namespace UfcComScraper
{
    public class FightResultEqualityComparer : IEqualityComparer<FightResult>
    {
        public bool Equals(FightResult x, FightResult y)
        {
            return x.Label == y.Label && x.Text == y.Text;
        }

        public int GetHashCode(FightResult obj)
        {
            return obj.Label.GetHashCode() + obj.Text.GetHashCode();
        }
    }
}