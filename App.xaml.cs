using System.Configuration;
using System.Data;
using System.Net;
using System.Windows;
using LiveCharts.Wpf;

namespace prototype
{
   
    public partial class App : Application
    {
        public static string ConnectionString { get; } = "Server = 34.126.135.52,1433;Database=finalproject;User Id = sqlserver;Password=admin;TrustServerCertificate=True ";
    }

}
