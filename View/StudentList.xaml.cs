using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace prototype.View
{
    public partial class StudentList : UserControl
    {
        private string connectionString;
        private Button _selectedButton;
        private string _currentDepartment;

        public StudentList()
        {
            connectionString = App.ConnectionString;
            InitializeComponent();
            _currentDepartment = "All";
            AllDepartmentButton_Click(this, null);
        }

        private async void AllDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(AllDepartmentButton);
            await LoadAllStudents();

        }

        private void BEDDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(BEDDepartmentButton);
            LoadStudents("BED");
        }

        private void CAEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CAEDepartmentButton);
            LoadStudents("CAE");
        }

        private void CAFAEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CAFAEDepartmentButton);
            LoadStudents("CAFAE");
        }

        private void CASEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CASEDepartmentButton);
            LoadStudents("CASE");
        }

        private void CCEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CCEDepartmentButton);
            LoadStudents("CCE");
        }

        private void CCJEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CCJEDepartmentButton);
            LoadStudents("CCJE");
        }

        private void CEEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CEEDepartmentButton);
            LoadStudents("CEE");
        }

        private void CHEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CHEDepartmentButton);
            LoadStudents("CHE");
        }

        private void CHSEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CHSEDepartmentButton);
            LoadStudents("CHSE");
        }

        private void CTEDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(CTEDepartmentButton);
            LoadStudents("CTE");
        }

        private void TSDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(TSDepartmentButton);
            LoadStudents("TS");
        }

        private void PSDepartmentButton_Click(object sender, RoutedEventArgs e)
        {
            HighlightSelectedButton(PSDepartmentButton);
            LoadStudents("PS");
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var addStudentWindow = new addStudent();
            addStudentWindow.ShowDialog();
            LoadStudents(_currentDepartment);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var deleteStudentWindow = new deleteStudent();
            deleteStudentWindow.ShowDialog();
            LoadStudents(_currentDepartment);
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            var updateStudentWindow = new updateStudent();
           updateStudentWindow.ShowDialog();
            LoadStudents(_currentDepartment);
        }

        public class Student
        {
            public string StudentNumber { get; set; }
            public string StudentName { get; set; }
        }

        private async void LoadStudents(string department)
        {
            if (department.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                LoadAllStudents();
                return;
            }
            else
            {
                StudentListDataGrid.ItemsSource = null;

                string query = $"SELECT Student_Number, Student_Name FROM users.Student_List_{department}";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    ObservableCollection<Student> students = new ObservableCollection<Student>();

                    try
                    {
                        await connection.OpenAsync();
                        using (SqlDataReader reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                students.Add(new Student
                                {
                                    StudentNumber = reader["Student_Number"].ToString(),
                                    StudentName = reader["Student_Name"].ToString()
                                });
                            }
                        }

                        StudentListDataGrid.ItemsSource = students;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error fetching students: {ex.Message}");
                    }
                }
            }
        }

        private async Task LoadAllStudents()
        {
            StudentListDataGrid.ItemsSource = null;

            var allStudents = new ObservableCollection<Student>();
            var departments = new List<string> { "BED", "CAE", "CAFAE", "CASE", "CCE", "CCJE", "CEE", "CHE", "CHSE", "CTE", "TS", "PS" };

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var department in departments)
                    {
                        string query = $"SELECT Student_Number, Student_Name FROM users.Student_List_{department}";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    allStudents.Add(new Student
                                    {
                                        StudentNumber = reader["Student_Number"].ToString(),
                                        StudentName = reader["Student_Name"].ToString()
                                    });
                                }
                            }
                        }
                    }
                }

                StudentListDataGrid.ItemsSource = allStudents;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error fetching all students: {ex.Message}");
            }
        }

        private void HighlightSelectedButton(Button button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.Tag = false;
                _selectedButton.Background = new SolidColorBrush(Color.FromRgb(178, 10, 7));
                _selectedButton.Foreground = new SolidColorBrush(Color.FromRgb(245, 207, 40));
                _selectedButton.FontWeight = FontWeights.Normal;
                _selectedButton.BorderThickness = new Thickness(1);
            }

            _selectedButton = button;
            _selectedButton.Tag = true;
            _selectedButton.Background = new SolidColorBrush(Color.FromRgb(245, 207, 40));
            _selectedButton.Foreground = new SolidColorBrush(Color.FromRgb(178, 10, 7));
            _selectedButton.FontWeight = FontWeights.Bold;
            _selectedButton.BorderThickness = new Thickness(2);

            _currentDepartment = button.Content.ToString();
        }

    }
}