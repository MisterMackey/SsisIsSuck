using System;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Collections.Generic;

namespace SSISISSUCK
{
    /// <summary>
    /// Object that can suggest datatypes for a flat file
    /// </summary>
    public class DataTypeSuggester
    {
        TransformBlock<string, string[]> StringSplit;
        ActionBlock<string[]> ColumnAssigner;
        private char delimiter;
        private ConcurrentBag<ConcurrentStack<string>> ColumnCollection;
        public DataTypeSuggester(char delimiter)
        {
            this.delimiter = delimiter;
            this.ColumnCollection = new ConcurrentBag<ConcurrentStack<string>>();
        }
        public async Task<string[]> SuggestDataType(string filePath, int rowsToRead, bool firstrowhasheaders)
        {
            Init(filePath);
            StreamReader reader = new StreamReader(filePath);
            if (firstrowhasheaders) { reader.ReadLine(); } //toss the first line
            StringSplit = new TransformBlock<string, string[]>(row =>
           {
               return row.Split(delimiter);
           });

            ColumnAssigner = new ActionBlock<string[]>(row =>
           {
           for (int i = 0; i < row.Count(); i++)
           {
                   ColumnCollection.ElementAt(i).Push(row[i]);
            }
           });

            StringSplit.LinkTo(ColumnAssigner, new DataflowLinkOptions { PropagateCompletion = true });
            

            Parallel.For(0, rowsToRead, new ParallelOptions {MaxDegreeOfParallelism =  1}, x =>
             {
                 x++;
                 StringSplit.Post(reader.ReadLine());
             });
            StringSplit.Complete();

            await ColumnAssigner.Completion;

            DoSuggestType();
            List<string> types = new List<string>();
            foreach (ConcurrentStack<string> type in ColumnCollection)
            {
                string HURR;
                if (type.TryPop(out HURR))
                {
                    types.Add(HURR);
                }
            }
            return types.ToArray();
        }

        private void Init(string file)
        {
            StreamReader reader = new StreamReader(file);
            int columncount = reader.ReadLine().Split(delimiter).Count();
            for (int i = 0; i < columncount; i++)
            {
                ColumnCollection.Add(new ConcurrentStack<string>());
            }
        }

        private void DoSuggestType()
        {
            Parallel.ForEach<ConcurrentStack<string>>(ColumnCollection, column =>
            {
                //dequeu all items, perform analysis, final step: enqueu the suggested datatype (preserves ordering in the bag i hope)

                int refint;
                double refdouble;
                bool refbool;
                bool CouldBeInteger = column.All<string>(x => int.TryParse(x, out refint));
                bool CouldBeDouble = column.All(x => double.TryParse(x, out refdouble));
                bool CouldBeBoolean = column.All(x => bool.TryParse(x, out refbool));
                bool CouldBeChar = column.All(x => x.Count() == 1);

                
                if (CouldBeBoolean) { column.Push("System.Boolean"); }
                else if (CouldBeInteger) { column.Push("System.Int32"); }
                else if (CouldBeDouble) { column.Push("System.Double"); }
                else if (CouldBeChar) { column.Push("System.Char"); }
                else
                {
                    int length = 0;
                    foreach (string s in column)
                    {
                        if (s.Length > length) { length = s.Length; }
                    }
                    column.Push("System.String; Length=" +length );
                }
            });
        }

        
    }
}
