using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class Cevent : UserControl
    {
        private readonly string connectionString = @"Server=MSI\SQLEXPRESS01; Database=LoginDB; Integrated Security=True; Encrypt=True; TrustServerCertificate=True";
        public ContentControl MainDisplay { get; set; }

        public Cevent(ContentControl mainDisplay)
        {
            InitializeComponent();
            MainDisplay = mainDisplay;
        }

        private void choosedept_btn(object sender, RoutedEventArgs e)
        {
            string eventName = EventNameTextBox.Text;
            DateTime? startDate = StartDatePicker.SelectedDate;
            DateTime? endDate = EndDatePicker.SelectedDate;
            ComboBoxItem startTimeItem = (ComboBoxItem)StartTimeComboBox.SelectedItem;
            ComboBoxItem endTimeItem = (ComboBoxItem)EndTimeComboBox.SelectedItem;

            if (string.IsNullOrEmpty(eventName) || startDate == null || endDate == null || startTimeItem == null || endTimeItem == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string startTimeString = startTimeItem.Content.ToString();
            string endTimeString = endTimeItem.Content.ToString();
            TimeSpan startTime = DateTime.Parse(startTimeString).TimeOfDay;
            TimeSpan endTime = DateTime.Parse(endTimeString).TimeOfDay;

            var eventDetails = new Dictionary<string, object>
            {
                { "EventName", eventName },
                { "StartDate", startDate.Value },
                { "EndDate", endDate.Value },
                { "StartTime", startTime },
                { "EndTime", endTime }
            };

            int eventID = SaveEventToDatabase(eventDetails);

            if (eventID != -1)
            {
                if (MainDisplay == null)
                {
                    MessageBox.Show("MainDisplay is not initialized.");
                    return;
                }

                try
                {
                    Choosedept choosedept_btn = new Choosedept(MainDisplay, eventID);
                    MainDisplay.Content = choosedept_btn;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred: {ex.Message}\n\n{ex.StackTrace}");
                }
            }
        }

        private int SaveEventToDatabase(Dictionary<string, object> eventDetails)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "INSERT INTO Events (EventName, StartDate, EndDate, StartTime, EndTime) OUTPUT INSERTED.EventID VALUES (@EventName, @StartDate, @EndDate, @StartTime, @EndTime)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@EventName", eventDetails["EventName"]);
                        command.Parameters.AddWithValue("@StartDate", eventDetails["StartDate"]);
                        command.Parameters.AddWithValue("@EndDate", eventDetails["EndDate"]);
                        command.Parameters.AddWithValue("@StartTime", eventDetails["StartTime"]);
                        command.Parameters.AddWithValue("@EndTime", eventDetails["EndTime"]);

                        connection.Open();
                        return (int)command.ExecuteScalar(); 
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return -1; 
            }
        }
    }
}
