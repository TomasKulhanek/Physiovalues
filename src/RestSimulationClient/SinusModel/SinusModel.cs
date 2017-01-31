using System;

namespace DymolaModel
{
    public class SinusModel 
    {
        public double Modeltime { get; set; }
        public double Variable { get; set; }
        private const double Ticker = 10; //jeden tik v milisekundach

        public void Step()
        {
            Modeltime += Ticker/1000;
            Variable = Compute(Modeltime);
        }

        public double Compute(double x)
        {
            return Math.Sin(x);
        }
    }
}