using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Default2Ini
{
    public class PhysiomodelConvert : IConvertBehavior
    {
        public string ConvertConstant(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool usednext)
        {
            return strLine;
        }

        public string ReplaceRow(string strLine, string varValue, Regex regex)
        {
                return regex.Replace(strLine, "varName = \"$1\", varValue = " + varValue );
        }
        public string ConvertVariables(string strLine, Dictionary<string, string> pairs, string nextStrLine, ref bool usednext)
        {
            var regex = new Regex("varName[ ]*=[ ]*\"([^\"]*)\"");
            var regexvarValue = new Regex(",[ ]*varValue[ ]*=[ ]*[-+0-9.e]*");
            var regextwolines = new Regex("varName[ ]*=[ ]*$");
            var match2lines = regextwolines.Match(strLine);
            usednext = match2lines.Success;
            if (usednext) 
                strLine = strLine + nextStrLine;
            var match = regex.Match(strLine);
            if (match.Success)
            {

                if (pairs.ContainsKey(match.Groups[1].Value))
                {
                    var varValue = pairs[match.Groups[1].Value]; //Regex.Find(strLine, regex)];
                    strLine = regexvarValue.Replace(strLine, ""); //delete any previous varValue
                    return ReplaceRow(strLine, varValue, regex);
                    //regex.Replace(strLine, "varName = \"$1\", varValue = " + varValue + " initType=Init.NoInit");
                }
                else return strLine;

            }
            else return strLine;
        }

        public bool CheckConstant(string strLine)
        {
            return false;
        }

        public bool CheckVariables(string strLine)
        {
            return Regex.IsMatch(strLine, "[.]*varName");
        }
    }
}