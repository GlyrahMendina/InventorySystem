using System;
using System.Windows;

namespace InventorySystem
{
    public partial class AdminDashboardWindow : Window
    {
        public AdminDashboardWindow()
        {
            InitializeComponent();

            txtWelcome.Text = "Welcome Back, " + MainWindow.LoggedInFirstName + "!";
            txtDashboardRole.Text = "Admin Dashboard";

            txtCurrentDate.Text = DateTime.Now.ToString("dddd, MMMM dd, yyyy");

            MainContent.Content = new DashboardPage();
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new DashboardPage();
        }

        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new InventoryPage();
        }

        private void btnAddCashierPage_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AddCashierPage();
        }

        private void btnGeneralReport_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new GeneralReportPage();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            MainWindow login = new MainWindow();
            login.Show();
            this.Close();
        }
    }
}