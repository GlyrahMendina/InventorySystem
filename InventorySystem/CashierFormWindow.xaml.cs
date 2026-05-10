using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventorySystem
{
    public partial class CashierFormWindow : Window
    {
        InventorySystemDataContext _db;
        private int? editingUserID = null;

        public CashierFormWindow()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            cmbShift.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
        }

        public CashierFormWindow(int userID)
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            editingUserID = userID;
            txtTitle.Text = "Edit Cashier";

            LoadCashierData();
        }

        private void LoadCashierData()
        {
            var cashier = _db.users.FirstOrDefault(u => u.userID == editingUserID);

            if (cashier != null)
            {
                txtFirstName.Text = cashier.firstName;
                txtLastName.Text = cashier.lastName;
                txtUsername.Text = cashier.userName;
                txtPassword.Text = cashier.password;
                txtContactNumber.Text = cashier.contactNumber;
                cmbShift.Text = cashier.assignedShift;
                cmbStatus.Text = cashier.status;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtFirstName.Text.Length == 0 ||
                txtLastName.Text.Length == 0 ||
                txtUsername.Text.Length == 0 ||
                txtPassword.Text.Length == 0)
            {
                MessageBox.Show("Please complete all required fields.");
                return;
            }

            string selectedShift = ((ComboBoxItem)cmbShift.SelectedItem).Content.ToString();
            string selectedStatus = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();

            if (editingUserID == null)
            {
                int newUserID = 1;

                if (_db.users.Any())
                {
                    newUserID = _db.users.Max(u => u.userID) + 1;
                }

                user newCashier = new user
                {
                    userID = newUserID,
                    firstName = txtFirstName.Text.Trim(),
                    lastName = txtLastName.Text.Trim(),
                    userName = txtUsername.Text.Trim(),
                    password = txtPassword.Text.Trim(),
                    roleID = 2,
                    contactNumber = txtContactNumber.Text.Trim(),
                    dateCreated = DateTime.Now,
                    assignedShift = selectedShift,
                    status = selectedStatus
                };

                _db.users.InsertOnSubmit(newCashier);
                _db.SubmitChanges();

                MessageBox.Show("Cashier account created successfully.");
            }
            else
            {
                var cashier = _db.users.FirstOrDefault(u => u.userID == editingUserID);

                if (cashier != null)
                {
                    cashier.firstName = txtFirstName.Text.Trim();
                    cashier.lastName = txtLastName.Text.Trim();
                    cashier.userName = txtUsername.Text.Trim();
                    cashier.password = txtPassword.Text.Trim();
                    cashier.contactNumber = txtContactNumber.Text.Trim();
                    cashier.assignedShift = selectedShift;
                    cashier.status = selectedStatus;

                    _db.SubmitChanges();

                    MessageBox.Show("Cashier account updated successfully.");
                }
            }

            DialogResult = true;
            Close();
        }
    }
}