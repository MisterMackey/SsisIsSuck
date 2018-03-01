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
        private string connectionString;
        private string tableName;
        private string sourceFile;
        private int LinesToRead = 10000;
        public DestinationTableCreator(string connectionstring, string tablename, string sourceFile)
        {
            this.connectionString = connectionstring;
            tableName = tablename;
            this.sourceFile = sourceFile;
        }

        public void CreateTable(bool firstRowHasHeaders, char delimiter = ',', double stringPadding = 0, bool SuggestDataTypes = false)
        {
            StreamReader reader = new StreamReader(sourceFile);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("create table [" + tableName + "](");
            string[] dataTypes;
            string[] firstRow = reader.ReadLine().Split(delimiter);
            
            if (SuggestDataTypes)
            {
                DataTypeSuggester suggest = new DataTypeSuggester(delimiter);
                dataTypes = suggest.SuggestDataType(sourceFile, LinesToRead, firstRowHasHeaders, stringPadding).Result;
            }
            else
            {
                dataTypes = new string[firstRow.Length];
                for (int i = 0; i < dataTypes.Length; i++)
                {
                    dataTypes[i++] = "VARCHAR(500)";
                }
            }
            if (!firstRowHasHeaders)
            {
                for (int i = 0; i < firstRow.Length; i++)
                {
                    firstRow[i] = "Column_" + (i+1);
                }
            }
            else { firstRow = checkForDuplicates(firstRow); }
            //build create statement
            for (int i = 0; i < firstRow.Length; i++)
            {
                sb.AppendLine("[" + firstRow[i] + "]" + " " + dataTypes[i] + ",");
            }
            sb.Remove(sb.Length - 3, 1);
            sb.Append(")");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand comm = new SqlCommand(sb.ToString(), con))
                {
                    con.Open();
                    comm.ExecuteNonQuery();
                    con.Close();
                }
            }
        }

        public string[] checkForDuplicates(string[] vals)
        {
            List<string> uniques = new List<string>();
            for (int i = 0; i < vals.Length; i++)
            {
                if (!uniques.Exists(x => x.Equals(vals[i])))
                {
                    uniques.Add(vals[i]);
                }
                else
                {
                    vals[i] += "_d";
                }
            }
            return uniques.ToArray();

        }
    }
}
