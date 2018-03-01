using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSISISSUCK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSISISSUCK.Tests
{
    [TestClass()]
    public class DataTypeSuggesterTests
    {
        [TestMethod()]
        public void SuggestDataTypeTest()
        {
            DataTypeSuggester killssis = new DataTypeSuggester('\t');

            string[] result = killssis.SuggestDataType(@"C:\Users\C51188\Documents\Axiom ultimo Jan 2018.txt", 10000, true).Result;

            foreach (string s in result)
            {
                Console.WriteLine(s);
            }
        }
    }
}