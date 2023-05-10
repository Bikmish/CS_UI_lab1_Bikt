using InterpolatorViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Text.RegularExpressions;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace InterpolatorView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel(new UIservices(grid), new GraphicProvider());
        }
    }

    public class GraphicProvider : IGraphicProvider
    {
        public object PlotModel { get; set; }
        public void DrawGraphics(double[] rdCoords, double[] rdValues, double[] sdCoords, double[] sdValues)
        {
            var mdl = new PlotModel();
            mdl.Title = "Function interpolation";
            mdl.Series.Clear();

            //drawing raw data points
            OxyColor color = OxyColors.DarkTurquoise;
            LineSeries ls = new LineSeries();
            for (int i = 0; i < rdCoords.Length; i++)
                ls.Points.Add(new DataPoint(rdCoords[i], rdValues[i]));

            ls.Title = "Raw data points";
            ls.Color = color;
            ls.LineStyle = LineStyle.None;
            ls.MarkerType = MarkerType.Circle;
            ls.MarkerSize = 5;
            ls.MarkerStroke = color;
            ls.MarkerFill = color;

            Legend leg = new Legend();
            mdl.Legends.Add(leg);
            mdl.Series.Add(ls);

            //drawing spline
            color = OxyColors.Purple;
            LineSeries ls1 = new LineSeries();
            for (int i = 0; i < sdValues.Length; i++)
                ls1.Points.Add(new DataPoint(sdCoords[i], sdValues[i]));

            ls1.Title = "Spline";
            ls1.Color = color;
            mdl.Series.Add(ls1);

            PlotModel = mdl;
        }
    }

    public class UIservices : IUIServices
    {
        public UIservices(Grid grid) => UIgrid = grid;
        public object UIgrid { get; set; }

        public string ChoosePath()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            return (bool)dlg.ShowDialog() ? dlg.FileName : String.Empty;
        }

        public string GetFileName()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            return (bool)dlg.ShowDialog() ? dlg.FileName : String.Empty;
        }
        public void ReportError(string message) => MessageBox.Show(message);
        public bool CanExecuteFromControls()
        {
            if (UIgrid != null)
            {
                foreach (FrameworkElement child in ((Grid)UIgrid).Children)
                    if (Validation.GetHasError(child))
                        return false;
                return true;
            }
            else return false;
        }
    }

    public class ToDoubleTupleConverter : IValueConverter
    {
        string Format = @"-?[0-9]+(\.[0-9]+)?";
        int Num = 2;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reg = Regex.Matches((string)value ?? "-1", Format);
            return reg?.Count != Num ? null : Num == 1 ? Double.Parse(reg[0].Value) : new double[] { Double.Parse(reg[0].Value), Double.Parse(reg[1].Value) };
        }
    }

    public class ToIntConverter : IValueConverter
    {
        string Format = @"-?\d+";
        int Num = 1;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reg = Regex.Matches((string)value ?? "-1", Format);
            return reg?.Count != Num ? null : Num == 1 ? Double.Parse(reg[0].Value) : new double[] { Double.Parse(reg[0].Value), Double.Parse(reg[1].Value) };
        }
    }

    public class ToDoubleConverter : IValueConverter
    {
        string Format = @"-?[0-9]+(\.[0-9]+)?";
        int Num = 1;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var reg = Regex.Matches((string)value ?? "-1", Format);
            return reg?.Count != Num ? null : Num == 1 ? Double.Parse(reg[0].Value) : new double[] { Double.Parse(reg[0].Value), Double.Parse(reg[1].Value) };
        }
    }
}
