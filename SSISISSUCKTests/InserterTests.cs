using Microsoft.VisualStudio.TestTools.UnitTesting;
using SSISISSUCK;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;

namespace SSISISSUCK.Tests
{
    [TestClass()]
    public class InserterTests
    {
        private bool Done = false;
        private int num = 0;
        [TestMethod()]
        public void InserterTest()
        {
            PipeLineContext c = new PipeLineContext();
            c.PathToSourceFile = @"C:\Users\C51188\Documents\Axiom ultimo Jan 2018.txt";
            c.ConnectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Liquidity;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            c.FieldDelimiter = '\t';
            c.IsSuggestingDataTypes = false;
            ConcurrentQueue<List<string>> RowsCollection = new ConcurrentQueue<List<string>>();
            SourceFileReader Reader = new SourceFileReader(c, RowsCollection);
            Inserter Writer = new Inserter(c, RowsCollection);
            DestinationTableCreator TableMaker = new DestinationTableCreator(c);

            Reader.ReadFinished += Writer.StopWriting;

            Writer.FinishedWriting += IsDone;

            TableMaker.CreateTable();
            Reader.StartReading();
            Writer.CreateConcurrentWriter();
            Writer.CreateConcurrentWriter();

            while (!Done)
            {
                Thread.Sleep(2000);
            }

        }

        private void IsDone()
        {
            if (++num ==2 ) { Done = true; }
        }

    }
}