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
using LiveCharts;
using LiveCharts.Wpf;

namespace prototype.View
{
    /// <summary>
    /// Interaction logic for dashboard.xaml
    /// </summary>
    public partial class dashboard : UserControl
    {
        public dashboard()
        {
            InitializeComponent();
            PointLabel = chartPoint => string.Format("{0}({1:P})", chartPoint.Y, chartPoint.Participation);

            DataContext = this;

            SeriesCollection = new SeriesCollection()
            {
                new ColumnSeries
                {
                    Title = "basta",
                    Values = new ChartValues<double>{20,10,36,50}
                }
            };

            SeriesCollection.Add(new ColumnSeries
            {
                Title = "Kuan",
                Values = new ChartValues<double> { 10, 50, 26}
            });

            SeriesCollection[1].Values.Add(48d);
            Labels = new[] { "CHE, CEE, CHSE" };
            Values = value => value.ToString();

            DataContext = this;
        }

        public Func<ChartPoint, string> PointLabel { get; set; }
        private void PieChart_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            var chart=(LiveCharts.Wpf.PieChart)chartPoint.ChartView;
            foreach (PieSeries series in chart.Series)
                series.PushOut = 0;

            var selectedSeries = (PieSeries)chartPoint.SeriesView;
            selectedSeries.PushOut = 8;
        }

        public SeriesCollection SeriesCollection { get; set; }

        public string[] Labels { get; set; }

        public Func<string, string> Values {  get; set; }
    }
}
