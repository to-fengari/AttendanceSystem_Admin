using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class Event : UserControl
    {
        private readonly string connectionString = @"Server=MSI\SQLEXPRESS01; Database=LoginDB; Integrated Security=True;TrustServerCertificate=True;";
        public ContentControl MainDisplay { get; set; }

        public Event(ContentControl mainDisplay)
        {
            InitializeComponent();
            MainDisplay = mainDisplay;
            LoadEvents();
        }

        private void LoadEvents()
        {
            List<EventModel> events = new List<EventModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT EventID, EventName, StartDate, EndDate, StartTime, EndTime FROM Events";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            events.Add(new EventModel
                            {
                                EventID = reader.GetInt32(0),
                                EventName = reader.GetString(1),
                                StartDate = reader.GetDateTime(2),
                                EndDate = reader.GetDateTime(3),
                                StartTime = reader.GetTimeSpan(4),
                                EndTime = reader.GetTimeSpan(5)
                            });
                        }
                    }
                }

                EventDataGrid.ItemsSource = events;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ViewDepartments_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null) return;

            int eventID = (int)button.Tag;

            if (MainDisplay != null)
            {
                MainDisplay.Content = new Displaydept(MainDisplay, eventID);
            }
        }
    }

    public class EventModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string StartTimeFormatted => DateTime.Today.Add(StartTime).ToString("hh:mm tt");
        public string EndTimeFormatted => DateTime.Today.Add(EndTime).ToString("hh:mm tt");
    }
}
