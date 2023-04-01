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
using ClassLibrary;
using System.Globalization;

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
        public MainWindow()
        {
            this.DataContext = this;
            RawItems = new();
            vData = new();
            InitializeComponent();
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
            }
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
        private void OpenAndExec_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            if ((bool)dlg.ShowDialog())
            {
                vData.Load(dlg.FileName);
                if (vData.NmSplineNodes == null || vData.LDer == null || vData.RDer == null)
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
            intBorders_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void numNodes_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.NmNodes");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"-?\d+", 1);
            numNodes_val.SetBinding(TextBox.TextProperty, binding);
        }

        private void numSplineNodes_val_Initialized(object sender, EventArgs e)
        {
            var binding = new Binding("vData.NmSplineNodes");
            binding.Mode = BindingMode.OneWayToSource;
            binding.Converter = new RegexConverter(@"-?\d+", 1);
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
