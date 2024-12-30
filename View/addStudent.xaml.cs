using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using CsvHelper;
using Microsoft.Win32;

namespace prototype.View
{
    public partial class addStudent : Window
    {
        string connectionString = App.ConnectionString;

        public addStudent()
        {
            InitializeComponent();
        }

        private async void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StudentNumberTextBox.Text) ||
                string.IsNullOrWhiteSpace(StudentNameTextBox.Text) ||
                DepartmentComboBox.SelectedItem == null ||
                string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            bool exists = await StudentNumberExists(StudentNumberTextBox.Text);
            if (exists)
            {
                MessageBox.Show("The student number already exists in another department. Please use a different student number.");
                return;
            }

            var student = new Student
            {
                StudentNumber = StudentNumberTextBox.Text,
                StudentName = StudentNameTextBox.Text,
                Department = ((ComboBoxItem)DepartmentComboBox.SelectedItem).Content.ToString(),
                Password = PasswordBox.Password
            };

            UserAuthentication authWindow = new UserAuthentication();
            if (authWindow.ShowDialog() == true)
            {
                bool isSaved = await SaveStudentToDatabase(student);
                if (isSaved)
                {
                    ClearInputFields();
                    MessageBox.Show("Student added successfully.");
                }
                else
                {
                    MessageBox.Show("Failed to add student. Please try again.");
                }
            }
        }

        private void ClearInputFields()
        {
            StudentNumberTextBox.Clear();
            StudentNameTextBox.Clear();
            DepartmentComboBox.SelectedIndex = -1;
            PasswordBox.Clear();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void UploadCSVFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Select a CSV file"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                if (!ValidateCSVFormat(filePath))
                {
                    MessageBox.Show("The CSV file format is incorrect or the file is currently in use by another process.. Please check the file and try again.");
                    return;
                }

                try
                {
                    UserAuthentication authWindow = new UserAuthentication();
                    if (authWindow.ShowDialog() == true)
                    {
                        using (var reader = new StreamReader(filePath))
                        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                        {
                            var records = csv.GetRecords<Student>().ToList();

                            foreach (var student in records)
                            {
                                if (await StudentNumberExists(student.StudentNumber))
                                {
                                    MessageBoxResult result = MessageBox.Show(
                                        $"The student number {student.StudentNumber} already exists. Would you like to skip this entry or cancel the operation?",
                                        "Student Number Exists",
                                        MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning);

                                    if (result == MessageBoxResult.No)
                                    {
                                        MessageBox.Show("Operation cancelled.");
                                        return;
                                    }
                                    continue;
                                }
                                await SaveStudentToDatabase(student);
                            }
                        }
                    }

                    MessageBox.Show("CSV file processed successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error processing CSV file: {ex.Message}");
                }
            }
        }

        private bool ValidateCSVFormat(string filePath)
        {
            if (IsFileInUse(filePath))
            {
                return false;
            }

            string[] expectedHeaders = { "StudentNumber", "StudentName", "Department", "Password" };
            var validDepartments = new HashSet<string> { "BED", "CAE", "CAFAE", "CASE", "CCE", "CCJE", "CEE", "CHE", "CHSE", "CTE", "TS", "PS" };

            using (var reader = new StreamReader(filePath))
            {
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    return false;
                }

                string[] headers = headerLine.Split(',');
                if (headers.Length != expectedHeaders.Length)
                {
                    return false;
                }

                for (int i = 0; i < expectedHeaders.Length; i++)
                {
                    if (headers[i].Trim() != expectedHeaders[i])
                    {
                        return false;
                    }
                }

                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string[] fields = line.Split(',');

                    if (fields.Length != headers.Length)
                    {
                        return false;
                    }

                    string studentNumber = fields[0].Trim();
                    if (studentNumber.Length != 6 || !Regex.IsMatch(studentNumber, @"^\d{6}$"))
                    {
                        return false;
                    }

                    string department = fields[2].Trim();
                    if (!validDepartments.Contains(department))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsFileInUse(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {   
                return true;
            }
        }

        private void StudentNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text) || (StudentNumberTextBox.Text.Length >= 6);
        }

        private async void StudentNumberTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ErrorTextBlock.Text = string.Empty;

            if (StudentNumberTextBox.Text.Length == 6)
            {
                string studentNumber = StudentNumberTextBox.Text.Trim();
                bool studentExists = await StudentNumberExists(studentNumber);

                if (studentExists)
                {
                    ErrorTextBlock.Text = "Student with the Student Number Already Exists";
                    ConfirmButton.IsEnabled = false;
                }
                else
                {
                    ErrorTextBlock.Text = string.Empty;
                    ConfirmButton.IsEnabled = true;
                }
            }
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
            public string Password { get; set; }
        }

        private async Task<bool> SaveStudentToDatabase(Student student)
        {
            string query = $"INSERT INTO users.Student_List_{student.Department} (Student_Number, Student_Name, Password) VALUES (@StudentNumber, @StudentName, @Password)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
                    command.Parameters.AddWithValue("@StudentName", student.StudentName);
                    command.Parameters.AddWithValue("@Password", student.Password);

                    try
                    {
                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving student: {ex.Message}");
                        return false;
                    }
                }
            }
        }

        private async Task<bool> StudentNumberExists(string studentNumber)
        {
            var departments = new List<string> { "BED", "CAE", "CAFAE", "CASE", "CCE", "CCJE", "CEE", "CHE", "CHSE", "CTE", "TS", "PS" };

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var department in departments)
                {
                    string query = $"SELECT COUNT(*) FROM users.Student_List_{department} WHERE Student_Number = @StudentNumber";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@StudentNumber", studentNumber);
                        int count = (int)await command.ExecuteScalarAsync();

                        if (count > 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}