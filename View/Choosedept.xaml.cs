using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Extensions.Logging;
using prototype.ViewModel;

namespace prototype.View
{
    public partial class Choosedept : UserControl
    {
        private readonly ContentControl _mainDisplay;
        private readonly Dictionary<string, object> _eventDetails;
        private readonly bool _isEditing;
        private readonly int? _eventId;

        public Choosedept(ContentControl mainDisplay, Dictionary<string, object> eventDetails, bool isEditing, int? eventId)
        {
            InitializeComponent();
            _mainDisplay = mainDisplay;
            _eventDetails = eventDetails;
            _isEditing = isEditing;
            _eventId = eventId;

            ChoosedeptVM viewModel = new ChoosedeptVM(eventDetails, isEditing, eventId);
            this.DataContext = viewModel;

            if (_isEditing)
            {
                SetToggleButtons(viewModel.SelectedDepartments);
            }
        }

        private void SetToggleButtons(List<string> selectedDepartments)
        {
            foreach (var department in selectedDepartments)
            {
                switch (department)
                {
                    case "BED":
                        BEDToggleButton.IsChecked = true;
                        break;
                    case "CAE":
                        CAEToggleButton.IsChecked = true;
                        break;
                    case "CAFAE":
                        CAFAEToggleButton.IsChecked = true;
                        break;
                    case "CASE":
                        CASEToggleButton.IsChecked = true;
                        break;
                    case "CCE":
                        CCEToggleButton.IsChecked = true;
                        break;
                    case "CCJE":
                        CCJEToggleButton.IsChecked = true;
                        break;
                    case "CEE":
                        CEEToggleButton.IsChecked = true;
                        break;
                    case "CHE":
                        CHEToggleButton.IsChecked = true;
                        break;
                    case "CHSE":
                        CHSEToggleButton.IsChecked = true;
                        break;
                    case "CTE":
                        CTEToggleButton.IsChecked = true;
                        break;
                    case "TS":
                        TSToggleButton.IsChecked = true;
                        break;
                    case "PS":
                        PSToggleButton.IsChecked = true; 
                        break;
                }
            }
        }

        private void ToggleButtonClickHandler(string departmentName, ToggleButton button)
        {
            var viewModel = (ChoosedeptVM)this.DataContext;

            if (button != null)
            {
                if (button.IsChecked == true)
                {
                    viewModel.AddDepartment(departmentName);
                }
                else
                {
                    viewModel.RemoveDepartment(departmentName);
                }
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

        private void Confirmed_Button(object sender, RoutedEventArgs e)
        {
            int? eventId = _eventId;
            var viewModel = (ChoosedeptVM)this.DataContext;
            viewModel.SaveEventAndDepartments(eventId);

            MessageBoxResult result = MessageBox.Show(
                "Event and departments saved successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            if (result == MessageBoxResult.OK || result == MessageBoxResult.None)
            {
                if (_mainDisplay != null)
                {
                    _mainDisplay.Content = new Event(_mainDisplay);

                    if (Application.Current.MainWindow is MainWindow mainWindow)
                    { 
                        mainWindow.HighlightEventButton();
                    }
                }
            }
        }

        private void Back_Button(object sender, RoutedEventArgs e)
        {
            if (_mainDisplay != null)
            {
                if (!_isEditing)
                {
                    _mainDisplay.Content = new Cevent(_mainDisplay, false, null);
                }
                else
                {
                    if (_eventId.HasValue)
                    {
                        _mainDisplay.Content = new Cevent(_mainDisplay, true, _eventId);
                    }
                    else
                    {
                        MessageBox.Show("Event ID is not set. Cannot load event details.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }
    }
}