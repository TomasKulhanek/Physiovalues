using System;
using System.Collections.Generic;
using System.IO;

namespace DymolaModel
{

    public class MatFile
    {

        BinaryReader b;
        int[] def = new int[5];
        string sectionName;
        int dataTypeSize;

        public class Curve
        {
            public Curve(string name)
            {
                variableName = name;
                dataInfoIdx = -1;
                dataset = -1;
                dataIdx = -1;
            }
            public Curve(string name, int dataInfoIdx)
            {
                this.variableName = name;
                this.dataInfoIdx = dataInfoIdx;
                this.dataset = -1;
                this.dataIdx = -1;
            }
            public string variableName;
            public int dataInfoIdx; //-1 if not valid
            public int dataset;     //-1 if not valid
            public int dataIdx;     //-1 if not valid
            public double[] data;
        };
        public static List<Curve> vars = null;

        MatFile(BinaryReader b)
        {
            this.b = b;
        }

        /** Read header of each section in "dsres.mat" file. **/
        void readSectionHeader()
        {
            def[0] = b.ReadInt32(); // type of array field index
            def[1] = b.ReadInt32(); def[2] = b.ReadInt32(); //array sizes
            def[3] = b.ReadInt32(); // reserved     
            def[4] = b.ReadInt32(); // length of section name
            switch (def[0])
            {
                case 51: dataTypeSize = 1; break; //char8
                case 20: dataTypeSize = 4; break; //int32
                case 10: dataTypeSize = 4; break; //float32
                default: dataTypeSize = 4; break;
            }
            System.Console.Write(def[0] + ", " + def[1] + ", " + def[2] + ", " + def[3] + ", " + def[4] + ", " + dataTypeSize + ", ");
            sectionName = new string(b.ReadChars(def[4]), 0, def[4] - 1); // section name: Aclass, name, description, dataInfo, data_1, data_2
            System.Console.WriteLine(sectionName + "; (tm)");
        }

        /** Read all names and dataInfoIdx values from "dsres.mat" file. 
         *  Array vars[def[2]] must be allocated!! **/
        void readNameSection()
        {
            char[] tmp = null;
            for (int i = 0; i < def[2]; i++)
            {
                tmp = b.ReadChars(def[1]);
                //int j = 0;
                //while (j < def[1] && (tmp[j] != ' ')) j++; //tomaton bug space in the middle might be significant, commented out
                vars.Insert(i, new Curve(new string(tmp).Trim(), i));
            }
        }

        /** Read dataInfoIdx values to curves determined by variable name in "dsres.mat" file. 
         *  Array vars[def[2]] must be allocated!! **/
        void readReducedNameSection()
        {
            string tmp = null;
            int fromVarIdx = 0; //increase speed, when names are sorted
            for (int i = 0; i < def[2]; i++)
            {
                char[] tmpChars = b.ReadChars(def[1]);
                int j = 0;
                while (j < def[1] && (tmpChars[j] != ' ')) j++;
                tmp = new string(tmpChars, 0, j);
                for (int c_offset = 0; c_offset < vars.Capacity; c_offset++)
                {
                    //   foreach (Curve c in vars)
                    if (tmp.CompareTo(vars[(fromVarIdx + c_offset) % vars.Capacity].variableName) == 0)
                    {
                        vars[(fromVarIdx + c_offset) % vars.Capacity].dataInfoIdx = i;
                        fromVarIdx += c_offset + 1;
                        break;
                    }
                }
            }
        }

        /** Read type of data and data indexes of curves determined by dataInfoIdx from "dsres.mat" file .**/
        void readDataInfoSectionSimple()
        {
            Int32[] dataInfo = new Int32[def[1]];
            for (int i = 0; i < def[2]; i++)
            {
                for (int j = 0; j < def[1]; j++) dataInfo[j] = b.ReadInt32();
                /*foreach (Curve c in vars)
                    if (c.dataInfoIdx == i)
                    {
                        c.dataset = dataInfo[0];
                        if (c.dataset == 0) c.dataset = 2; //uprednostni krivku
                        if (dataInfo[1] < 0)
                            dataInfo[1] = -dataInfo[1];
                        c.dataIdx = dataInfo[1] - 1;
                        //WTF - bug - constants have wrong values - from index 0 - it should have correct index
                        //tomaton fix bug #101
                        //if (c.dataset == 1) c.dataIdx = 0; 
                    }*/
            }
        }

        /** Read type of data and data indexes of curves determined by dataInfoIdx from "dsres.mat" file .**/
        void readDataInfoSection()
        {
            Int32[] dataInfo = new Int32[def[1]];
            var row = b.ReadBytes(def[1] *def[2] * sizeof(Int32));
            var curveindexes = new List<Curve>();
            for (int j = 0; j < vars.Capacity;j++ )
            {
                curveindexes.Insert(vars[j].dataInfoIdx,vars[j]);
            }
            for (int i = 0; i < def[2]; i++)
                {
                    //for (int j = 0; j < def[1]; j++) dataInfo[j] = b.ReadInt32(); //optimize reading
                    Buffer.BlockCopy(row, i * def[1] * sizeof(Int32), dataInfo, 0, def[1] * sizeof(Int32));
                    //var vars2 = vars.FindAll(c => c.dataInfoIdx == i);
                    //foreach (Curve c in vars2)
                    //    if (c.dataInfoIdx == i)
                    var c = curveindexes[i];
                    if (c != null)
                    {
                        c.dataset = dataInfo[0];
                        if (c.dataset == 0) c.dataset = 2; //uprednostni krivku
                        if (dataInfo[1] < 0)
                            dataInfo[1] = -dataInfo[1];
                        c.dataIdx = dataInfo[1] - 1;
                        //bug - constants have wrong values - from index 0 - it should have correct index
                        //tomaton fix bug #101
                        //if (c.dataset == 1) c.dataIdx = 0; 
                    }

                }
        }

        /** Skip section data. **/
        void seekSection()
        {
            b.BaseStream.Seek(dataTypeSize * def[1] * def[2], System.IO.SeekOrigin.Current);
        }

        /** Read curves data (in single precision) determined by dataIdx from "dsres.mat" file. **/
        void readDataSection(int dataset)
        {
            foreach (Curve c in vars)
                if (c.dataset == dataset)
                    c.data = new double[def[2]];

            float[] row = new float[def[1]];
            for (int i = 0; i < def[2]; i++)
            {
                for (int j = 0; j < def[1]; j++) row[j] = b.ReadSingle();
                foreach (Curve c in vars)
                    if (c.dataset == dataset)
                    {
                        if (c.dataIdx < 0 || c.dataIdx >= row.Length)
                            Console.Error.WriteLine("Error dataIdx is .mat file for variable " + c.variableName + " -> dataInfo(:," + (c.dataInfoIdx + 1) + ") is out of bounds!");
                        c.data[i] = row[c.dataIdx];
                    }
            }
        }

        /** Read curves data (in single precision) determined by dataIdx from "dsres.mat" file. **/
        int rowindex = 0;
        public float[] getRowDataSection()
        {
            if (rowindex < def[2])
            {
                int dataset = 2;
            float[] row = new float[def[1]];
            byte[] rowb = new byte[def[1]*sizeof (float)];
//            for (int i = 0; i < def[2]; i++)
            //{
                //for (int j = 0; j < def[1]; j++) row[j] = b.ReadSingle();
            //reads a row in single call
            rowb = b.ReadBytes(def[1]*sizeof (float)); 
            if ((rowindex % 10) == 0) Console.Write(rowindex+" ");
            //copy the data into float array
            Buffer.BlockCopy(rowb,0,row,0,def[1]*sizeof(float));
                /*foreach (Curve c in vars)
                    if (c.dataset == 2)
                    {
                        if (c.dataIdx < 0 || c.dataIdx >= row.Length)
                            Console.Error.WriteLine("Error dataIdx is .mat file for variable " + c.variableName + " -> dataInfo(:," + (c.dataInfoIdx + 1) + ") is out of bounds!");
                        c.data[i] = row[c.dataIdx];
                    }*/
            
                rowindex++;
                return row;
            }
            else return null;

        }

        public int getRowSizeinBytes()
        {//TODO
            return (vars.Capacity)*sizeof (float);
        }

        public byte[] getRowDataSectionBytes()
        {
            if (rowindex < def[2])
            {
                int dataset = 2;
                //float[] row = new float[def[1]];
                byte[] rowb = new byte[def[1]*sizeof (float)];
                //            for (int i = 0; i < def[2]; i++)
                //{
                //for (int j = 0; j < def[1]; j++) row[j] = b.ReadSingle();
                //reads a row in single call
                rowb = b.ReadBytes(def[1]*sizeof (float));
                var myrowb = new byte[vars.Capacity*sizeof (float)];
                int myrowbindex = 0;
                //for (var i=0;i<)
                if ((rowindex%10) == 0) Console.Write(rowindex + " ");
                //copy the data into float array
                //Buffer.BlockCopy(rowb, 0, row, 0, def[1] * sizeof(float));
                foreach (Curve c in vars)
                    if (c.dataset == 2)
                    {
                        if (c.dataIdx < 0 || c.dataIdx >= (rowb.Length)/sizeof (float))
                            Console.Error.WriteLine("Error dataIdx is .mat file for variable " + c.variableName +
                                                    " -> dataInfo(:," + (c.dataInfoIdx + 1) + ") is out of bounds!");
                        Buffer.BlockCopy(rowb, c.dataIdx*sizeof (float), myrowb, myrowbindex, sizeof (float));
                        myrowbindex += sizeof (float);
                    }
                    else if (c.dataset == 1) //const values are coppied to the row
                    {
                        //Buffer.BlockCopy(c.data[0], c.dataIdx * sizeof(float), myrowb, myrowbindex, sizeof(float));
                        float myfloat = (float) c.data[0];
                        Buffer.BlockCopy(BitConverter.GetBytes(myfloat), 0, myrowb, myrowbindex, sizeof (float));
                        myrowbindex += sizeof(float);                        
                    }
            

            rowindex++;
                return myrowb;
            }
            else return null;

        }

        //reads variable from the givven position in mat file, ignores other variables to keep memory usage low
        //buggy bad solution
        public double[] readVariable(int position)
        {
            //keeps the current position as beginning of data
            var dataposition = b.BaseStream.Position;

            var data = new double[def[2]];
            for (int i=0;i<def[2];i++)
            {                
                //skip preceeding items
                b.BaseStream.Seek((position)*sizeof (float),System.IO.SeekOrigin.Current);
                data[i] = b.ReadSingle();
                //skip following items
                b.BaseStream.Seek((def[1] - position - 1)*sizeof (float), System.IO.SeekOrigin.Current);
            }
            //return reading position to the beginning of data
            b.BaseStream.Seek(dataposition, SeekOrigin.Begin);
            return data;
        }

        /** visual test **/
        void printVariables()
        {
            foreach (Curve c in vars)
            {
                System.Console.WriteLine(c.variableName + ", " + c.dataset + ", " + c.dataInfoIdx + ", " + c.dataIdx + ", " + c.data.Length);
                System.Console.ReadKey();
                foreach (double val in c.data)
                {
                    System.Console.Write((double)val);
                    System.Console.Write(" ");
                }
                System.Console.WriteLine();
            }
        }


        /** MatFile must be set! **/
        public static string[] getCurveNames()
        {
            string[] names;
            names = new string[vars.Capacity - 1];
            for (int i = 1; i < vars.Capacity; i++)
            {
                names[i - 1] = vars[i].variableName;
            }
            return names;
        }

        /** MatFile must be set! **/
        public static string[] getCurveNamesWithTime()
        {
            string[] names;
            names = new string[vars.Capacity];
            for (int i = 0; i < vars.Capacity; i++)
            {
                names[i] = vars[i].variableName;
            }
            return names;
        }

        /** MatFile must be set! **/
        public static double[] getTimePoints()
        {
            return vars[0].data;
        }

        public void Close()
        {
            b.Close();
        }
         

        static void copyCurveToArray(ref double[,] dt, Curve c, int curveIdxInArray, int data_size, bool transposed)
        {
            if (c.dataset == 2)
                for (int j = 0; j < data_size; j++)
                    if(!transposed)
                        dt[curveIdxInArray, j] = c.data[j]; //data zostavaju konstantne v kazdom case
                    else
                        dt[j, curveIdxInArray] = c.data[j]; //data zostavaju konstantne v kazdom case
            else if (c.dataset == 1)
                for (int j = 0; j < data_size; j++)
                    if(!transposed)
                        dt[curveIdxInArray, j] = c.data[0];
                    else
                        dt[j, curveIdxInArray] = c.data[0];
            else
                Console.Error.WriteLine("divny dataset = " + c.dataset + " premennej " + c.variableName + " dataIdx =" + c.dataIdx + ", dataInfoIdx =" + c.dataInfoIdx);
        }

        static void copyCurveToArray2(ref double[][] dt, Curve c, int curveIdxInArray, int data_size, bool transposed)
        {
            if (c.dataset == 2)
                for (int j = 0; j < data_size; j++)
                    if (!transposed)
                        dt[curveIdxInArray][j] = c.data[j]; //data zostavaju konstantne v kazdom case
                    else
                        dt[j][curveIdxInArray] = c.data[j]; //data zostavaju konstantne v kazdom case
            else if (c.dataset == 1)
                for (int j = 0; j < data_size; j++)
                    if (!transposed)
                        dt[curveIdxInArray][j] = c.data[0];
                    else
                        dt[j][curveIdxInArray] = c.data[0];
            else
                Console.Error.WriteLine("divny dataset = " + c.dataset + " premennej " + c.variableName + " dataIdx =" + c.dataIdx + ", dataInfoIdx =" + c.dataInfoIdx);
        }

        static int getValidCurvesNumber()
        {
            int n=0;
            for (int i = 0; i < vars.Capacity; i++)
                if (vars[i].dataInfoIdx < 0 || vars[i].dataIdx < 0 || vars[i].dataset < 0 || vars[i].data == null)
                    vars[i].dataInfoIdx = -1;
                else n++;
            return n;
        }

        /** MatFile must be set! 
         * return
         *      transposed:   [i,:] is the point in step i,   [:,j] is the values of curve j 
         *      !transposed:  [j,:] is the values of curve j, [:,i] is the point in step i
         **/
        public static double[,] getDataWithoutTimePoints(bool transposed)
        {

            double[,] dt;
            if(!transposed)
                dt = new double[getValidCurvesNumber() - 1, vars[0].data.Length];
            else
                dt = new double[vars[0].data.Length, getValidCurvesNumber() - 1];

            int j=0;
            for (int i = 1; i < vars.Capacity; i++)
                if (vars[i].dataInfoIdx >= 0)
                {
                    copyCurveToArray(ref dt, vars[i], j, vars[0].data.Length, transposed);
                    j++;
                }
            return dt;
        }

        /** MatFile must be set! 
         * return
         *      transposed:   [i,:] is the point in step i,   [:,j] is the values of curve j 
         *      !transposed:  [j,:] is the values of curve j, [:,i] is the point in step i
         **/
        public static double[,] getDataWithTimePoints(bool transposed)
        {
            double[,] dt;
//            dt = new double[getValidCurvesNumber(), vars[0].data.Length];
            if (!transposed)
                dt = new double[getValidCurvesNumber(), vars[0].data.Length];
            else
                dt = new double[vars[0].data.Length, getValidCurvesNumber()];

            int j = 0;
            for (int i = 0; i < vars.Capacity; i++)
                if (vars[i].dataInfoIdx >= 0)
                {
                    copyCurveToArray(ref dt, vars[i], j, vars[0].data.Length, transposed);
                    j++;
                }
            return dt;
        }

        //tomaton added a function returning array of array - possible to index a subarray
        public static double[][] getDataWithTimePoints2(bool transposed)
        {
            double[][] dt;
            //            dt = new double[getValidCurvesNumber(), vars[0].data.Length];
            if (!transposed)
                dt = new double[getValidCurvesNumber()][];//[vars[0].data.Length];
            else
                dt = new double[vars[0].data.Length][];//[getValidCurvesNumber()];

            int j = 0;
            for (int i = 0; i < vars.Capacity; i++)
                if (vars[i].dataInfoIdx >= 0)
                {
                    copyCurveToArray2(ref dt, vars[i], j, vars[0].data.Length, transposed);
                    j++;
                }
            return dt;
        }


        /** Read data of time and selected variables from filename. 
         * After calling data are accesible by functions: getCurveNames(), getTimePoints(), getDataWithoutTimePoints() **/
        public static void readSelectedFromFile(string[] varNames, string filename)
        {
            if (!File.Exists(filename))
            {
                System.Console.WriteLine("ERROR: Subor " + filename + " neexistuje!");
                throw new FileNotFoundException("potrebny subor s vysledkami neexistuje!", filename);
            }
            MatFile p = new MatFile(new BinaryReader(File.Open(filename, FileMode.Open)));

            vars = new List<Curve>(varNames.Length + 1);  //read only selected curves, if exist
            vars[0] = new Curve("Time");
            for (int i = 0; i < varNames.Length; i++)
                vars[i + 1] = new Curve(varNames[i]);

            p.readSectionHeader();
            p.seekSection();  //Aclass
            p.readSectionHeader();
            p.readReducedNameSection(); //name
            p.readSectionHeader();
            p.seekSection();    //description
            p.readSectionHeader();
            p.readDataInfoSection();    //dataInfo
            p.readSectionHeader();
            p.readDataSection(1);  //data_1
            p.readSectionHeader();
            p.readDataSection(2);  //data_2
            p.Close(); //tomaton fix - not closed after read cause error on next iteration - file access denied
        }

        /** Read all data of time and selected variables from filename.
         * After calling data are accesible by functions: getCurveNames(), getTimePoints(), getDataWithoutTimePoints() **/
        public static void read(string filename)
        {
            var p = readHeader(filename,false);
            p.readVariables();
            p.Close();
        }



        static char[,] stringsToData(string[] strings, bool transposed)
        {
            int uniformLength = 0;
            foreach (string s in strings)
                if (uniformLength < s.Length) uniformLength = s.Length;

            char[,] dt=null;
            if (transposed) // name, description
            { dt = new char[uniformLength, strings.Length]; }
            else // aClass
            { dt = new char[strings.Length, uniformLength]; }

            for (int i = 0; i < strings.Length; i++)
                for (int j = 0; j < uniformLength; j++)
                    if (transposed) // name, description
                        dt[j, i] = (j < strings[i].Length) ? strings[i][j] : ' '; //((j == strings[i].Length) ? (char)0 :' ');
                    else // aClass
                        dt[i, j] = (j < strings[i].Length) ? strings[i][j] : ' ';

            return dt;
        }


        static void writeSection<T>(BinaryWriter bw, string sectionName, T[,] data, TypeCode typeCode, bool transposeData)
        { 
            switch (typeCode) 
            {
                case TypeCode.Double: bw.Write(10); break;
                case TypeCode.Int32: bw.Write(20); break;
                case TypeCode.Char: bw.Write(51); break;
                default: throw new Exception("type of data not known!");
            }

            if (transposeData)
            {
                bw.Write(data.GetLength(0));
                bw.Write(data.GetLength(1));
            } else {
                bw.Write(data.GetLength(1));
                bw.Write(data.GetLength(0)); 
            }
            bw.Write(0);
            bw.Write(sectionName.Length + 1);
            bw.Write(sectionName.ToCharArray());
            bw.Write((char)0);

            if (transposeData)
                for (int j = 0; j < data.GetLength(1); j++)
                    for (int i = 0; i < data.GetLength(0); i++)
                    {
                        object val = data[i, j];
                        switch (typeCode)
                        {
                            case TypeCode.Double: bw.Write((float)((double)val)); break;
                            case TypeCode.Int32: bw.Write((int)val); break;
                            case TypeCode.Char: bw.Write((char)val); break;
                        }
                    }
            else
                for (int i = 0; i < data.GetLength(0); i++)
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        object val = data[i, j];
                        switch (typeCode)
                        {
                            case TypeCode.Double: bw.Write((float)((double)val)); break;
                            case TypeCode.Int32: bw.Write((int)val); break;
                            case TypeCode.Char: bw.Write((char)val); break;
                        }
                    }

            
        }

        static int[,] createSimpleDataInfo(int numberOfVariables,int dataset)
        {
            int[,] dt = new int[4, numberOfVariables];
            for (int i = 0; i < numberOfVariables; i++)
            {
                dt[0, i] = dataset; dt[1, i] = i+1; dt[2, i] = 0; dt[3, i] = -1;
            }
            return dt;
        }

        public static void write(string filename, string[] variableNames, double[,] dataWithTime)
        {
            string[] aclassData = { "Atrajectory", "1.1", "", "binTrans" };
            string[] descriptionData = new string[variableNames.Length];
            double[,] constData = new double[1, 1];
            constData[0, 0] = 0;
            for(int i=0; i<variableNames.Length; i++) descriptionData[i]=" ";
            BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Create));

            writeSection(bw, "Aclass", stringsToData(aclassData, true), TypeCode.Char, false);
            writeSection(bw, "name", stringsToData(variableNames, false), TypeCode.Char, false);
            writeSection(bw, "description", stringsToData(descriptionData, false), TypeCode.Char, false);
            writeSection(bw, "dataInfo", createSimpleDataInfo(variableNames.Length, 2), TypeCode.Int32, true);
            writeSection(bw, "data_1", constData, TypeCode.Double, true);
            writeSection(bw, "data_2", dataWithTime, TypeCode.Double, true);

            bw.Close();
        }

        public static MatFile readHeader(string filename)
        {
            return readHeader(filename, true);
        }


        public static MatFile readHeader(string filename,bool simple)
        {
            if (!File.Exists(filename))
            {
                System.Console.WriteLine("ERROR: Subor " + filename + " neexistuje v adresari " + Directory.GetCurrentDirectory() + "!");
                throw new FileNotFoundException("potrebny subor s experimentalnymi datami neexistuje!", filename);
            }
            MatFile p = new MatFile(new BinaryReader(File.Open(filename, FileMode.Open)));
            p.readSectionHeader();
            p.seekSection();  //Aclass
            p.readSectionHeader();
            vars = new List<Curve>(p.def[2]);//Curve[p.def[2]]; //read all curves
            p.readNameSection(); //name
            p.readSectionHeader();
            p.seekSection();    //description
            p.readSectionHeader();
            //if (simple) p.readDataInfoSectionSimple(); else   //dataInfo, we need dataInfo - read constant values etc.
            p.readDataInfoSection();    //dataInfo
            p.readSectionHeader();
            p.readDataSection(1);  //data_1
            p.readSectionHeader();
            return p;
        }

        //reads all variables at once - on big files cause outofmemoryexception
        public void readVariables()
        {
            readDataSection(2);  //data_2, bug out of memory on big files            
        }

        /** Read curves data (in single precision) determined by dataIdx from "dsres.mat" file. **/
        public double[] readVariableByOne()
        {
            //foreach (Curve c in vars)
                //if (c.dataset == 2)
                    //c.data = new double[def[2]];
            var data = new double[def[2]];
            float[] row = new float[def[1]];
            for (int i = 0; i < def[2]; i++)
            {
                for (int j = 0; j < def[1]; j++) row[j] = b.ReadSingle();
                foreach (Curve c in vars)
                    if (c.dataset == 2)
                    {
                        if (c.dataIdx < 0 || c.dataIdx >= row.Length)
                            Console.Error.WriteLine("Error dataIdx is .mat file for variable " + c.variableName + " -> dataInfo(:," + (c.dataInfoIdx + 1) + ") is out of bounds!");
                        data[i] = row[c.dataIdx];
                    }
            }
            return data;
        }

    }
}
