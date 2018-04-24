using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace SSISISSUCK
{
    public class SourceFileReader
    {
        private readonly ConcurrentQueue<List<string>> RowsCollection;
        private readonly PipeLineContext Context;
        public event Action ReadFinished;

        public SourceFileReader(PipeLineContext c, ConcurrentQueue<List<string>> rows)
        {
            RowsCollection = rows;
            Context = c;
        }

        private void OnReadFinished()
        {
            ReadFinished?.Invoke();
        }
        /// <summary>
        /// starts reading in the singlethreaded department (read runs on a seperate thread) the Readfinished event will be raised once the read has finished. Buffers items according to settings in the context.
        /// </summary>
        public void StartReading()
        {
            ThreadPool.QueueUserWorkItem(DoStartReading);
        }

        private void DoStartReading(object state)
        {
            using (StreamReader Reader = new StreamReader(Context.PathToSourceFile))
            {
                if (Context.FirstRowContainsHeaders) { Reader.ReadLine(); }//toss first line
                string row;
                char delim = Context.FieldDelimiter;
                int maxbuffer = Context.ReaderBufferSize;
                int minbuffer = (int)(maxbuffer * 0.8);
                while ((row = Reader.ReadLine()) != null)
                {
                    RowsCollection.Enqueue(row.Split(delim).ToList());
                    if (RowsCollection.Count > maxbuffer)
                    {
                        while (RowsCollection.Count > minbuffer)
                        {
                            Thread.Sleep(500);
                        }
                    }
                }
                OnReadFinished();
                
            }
        }
    }
}
