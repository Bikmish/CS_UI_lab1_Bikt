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
        public string Ends { get; set; }
        public string NmNodes { get; set; }
        public string NmSplineNodes { get; set; }
        public string SelectedFunc { get; set; }
        public string LDer { get; set; }
        public string RDer { get; set; }
        public bool IsUniform { get; set; }

        //for SplineData initialization
        public double[] EdgeDerivs { get; set; }
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
