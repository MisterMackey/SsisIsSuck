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
            PipeLineContext c = new PipeLineContext();
            c.PathToSourceFile = @"C:\Users\C51188\Documents\Axiom ultimo Jan 2018.txt";
            c.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Liquidity;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            c.FieldDelimiter = '\t';
            DestinationTableCreator creator = new DestinationTableCreator(c);
            creator.CreateTable();

        }
    }
}