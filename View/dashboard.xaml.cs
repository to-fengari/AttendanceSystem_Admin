using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class dashboard : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int totalStudents;
        public int TotalStudents
        {
            get => totalStudents;
            set
            {
                totalStudents = value;
                OnPropertyChanged(nameof(TotalStudents));
            }
        }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public string IncludedDepartments { get; set; }

        public dashboard(string selectedEventName)
        {
            InitializeComponent();
            LoadEventNames(selectedEventName);
        }

        private void LoadEventNames(string selectedEventName)
        {
            string connectionString = App.ConnectionString;
            List<string> eventNames = new List<string>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT EventName FROM event.Event_List";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            eventNames.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            EventComboBox.ItemsSource = eventNames;

            if (!string.IsNullOrEmpty(selectedEventName) && eventNames.Contains(selectedEventName))
            {
                EventComboBox.SelectedItem = selectedEventName;
            }
            else if (eventNames.Any())
            {
                EventComboBox.SelectedItem = eventNames[0];
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (EventComboBox.SelectedItem != null)
            {
                string selectedEventName = EventComboBox.SelectedItem.ToString();
                LoadData(selectedEventName);
            }
        }

        private void EventComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventComboBox.SelectedItem != null)
            {
                string selectedEventName = EventComboBox.SelectedItem.ToString();
                LoadData(selectedEventName);
            }
        }

        private void LoadData(string selectedEventName)
        {
            int eventID = GetEventID(selectedEventName);
            if (eventID == -1)
            {
                MessageBox.Show("Event ID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var includedDepartments = GetIncludedDepartments(eventID);

            if (includedDepartments == null || !includedDepartments.Any())
            {
                MessageBox.Show("No departments found for the selected event.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var studentCounts = GetStudentCountsByDepartment(includedDepartments, eventID);

            TotalStudents = studentCounts.Values.Sum(x => x.Total);
            IncludedDepartments = string.Join(", ", studentCounts.Keys);

            PieChart.Series = new SeriesCollection();
            int index = 0;

            foreach (var entry in studentCounts)
            {
                Color attendedColor = GetDepartmentColor(index);
                Color nonAttendedColor = GetLighterShade(attendedColor, 0.5f);

                int attendedCount = entry.Value.Attended;
                int totalCount = entry.Value.Total;
                double attendedPercentage = totalCount > 0 ? (double)attendedCount / totalCount * 100 : 0;
                double notAttendedPercentage = totalCount > 0 ? (double)(totalCount - attendedCount) / totalCount * 100 : 0;

                PieChart.Series.Add(new PieSeries
                {
                    Title = $"{entry.Key} (Attended: {attendedPercentage:F1}%)",
                    Values = new ChartValues<int> { attendedCount },
                    DataLabels = true,
                    Fill = new SolidColorBrush(attendedColor)
                });

                PieChart.Series.Add(new PieSeries
                {
                    Title = $"{entry.Key} (Not Attended: {notAttendedPercentage:F1}%)",
                    Values = new ChartValues<int> { totalCount - attendedCount },
                    DataLabels = false,
                    Fill = new SolidColorBrush(nonAttendedColor)
                });

                index++;
            }

            SeriesCollection = new SeriesCollection
    {
        new ColumnSeries
        {
            Title = "Total Students",
            Values = new ChartValues<int>(studentCounts.Values.Select(x => x.Total))
        },
        new ColumnSeries
        {
            Title = "Attended Students",
            Values = new ChartValues<int>(studentCounts.Values.Select(x => x.Attended))
        }
    };

            Labels = studentCounts.Keys.ToArray();

            CartesianChart.Series = SeriesCollection;
            CartesianChart.AxisX[0].Labels = Labels;

            DataContext = this;
        }

        private int GetEventID(string eventName)
        {
            int eventID = -1;
            string connectionString = App.ConnectionString;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT EventID FROM event.Event_List WHERE EventName = @EventName";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EventName", eventName);
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            eventID = Convert.ToInt32(result);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return eventID;
        }

        private List<string> GetIncludedDepartments(int eventID)
        {
            List<string> departments = new List<string>();
            string connectionString = App.ConnectionString;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Departments FROM event.Event_List WHERE EventID = @EventID";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EventID", eventID);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string departmentString = reader.GetString(0);
                                departments = departmentString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                              .Select(d => d.Trim())
                                                              .ToList();
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return departments;
        }

        private Dictionary<string, (int Attended, int Total)> GetStudentCountsByDepartment(List<string> departments, int eventID)
        {
            Dictionary<string, (int Attended, int Total)> studentCounts = new Dictionary<string, (int, int)>();
            string connectionString = App.ConnectionString;

            try
            {
                foreach (var department in departments)
                {
                    string studentTableName = $"users.Student_List_{department}";
                    string attendanceTableName = $"event.Attendance_{eventID}";

                    string totalQuery = $"SELECT COUNT(*) FROM {studentTableName};";
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(totalQuery, connection))
                        {
                            connection.Open();
                            int totalCount = (int)command.ExecuteScalar();

                            string attendedQuery = $"SELECT COUNT(*) FROM {attendanceTableName} WHERE Department = @DepartmentName;";
                            command.CommandText = attendedQuery;
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@DepartmentName", department);

                            int attendedCount = (int)command.ExecuteScalar();

                            studentCounts[department] = (attendedCount, totalCount);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return studentCounts;
        }

        private Color GetLighterShade(Color color, float factor)
        {
            return Color.FromArgb(
                color.A,
                (byte)Math.Min(color.R + (255 - color.R) * factor, 255),
                (byte)Math.Min(color.G + (255 - color.G) * factor, 255),
                (byte)Math.Min(color.B + (255 - color.B) * factor, 255)
            );
        }

        private Color GetDepartmentColor(int index)
        {
            byte r = (byte)((index * 50) % 256);
            byte g = (byte)((index * 100) % 256);
            byte b = (byte)((index * 150) % 256);
            return Color.FromRgb(r, g, b);
        }

        public Func<double, string> LabelFormatter => value =>
        {
            if (value >= 0 && value < Labels.Length)
            {
                return Labels[(int)value];
            }
            return string.Empty;
        };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }   
    }
}