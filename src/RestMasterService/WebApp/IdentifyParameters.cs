using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestMasterService.WebApp
{
    public class IdentifyParameters //DTO
    {
        public bool IsActive { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public string Name { get; set; }
        public double Value { get; set; }
        public IdentifyParameters (string[] itemvalues)
        {
            if (itemvalues.Length!=5) throw new Exception("Identify parameters constructor needs 5 items, string[5] is expected.");
            Name = itemvalues[0];
            IsActive = Boolean.Parse(itemvalues[1]);
            Value = Double.Parse(itemvalues[2]);
            Min = Double.Parse(itemvalues[3]);
            Max = Double.Parse(itemvalues[4]);
        }
    }
}