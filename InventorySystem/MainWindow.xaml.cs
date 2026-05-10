using System;
using System.Linq;
using System.Windows;

namespace InventorySystem
{
    public partial class MainWindow : Window
    {
        InventorySystemDataContext _db;

        public static int LoggedInUserID { get; private set; }
        public static string LoggedInFullName { get; private set; }
        public static int LoggedInRoleID { get; private set; }

        private bool isPasswordVisible = false;
        public static string LoggedInFirstName { get; private set; }

        public MainWindow()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );
        }

        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            string username = txtEmail.Text.Trim();
            string password = isPasswordVisible
                ? txtVisiblePassword.Text.Trim()
                : txtPassword.Password.Trim();

            if (username.Length == 0 || password.Length == 0)
            {
                MessageBox.Show("Please enter your username and password.");
                return;
            }

            try
            {
                var login = (from u in _db.users
                             where u.userName == username &&
                                   u.password == password
                             select u).FirstOrDefault();

                if (login != null)
                {
                    if (login.status != "Active")
                    {
                        MessageBox.Show(
                            "This account is not active. Please contact the admin.",
                            "Login Blocked",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning
                        );

                        ClearLoginFields();
                        return;
                    }

                    LoggedInUserID = login.userID;
                    LoggedInFirstName = login.firstName;
                    LoggedInFullName = login.firstName + " " + login.lastName;
                    LoggedInRoleID = login.roleID;

                    MessageBox.Show("Login successful! Welcome back " + LoggedInFullName + "!");

                    if (LoggedInRoleID == 1)
                    {
                        AdminDashboardWindow admin = new AdminDashboardWindow();
                        admin.Show();
                    }
                    else if (LoggedInRoleID == 2)
                    {
                        CashierDashboardWindow cashier = new CashierDashboardWindow();
                        cashier.Show();
                    }
                    else
                    {
                        MessageBox.Show("Unknown role. Please contact the admin.");
                        return;
                    }

                    this.Close();
                }
                else
                {
                    MessageBox.Show(
                        "You have entered an invalid username/password. Please try again!",
                        "Invalid Username/Password!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                    );

                    ClearLoginFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Login error: " + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void ClearLoginFields()
        {
            txtEmail.Text = string.Empty;
            txtPassword.Password = string.Empty;
            txtVisiblePassword.Text = string.Empty;
            txtEmail.Focus();
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible)
            {
                txtVisiblePassword.Text = txtPassword.Password;

                txtVisiblePassword.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;

                eyeSlash.Visibility = Visibility.Visible;
                isPasswordVisible = true;
            }
            else
            {
                txtPassword.Password = txtVisiblePassword.Text;

                txtVisiblePassword.Visibility = Visibility.Collapsed;
                txtPassword.Visibility = Visibility.Visible;

                eyeSlash.Visibility = Visibility.Collapsed;
                isPasswordVisible = false;
            }
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!isPasswordVisible)
            {
                txtVisiblePassword.Text = txtPassword.Password;
            }
        }

        private void txtVisiblePassword_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (isPasswordVisible)
            {
                txtPassword.Password = txtVisiblePassword.Text;
            }
        }
    }
}