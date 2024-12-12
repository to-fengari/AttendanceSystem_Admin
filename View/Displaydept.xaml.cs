using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class Displaydept : UserControl
    {
        private readonly string connectionString = @"Server=MSI\SQLEXPRESS01; Database=LoginDB; Integrated Security=True;TrustServerCertificate=True;";
        public ContentControl MainDisplay { get; set; }
        private int EventID { get; set; }

        public Displaydept(ContentControl mainDisplay, int eventID)
        {
            InitializeComponent();
            MainDisplay = mainDisplay;
            EventID = eventID;

            LoadDepartments();
        }

        private void LoadDepartments()
        {
            List<DepartmentModel> departments = new List<DepartmentModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT DepartmentName FROM SelectedDepartments WHERE EventID = @EventID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EventID", EventID);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            departments.Add(new DepartmentModel
                            {
                                DepartmentName = reader.GetString(0)
                            });
                        }
                    }
                }

                DepartmentListView.ItemsSource = departments;
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Database error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ViewDetails_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null || button.Tag == null) return;

            string departmentName = button.Tag.ToString();

            var listWindow = new list(departmentName, EventID);
            listWindow.Show();
        }
    }

    public class DepartmentModel
    {
        public string DepartmentName { get; set; }
    }
}
