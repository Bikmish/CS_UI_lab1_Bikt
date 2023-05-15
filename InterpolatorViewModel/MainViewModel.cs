using System.ComponentModel;
using ClassLibrary;
using System.Windows.Input;

namespace InterpolatorViewModel
{
    public interface IUIServices
    {
        string GetFileName();
        string ChoosePath();
        void ReportError(string message);
        bool CanExecuteFromControls();
    }

    public interface IGraphicProvider
    {
        object DrawGraphics(double[] rdCoords, double[] rdValues, double[] sdCoords, double[] sdValues);
    }

    public class MainViewModel : ViewModelBase, IDataErrorInfo
    {
        public double[] Ends { get; set; }
        public int? NmNodes { get; set; }
        public int? NmSplineNodes { get; set; }
        public bool IsUniform { get; set; }
        public string SelectedFunc { get; set; }
        public double? LDer { get; set; }
        public double? RDer { get; set; }
        public RawData? rd;
        public SplineData? sd;
        public List<String> RawItems
        {
            get
            {
                var rItems = new List<string>() { };
                for (int i = 0; i < (rd?.NodeCoords.Length ?? 0); ++i)
                    rItems.Add($"x = {rd.NodeCoords[i]:N2}, value = {rd.NodeValues[i]:N2}");
                return rItems;
            }
        }
        public List<SplineDataItem> SplineItems { get => sd?.Items ?? new List<SplineDataItem>(); }
        public SplineDataItem? SelectedSplineItem { set { SelectedSplineItemInfo = value != null ? value.ToString() : "No spline item selected."; RaisePropertyChanged(nameof(SelectedSplineItemInfo)); } }
        public string SelectedSplineItemInfo { get; set; }
        public string IntegralValue { get => sd?.Integral.ToString("N2") ?? ""; }
        public ICommand ExecuteFromControlsCommand { get; private set; }
        public ICommand ExecuteFromFileCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        private readonly IUIServices uiServices;
        private readonly IGraphicProvider graphicProvider;
        public object PlotModel { get; set; }
        public MainViewModel(IUIServices uiServices, IGraphicProvider graphicProvider)
        {
            (this.uiServices, this.graphicProvider) = (uiServices, graphicProvider);
            ExecuteFromControlsCommand = new RelayCommand(ExecuteFromControls, _ => uiServices.CanExecuteFromControls());
            ExecuteFromFileCommand = new RelayCommand(ExecuteFromFile, _ => CanExecuteFromFile());
            SaveCommand = new RelayCommand(SaveRawData, _ => CanSaveRawData());
        }
        private void ExecuteFromControls(object sender)
        {
            rd = new RawData(Ends, (int)NmNodes, IsUniform, (SelectedFunc ?? "") == FRawEnum.Linear.ToString() ? RawData.f1 : (SelectedFunc ?? "") == FRawEnum.Cubic.ToString() ? RawData.f2 : RawData.f3);
            sd = new SplineData(rd, new double[] { (double)LDer, (double)RDer }, (int)NmSplineNodes);
            CalculateAndShow();
        }

        private void ExecuteFromFile(object sender)
        {
            rd = new RawData();
            var path = uiServices.GetFileName();
            if (path != String.Empty && path != null && RawData.Load(path, rd))
            {
                sd = new SplineData(rd, new double[] { (double)LDer, (double)RDer }, (int)NmSplineNodes);
                CalculateAndShow();
            }
            else
                uiServices.ReportError("Loading failed.");
        }

        private void SaveRawData(object sender)
        {
            var path = uiServices.ChoosePath();
            if(path != String.Empty && path != null && rd.Save(path))
                uiServices.ReportError("Saved successfully!");
            else
                uiServices.ReportError("Saving failed.");
        }

        private bool CanSaveRawData() => rd != null && rd.NumNodes >= 2 && rd.EndsCoords[0] < rd.EndsCoords[1];

        private bool CanExecuteFromFile() => !(NmSplineNodes == null || LDer == null || RDer == null);

        private void CalculateAndShow()
        {
            sd.Interpolate();

            var (sdCoords, sdVals) = (new double[(int)NmSplineNodes], new double[(int)NmSplineNodes]);
            for (int i = 0; i < (int)NmSplineNodes; ++i)
                (sdCoords[i], sdVals[i]) = (sd.Items[i].Coord, sd.Items[i].Values[0]);

            PlotModel = graphicProvider.DrawGraphics(rd.NodeCoords, rd.NodeValues, sdCoords, sdVals);
            RaisePropertyChanged(nameof(SplineItems));
            RaisePropertyChanged(nameof(RawItems));
            RaisePropertyChanged(nameof(IntegralValue));
            RaisePropertyChanged(nameof(PlotModel));
        }

        public string Error => String.Empty;

        public string this[string columnName]
        {
            get
            {
                string error = String.Empty;
                switch (columnName)
                {
                    case nameof(NmNodes):
                        if (NmNodes == null || NmNodes < 2)
                            error = "The number of nodes must be greater than or equal to 2!";
                        break;
                    case nameof(NmSplineNodes):
                        if (NmSplineNodes == null || NmSplineNodes < 3)
                            error = "The number of spline nodes must be greater than or equal to 2!";
                        break;
                    case nameof(Ends):
                        if (Ends == null || Ends?[0] >= Ends?[1])
                            error = "Left border of interpolation segment must be less than right border!";
                        break;
                    case nameof(LDer):
                        if (LDer == null)
                            error = "Derivative on left border of segment can't be null!";
                        break;
                    case nameof(RDer):
                        if (RDer == null)
                            error = "Derivative on right border of segment can't be null!";
                        break;
                }
                return error;
            }
        }

    }

}