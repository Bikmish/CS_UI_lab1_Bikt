using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ClassLibrary;

namespace WPF_UI
{
    public class ViewData: IDataErrorInfo
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

        public string Error => String.Empty;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case "NmNodes":
                        if (NmNodes == null || NmNodes < 2)
                            error = "The number of nodes must be greater than or equal to 2!";
                        break;
                    case "NmSplineNodes":
                        if (NmSplineNodes == null || NmSplineNodes < 3)
                            error = "The number of spline nodes must be greater than or equal to 2!";
                        break;
                    case "Ends":
                        if (Ends == null || Ends?[0] >= Ends?[1])
                            error = "Left border of interpolation segment must be less than right border!";
                        break;
                }
                //if(error != String.Empty)
                //    MessageBox.Show(error);
                return error;
            }
        }
    }
}
