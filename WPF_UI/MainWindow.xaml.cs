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
            var intBordersReg = Regex.Matches(vData.Ends??"", @"[0-9]+(\.[0-9]+)?");
            var numNodesReg = Regex.Matches(vData.NmNodes??"", @"-?\d+");
            var numSplineNodesReg = Regex.Matches(vData.NmSplineNodes??"", @"-?\d+");
            var leftDerReg = Regex.Matches(vData.LDer??"", @"-?\d+");
            var rightDerReg = Regex.Matches(vData.RDer??"", @"-?\d+");
            FRaw Func = (vData.SelectedFunc??"") == FRawEnum.Linear.ToString() ? RawData.f1 : (vData.SelectedFunc??"") == FRawEnum.Cubic.ToString() ? RawData.f2 : RawData.f3;
            if (intBordersReg.Count != 2 || numNodesReg.Count != 1 || numSplineNodesReg.Count != 1 || leftDerReg.Count != 1 || rightDerReg.Count != 1)
                MessageBox.Show("Enter correct contorls!");
            else 
            {
                vData.rd = new RawData(new double[] { Double.Parse(intBordersReg[0].Value), Double.Parse(intBordersReg[1].Value) }, Int32.Parse(numNodesReg[0].Value), vData.IsUniform, Func);
                vData.sd = new SplineData(vData.rd, new double[] { Double.Parse(leftDerReg[0].Value), Double.Parse(rightDerReg[0].Value) }, Int32.Parse(numSplineNodesReg[0].Value));
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
                var numSplineNodesReg = Regex.Matches(vData.NmSplineNodes ?? "", @"-?\d+");
                var leftDerReg = Regex.Matches(vData.LDer ?? "", @"-?\d+");
                var rightDerReg = Regex.Matches(vData.RDer ?? "", @"-?\d+");
                if (numSplineNodesReg.Count != 1 || leftDerReg.Count != 1 || rightDerReg.Count != 1)
                    MessageBox.Show("Enter correct contorls!");
                else
                {
                    vData.sd = new SplineData(vData.rd, new double[] { Convert.ToDouble(leftDerReg[0].Value), Convert.ToDouble(rightDerReg[0].Value) }, Convert.ToInt32(numSplineNodesReg[0].Value));
                    vData.sd.Interpolate();
                    SetBindings();
                }
            }
        }

        private void intBorders_val_Initialized(object sender, EventArgs e)
        {
            intBorders_val.SetBinding(TextBox.TextProperty, new Binding("vData.Ends"));
        }

        private void numNodes_val_Initialized(object sender, EventArgs e)
        {
            numNodes_val.SetBinding(TextBox.TextProperty, new Binding("vData.NmNodes"));
        }

        private void numSplineNodes_val_Initialized(object sender, EventArgs e)
        {
            numSplineNodes_val.SetBinding(TextBox.TextProperty, new Binding("vData.NmSplineNodes"));
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
            leftDer_val.SetBinding(TextBox.TextProperty, new Binding("vData.LDer"));
        }

        private void rightDer_val_Initialized(object sender, EventArgs e)
        {
            rightDer_val.SetBinding(TextBox.TextProperty, new Binding("vData.RDer"));
        }
    }
}
