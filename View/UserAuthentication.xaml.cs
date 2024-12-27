using System.Windows;

namespace prototype.View
{
    public partial class UserAuthentication : Window
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public UserAuthentication()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Username = UsernameTextBox.Text;
            Password = PasswordBox.Password;

            UserSession.CurrentUsername = Username;
            UserSession.CurrentPassword = Password;

            this.DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}