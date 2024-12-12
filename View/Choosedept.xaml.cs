using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using prototype.ViewModel;  // Add this for ViewModel access

namespace prototype.View
{
    public partial class Choosedept : UserControl
    {
        public ContentControl MainDisplay { get; set; }

        public Choosedept(ContentControl mainDisplay, int eventID)
        {
            InitializeComponent();
            MainDisplay = mainDisplay;
            this.DataContext = new ChoosedeptVM(eventID); // Pass the eventID to the ViewModel
        }

        // ToggleButton click events (for each department)
        private void ToggleButtonClickHandler(string departmentName, ToggleButton button)
        {
            var viewModel = (ChoosedeptVM)this.DataContext;

            if (button != null && button.IsChecked == true)
            {
                viewModel.AddDepartment(departmentName);
            }
            else
            {
                viewModel.RemoveDepartment(departmentName);
            }
        }

        private void BED(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("BED", sender as ToggleButton);
        private void CAE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CAE", sender as ToggleButton);
        private void CAFAE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CAFAE", sender as ToggleButton);
        private void CASE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CASE", sender as ToggleButton);
        private void CCE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CCE", sender as ToggleButton);
        private void CCJE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CCJE", sender as ToggleButton);
        private void CEE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CEE", sender as ToggleButton);
        private void CHE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CHE", sender as ToggleButton);
        private void CHSE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CHSE", sender as ToggleButton);
        private void CTE(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("CTE", sender as ToggleButton);
        private void TS(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("TS", sender as ToggleButton);
        private void PS(object sender, RoutedEventArgs e) => ToggleButtonClickHandler("PS", sender as ToggleButton);

        // Confirm button click event
        private void Confirmed_Button(object sender, RoutedEventArgs e)
        {
            var viewModel = (ChoosedeptVM)this.DataContext;
            viewModel.SaveDepartments();
        }

        private void Back_Button(object sender, RoutedEventArgs e)
        {

        }
    }
}
