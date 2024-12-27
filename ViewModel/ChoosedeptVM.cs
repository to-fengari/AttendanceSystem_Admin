using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace prototype.ViewModel
{
    public class ChoosedeptVM : INotifyPropertyChanged
    {
        private string connectionString = App.ConnectionString;
        private List<string> _selectedDepartments;
        private Dictionary<string, object> _eventDetails;
        private bool _isEditing;
        private int? _eventId;
        public event Action<string> EventCreated;

        public ChoosedeptVM(Dictionary<string, object> eventDetails, bool isEditing, int? eventId)
        {
            _eventDetails = eventDetails;
            _isEditing = isEditing;
            _selectedDepartments = new List<string>();
            _eventId = eventId;

            if (_isEditing)
            {
                LoadExistingDepartments();
            }
        }

        private void LoadExistingDepartments()
        {
            if (!_eventDetails.ContainsKey("EventID"))
            {
                MessageBox.Show("Event ID is missing.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int eventId = (int)_eventDetails["EventID"];

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Departments FROM event.Event_List WHERE EventID = @EventID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EventID", eventId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string departmentsString = reader.GetString(0);
                            if (!string.IsNullOrEmpty(departmentsString))
                            {
                                var departments = departmentsString.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                                foreach (var department in departments)
                                {
                                    AddDepartment(department);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("No departments found for this event.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public List<string> SelectedDepartments
        {
            get => _selectedDepartments;
            set
            {
                _selectedDepartments = value;
                OnPropertyChanged();
            }
        }

        public void AddDepartment(string departmentName)
        {
            if (!_selectedDepartments.Contains(departmentName))
            {
                _selectedDepartments.Add(departmentName);
            }
        }

        public void RemoveDepartment(string departmentName)
        {
            if (_selectedDepartments.Contains(departmentName))
            {
                _selectedDepartments.Remove(departmentName);
            }
        }

        public void SaveEventAndDepartments(int? eventId)
        {
            int? _eventId = eventId;

            if (_selectedDepartments.Count == 0)
            {
                MessageBox.Show("No department selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string departmentsConcatenated = string.Join(", ", _selectedDepartments);

                    if (_isEditing)
                    {
                        string eventQuery = "UPDATE event.Event_List SET EventName = @EventName, StartDate = @StartDate, EndDate = @EndDate, StartTime = @StartTime, EndTime = @EndTime, Departments = @Departments WHERE EventID = @EventID";
                        using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                        {
                            eventCommand.Parameters.AddWithValue("@EventID", _eventDetails["EventID"]);
                            eventCommand.Parameters.AddWithValue("@EventName", _eventDetails["EventName"]);
                            eventCommand.Parameters.AddWithValue("@StartDate", _eventDetails["StartDate"]);
                            eventCommand.Parameters.AddWithValue("@EndDate", _eventDetails["EndDate"]);
                            eventCommand.Parameters.AddWithValue("@StartTime", _eventDetails["StartTime"]);
                            eventCommand.Parameters.AddWithValue("@EndTime", _eventDetails["EndTime"]);
                            eventCommand.Parameters.AddWithValue("@Departments", departmentsConcatenated);

                            eventCommand.ExecuteNonQuery();
                        }
                        MessageBox.Show("Event Edited successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        string eventQuery = @"
                        INSERT INTO event.Event_List (EventName, StartDate, EndDate, StartTime, EndTime, Departments) 
                        VALUES (@EventName, @StartDate, @EndDate, @StartTime, @EndTime, @Departments);
                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                        using (SqlCommand eventCommand = new SqlCommand(eventQuery, connection))
                        {
                            eventCommand.Parameters.AddWithValue("@EventName", _eventDetails["EventName"]);
                            eventCommand.Parameters.AddWithValue("@StartDate", _eventDetails["StartDate"]);
                            eventCommand.Parameters.AddWithValue("@EndDate", _eventDetails["EndDate"]);
                            eventCommand.Parameters.AddWithValue("@StartTime", _eventDetails["StartTime"]);
                            eventCommand.Parameters.AddWithValue("@EndTime", _eventDetails["EndTime"]);
                            eventCommand.Parameters.AddWithValue("@Departments", departmentsConcatenated);

                            _eventId = (int?)eventCommand.ExecuteScalar();
                        }
                        CreateTable(_eventId, _isEditing);
                    }
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CreateTable(int? eventID, bool isEditing)
        {
            if (isEditing || eventID == null)
            {
                return;
            }

            string tableName = $"Attendance_{eventID}";

            string TableQuery = $@"
                CREATE TABLE event.{tableName} (
                    Student_Number INT NOT NULL,
                    Student_Name NVARCHAR(100) NOT NULL,
                    Department NVARCHAR(100) NOT NULL,
                    Attendance_Time DATETIME NOT NULL
                );";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(TableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}