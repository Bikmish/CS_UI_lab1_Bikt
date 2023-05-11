using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace ClassLibrary
{
    public struct SplineDataItem
    {
        public double Coord { get; set; }
        public double[] Values { get; set; }
        public SplineDataItem(double coord, double[] vals) => (Coord, Values) = (coord, vals);
        public string ToString(string format) => $"x = {Coord}\n\tvalue = {Values[0]}\n\tderiv1 = {Values[1]}\n\tderiv2 = {Values[2]}";
        public override string ToString() => Values != null ? $"x = {Coord:N2}\n\tvalue = {Values[0]:N2}\n\tderiv1 = {Values[1]:N2}\n\tderiv2 = {Values[2]:N2}" : "No item selected";
        public string Info => $"x = {Coord:N2}\n\tvalue = {Values[0]:N2}\n\tderiv1 = {Values[1]:N2}\n\tderiv2 = {Values[2]:N2}";
    }

    public class SplineData
    {
        public RawData Data { get; set; }
        public int NumNodes { get; set; }
        public double[] EdgeDerivs { get; set; }
        public double Integral { get; set; }
        public List<SplineDataItem> Items { get; set; }
        public SplineData(RawData rd, double[] derivs, int nNodes) => (Data, NumNodes, EdgeDerivs) = (rd, nNodes, derivs);
        public bool Interpolate()
        {
            int nx = Data.NodeCoords.Length;
            int ny = 1;
            int nsite = NumNodes;

            double[] site = new double[nsite];
            var delta = (Data.EndsCoords[1] - Data.EndsCoords[0]) / ((nsite - 1) == 0 ? 1 : nsite - 1);
            for (int i = 0; i < nsite; ++i)
                site[i] = Data.EndsCoords[0] + i * delta;
                //site[i] = i < nx ? Data.NodeCoords[i] : (Data.IsUniform ? Data.EndsCoords[0] + i * delta : Data.EndsCoords[0] + ((i == 0 || i == nx - 1) ? i : i - new Random().NextDouble()) * delta);

            double[] coords = new double[nx];
            double[] y = new double[ny * nx];
            Array.Copy(Data.NodeCoords, coords, nx);
            Array.Copy(Data.NodeValues, y, nx);
            double[] scoeff = new double[ny * 4 * (nx - 1)];
            double[] result = new double[ny * 3 * nsite];
            int ret = 0;
            double[] integral = new double[1];
            for (int i = 0; i < nx - 1; ++i)
                if (coords[i] > coords[i + 1])
                {
                    double tmpC = coords[i + 1];
                    coords[i + 1] = coords[i];
                    coords[i] = tmpC;

                    double tmpV = y[i + 1];
                    y[i + 1] = y[i];
                    y[i] = tmpV;
                }

            double[] Values = new double[nsite];
            double[] Deriv1Vals = new double[nsite];
            double[] Deriv2Vals = new double[nsite];

            try
            {
                CubeInterpolate(nx, ny, Data.IsUniform ? new double[] { Data.EndsCoords[0], Data.EndsCoords[1] } : coords, y, scoeff, nsite, Data.IsUniform ? new double[] { site[0], site[nsite-1] } : site, 3, new int[] { 1, 1, 1 }, result, ref ret, new double[] { site[0], site[site.Length-1] }, integral, EdgeDerivs, Data.IsUniform);
                Integral = integral[0];
                for (int i = 0; i < result.Length; ++i)
                {
                    switch (i % 3)
                    {
                        case 0:
                            Values[i / 3] = result[i];
                            break;
                        case 1:
                            Deriv1Vals[i / 3] = result[i];
                            break;
                        case 2:
                            Deriv2Vals[i / 3] = result[i];
                            break;
                    }
                }

                Items = new();

                for (int i = 0; i < nsite; ++i)
                    Items.Add(new SplineDataItem(site[i], new double[] { Values[i], Deriv1Vals[i], Deriv2Vals[i] }));

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        [DllImport("..\\..\\..\\..\\x64\\DEBUG\\CPP_DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CubeInterpolate(int nx, int ny, double[] x, double[] y, double[] scoeff, int nsite, double[] site, int ndorder, int[] dorder, double[] result, ref int ret, double[] intEdges, double[] integral, double[] edgeDerivs, bool isUniform);
    }
}
