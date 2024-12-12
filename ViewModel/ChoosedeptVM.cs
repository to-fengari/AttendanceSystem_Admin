using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace prototype.ViewModel
{
    public class ChoosedeptVM : INotifyPropertyChanged
    {
        private List<string> _selectedDepartments = new List<string>(); //para sa departments
        private int _eventID; 

        public ChoosedeptVM(int eventID)
        {
            _eventID = eventID; //gipasa ang ID
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

        public void SaveDepartments()
        {
            if (_selectedDepartments.Count == 0)
            {
                MessageBox.Show("No department selected.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string connectionString = @"Server=MSI\SQLEXPRESS01; Database=LoginDB; Integrated Security=True;TrustServerCertificate=True; ";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (string department in _selectedDepartments)
                    {
                        string query = "INSERT INTO SelectedDepartments (EventID, DepartmentName, AddedDate) VALUES (@EventID, @DepartmentName, GETDATE())";
                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@EventID", _eventID); 
                            command.Parameters.AddWithValue("@DepartmentName", department);
                            command.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Departments saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                _selectedDepartments.Clear(); 
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
