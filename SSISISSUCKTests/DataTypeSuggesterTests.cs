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
            PipeLineContext c = new PipeLineContext();
            c.PathToSourceFile = @"C:\Users\C51188\Documents\Axiom ultimo Jan 2018.txt";
            c.FieldDelimiter = '\t';
            DataTypeSuggester killssis = new DataTypeSuggester(c);

            string[] result = killssis.SuggestDataType().Result;

            foreach (string s in result)
            {
                Console.WriteLine(s);
            }
        }
    }
}