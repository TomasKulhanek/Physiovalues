using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Default2Ini
{
    interface IConvertBehavior
    {
        string ConvertConstant(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool usednext);
        string ConvertVariables(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool usednext);
        bool CheckConstant(string strLine);
        bool CheckVariables(string strLine);

    }
}
