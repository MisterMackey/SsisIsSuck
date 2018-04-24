using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSISISSUCK
{
    [ComVisible(true)
        ,Guid("b5ea349d-0c7b-42c0-bd70-15acf1fba60c")
        ,ClassInterface(ClassInterfaceType.None)
        ,ProgId("FileLoader.StageOneLoader")]
    public class COMLoader : ICOMLoader
    {
        private PipeLineContext Context;
        private int NumberOfWriterThreads = 3;
        private int NumberOfFinishedThreads = 0;
        public COMLoader()
        {
            Context = new PipeLineContext();
        }

        #region Interface
        public void CreateTable()
        {
            DestinationTableCreator TableMaker = new DestinationTableCreator(Context);
            TableMaker.CreateTable();
        }

        public void SetDelimiter(char delim)
        {
            Context.FieldDelimiter = delim;
        }

        public void SetDestination(string connectionString)
        {
            Context.ConnectionString = connectionString;
        }

        public void SetFilePath(string filePath)
        {
            Context.PathToSourceFile = filePath;
        }

        public void SetFirstLineContainsHeaderBool(bool val)
        {
            Context.FirstRowContainsHeaders = val;
        }

        public void SetGuessDataType(bool val)
        {
            Context.IsSuggestingDataTypes = val;
        }

        public void SetTableName(string name)
        {
            Context.DestinationTableName = name;
        }

        public async Task StartLoading()
        {
            ConcurrentQueue<List<string>> Queu = new ConcurrentQueue<List<string>>();
            //create threads to transfer file
            SourceFileReader Reader = new SourceFileReader(Context, Queu);
            Inserter Writer = new Inserter(Context, Queu);
            Writer.done = false;
            Reader.ReadFinished += Writer.StopWriting;
            Writer.FinishedWriting += OnWriterFinishing;

            // start everything up and monitor for finish
            Reader.StartReading();
            for (int i = 0; i < NumberOfWriterThreads; i++)
            {
                Writer.CreateConcurrentWriter();
            }

            await Task.Run(() =>
            {
                while (NumberOfWriterThreads > NumberOfFinishedThreads)
                {
                    Task.Delay(1000).Wait();
                }
            });

        }
        #endregion

        private void OnWriterFinishing()
        {
            NumberOfFinishedThreads++;
        }
    }
}
