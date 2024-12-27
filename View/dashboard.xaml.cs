using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class dashboard : UserControl
    {
        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public int TotalStudents { get; set; }
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
            // Get the event ID for the selected event name
            int eventID = GetEventID(selectedEventName);
            if (eventID == -1)
            {
                MessageBox.Show("Event ID not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Get the included departments for the selected event ID
            var includedDepartments = GetIncludedDepartments(eventID);

            if (includedDepartments == null || !includedDepartments.Any())
            {
                MessageBox.Show("No departments found for the selected event.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var studentCounts = GetStudentCountsByDepartment(includedDepartments);

            TotalStudents = studentCounts.Values.Sum();
            IncludedDepartments = string.Join(", ", studentCounts.Keys);

            PieChart.Series = new SeriesCollection();
            foreach (var entry in studentCounts)
            {
                PieChart.Series.Add(new PieSeries
                {
                    Title = entry.Key,
                    Values = new ChartValues<int> { entry.Value },
                    DataLabels = true
                });
            }

            SeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Students",
                    Values = new ChartValues<int>(studentCounts.Values)
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

        private Dictionary<string, int> GetStudentCountsByDepartment(List<string> departments)
        {
            Dictionary<string, int> studentCounts = new Dictionary<string, int>();
            string connectionString = App.ConnectionString;

            try
            {
                foreach (var department in departments)
                {
                    string tableName = $"users.Student_List_{department}";
                    string query = $"SELECT COUNT(*) FROM {tableName};";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            connection.Open();
                            int studentCount = (int)command.ExecuteScalar();
                            studentCounts[department] = studentCount;
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

        public Func<double, string> LabelFormatter => value =>
        {
            if (value >= 0 && value < Labels.Length)
            {
                return Labels[(int)value];
            }
            return string.Empty;
        };
    }
}