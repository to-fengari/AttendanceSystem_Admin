using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class Displaydept : UserControl
    {
        private readonly string connectionString = App.ConnectionString;
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
            ObservableCollection<DepartmentModel> departments = new ObservableCollection<DepartmentModel>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT Departments FROM event.Event_List WHERE EventID = @EventID";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EventID", EventID);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // Assuming Departments is a concatenated string
                            string departmentsString = reader.GetString(0);
                            var departmentNames = departmentsString.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var departmentName in departmentNames)
                            {
                                departments.Add(new DepartmentModel
                                {
                                    DepartmentName = departmentName
                                });
                            }
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