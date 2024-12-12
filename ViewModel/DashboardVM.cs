using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using prototype.Model;

namespace prototype.ViewModel
{
     class DashboardVM : Utilities.ViewModelBase
    {
        private readonly PageModel _pageModel;
        public int Student
        {
            get { return _pageModel.TotalStudent; }
            set { _pageModel.TotalStudent = value; OnPropertyChanged(); }
        }

        public DashboardVM()
        {
            _pageModel = new PageModel();
            Student = 100;
            _pageModel = new PageModel();
            DepartmentIncluded = 3;
        }

        public int DepartmentIncluded
        {
            get { return _pageModel.Department; }
            set { _pageModel.Department = value; OnPropertyChanged(); }
        }


    }
}
