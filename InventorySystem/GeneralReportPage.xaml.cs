using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace InventorySystem
{
    public partial class GeneralReportPage : UserControl
    {
        InventorySystemDataContext _db;

        public GeneralReportPage()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            LoadReport();
            LoadTransactions();
            LoadSalesChart();
        }

        private void LoadReport()
        {
            txtTotalProducts.Text = _db.products
                .Count(p => p.status == "Active")
                .ToString();

            var lowStock = from p in _db.products
                           join i in _db.inventories
                           on p.productID equals i.productID
                           where p.status == "Active" &&
                                 i.quantity <= p.reoderLevel
                           select p;

            txtLowStock.Text = lowStock.Count().ToString();

            DateTime startMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            DateTime nextMonth = startMonth.AddMonths(1);

            decimal totalSalesMonth = _db.orders
                .Where(o => o.date >= startMonth && o.date < nextMonth)
                .ToList()
                .Sum(o => o.netTotal);

            txtTotalSalesMonth.Text = "PHP " + totalSalesMonth.ToString("N2");

            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            txtTransactionsToday.Text = _db.orders
                .Count(o => o.date >= today && o.date < tomorrow)
                .ToString();
        }

        private void LoadTransactions()
        {
            var transactions = _db.orders
                .OrderByDescending(o => o.date)
                .Take(10)
                .ToList()
                .Select(o => new
                {
                    OrderText = "Order #" + o.orderID,
                    DateText = o.date.ToString("MMMM dd, yyyy hh:mm tt"),
                    TotalText = "PHP " + o.netTotal.ToString("N2")
                })
                .ToList();

            lvTransactions.ItemsSource = transactions;
        }

        private void LoadSalesChart()
        {
            chartCanvas.Children.Clear();

            double width = 520;
            double height = 300;
            double left = 40;
            double bottom = 280;

            Line xAxis = new Line
            {
                X1 = left,
                Y1 = bottom,
                X2 = width,
                Y2 = bottom,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            Line yAxis = new Line
            {
                X1 = left,
                Y1 = 20,
                X2 = left,
                Y2 = bottom,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            chartCanvas.Children.Add(xAxis);
            chartCanvas.Children.Add(yAxis);

            var salesData = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    DateTime day = DateTime.Today.AddDays(-6 + i);
                    DateTime nextDay = day.AddDays(1);

                    decimal total = _db.orders
                        .Where(o => o.date >= day && o.date < nextDay)
                        .ToList()
                        .Sum(o => o.netTotal);

                    return new
                    {
                        Day = day,
                        Total = total
                    };
                })
                .ToList();

            decimal maxSales = salesData.Max(s => s.Total);

            if (maxSales <= 0)
                maxSales = 1;

            Polyline salesLine = new Polyline
            {
                Stroke = Brushes.Black,
                StrokeThickness = 3
            };

            for (int i = 0; i < salesData.Count; i++)
            {
                double x = left + (i * 75);
                double y = bottom - ((double)(salesData[i].Total / maxSales) * 230);

                salesLine.Points.Add(new System.Windows.Point(x, y));

                Ellipse point = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    Fill = Brushes.White,
                    Stroke = Brushes.Black,
                    StrokeThickness = 3
                };

                Canvas.SetLeft(point, x - 6);
                Canvas.SetTop(point, y - 6);
                chartCanvas.Children.Add(point);

                TextBlock label = new TextBlock
                {
                    Text = salesData[i].Day.ToString("MM/dd"),
                    FontSize = 11,
                    Foreground = Brushes.Gray
                };

                Canvas.SetLeft(label, x - 18);
                Canvas.SetTop(label, bottom + 8);
                chartCanvas.Children.Add(label);
            }

            chartCanvas.Children.Add(salesLine);
        }
    }
}