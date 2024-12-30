using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class deleteStudent : Window
    {
        private string connectionString;

        public deleteStudent()
        {
            InitializeComponent();
            connectionString = App.ConnectionString;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string studentNumber = StudentNumberTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(studentNumber))
            {
                MessageBox.Show("Please enter a student number.");
                return;
            }

            Student student = await GetStudentDetails(studentNumber);
            if (student == null)
            {
                MessageBox.Show("Student not found.");
                return;
            }
            StudentNameTextBlock.Text = $"Name: {student.StudentName}";
            DepartmentTextBlock.Text = $"Department: {student.Department}";
            StudentNameTextBlock.Visibility = Visibility.Visible;
            DepartmentTextBlock.Visibility = Visibility.Visible;

            UserAuthentication authWindow = new UserAuthentication();
            if (authWindow.ShowDialog() == true)
            {
                bool deleted = await DeleteStudentFromDatabase(student.StudentNumber, student.Department);
                if (deleted)
                {
                    MessageBox.Show("Student deleted successfully.");
                    StudentNumberTextBox.Clear();
                }
                else
                {
                    MessageBox.Show("Failed to delete student. Please check if the student number exists.");
                }
            }
        }

        private async Task<Student> GetStudentDetails(string studentNumber)
        {
            var departments = new List<string> { "BED", "CAE", "CAFAE", "CASE", "CCE", "CCJE", "CEE", "CHE", "CHSE", "CTE", "TS", "PS" };

            foreach (var department in departments)
            {
                string query = $"SELECT Student_Number, Student_Name FROM users.Student_List_{department} WHERE Student_Number = @StudentNumber";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@StudentNumber", studentNumber);

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                return new Student
                                {
                                    StudentNumber = reader["Student_Number"].ToString(),
                                    StudentName = reader["Student_Name"].ToString(),
                                    Department = department
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error fetching student details: {ex.Message}");
                    }
                }
            }
            return null;
        }

        private async Task<bool> DeleteStudentFromDatabase(string studentNumber, string department)
        {
            string query = $"DELETE FROM users.Student_List_{department} WHERE Student_Number = @StudentNumber";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@StudentNumber", studentNumber);

                try
                {
                    await connection.OpenAsync();
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error deleting student: {ex.Message}");
                    return false;
                }
            }
        }

        private void StudentNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private async void StudentNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorTextBlock.Text = string.Empty;
            if (StudentNumberTextBox.Text.Length == 6)
            {
                string studentNumber = StudentNumberTextBox.Text.Trim();
                Student student = await GetStudentDetails(studentNumber);

                if (student != null)
                {
                    StudentNameTextBlock.Text = $"Name: {student.StudentName}";
                    DepartmentTextBlock.Text = $"Department: {student.Department}";
                    StudentNameTextBlock.Visibility = Visibility.Visible;
                    DepartmentTextBlock.Visibility = Visibility.Visible;
                    DeleteButton.IsEnabled = true;

                }
                else
                {
                    ErrorTextBlock.Text = "Student not found.";
                    StudentNameTextBlock.Text = "Name:";
                    DepartmentTextBlock.Text = "Department:";
                    DeleteButton.IsEnabled = false;
                }
            }
            else
            {
                StudentNameTextBlock.Text = "Name:";
                DepartmentTextBlock.Text = "Department:";
                DeleteButton.IsEnabled = false;

            }

            if (StudentNumberTextBox.Text.Length > 6)
            { 
                StudentNumberTextBox.Text = StudentNumberTextBox.Text.Substring(0, 6);
                StudentNumberTextBox.SelectionStart = StudentNumberTextBox.Text.Length;
                DeleteButton.IsEnabled = false;

            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); 
        }

        private static bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        public class Student
        {
            public string StudentNumber { get; set; }
            public string StudentName { get; set; }
            public string Department { get; set; }
        }
    }
}