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
using Microsoft.Win32;
using System.Windows.Forms.DataVisualization.Charting;
using System.Numerics;
using System.Data;
using System.Security.Cryptography;
using System.Drawing;

namespace FilterRLC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Chart chart01;      //chart objet
        private double U1;          //input voltage amplitude
        private double fmin;        //minimum frequency of input voltage
        private double fmax;        //maximum frequency of input voltage
        private double R1;          //Resistance (resistor_1)
        private double R2;          //Resistance (restistor_2)
        private double L1;          //Inductance
        private double C1;          //Capacitance
        private double[,] Results;  //transmittance scoreboard
        private int size;           //number of rows in the results array

        private ContextMenu circuitContextMenu = null;

        public MainWindow()
        {
            InitializeComponent();

            this.U1 = 10;
            this.fmin = 0;
            this.fmax = 1000;
            this.R1 = 1;
            this.R2 = 1;
            this.L1 = 0.001;
            this.C1 = 0.0005;
            this.size = 1000;
            //---
            this.txtMagnitude.Text=U1.ToString();
            this.txtFreqMin.Text=fmin.ToString();
            this.txtFreqMax.Text=fmax.ToString();
            this.txtResistance_1.Text=R1.ToString(); 
            this.txtResistance_2.Text=R2.ToString();
            this.txtInductance.Text=L1.ToString();
            this.txtCapacitance.Text=C1.ToString();
            this.txtPoints.Text=size.ToString();
            //---


        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            chart01 = new Chart();
            host.Child = chart01;
            chart01.ChartAreas.Add(new ChartArea("Magnitude"));
            chart01.ChartAreas.Add(new ChartArea("Phase"));

        }

        private void openFile_Click(object sender, RoutedEventArgs e)
        {
            Calculations calculations= new Calculations();
            calculations.Show();
        }

        private void clearWaveforms_Click(object sender, RoutedEventArgs e)
        {
            resetValue();
            chart01.Series.Clear();
            chart01.Titles.Clear();
            chart01.ChartAreas["Magnitude"].AxisX.ScaleView.ZoomReset();
            chart01.ChartAreas["Magnitude"].AxisY.ScaleView.ZoomReset();
            chart01.ChartAreas["Phase"].AxisX.ScaleView.ZoomReset();
            chart01.ChartAreas["Phase"].AxisY.ScaleView.ZoomReset();
        }

        private void exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void filterParams_Click(object sender, RoutedEventArgs e)
        {
            ParamWindow paramDialog = new ParamWindow();

           
            paramDialog.txtResistance_1.Text = this.txtResistance_1.Text;
            paramDialog.txtResistance_2.Text = this.txtResistance_2.Text;
            paramDialog.txtInductance.Text = this.txtInductance.Text;
            paramDialog.txtCapacitance.Text = this.txtCapacitance.Text;
            paramDialog.txtMagnitude.Text = this.txtMagnitude.Text;
            paramDialog.txtFreqMin.Text = this.txtFreqMin.Text;
            paramDialog.txtFreqMax.Text = this.txtFreqMax.Text;
            paramDialog.txtPoints.Text = this.txtPoints.Text;

            bool? dialogResult = paramDialog.ShowDialog();

            if (dialogResult == true)
            {
                if (!Int32.TryParse(paramDialog.txtPoints.Text, out size) || size<=0)
                {
                    MessageBox.Show("Błędna wartość rozmiaru tablicy", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!Double.TryParse(paramDialog.txtMagnitude.Text, out U1) || U1 <= 0)
                {
                    MessageBox.Show("Błędna wartość amplitudy", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtMagnitude.Text = U1.ToString();

                if (!Double.TryParse(paramDialog.txtFreqMin.Text, out fmin) || fmin < 0)
                {
                    MessageBox.Show("Błędna wartość częstotliwości min.", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtFreqMin.Text = fmin.ToString();

                if (!Double.TryParse(paramDialog.txtFreqMax.Text, out fmax) || fmax <= 0)
                {
                    MessageBox.Show("Błędna wartość częstotliwości max.", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtFreqMax.Text = fmax.ToString();

                if (!Double.TryParse(paramDialog.txtResistance_1.Text, out R1) || R1 <= 0)
                {
                    MessageBox.Show("Błedna wartość rezystancji R1", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtResistance_1.Text = R1.ToString();

                if (!Double.TryParse(paramDialog.txtResistance_2.Text, out R2) || R2 <= 0)
                {
                    MessageBox.Show("Błędny format rezystancji R2", "Parametry",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtResistance_2.Text = R2.ToString();

                if (!Double.TryParse(paramDialog.txtInductance.Text, out L1) || L1 <= 0)
                {
                    MessageBox.Show("Błedna wartość indukcyjności", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtInductance.Text = L1.ToString();

                if (!Double.TryParse(paramDialog.txtCapacitance.Text, out C1) || C1 <= 0)
                {
                    MessageBox.Show("Błedna wartość pojemności", "Parametry",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                this.txtCapacitance.Text = C1.ToString();
            }
        }

        private void aboutWindow_Click(object sender, RoutedEventArgs e)
        {
          AboutWindow aboutWindow = new AboutWindow();
          aboutWindow.Show();

        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            if (!Double.TryParse(txtMagnitude.Text, out U1) || U1 <= 0)
            {
                MessageBox.Show("Błędna wartość amplitudy", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtFreqMin.Text, out fmin) || fmin < 0)
            {
                MessageBox.Show("Błędna wartość częstotliwości min.", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtFreqMax.Text, out fmax) || fmax <= 0)
            {
                MessageBox.Show("Błędna wartość częstotliwości max.", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtResistance_1.Text, out R1) || R1 <= 0)
            {
                MessageBox.Show("Błedna wartość rezystancji R1", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtResistance_2.Text, out R2) || R2 <= 0)
            {
                MessageBox.Show("Błedna wartość rezystancji R2", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtInductance.Text, out L1) || L1 <= 0)
            {
                MessageBox.Show("Błedna wartość indukcyjności", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Double.TryParse(txtCapacitance.Text, out C1) || C1 <= 0)
            {
                MessageBox.Show("Błedna wartość pojemności", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!Int32.TryParse(txtPoints.Text, out size) || size <= 0)
            {
                MessageBox.Show("Błedna wartość liczby kroków", "Parametry",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //---
            Transmittance();
            DrawWaveforms();

        }

        private void Transmittance()
        {
            this.Results = new double[size, 3];
            Complex Z1;
            Complex Z2; 
            Complex Z3; 
            Complex Z4;
            Complex Z5;
            double f = fmin;
            double df = (fmax - fmin) / (size - 1);
            double omega = 0; 

            if (fmin == 0)
            {
                fmin = 0000000000.1;
            }

            for (int i = 0; i < size; i++)
            {
                omega = 2 * Math.PI * f;
                Z1 = new Complex(R1, 0);
                Z2 = new Complex(R2, 0);
                Z3 = new Complex(0, -1 / (omega * C1));
                Z4 = new Complex(0, omega * L1);
                Z5 = U1 * (Z2 * Z4) / ((Z1 + Z2) * (Z3 + Z4) + Z1 * Z2);

                Results[i, 0] = f;
                Results[i, 1] = Z5.Magnitude;
                Results[i, 2] = Z5.Phase;
                f += df;
            }
        }


        public void DrawWaveforms()
        {
            DataTable dTable;   //Reprezentacja danych z Results 
            DataView dView;     //Reprezentacja DataTable na Chart 
            dTable = new DataTable();
            DataColumn column;
            DataRow row;
            //--- 
            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "Frequency";
            dTable.Columns.Add(column);
            //--- 
            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "Transmittance";
            dTable.Columns.Add(column);
            //--- 
            column = new DataColumn();
            column.DataType = Type.GetType("System.Double");
            column.ColumnName = "PhaseSpectrum";
            dTable.Columns.Add(column);
            //--- 
            for (int i = 0; i < size; i++)
            {
                row = dTable.NewRow();
                row["Frequency"] = Results[i, 0];
                row["Transmittance"] = Results[i, 1];
                row["PhaseSpectrum"] = Results[i, 2];
                dTable.Rows.Add(row);
            }
            //--- 
            dView = new DataView(dTable);
            //--- 
            chart01.Series.Clear();
            chart01.Titles.Clear();
            chart01.Legends.Clear();
            chart01.Titles.Add("U2");
            //--- 
            chart01.DataBindTable(dView, "Frequency");
            //--- 
            chart01.Series["Transmittance"].ChartType = SeriesChartType.Line;
            chart01.Series["PhaseSpectrum"].ChartType = SeriesChartType.Line;
            //--- 
            chart01.Series["Transmittance"].ChartArea = "Magnitude";
            chart01.Series["PhaseSpectrum"].ChartArea = "Phase";
            //--- 
            chart01.ChartAreas["Magnitude"].AxisX.LabelStyle.Format = "{#0.0}";
            chart01.ChartAreas["Magnitude"].AxisX.Minimum = 0;
            chart01.ChartAreas["Magnitude"].AxisX.LabelStyle.Font = new Font("Arial", 8);

            chart01.ChartAreas["Magnitude"].AxisY.LabelStyle.Font = new Font("Arial", 8);
            //---Cursor enabling 
            chart01.ChartAreas["Magnitude"].CursorX.IsUserEnabled = true;
            chart01.ChartAreas["Magnitude"].CursorY.IsUserEnabled = true;
            chart01.ChartAreas["Magnitude"].CursorX.IsUserSelectionEnabled = true;
            chart01.ChartAreas["Magnitude"].CursorY.IsUserSelectionEnabled = true;
            chart01.ChartAreas["Magnitude"].CursorX.Interval = 1;
            chart01.ChartAreas["Magnitude"].CursorY.Interval = 0.1;
            chart01.ChartAreas["Magnitude"].CursorX.LineColor = System.Drawing.Color.BlueViolet;
            chart01.ChartAreas["Magnitude"].CursorY.LineColor = System.Drawing.Color.BlueViolet;
            chart01.ChartAreas["Magnitude"].CursorX.LineDashStyle = ChartDashStyle.Dash;
            chart01.ChartAreas["Magnitude"].CursorY.LineDashStyle = ChartDashStyle.Dash;
            chart01.ChartAreas["Magnitude"].AxisX.ScaleView.Zoomable = true;
            chart01.ChartAreas["Magnitude"].AxisY.ScaleView.Zoomable = true;
            chart01.ChartAreas["Magnitude"].AxisX.Interval = double.NaN;
            chart01.ChartAreas["Magnitude"].AxisY.Interval = double.NaN;
            chart01.ChartAreas["Magnitude"].AxisY.ScaleView.SmallScrollMinSize = 0.1;

            chart01.ChartAreas["Phase"].AxisX.LabelStyle.Format = "{#0.0}";
            chart01.ChartAreas["Phase"].AxisX.LabelStyle.Font = new Font("Arial", 8);
            chart01.ChartAreas["Phase"].AxisX.Minimum = 0;

            chart01.ChartAreas["Phase"].AxisY.LabelStyle.Font = new Font("Arial", 8);
            //---Cursor 
            chart01.ChartAreas["Phase"].CursorX.IsUserEnabled = true;
            chart01.ChartAreas["Phase"].CursorY.IsUserEnabled = true;
            chart01.ChartAreas["Phase"].CursorX.IsUserSelectionEnabled = true;
            chart01.ChartAreas["Phase"].CursorY.IsUserSelectionEnabled = true;
            chart01.ChartAreas["Phase"].CursorX.Interval = 1;
            chart01.ChartAreas["Phase"].CursorY.Interval = 0.1;
            chart01.ChartAreas["Phase"].CursorX.LineColor = System.Drawing.Color.DarkRed;
            chart01.ChartAreas["Phase"].CursorY.LineColor = System.Drawing.Color.DarkRed;
            chart01.ChartAreas["Phase"].CursorX.LineDashStyle = ChartDashStyle.Dash;
            chart01.ChartAreas["Phase"].CursorY.LineDashStyle = ChartDashStyle.Dash;
            chart01.ChartAreas["Phase"].AxisX.ScaleView.Zoomable = true;
            chart01.ChartAreas["Phase"].AxisY.ScaleView.Zoomable = true;
            chart01.ChartAreas["Phase"].AxisX.Interval = double.NaN;
            chart01.ChartAreas["Phase"].AxisY.Interval = double.NaN;
            chart01.ChartAreas["Phase"].AxisY.ScaleView.SmallScrollMinSize = 0.1;

            //--- 
            chart01.Series["Transmittance"].Color = System.Drawing.Color.Red;
            chart01.Series["PhaseSpectrum"].Color = System.Drawing.Color.Blue;
            //--- 
            chart01.Titles[0].Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold);

            //--chart Magnitude
            chart01.ChartAreas[0].AxisX.TitleFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular);
            chart01.ChartAreas[0].AxisX.Title = "Frequency [Hz]";
            chart01.ChartAreas[0].AxisX.TitleAlignment = StringAlignment.Center;

            chart01.ChartAreas[0].AxisY.TitleFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular);
            chart01.ChartAreas[0].AxisY.Title = "Magnitude [V]";
            chart01.ChartAreas[0].AxisY.TitleAlignment = StringAlignment.Center;



            //--- chart Phase
            chart01.ChartAreas[1].AxisX.TitleFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular);
            chart01.ChartAreas[1].AxisX.Title = "Frequency [Hz]";
            chart01.ChartAreas[1].AxisX.TitleAlignment = StringAlignment.Center;

            chart01.ChartAreas[1].AxisY.TitleFont = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular);
            chart01.ChartAreas[1].AxisY.Title = "Phase [rad]";
            chart01.ChartAreas[1].AxisY.TitleAlignment = StringAlignment.Center;
        }

        private void resetValue()
        {
            this.U1 = 10;
            this.txtMagnitude.Text = this.U1.ToString();

            this.fmin = 0;
            this.txtFreqMin.Text = this.fmin.ToString();

            this.fmax = 1000;
            this.txtFreqMax.Text = this.fmax.ToString();

            this.R1 = 1;
            this.txtResistance_1.Text =this.R1.ToString();

            this.R2 = 1;
            this.txtResistance_2.Text =this.R2.ToString();

            this.L1 = 0.001;
            this.txtInductance.Text = this.L1.ToString();

            this.C1 = 0.0005;
            this.txtCapacitance.Text = this.C1.ToString(); 

            this.size = 1000;
            this.txtPoints.Text = this.size.ToString();
        }

        private void btnResetZoom_Click(object sender, RoutedEventArgs e)
        {
            chart01.ChartAreas["Magnitude"].AxisX.ScaleView.ZoomReset();
            chart01.ChartAreas["Magnitude"].AxisY.ScaleView.ZoomReset();

            chart01.ChartAreas["Phase"].AxisX.ScaleView.ZoomReset();
            chart01.ChartAreas["Phase"].AxisY.ScaleView.ZoomReset();

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
