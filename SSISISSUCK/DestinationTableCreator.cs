using System;
using System.IO;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSISISSUCK
{
    public class DestinationTableCreator
    {
        private readonly PipeLineContext Context;
        public DestinationTableCreator(PipeLineContext c)
        {
            Context = c;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public void CreateTable()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("create table [" + Context.DestinationTableName + "](");

            string Name;
            //build create statement
            for (int i = 0; i < Context.ColumnNames.Length; i++)
            {
                // escape brackets inside column names
                if (Context.ColumnNames[i].Contains("]"))
                {
                    Name = Context.ColumnNames[i].Replace("]", "]]");
                }
                else
                {
                    Name = Context.ColumnNames[i];
                }
                //build create column statement
                sb.AppendLine("[" + Name + "]" + " " + Context.DataTypes[i] + ",");
            }
            sb.Remove(sb.Length - 3, 1);
            sb.Append(")");

            using (SqlConnection con = new SqlConnection(Context.ConnectionString))
            {
#pragma warning disable S3649 // User-provided values should be sanitized before use in SQL statements
                using (SqlCommand comm = new SqlCommand(sb.ToString(), con))
#pragma warning restore S3649 // User-provided values should be sanitized before use in SQL statements
                {
                    con.Open();
                    comm.ExecuteNonQuery();
                    con.Close();
                }
            }
        }


    }
}
