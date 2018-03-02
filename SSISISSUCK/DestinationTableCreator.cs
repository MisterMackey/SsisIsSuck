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
        private PipeLineContext Context;
        public DestinationTableCreator(PipeLineContext c)
        {
            Context = c;
        }

        public void CreateTable()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("create table [" + Context.DestinationTableName + "](");
            

            //build create statement
            for (int i = 0; i < Context.ColumnNames.Length; i++)
            {
                sb.AppendLine("[" + Context.ColumnNames[i] + "]" + " " + Context.DataTypes[i] + ",");
            }
            sb.Remove(sb.Length - 3, 1);
            sb.Append(")");

            using (SqlConnection con = new SqlConnection(Context.ConnectionString))
            {
                using (SqlCommand comm = new SqlCommand(sb.ToString(), con))
                {
                    con.Open();
                    comm.ExecuteNonQuery();
                    con.Close();
                }
            }
        }


    }
}
