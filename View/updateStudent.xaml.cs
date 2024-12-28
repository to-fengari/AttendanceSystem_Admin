using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class updateStudent : Window
    {
        private string connectionString;

        public updateStudent()
        {
            InitializeComponent();
            connectionString = App.ConnectionString;
            StudentNameTextBox.IsEnabled = false;
            DepartmentComboBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
        }

        private async void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            UserAuthentication authWindow = new UserAuthentication();
            if (authWindow.ShowDialog() == true)
            {
                string studentNumber = StudentNumberTextBox.Text.Trim();
                string studentName = StudentNameTextBox.Text.Trim();
                string newDepartment = ((ComboBoxItem)DepartmentComboBox.SelectedItem)?.Content.ToString();
                string password = PasswordBox.Password;

                if (string.IsNullOrWhiteSpace(studentNumber) || string.IsNullOrWhiteSpace(studentName) || string.IsNullOrWhiteSpace(newDepartment) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                Student student = await GetStudentDetails(studentNumber);
                if (student == null)
                {
                    MessageBox.Show("Student not found.");
                    return;
                }

                string oldDepartment = student.Department;

                if (oldDepartment != newDepartment)
                {
                    bool moved = await MoveStudentToNewDepartment(studentNumber, studentName, oldDepartment, newDepartment, password);
                    if (moved)
                    {
                        MessageBox.Show("Student moved to new department successfully.");
                    }
                    else
                    {
                        MessageBox.Show("Failed to move student. Please try again.");
                    }
                }
                else
                {
                    bool updated = await UpdateStudentInDatabase(studentNumber, studentName, newDepartment, password);
                    if (updated)
                    {
                        MessageBox.Show("Student updated successfully.");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update student. Please try again.");
                    }
                }
            }
        }

        private async Task<Student> GetStudentDetails(string studentNumber)
        {
            var departments = new List<string> { "BED", "CAE", "CAFAE", "CASE", "CCE", "CCJE", "CEE", "CHE", "CHSE", "CTE", "TS", "PS" };

            foreach (var department in departments)
            {
                string query = $"SELECT Student_Number, Student_Name, Password FROM users.Student_List_{department} WHERE Student_Number = @StudentNumber";

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
                                    Department = department,
                                    Password = reader["Password"].ToString()
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

        private async Task<bool> UpdateStudentInDatabase(string studentNumber, string studentName, string department, string password)
        {
            string query = $"UPDATE users.Student_List_{department} SET Student_Name = @StudentName, Password = @Password WHERE Student_Number = @StudentNumber";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentNumber", studentNumber);
                    command.Parameters.AddWithValue("@StudentName", studentName);
                    command.Parameters.AddWithValue("@Password", password);

                    try
                    {
                        await connection.OpenAsync();
                        int rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                    catch (SqlException sqlEx)
                    {
                        MessageBox.Show($"SQL Error: {sqlEx.Message}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error updating student: {ex.Message}");
                        return false;
                    }
                }
            }
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
                    StudentNameTextBox.Text = student.StudentName;
                    DepartmentComboBox.SelectedItem = DepartmentComboBox.Items
                        .Cast<ComboBoxItem>()
                        .FirstOrDefault(item => item.Content.ToString() == student.Department);
                    StudentNameTextBox.IsEnabled = true;
                    DepartmentComboBox.IsEnabled = true;
                    PasswordBox.IsEnabled = true;
                    PasswordBox.Password = student.Password;
                }
                else
                {
                    ErrorTextBlock.Text = "Student not found.";
                    ClearStudentFields();
                }
            }
            else
            {
                ClearStudentFields();
            }

            if (StudentNumberTextBox.Text.Length > 6)
            {
                StudentNumberTextBox.Text = StudentNumberTextBox.Text.Substring(0, 6);
                StudentNumberTextBox.SelectionStart = StudentNumberTextBox.Text.Length;
            }
        }

        private void ClearStudentFields()
        {
            StudentNameTextBox.Clear();
            DepartmentComboBox.SelectedIndex = -1;
            StudentNameTextBox.IsEnabled = false;
            DepartmentComboBox.IsEnabled = false;
            PasswordBox.IsEnabled = false;
            PasswordBox.Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void StudentNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        private static bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^[0-9]+$");
        }

        private async Task<bool> MoveStudentToNewDepartment(string studentNumber, string studentName, string oldDepartment, string newDepartment, string password)
        {
            string insertQuery = $"INSERT INTO users.Student_List_{newDepartment} (Student_Number, Student_Name, Password) VALUES (@StudentNumber, @StudentName, @Password)";
            string deleteQuery = $"DELETE FROM users.Student_List_{oldDepartment} WHERE Student_Number = @StudentNumber";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection, transaction))
                        {
                            insertCommand.Parameters.AddWithValue("@StudentNumber", studentNumber);
                            insertCommand.Parameters.AddWithValue("@StudentName", studentName);
                            insertCommand.Parameters.AddWithValue("@Password", password);
                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection, transaction))
                        {
                            deleteCommand.Parameters.AddWithValue("@StudentNumber", studentNumber);
                            await deleteCommand.ExecuteNonQueryAsync();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (SqlException sqlEx)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"SQL Error while moving student: {sqlEx.Message}");
                        return false;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show($"Error moving student: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        public class Student
        {
            public string StudentNumber { get; set; }
            public string StudentName { get; set; }
            public string Department { get; set; }
            public string Password { get; set; }
        }
    }
}