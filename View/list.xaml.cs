using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace prototype.View
{
    public partial class list : Window
    {
        private readonly string DepartmentName;
        private readonly int EventID;

        public list(string departmentName, int eventID)
        {
            InitializeComponent();
            DepartmentName = departmentName;
            EventID = eventID;

            LoadDepartmentDetails();
        }

        private void LoadDepartmentDetails()
        {
            DepartmentNameText.Text = DepartmentName;  
        }
    }

    public class DetailModel
    {
        public string Detail { get; set; }
    }
}
