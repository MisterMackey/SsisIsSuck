using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Data;

namespace SSISISSUCK
{
    public class Inserter
    {

        //public ConcurrentQueue<List<string>> Rows {get;set;}
        public bool done { get; set; }
        private PipeLineContext context;

        public Inserter(PipeLineContext context)
        {
            this.context = context;

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
        public void BigQueu(Object RowsCollection)
        {
            ConcurrentQueue<List<string>> Rows = (ConcurrentQueue<List<string>>)RowsCollection;
            StringBuilder sb = new StringBuilder();
            DataTable dataTable = new DataTable();
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
            while (!done)
            {

                int batchsize = 10000;
                int count = 0;
                while (!done && count < batchsize)
                {
                    List<string> row;
                    if (Rows.TryDequeue(out row))
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
                    Bulk.DestinationTableName = "dbo.[AxiomData]";
                    Bulk.BatchSize = 50000;
                    Bulk.WriteToServer(dataTable);

                }
                dataTable.Clear();

            }
        }
    }
}
