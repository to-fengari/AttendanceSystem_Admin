using Microsoft.Data.SqlClient;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace prototype.View
{
    public partial class Cevent : UserControl
    {
        public ContentControl MainDisplay { get; set; }
        private readonly string connectionString = App.ConnectionString;
        private bool isEditing;
        private int? eventId;

        public Cevent(ContentControl mainDisplay, bool isEditing, int? eventId = null)
        {   
            InitializeComponent();
            MainDisplay = mainDisplay;
            this.eventId = eventId;
            this.isEditing = isEditing;

            SetDatePickers();

            if (isEditing == true)
            {
                CeventTitle.Text = "Edit Event";
                LoadEventDetails(eventId.Value);
            }
            else
            {
                isEditing = false;
                CeventTitle.Text = "Create New Event";
                CreateNewEvent();
            }
        }

        private void SetDatePickers()
        {
            StartDatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
            EndDatePicker.DisplayDateStart = DateTime.Today.AddDays(1);
        }

        private void LoadEventDetails(int eventId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT EventName, StartDate, EndDate, StartTime, EndTime FROM event.Event_List WHERE EventID = @EventID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EventID", eventId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            EventNameTextBox.Text = reader.GetString(0);
                            StartDatePicker.SelectedDate = reader.GetDateTime(1);
                            EndDatePicker.SelectedDate = reader.GetDateTime(2);
                            StartTimeComboBox.SelectedItem = GetComboBoxItemByTime(reader.GetTimeSpan(3));
                            EndTimeComboBox.SelectedItem = GetComboBoxItemByTime(reader.GetTimeSpan(4));
                        }
                        else
                        {
                            MessageBox.Show("Event not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateNewEvent()
        {
            EventNameTextBox.Text = string.Empty;
            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate = DateTime.Today;
            StartTimeComboBox.SelectedIndex = 0;
            EndTimeComboBox.SelectedIndex = 0;
        }

        private ComboBoxItem GetComboBoxItemByTime(TimeSpan time)
        {
            foreach (ComboBoxItem item in StartTimeComboBox.Items)
            {
                if (DateTime.TryParse(item.Content.ToString(), out DateTime parsedTime) && parsedTime.TimeOfDay == time)
                {
                    return item;
                }
            }
            return null;
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

            if (endTime <= startTime)
            {
                MessageBox.Show("End Time must be later than Start Time.", "Invalid Time Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var eventDetails = new Dictionary<string, object>
            {
                { "EventName", eventName },
                { "StartDate", startDate.Value },
                { "EndDate", endDate.Value },
                { "StartTime", startTime },
                { "EndTime", endTime }
            };

            if (isEditing && eventId.HasValue)
            {
                eventDetails["EventID"] = eventId.Value;
            }

            if (MainDisplay == null)
            {
                MessageBox.Show("MainDisplay is not initialized.");
                return;
            }

            try
            {
                Choosedept choosedept = new Choosedept(MainDisplay, eventDetails, isEditing, eventId);
                MainDisplay.Content = choosedept;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}\n\n{ex.StackTrace}");
            }
        }

        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
            {
                MessageBox.Show("End Date must be on or after Start Date.", "Invalid Date Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate; 
            }
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate < StartDatePicker.SelectedDate)
            {
                MessageBox.Show("End Date must be on or after Start Date.", "Invalid Date Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                EndDatePicker.SelectedDate = StartDatePicker.SelectedDate; 
            }
        }
    }
}