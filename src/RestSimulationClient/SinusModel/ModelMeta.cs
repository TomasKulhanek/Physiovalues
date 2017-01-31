using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DymolaModel
{
    public struct VariableMeta
    {
        public string Description;
        public string Units;
        public double InitialValue;
    }

    public class ModelMeta
    {
        //holds path to execute modelinstance which will generate the list of variables and values during simulation - DymosimModel
        public string ModelInstancePath { get; set; }
        public string ModelInstanceOutput { get; set; }
        //public Dictionary<string,VariableMeta> VariableMetas { get; set; }
        //WORKAROUND asp.net json has problems to serialize/deserialize Dictionary<string,custom type> and interoperable with RESTSharp client JSON serializer
        // changed to 
        public Dictionary<string,List<string>> VariableMetas { get; set; }
    }

}
