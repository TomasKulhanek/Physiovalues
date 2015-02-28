using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Default2Ini
{
    class HumModFileConvert : HumModConvert
    {

        public string ReplaceRow(string strLine, string varValue, Regex regex,  bool fromFile)
        {
            return regex.Replace(strLine,
                "varName = \"$1\", varValue = " + varValue + ", initType=Physiolibrary.Utilities.Init.FromFile");
        }

    }
}
