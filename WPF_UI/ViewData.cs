using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClassLibrary;

namespace WPF_UI
{
    public class ViewData
    {
        //for RawData initialization
        public double[] Ends { get; set; }
        public int? NmNodes { get; set; }
        public string SelectedFunc { get; set; }
        public bool IsUniform { get; set; }

        //for SplineData initialization
        public double? LDer { get; set; }
        public double? RDer { get; set; }
        public int? NmSplineNodes { get; set; }
        public RawData? rd;
        public SplineData? sd;
        public ViewData() { }
        public bool Save(string filename)
        {
            if(rd.Save(filename))
                return true;

            MessageBox.Show("Saving error!");
            return false;
        }

        public bool Load(string filename)
        {
            rd = new RawData();
            if (RawData.Load(filename, rd))
                return true;

            MessageBox.Show("Loading error!");
            return false;
        }
    }
}
