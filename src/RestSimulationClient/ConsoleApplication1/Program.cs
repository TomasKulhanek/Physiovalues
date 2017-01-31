using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ConsoleApplication1
{
    class Program
    {
        [DllImport("CW2FMIDriver.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetVariableValuesTest( double[] value,int length);//, out double[] values);

        static void Main(string[] args)
        {
            Console.WriteLine("Test returning double");
            var number = new double[] {1.23,2.23,3.1,1.4};
            for (int i = 0; i < number.Length;i++ ) Console.WriteLine(number[i]);
            Console.WriteLine("calling c code");
            GetVariableValuesTest(number,number.Length);
            for (int i = 0; i < number.Length; i++) Console.WriteLine(number[i]);
            //Console.WriteLine(number[0]);
            Console.ReadKey();

        }
    }
}
