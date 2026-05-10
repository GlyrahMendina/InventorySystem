using System;
using System.Linq;
using System.Windows.Controls;

namespace InventorySystem
{
    public partial class DashboardPage : UserControl
    {
        InventorySystemDataContext _db;

        public DashboardPage()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            txtTotalProducts.Text = _db.products.Count().ToString();

            txtLowStock.Text = _db.inventories
                .Count(i => i.quantity <= 5)
                .ToString();

            decimal totalSales = _db.orders.Any()
                ? _db.orders.Sum(o => o.netTotal)
                : 0;

            txtTotalSales.Text = "PHP " + totalSales.ToString("N2");

            DateTime today = DateTime.Today;

            txtTransactionsToday.Text = _db.orders
                .Count(o => o.date.Date == today)
                .ToString();
        }
    }
}