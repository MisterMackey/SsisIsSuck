using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SSISISSUCK
{
    public class PipeLineContext
    {
        public string ConnectionString { get; set; }
        public char FieldDelimiter { get; set; }
        public string PathToSourceFile { get; set; }
        public string DestinationTableName { get; set; }
        private string[] _ColumnNames;
        private string[] _DataTypes;
        public string[] ColumnNames
        {
            get
            {
                if (_ColumnNames != null) { return _ColumnNames; }
                else { _ColumnNames = GetColumnNames(); return _ColumnNames; }
            }
        }
        public string[] DataTypes
        {
            get
            {
                if (_DataTypes != null) { return _DataTypes; }
                else { _DataTypes = GetDataTypes(); return _DataTypes; }
            }
        }
        public int LinesToScan { get; set; }
        public bool FirstRowContainsHeaders { get; set; }
        public bool IsSuggestingDataTypes { get; set; }
        public double StringPadding { get; set; }
        public int ReaderBufferSize { get; set; }

        public PipeLineContext() //default values
        {
            ConnectionString = @"Server=.\SQLEXPRESS; Database=master; Integrated Security=True;";
            FieldDelimiter = ',';
            PathToSourceFile = string.Empty;
            DestinationTableName = "SSISSUCK";
            LinesToScan = 10000;
            FirstRowContainsHeaders = true;
            IsSuggestingDataTypes = true;
            StringPadding = 100;
            ReaderBufferSize = 50000;
        }

        private string[] GetDataTypes()
        {
            if (IsSuggestingDataTypes)
            {
                DataTypeSuggester suggest = new DataTypeSuggester(this);
                return  suggest.SuggestDataType().Result;
            }
            else
            {
                string[] dataTypes = new string[ColumnNames.Length];
                for (int i = 0; i < dataTypes.Length; i++)
                {
                    dataTypes[i] = "VARCHAR(500)";
                }
                return dataTypes;
            }
        }
        private string[] GetColumnNames()
        {
            StreamReader reader = new StreamReader(PathToSourceFile);
            string[] firstRow = reader.ReadLine().Split(FieldDelimiter).ToArray();
            if (!FirstRowContainsHeaders)
            {
                for (int i = 0; i < firstRow.Length; i++)
                {
                    firstRow[i] = "Column_" + (i + 1);
                }
                return firstRow;
            }
            else
            {
                List<string> uniques = new List<string>();
                for (int i = 0; i < firstRow.Length; i++)
                {
                    if (!uniques.Exists(x => x.Equals(firstRow[i])))
                    {
                        uniques.Add(firstRow[i]);
                    }
                    else
                    {
                        firstRow[i] += "_d";
                        uniques.Add(firstRow[i]);
                    }
                }
                return firstRow;
            }
        }
    }
}
