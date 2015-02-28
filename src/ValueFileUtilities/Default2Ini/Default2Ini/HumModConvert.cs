using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Default2Ini
{
    class HumModConvert : IConvertBehavior
    {
        public string ConvertConstant(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool fromFile)
        {
            var regex = new Regex("varName[ ]*=[ ]*\"([^\"]*)[^\\)]*");
            var match = regex.Match(strLine);
            if (match.Success)
            {
                var varValue = pairs[match.Groups[1].Value];//Regex.Find(strLine, regex)];
                return ReplaceRow(strLine, varValue, regex);
            }
            else return strLine;
        }

        public string ReplaceRow(string strLine, string varValue, Regex regex)
        {
                return regex.Replace(strLine, "varName = \"$1\", varValue = " + varValue + ", initType=Physiolibrary.Utilities.Init.NoInit");
        }

        public string ConvertVariables(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool fromFile)
        {
            var regex = new Regex("varName[ ]*=[ ]*\"([^\"]*)\"[^\\)]*");
            var match = regex.Match(strLine);
            if (match.Success)
            {
                var varValue = pairs[match.Groups[1].Value];//Regex.Find(strLine, regex)];
                return ReplaceRow(strLine, varValue, regex); //regex.Replace(strLine, "varName = \"$1\", varValue = " + varValue + " initType=Init.NoInit");
            }
            else return strLine;
            //var varValue = pairs[Regex.Find(strLine,regex)];
            //return Regex.Replace(strLine, regex,"varname = \"$1\" varValue = \"" + varValue + "\"");
        }

        public bool CheckConstant(string strLine)
        {
            return Regex.IsMatch(strLine, "[ ]*Physiolibrary\\.Utilities\\.ConstantFromFile");
        }

        public bool CheckVariables(string strLine)
        {
            return Regex.IsMatch(strLine, "[ ]*Variable");
        }

    }
}
