// NOTE: The original source code of this component was lost. This file was recovered from the
// compiled assembly UMRO.Utils.DVHViewer-0.9.3.0.dll (version 0.9.3.0) by decompilation (ILSpy) in September 2026.
// Copyright (C) The Regents of the University of Michigan. Licensed under GPL-3.0 (see LICENSE.txt).

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;

namespace UMRO.Utils.DVHViewer
{
    public partial class DVHViewer : UserControl
    {
        private bool _bioDose = false;

        private DVHViewModel _viewModel;

        private bool _canVolumeBeRelative = true;

        private double _prescribedDose;

        public bool BioDose
        {
            get
            {
                return _bioDose;
            }
            set
            {
                _bioDose = value;
            }
        }

        public bool ShowLegend
        {
            set
            {
                if (value)
                {
                    lstDVH.Visibility = Visibility.Visible;
                }
                else
                {
                    lstDVH.Visibility = Visibility.Collapsed;
                }
            }
        }

        public double PrescribedDose
        {
            get
            {
                return _prescribedDose;
            }
            set
            {
                _prescribedDose = value;
                radDoseAbs.IsEnabled = true;
                radDoseRel.IsEnabled = true;
            }
        }

        public DVHViewer(bool biodose)
        {
            BioDose = biodose;
            ConstructMe();
        }

        public DVHViewer()
        {
            ConstructMe();
        }

        public void ClearData()
        {
            _viewModel.MyModel.Series.Clear();
            _viewModel.DVHCurves.Clear();
            RefreshPlot();
        }

        public void AddDirectDVH(string structName, Color structColor, Point[] DVHData, string doseUnit, double structVolume = 0.0)
        {
            if (doseUnit != "Gy" && doseUnit != "cGy")
            {
                throw new ApplicationException("DVHViewer can only handle absolute dose in Gy or cGy");
            }
            double[] array = new double[DVHData.Length];
            double[] array2 = new double[DVHData.Length];
            double[] array3 = new double[DVHData.Length];
            if (structVolume == 0.0)
            {
                radVolAbs.IsEnabled = false;
                radVolRel.IsEnabled = false;
                _viewModel.IsVolumeAbsolute = true;
                _canVolumeBeRelative = false;
            }
            else
            {
                _viewModel.IsVolumeRelative = true;
            }
            for (int i = 0; i < DVHData.Length; i++)
            {
                double num = 0.0;
                for (int j = i; j < DVHData.Length; j++)
                {
                    num += DVHData[j].Y;
                }
                array2[i] = num;
                array3[i] = DVHData[i].Y;
                array[i] = DVHData[i].X;
            }
            AddDVHsToPlot(structName, structVolume, structColor, array, array3, array2, doseUnit);
        }

        public void AddCumulativeDVH(string structName, Color structColor, Point[] DVHData, string doseUnit, double structVolume = 0.0)
        {
            if (doseUnit != "Gy" && doseUnit != "cGy")
            {
                throw new ApplicationException("DVHViewer can only handle absolute dose in Gy or cGy");
            }
            double[] array = new double[DVHData.Length];
            double[] array2 = new double[DVHData.Length];
            double[] array3 = new double[DVHData.Length];
            if (structVolume == 0.0)
            {
                radVolAbs.IsEnabled = false;
                radVolRel.IsEnabled = false;
                _viewModel.IsVolumeAbsolute = true;
                _canVolumeBeRelative = false;
            }
            else
            {
                _viewModel.IsVolumeRelative = true;
            }
            array3[DVHData.Length - 1] = DVHData[DVHData.Length - 1].Y;
            array[DVHData.Length - 1] = DVHData[DVHData.Length - 1].X;
            array2[DVHData.Length - 1] = DVHData[DVHData.Length - 1].Y;
            for (int i = 0; i < DVHData.Length - 1; i++)
            {
                array2[i] = DVHData[i].Y;
                array3[i] = DVHData[i].Y - DVHData[i + 1].Y;
                array[i] = DVHData[i].X;
            }
            AddDVHsToPlot(structName, structVolume, structColor, array, array3, array2, doseUnit);
        }

        private void ConstructMe()
        {
            _viewModel = new DVHViewModel();
            base.DataContext = _viewModel;
            InitializeComponent();
            SetupGraph();
            radDoseAbs.IsEnabled = false;
            radDoseRel.IsEnabled = false;
        }

        private void AddDVHsToPlot(string structName, double strucVolumen, Color structColor, double[] dvhDose, double[] difVol, double[] cumVol, string doseUnit)
        {
            OxyPlot.Series.LineSeries lineSeries = new OxyPlot.Series.LineSeries();
            OxyPlot.Series.LineSeries lineSeries2 = new OxyPlot.Series.LineSeries();
            lineSeries.Title = structName + "(D)";
            lineSeries2.Title = structName + "(C)";
            lineSeries.IsVisible = _viewModel.IsDifDVH;
            lineSeries2.IsVisible = _viewModel.IsCumDVH;
            lineSeries.Color = OxyColor.FromRgb(structColor.R, structColor.G, structColor.B);
            lineSeries2.Color = OxyColor.FromRgb(structColor.R, structColor.G, structColor.B);
            if (structName.Contains("(EQD2"))
            {
                lineSeries.LineStyle = LineStyle.Dash;
                lineSeries2.LineStyle = LineStyle.Dash;
            }
            if (structName.Contains("(EQD2-LQL"))
            {
                lineSeries.LineStyle = LineStyle.DashDot;
                lineSeries2.LineStyle = LineStyle.DashDot;
            }
            string text = doseUnit;
            if (BioDose)
            {
                text = doseUnit + "-EQD2";
            }
            _viewModel.MyModel.Axes[0].Title = "Dose (" + text + ")";
            _viewModel.MyModel.Axes[1].Title = "Volume (cc)";
            double num = dvhDose.Max();
            for (int i = 0; i < dvhDose.Length; i++)
            {
                double num2 = dvhDose[i];
                double num3 = difVol[i];
                double num4 = cumVol[i];
                if (_viewModel.IsDoseRelative)
                {
                    num2 = 100.0 * num2 / num;
                    _viewModel.MyModel.Axes[0].Title = text + " (%)";
                }
                if (_viewModel.IsVolumeRelative && strucVolumen != 0.0)
                {
                    num3 = 100.0 * num3 / strucVolumen;
                    num4 = 100.0 * num4 / strucVolumen;
                    _viewModel.MyModel.Axes[1].Title = "Volume (%)";
                }
                lineSeries.Points.Add(new DataPoint(num2, num3));
                lineSeries2.Points.Add(new DataPoint(num2, num4));
            }
            _viewModel.MyModel.Series.Add(lineSeries);
            _viewModel.MyModel.Series.Add(lineSeries2);
            RefreshPlot();
            _viewModel.DVHCurves.Add(new DVHCurve(structName, isVisible: true, structColor, num, strucVolumen, doseUnit));
        }

        private void SetupGraph()
        {
            string text = "Gy";
            if (BioDose)
            {
                text = "Gy-EQD2";
            }
            _viewModel.MyModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Dose (" + text + ")"
            });
            _viewModel.MyModel.Axes[0].MajorGridlineStyle = LineStyle.Dash;
            _viewModel.MyModel.Axes.Add(new OxyPlot.Axes.LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Volume (cc)"
            });
            _viewModel.MyModel.Axes[1].MajorGridlineStyle = LineStyle.Dash;
        }

        private void RefreshPlot()
        {
            _viewModel.MyModel.ResetAllAxes();
            oxyControl.InvalidatePlot();
        }

        private void dvhType_Checked(object sender, RoutedEventArgs e)
        {
            foreach (OxyPlot.Series.Series item in _viewModel.MyModel.Series)
            {
                string structureId = item.Title.Substring(0, item.Title.Length - 3);
                DVHCurve dVHCurve = _viewModel.DVHCurves.FirstOrDefault((DVHCurve s) => s.StructureId == structureId);
                if (dVHCurve.IsVisible)
                {
                    if (item.Title.Contains("(D)"))
                    {
                        item.IsVisible = _viewModel.IsDifDVH;
                    }
                    else
                    {
                        item.IsVisible = _viewModel.IsCumDVH;
                    }
                }
            }
            RefreshPlot();
        }

        private void bthReset_Click(object sender, RoutedEventArgs e)
        {
            foreach (DVHCurve dVHCurf in _viewModel.DVHCurves)
            {
                dVHCurf.IsVisible = true;
            }
            foreach (OxyPlot.Series.LineSeries item in _viewModel.MyModel.Series)
            {
                if (item.Title.Contains("(C"))
                {
                    item.IsVisible = true;
                }
            }
            _viewModel.IsDoseAbsolute = true;
            _viewModel.IsVolumeRelative = _canVolumeBeRelative;
            _viewModel.IsCumDVH = true;
            RefreshPlot();
        }

        private void doseType_Checked(object sender, RoutedEventArgs e)
        {
            foreach (OxyPlot.Series.LineSeries item in _viewModel.MyModel.Series)
            {
                string structureId = item.Title.Substring(0, item.Title.Length - 3);
                DVHCurve dVHCurve = _viewModel.DVHCurves.SingleOrDefault((DVHCurve s) => s.StructureId == structureId);
                string text = dVHCurve.DoseUnit;
                if (BioDose)
                {
                    text += "-EQD2";
                }
                if (dVHCurve == null)
                {
                    continue;
                }
                if (_viewModel.IsDoseAbsolute)
                {
                    _viewModel.MyModel.Axes[0].Title = "Dose (" + text + ")";
                    for (int i = 0; i < item.Points.Count; i++)
                    {
                        DataPoint dataPoint = item.Points[i];
                        item.Points[i] = new DataPoint(dataPoint.X * PrescribedDose / 100.0, dataPoint.Y);
                    }
                }
                else
                {
                    _viewModel.MyModel.Axes[0].Title = "Dose (%)";
                    for (int j = 0; j < item.Points.Count; j++)
                    {
                        DataPoint dataPoint2 = item.Points[j];
                        item.Points[j] = new DataPoint(dataPoint2.X * 100.0 / PrescribedDose, dataPoint2.Y);
                    }
                }
            }
            RefreshPlot();
        }

        private void volType_Checked(object sender, RoutedEventArgs e)
        {
            foreach (OxyPlot.Series.LineSeries item in _viewModel.MyModel.Series)
            {
                string structureId = item.Title.Substring(0, item.Title.Length - 3);
                DVHCurve dVHCurve = _viewModel.DVHCurves.SingleOrDefault((DVHCurve s) => s.StructureId == structureId);
                if (dVHCurve == null)
                {
                    continue;
                }
                bool flag = item.Title.Contains("(C)");
                if (_viewModel.IsVolumeAbsolute)
                {
                    _viewModel.MyModel.Axes[1].Title = "Volume (cc)";
                    for (int i = 0; i < item.Points.Count; i++)
                    {
                        DataPoint dataPoint = item.Points[i];
                        item.Points[i] = new DataPoint(dataPoint.X, dataPoint.Y * dVHCurve.MaxVolume / 100.0);
                    }
                }
                else
                {
                    _viewModel.MyModel.Axes[1].Title = "Volume (%)";
                    for (int j = 0; j < item.Points.Count; j++)
                    {
                        DataPoint dataPoint2 = item.Points[j];
                        item.Points[j] = new DataPoint(dataPoint2.X, 100.0 * dataPoint2.Y / dVHCurve.MaxVolume);
                    }
                }
            }
            RefreshPlot();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            StackPanel stackPanel = checkBox.Content as StackPanel;
            TextBlock textBlock = stackPanel.Children[1] as TextBlock;
            string structureId = textBlock.Text;
            if (_viewModel.IsCumDVH)
            {
                OxyPlot.Series.LineSeries lineSeries = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == structureId + "(C)") as OxyPlot.Series.LineSeries;
                lineSeries.IsVisible = !lineSeries.IsVisible;
            }
            else
            {
                OxyPlot.Series.LineSeries lineSeries2 = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == structureId + "(D)") as OxyPlot.Series.LineSeries;
                lineSeries2.IsVisible = !lineSeries2.IsVisible;
            }
            RefreshPlot();
        }

        private void Image_MouseEnter(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Hand;
        }

        private void Image_MouseLeave(object sender, MouseEventArgs e)
        {
            base.Cursor = Cursors.Arrow;
        }

        private void btnAll_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (DVHCurve curve in _viewModel.DVHCurves)
            {
                curve.IsVisible = true;
                if (_viewModel.IsCumDVH)
                {
                    OxyPlot.Series.LineSeries lineSeries = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == curve.StructureId + "(C)") as OxyPlot.Series.LineSeries;
                    lineSeries.IsVisible = true;
                }
                else
                {
                    OxyPlot.Series.LineSeries lineSeries2 = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == curve.StructureId + "(D)") as OxyPlot.Series.LineSeries;
                    lineSeries2.IsVisible = true;
                }
            }
            RefreshPlot();
        }

        private void btnNone_MouseDown(object sender, MouseButtonEventArgs e)
        {
            foreach (DVHCurve curve in _viewModel.DVHCurves)
            {
                curve.IsVisible = false;
                if (_viewModel.IsCumDVH)
                {
                    OxyPlot.Series.LineSeries lineSeries = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == curve.StructureId + "(C)") as OxyPlot.Series.LineSeries;
                    lineSeries.IsVisible = false;
                }
                else
                {
                    OxyPlot.Series.LineSeries lineSeries2 = _viewModel.MyModel.Series.FirstOrDefault((OxyPlot.Series.Series s) => s.Title == curve.StructureId + "(D)") as OxyPlot.Series.LineSeries;
                    lineSeries2.IsVisible = false;
                }
            }
            RefreshPlot();
        }
    }
}
