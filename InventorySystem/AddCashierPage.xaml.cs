using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventorySystem
{
    public partial class AddCashierPage : UserControl
    {
        InventorySystemDataContext _db;
        private string currentCashierStatus = "Active";

        public AddCashierPage()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            PermanentlyDeleteOldCashiers();
            LoadCashiers("Active");
        }

        private void PermanentlyDeleteOldCashiers()
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-30);

            var oldDeletedCashiers = _db.users
                .Where(u => u.roleID == 2 &&
                            u.status == "Deleted" &&
                            u.deletedDate != null &&
                            u.deletedDate <= cutoffDate)
                .ToList();

            if (oldDeletedCashiers.Any())
            {
                _db.users.DeleteAllOnSubmit(oldDeletedCashiers);
                _db.SubmitChanges();
            }
        }

        private void LoadCashiers(string status)
        {
            currentCashierStatus = status;

            var cashierList = from u in _db.users
                              where u.roleID == 2 &&
                                    u.status == status
                              select u;

            var cashiers = cashierList.ToList().Select(u => new
            {
                u.userID,
                FullName = u.firstName + " " + u.lastName,
                Email = u.userName,
                ContactText = "Contact No.: " + u.contactNumber,
                ShiftText = "Assigned Shift: " + u.assignedShift,
                StatusText = u.status,
                CashierID = "Cashier ID: CSH-" + u.userID.ToString("000"),
                DateCreatedText = "Date Created: " +
                    Convert.ToDateTime(u.dateCreated).ToString("MMMM dd, yyyy"),
                IsNotDeleted = u.status != "Deleted"
            }).ToList();

            lvCashiers.ItemsSource = cashiers;
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            LoadCashiers("Active");
        }

        private void btnInactive_Click(object sender, RoutedEventArgs e)
        {
            LoadCashiers("Inactive");
        }

        private void btnDeleted_Click(object sender, RoutedEventArgs e)
        {
            PermanentlyDeleteOldCashiers();
            LoadCashiers("Deleted");
        }

        private void btnAddCashier_Click(object sender, RoutedEventArgs e)
        {
            CashierFormWindow form = new CashierFormWindow();

            if (form.ShowDialog() == true)
            {
                LoadCashiers("Active");
            }
        }

        private void btnEditCashier_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int userID = Convert.ToInt32(btn.Tag);

            var cashier = _db.users.FirstOrDefault(u => u.userID == userID);

            if (cashier != null && cashier.status == "Deleted")
            {
                MessageBox.Show("Deleted accounts can no longer be edited.");
                return;
            }

            CashierFormWindow form = new CashierFormWindow(userID);

            if (form.ShowDialog() == true)
            {
                LoadCashiers(currentCashierStatus);
            }
        }

        private void btnDeleteCashier_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            int userID = Convert.ToInt32(btn.Tag);

            var cashier = _db.users.FirstOrDefault(u => u.userID == userID);

            if (cashier != null)
            {
                if (cashier.status == "Deleted")
                {
                    MessageBox.Show("This account is already deleted.");
                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to move this cashier to Deleted?",
                    "Soft Delete Cashier",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    cashier.status = "Deleted";
                    cashier.deletedDate = DateTime.Now;

                    _db.SubmitChanges();

                    MessageBox.Show("Cashier moved to Deleted list. It will be permanently deleted after 30 days.");
                    LoadCashiers(currentCashierStatus);
                }
            }
        }
    }
}