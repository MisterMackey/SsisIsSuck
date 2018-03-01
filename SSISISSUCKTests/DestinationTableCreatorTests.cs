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
    public class DestinationTableCreatorTests
    {


        [TestMethod()]
        public void CreateTableTest()
        {
            DestinationTableCreator creator = new DestinationTableCreator(
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Liquidity;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False",
                "YO MAMA",
                @"C:\Users\C51188\Documents\Axiom ultimo Jan 2018.txt");
            creator.CreateTable(true, '\t', 50, true);

        }
    }
}