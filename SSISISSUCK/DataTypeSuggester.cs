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
        private PipeLineContext Context;
        private ConcurrentBag<ConcurrentStack<string>> ColumnCollection;
        public DataTypeSuggester(PipeLineContext c)
        {
            Context = c;
            this.ColumnCollection = new ConcurrentBag<ConcurrentStack<string>>();
        }
        public async Task<string[]> SuggestDataType()
        {
            Init(Context.PathToSourceFile);
            StreamReader reader = new StreamReader(Context.PathToSourceFile);
            if (Context.FirstRowContainsHeaders) { reader.ReadLine(); } //toss the first line
            StringSplit = new TransformBlock<string, string[]>(row =>
           {
               return row.Split(Context.FieldDelimiter);
           });

            ColumnAssigner = new ActionBlock<string[]>(row =>
           {
           for (int i = 0; i < row.Count(); i++)
           {
                   ColumnCollection.ElementAt(i).Push(row[i]);
            }
           });

            StringSplit.LinkTo(ColumnAssigner, new DataflowLinkOptions { PropagateCompletion = true });

            string s;
            Parallel.For(0, Context.LinesToScan, new ParallelOptions {MaxDegreeOfParallelism =  1}, x =>
             {
                 x++;
                 if ((s = reader.ReadLine()) != null)
                 {
                     StringSplit.Post(s);
                 }
             });
            StringSplit.Complete();

            await ColumnAssigner.Completion;

            DoSuggestType(Context.StringPadding);
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
            int columncount = reader.ReadLine().Split(Context.FieldDelimiter).Count();
            for (int i = 0; i < columncount; i++)
            {
                ColumnCollection.Add(new ConcurrentStack<string>());
            }
        }

        private void DoSuggestType(double stringpadding)
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

                
                if (CouldBeBoolean) { column.Push("BIT"); }
                else if (CouldBeInteger) { column.Push("INT"); }
                else if (CouldBeDouble) { column.Push("FLOAT"); }
                else if (CouldBeChar) { column.Push("CHAR"); }
                else
                {
                    int length = 0;
                    foreach (string s in column)
                    {
                        if (s.Length > length) { length = s.Length; }
                    }
                    length = (int)(length * (stringpadding / 100));//add padding and round to int
                    if (length == 0) { length = 500; } //incase of empty column just give 500
                    column.Push("VARCHAR(" + length +")"); 
                }
            });
        }

        
    }
}
