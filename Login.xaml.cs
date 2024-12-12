using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Data.SqlClient;

namespace prototype.View
{
    public partial class Login : Window
    {
        // SQL Server connection string (update with your server details)
        private readonly string connectionString = @"Server=MSI\SQLEXPRESS01; Database=LoginDB; Integrated Security=True; Encrypt=True; TrustServerCertificate=True";
        public Login()
        {
            InitializeComponent();
        }

        // Event handler for the minimize button click
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Minimize the current window
            this.WindowState = WindowState.Minimized;
        }

        // Event handler for the login button click
        private void loginmin_Click(object sender, RoutedEventArgs e)
        {
            string name = username.Text;
            string pass = password.Password;

            if (IsValidUser(name, pass))
            {
                // If login is successful, show the main dashboard
                var dashboard = new MainWindow();
                dashboard.Show();
                this.Close();
            }
            else
            {
                // If login fails, show an error message
                MessageBox.Show("Invalid username or password", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Method to validate username and password against the database
        private bool IsValidUser(string username, string password)
        {
            bool isValid = false;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM Users WHERE username = @username AND password = @password";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);

                        connection.Open();
                        int count = Convert.ToInt32(command.ExecuteScalar());

                        if (count > 0)
                        {
                            isValid = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return isValid;
        }

        private void username_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // Explicitly call the loginmin_Click method
                loginmin_Click(loginmin_Click, new RoutedEventArgs());
            }
        }
    }
}
