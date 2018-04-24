using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSISISSUCK
{
    [ComVisible(true)
        ,Guid("76bfcb46-6860-48f6-bfeb-df932b931a59")
        ,InterfaceType(ComInterfaceType.InterfaceIsDual)]
    interface ICOMLoader
    {
        void SetFilePath(string filePath);

        void SetDestination(string connectionString);

        void SetDelimiter(char delim);

        void SetFirstLineContainsHeaderBool(bool val);

        void SetTableName(string name);

        void SetGuessDataType(bool val);

        void CreateTable();

        Task StartLoading();
    }
}
