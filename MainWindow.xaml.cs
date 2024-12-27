using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using prototype.View;
using prototype.ViewModel;

namespace prototype
{
    public partial class MainWindow : Window
    {
        public List<string> EventList { get; set; } = new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            LoadHomeView();
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private bool isMaximized = false;
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (isMaximized)
                {
                    this.WindowState = WindowState.Normal;
                    this.Width = 1080;
                    this.Height = 720;

                    isMaximized = false;
                }
            }
        }

        private void Dashboardbtn(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            Button? clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }

            dashboard dashboard = new dashboard(string.Empty);
            MainDisplay.Content = dashboard;
        }


        private void Ceeventbtn_Click(object sender, RoutedEventArgs e)
        {
            bool isEditing = false;
            int eventId = 0;
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }

            Cevent cevent = new Cevent(MainDisplay, isEditing, eventId);
            MainDisplay.Content = cevent;
        }

        private void Event_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            Button? clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }

            if (MainDisplay != null)
            {
                Event eventView = new Event(MainDisplay);

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    foreach (string eventDetails in mainWindow.EventList)
                    {
                        Button eventButton = new Button
                        {
                            Content = eventDetails,
                            Width = 100,
                            Height = 100,
                            Background = Brushes.LightBlue
                        };

                        if (eventView.FindName("ButtonContainer") is Panel container)
                        {
                            container.Children.Add(eventButton);
                        }
                    }
                }
                MainDisplay.Content = eventView;
            }
        }

        private void StudentList_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }

            StudentList studentList = new StudentList();
            MainDisplay.Content = studentList;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Homebtn(object sender, RoutedEventArgs e)
        {
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            Button clickedButton = sender as Button;
            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }

            LoadHomeView();
        }

        private void LoadHomeView()
        {
            Home homeView = new Home();
            MainDisplay.Content = homeView;
        }

        private void logout_btn(object sender, RoutedEventArgs e)
        {
            Login loginWindow = new Login();
            loginWindow.Show();

            this.Close();
        }

        private void htu_Click(object sender, RoutedEventArgs e)
        {
            Howtouse howtouse = new Howtouse();

            MainDisplay.Content = howtouse;

            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }
            Button clickedButton = sender as Button;

            if (clickedButton != null)
            {
                clickedButton.Tag = true;
            }
        }

        public void HighlightEventButton()
        {
            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button)
                {
                    button.Tag = false;
                }
            }

            foreach (UIElement element in MenuContainer.Children)
            {
                if (element is Button button && button.Name == "EventButton")
                {
                    button.Tag = true;
                    break;
                }
            }
        }
    }

}