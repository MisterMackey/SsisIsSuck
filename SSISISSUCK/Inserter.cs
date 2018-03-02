using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data;

namespace SSISISSUCK
{
    public class Inserter
    {

        //public ConcurrentQueue<List<string>> Rows {get;set;}
        public bool done { get; set; }
        private readonly PipeLineContext context;
        private ConcurrentQueue<List<string>> RowsCollection;
        public event Action FinishedWriting;
        public void StopWriting()
        {
            done = true;
        }
        private void OnFinishedWriting()
        {
            FinishedWriting?.Invoke();
        }
        public Inserter(PipeLineContext context, ConcurrentQueue<List<string>> RowsCollection)
        {
            this.context = context;
            this.RowsCollection = RowsCollection;
            done = false;
        }
        public void Queu(Object rowitem)
        {
            ConcurrentQueue<List<string>> Rows = (ConcurrentQueue<List<string>>)rowitem;
            int current = 0;
            StringBuilder sb = new StringBuilder();

                sb.AppendLine("insert into dbo.[AxiomData] VALUES ");
            int BatchSize = 1000;
            while (!done && current < BatchSize)
            {
                List<string> row = new List<string>();
                if (Rows.TryDequeue(out row))
                {
                    sb.Append("(");
                    foreach (string val in row)
                    {
                        sb.Append(@"'" + val.Replace("'", "''") + @"', ");
                    }
                    sb.Remove(sb.Length - 2, 2);
                    if (current != BatchSize-1)
                    {
                        sb.Append("),");
                    }
                    else
                    {
                        sb.Append(")");
                    }
                    sb.AppendLine();
                    current++;
                }
            }
            using (SqlConnection con = new SqlConnection(context.ConnectionString))
            {
                con.Open();
                SqlCommand com = new SqlCommand(sb.ToString(), con);
                com.ExecuteNonQuery();
                con.Close();
            }
                sb.Clear();
                current = 0;

            
        }
        /// <summary>
        /// creates a concurrent writer using the context of the parent object, dont make too many or your server will throw a hissyfit
        /// </summary>
        public void CreateConcurrentWriter()
        {
            ThreadPool.QueueUserWorkItem(BigQueu);
        }
        private void BigQueu(object state)
        {
            StringBuilder sb = new StringBuilder();
            DataTable dataTable = new DataTable();
            string TableName = context.DestinationTableName;
            foreach (string s in context.ColumnNames)
            {
                try
                {
                    dataTable.Columns.Add(s);
                }
                catch (System.Data.DuplicateNameException)
                {
                    dataTable.Columns.Add("if you are reading this, ur app done goofed");
                }
            }

            while (!(done && RowsCollection.IsEmpty))
            {

                int batchsize = 10000;
                int count = 0;
                while (!(done && RowsCollection.IsEmpty) && count < batchsize)
                {
                    List<string> row;
                    if (RowsCollection.TryDequeue(out row))
                    {
                        DataRow newRow = dataTable.NewRow();
                        for (int i = 0; i < row.Count; i++)
                        {
                            newRow[i] = row[i];
                        }
                        dataTable.Rows.Add(newRow);
                        count++;
                    }
                }
                using (SqlBulkCopy Bulk = new SqlBulkCopy(context.ConnectionString, SqlBulkCopyOptions.TableLock))
                {
                    Bulk.DestinationTableName = TableName;
                    Bulk.BatchSize = batchsize;
                    try
                    {
                        Bulk.WriteToServer(dataTable);
                    }
                    catch (Exception e)
                    {
                        e.Data["DataTableColumns"] = dataTable.Columns.ToString();
                        
                        throw new AggregateException("something went wrong trying to insert data, see innerexception", e);
                    }

                }
                dataTable.Clear();

            }

            OnFinishedWriting();
        }
    }
}
