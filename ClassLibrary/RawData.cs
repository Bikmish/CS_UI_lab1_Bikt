using System;
using System.IO;

namespace ClassLibrary
{
    public delegate double FRaw(double x);
    public enum FRawEnum { Linear, Cubic, Random };
    public class RawData
    {
        public double[] EndsCoords { get; set; }
        public int NumNodes { get; set; }
        public bool IsUniform { get; set; }
        public FRaw Func { get; set; }
        public double[] NodeCoords;
        public double[] NodeValues;
        public RawData(double[] endsCoords, int nNodes, bool isUni, FRaw func)
        {
            (EndsCoords, NumNodes, IsUniform, Func, NodeCoords, NodeValues) = (endsCoords, nNodes, isUni, func, new double[nNodes], new double[nNodes]);
            AddValues(this);
        }
        public RawData() { }
        public void AddValues(RawData rd)
        {
            var delta = (rd.EndsCoords[1] - rd.EndsCoords[0]) / ((rd.NodeValues.Length - 1) == 0 ? 1 : rd.NodeValues.Length - 1);
            for (int i = 0; i < rd.NodeValues.Length; ++i)
                (rd.NodeCoords[i], rd.NodeValues[i]) = (rd.IsUniform ? rd.EndsCoords[0] + i * delta : rd.EndsCoords[0] + ((i==0 || i== rd.NodeValues.Length-1) ? i : i - new Random().NextDouble()) * delta, rd.Func(i));
        }
        public RawData(string filename) => Load(filename, this); //out this???
        public static double f1(double x) => x + 5;
        public static double f2(double x) => x * x * x + 1;
        public static double f3(double x) => new Random().NextDouble() * x;
        public bool Save(string filename)
        {
            (FileStream stream, StreamWriter writer, bool success) = (null, null, true);
            try
            {
                stream = File.Create(filename);
                writer = new StreamWriter(stream);
                writer.WriteLine(EndsCoords[0].ToString());
                writer.WriteLine(EndsCoords[1].ToString());
                writer.WriteLine(NumNodes.ToString());
                writer.WriteLine(IsUniform.ToString());

                if (Func == f2)
                    writer.WriteLine(FRawEnum.Cubic.ToString());
                else if (Func == f3)
                    writer.WriteLine(FRawEnum.Random.ToString());
                else
                    writer.WriteLine(FRawEnum.Linear.ToString());

                writer.WriteLine(NodeCoords.Length.ToString());
                for (int i = 0; i < NodeCoords.Length; ++i)
                {
                    writer.WriteLine(NodeCoords[i].ToString());
                    writer.WriteLine(NodeValues[i].ToString());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Saving error! {e}\n");
                success = false;
            }
            finally
            {
                if (writer != null) { writer.Dispose(); }
                if (stream != null) { stream.Close(); }
            }
            return success;
        }
        public static bool Load(string filename, RawData rd)
        {
            (FileStream stream, StreamReader reader, bool success) = (null, null, true);
            try
            {
                stream = File.OpenRead(filename);
                reader = new StreamReader(stream);

                rd.EndsCoords = new double[2];
                rd.EndsCoords[0] = Convert.ToDouble(reader.ReadLine());
                rd.EndsCoords[1] = Convert.ToDouble(reader.ReadLine());
                rd.NumNodes = Convert.ToInt32(reader.ReadLine());
                rd.IsUniform = Convert.ToBoolean(reader.ReadLine());
                
                var func = reader.ReadLine();
                if (func == FRawEnum.Cubic.ToString())
                    rd.Func = f2;
                else if (func == FRawEnum.Random.ToString())
                    rd.Func = f3;
                else
                    rd.Func = f1;

                var valsCount = Convert.ToInt32(reader.ReadLine());
                (rd.NodeCoords, rd.NodeValues) = (new double[valsCount], new double[valsCount]);
                for (int i = 0; i < valsCount; i++)
                {
                    rd.NodeCoords[i] = Convert.ToDouble(reader.ReadLine());
                    rd.NodeValues[i] = Convert.ToDouble(reader.ReadLine());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Loading error! {e}\n");
                success = false;
            }
            finally
            {
                if (reader != null) { reader.Dispose(); }
                if (stream != null) { stream.Close(); }
            }
            return success;
        }
    }
}
