using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;

namespace prototype.View
{
    public partial class list : Window
    {
        private readonly string DepartmentName;
        private readonly int EventID;
        private List<StudentModel> students;

        public list(string departmentName, int eventID)
        {
            InitializeComponent();
            DepartmentName = departmentName;
            EventID = eventID;

            LoadDepartmentDetails(EventID);
        }

        private void LoadDepartmentDetails(int eventID)
        {
            DepartmentNameText.Text = DepartmentName;

            List<StudentModel> students = new List<StudentModel>();

            string connectionString = App.ConnectionString;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string EventID = $"{eventID}";
                    {
                        string query = $@"SELECT Student_Number, Student_Name FROM event.Attendance_{EventID} WHERE Department = @DepartmentName";
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@DepartmentName", DepartmentName);
                        connection.Open();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                students.Add(new StudentModel
                                {
                                    StudentID = reader.GetInt32(0),
                                    StudentName = reader.GetString(1)
                                });
                            }
                        }
                    }

                    DetailsListView.ItemsSource = students;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (students == null || students.Count == 0)
            {
                MessageBox.Show("No data available to export.", "Export Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string filePath = $"{DepartmentName}_Attendees.xlsx";

            try
            {
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Attendees");
                    worksheet.Cells[1, 1].Value = "Student ID";
                    worksheet.Cells[1, 2].Value = "Student Name";

                    for (int i = 0; i < students.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = students[i].StudentID;
                        worksheet.Cells[i + 2, 2].Value = students[i].StudentName;
                    }

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        package.SaveAs(fileStream);
                    }
                }

                MessageBox.Show($"Data exported successfully to {filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (IOException ex)
            {
                MessageBox.Show($"File export error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }

    public class StudentModel
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
    }
}