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
using System.Text.RegularExpressions;
using OxyPlot;
using ClassLibrary;
using System.Globalization;
using OxyPlot.Series;
using OxyPlot.Legends;

namespace WPF_UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewData vData { get; set; }
        public string IntegralValue { get=>vData?.sd?.Integral.ToString() ?? ""; }
        public List<SplineDataItem> SplineItems { get => vData?.sd?.Items; }
        public List<String> RawItems { get; }
        public static RoutedCommand ExecuteFromControlsCommand = new RoutedCommand("ExecuteFromControls", typeof(WPF_UI.MainWindow));
        public static RoutedCommand ExecuteFromFileCommand = new RoutedCommand("ExecuteFromFile", typeof(WPF_UI.MainWindow));
        public MainWindow()
        {
            this.DataContext = this;
            RawItems = new();
            vData = new();
            InitializeComponent();

            this.CommandBindings.Add(new CommandBinding(ExecuteFromControlsCommand, execControls_Click, CanExecuteFromControlsCommandHandler));
            this.CommandBindings.Add(new CommandBinding(ExecuteFromFileCommand, ExecuteFromFileHandler, CanExecuteFromFileCommandHandler));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, Save_Click, CanSaveCommandHandler));
        }

        private void execControls_Click(object sender, RoutedEventArgs e)
        {
            FRaw Func = (vData.SelectedFunc??"") == FRawEnum.Linear.ToString() ? RawData.f1 : (vData.SelectedFunc??"") == FRawEnum.Cubic.ToString() ? RawData.f2 : RawData.f3;
            if (vData.Ends == null || vData.NmNodes == null || vData.NmSplineNodes == null || vData.LDer == null || vData.RDer == null)
                MessageBox.Show("Enter correct contorls!");
            else 
            {
                vData.rd = new RawData(vData.Ends, (int)vData.NmNodes, vData.IsUniform, Func);
                vData.sd = new SplineData(vData.rd, new double[] { (double)vData.LDer, (double)vData.RDer }, (int)vData.NmSplineNodes);
                vData.sd.Interpolate();
                SetBindings();
                DrawGraphics();
            }
        }

        private void CanExecuteFromControlsCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (grid != null)
            {
                foreach (FrameworkElement child in grid.Children)
                {
                    if (Validation.GetHasError(child))
                    {
                        e.CanExecute = false;
                        return;
                    }
                    e.CanExecute = true;
                }
            }
            else e.CanExecute = false;
        }

        private void CanExecuteFromFileCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (vData.rd != null)
            {
                if (vData.rd.NumNodes < 2 || vData.rd.EndsCoords[0] >= vData.rd.EndsCoords[1] || vData.NmSplineNodes == null || vData.LDer == null || vData.RDer == null)
                    e.CanExecute = false;
                else
                    e.CanExecute = true;
            }
            else if (vData.NmSplineNodes == null || vData.LDer == null || vData.RDer == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void execFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if ((bool)dlg.ShowDialog())
                vData.Load(dlg.FileName);
        }

        private void ExecuteFromFileHandler(object sender, RoutedEventArgs e)
        {
            vData.sd = new SplineData(vData.rd, new double[] { (double)vData.LDer, (double)vData.RDer }, (int)vData.NmSplineNodes);
            vData.sd.Interpolate();
            SetBindings();
            DrawGraphics();
        }

        private void SetBindings()
        {
            RawItems.Clear();
            for (int i = 0; i < vData.rd.NodeCoords.Length; ++i)
                RawItems.Add($"x = {vData.rd.NodeCoords[i]:N2}, value = {vData.rd.NodeValues[i]:N2}");
            BindingOperations.ClearAllBindings(rdList);
            rdList.SetBinding(ListBox.ItemsSourceProperty, new Binding("RawItems"));
            sdList.SetBinding(ListBox.ItemsSourceProperty, new Binding("SplineItems"));
            integral_textBlock.SetBinding(TextBlock.TextProperty, new Binding("IntegralValue"));
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            if((bool)dlg.ShowDialog() && vData!=null)
            {
                vData.Save(dlg.FileName);
                MessageBox.Show("Data saved!");
            }
            else
            {
                MessageBox.Show("Saving failed!");
            }
        }

        private void CanSaveCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (vData.rd != null)
            {
                if (vData.rd.NumNodes < 2 || vData.rd.EndsCoords[0] >= vData.rd.EndsCoords[1])
                    e.CanExecute = false;
                else
                    e.CanExecute = true;
            }
            else
                e.CanExecute = false;
        }

        private void OpenAndExec_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if ((bool)dlg.ShowDialog())
            {
                bool success = vData.Load(dlg.FileName);
                if (vData.NmSplineNodes == null || vData.LDer == null || vData.RDer == null || !success)
                    MessageBox.Show("Enter correct contorls!");
                else
                {
                    vData.sd = new SplineData(vData.rd, new double[] { (double)vData.LDer, (double)vData.RDer }, (int)vData.NmSplineNodes);
                    vData.sd.Interpolate();
                    SetBindings();
                }
            }
        }

        private void intBorders_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.Ends");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"[0-9]+(\.[0-9]+)?", 2);
            binding.ValidatesOnDataErrors = true;
            intBorders_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void numNodes_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.NmNodes");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"-?\d+", 1);
            binding.ValidatesOnDataErrors = true;
            numNodes_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void numSplineNodes_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.NmSplineNodes");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"-?\d+", 1);
            binding.ValidatesOnDataErrors = true;
            numSplineNodes_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void rbUni_Initialized(object sender, EventArgs e)
        {
            rbUni.SetBinding(RadioButton.IsCheckedProperty, new Binding("vData.IsUniform"));
        }

        private void cbFunc_Initialized(object sender, EventArgs e)
        {
            cbFunc.SetBinding(ComboBox.SelectedItemProperty, new Binding("vData.SelectedFunc"));
        }

        private void leftDer_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.LDer");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"[0-9]+(\.[0-9]+)?", 1);
            leftDer_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void rightDer_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.RDer");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"[0-9]+(\.[0-9]+)?", 1);
            rightDer_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void execControls_Initialized(object sender, EventArgs e)
        {
            execControls.Command = ExecuteFromControlsCommand;
        }        
        
        private void MenuExecControls_Initialized(object sender, EventArgs e)
        {
            MenuExecFromControls.Command = ExecuteFromControlsCommand;
        }

        private void execFile_Initialized(object sender, EventArgs e)
        {
            execFile.Command = ExecuteFromFileCommand;
        }        
        
        private void MenuExecFile_Initialized(object sender, EventArgs e)
        {
            MenuExecFromFile.Command = ExecuteFromFileCommand;
        }

        private void save_Initialized(object sender, EventArgs e)
        {
            save.Command = ApplicationCommands.Save;
        }        
        
        private void MenuSave_Initialized(object sender, EventArgs e)
        {
            MenuSave.Command = ApplicationCommands.Save;
        }

        public void DrawGraphics()
        {
            var mdl = new PlotModel();
            mdl.Title = "Function interpolation";
            mdl.Series.Clear();

            //drawing raw data points
            OxyColor color = OxyColors.DeepPink;
            LineSeries lineSeries = new LineSeries();
            for (int i = 0; i < vData.rd.NumNodes; i++)
                lineSeries.Points.Add(new DataPoint(vData.rd.NodeCoords[i], vData.rd.NodeValues[i]));

            lineSeries.Title = "Raw data points";
            lineSeries.Color = color;
            lineSeries.LineStyle = LineStyle.None;
            lineSeries.MarkerType = MarkerType.Circle;
            lineSeries.MarkerSize = 4;
            lineSeries.MarkerStroke = color;
            lineSeries.MarkerFill = color;

            Legend leg = new Legend();
            mdl.Legends.Add(leg);
            mdl.Series.Add(lineSeries);

            //drawing spline points
            color = OxyColors.Aqua;
            LineSeries lineSeries1 = new LineSeries();
            for (int i = 0; i < vData.NmSplineNodes; i++)
            {
                lineSeries1.Points.Add(new DataPoint(vData.sd.Items[i].Coord, vData.sd.Items[i].Values[0]));
            }

            lineSeries1.Title = "Spline points";
            lineSeries1.Color = color;
            lineSeries1.MarkerSize = 4;

            mdl.Series.Add(lineSeries1);

            plotModel.Model = mdl;
        }
    }
    public class RegexConverter : IValueConverter
    {
        public string Format;
        public int Num;
        public RegexConverter(string format, int num) => (Format, Num) = (format, num);
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
