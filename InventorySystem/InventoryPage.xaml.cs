using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace InventorySystem
{
    public partial class InventoryPage : UserControl
    {
        InventorySystemDataContext _db;
        private string currentStatus = "Active";

        public InventoryPage()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            AutoInactiveNoStockProducts();
            LoadCategoryFilter();
            LoadProducts("Active");
        }

        private void AutoInactiveNoStockProducts()
        {
            var noStockProducts = from p in _db.products
                                  join i in _db.inventories
                                  on p.productID equals i.productID
                                  where i.quantity == 0 &&
                                        p.status == "Active"
                                  select new
                                  {
                                      ProductData = p,
                                      InventoryData = i
                                  };

            foreach (var item in noStockProducts.ToList())
            {
                item.ProductData.status = "Inactive";
                item.InventoryData.status = "Inactive";
            }

            _db.SubmitChanges();
        }

        private void LoadProducts(string status)
        {
            currentStatus = status;

            AutoInactiveNoStockProducts();

            string search = txtSearch.Text.Trim().ToLower();
            string selectedCategory = cmbCategoryFilter.SelectedItem == null
                ? "All Categories"
                : cmbCategoryFilter.SelectedItem.ToString();

            var productList = from p in _db.products
                              join i in _db.inventories
                              on p.productID equals i.productID
                              where p.status == status &&
                                      (selectedCategory == "All Categories" || p.type == selectedCategory) &&
                                      (p.name.ToLower().Contains(search) ||
                                       p.type.ToLower().Contains(search))

                              select new
                              {
                                  ProductID = p.productID,
                                  ProductName = p.name,
                                  Category = p.type,
                                  SellingPrice = p.sellingPrice,
                                  Stock = i.quantity,
                                  ReorderLevel = p.reoderLevel,
                                  Status = p.status
                              };

            var products = productList.ToList().Select(p => new
            {
                p.ProductID,
                p.ProductName,
                p.Category,
                PriceText = "PHP " + p.SellingPrice.ToString("N2"),
                p.Stock,
                StockLevel = GetStockLevel(p.Stock, p.ReorderLevel),
                p.Status,

                ToggleButtonText =
                    p.Status == "Active" ? "Inactive" :
                    p.Status == "Inactive" ? "Active" :
                    "Restore",

                CanEdit = p.Status != "Deleted",
                CanDelete = p.Status != "Deleted"
            }).ToList();

            lvProducts.ItemsSource = products;
        }

        private string GetStockLevel(int quantity, int reorderLevel)
        {
            if (quantity == 0)
                return "Out of Stock";

            if (quantity <= reorderLevel)
                return "Low Stock";

            return "In Stock";
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts(currentStatus);
        }

        private void btnActive_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts("Active");
        }

        private void btnInactive_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts("Inactive");
        }

        private void btnDeleted_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts("Deleted");
        }

        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            ProductFormWindow form = new ProductFormWindow();

            if (form.ShowDialog() == true)
            {
                LoadProducts("Active");
            }
        }

        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == null)
                return;

            int productID = Convert.ToInt32(btn.Tag);

            var productData = _db.products.FirstOrDefault(p => p.productID == productID);

            if (productData != null && productData.status == "Deleted")
            {
                MessageBox.Show("Deleted products cannot be edited.");
                return;
            }

            ProductFormWindow form = new ProductFormWindow(productID);

            if (form.ShowDialog() == true)
            {
                LoadProducts(currentStatus);
            }
        }

        private void btnToggleProductStatus_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == null)
                return;

            int productID = Convert.ToInt32(btn.Tag);

            var productData = _db.products.FirstOrDefault(p => p.productID == productID);
            var inventoryData = _db.inventories.FirstOrDefault(i => i.productID == productID);

            if (productData == null || inventoryData == null)
                return;

            if (productData.status == "Active")
            {
                productData.status = "Inactive";
                inventoryData.status = "Inactive";

                MessageBox.Show("Product moved to Inactive list.");
            }
            else if (productData.status == "Inactive")
            {
                if (inventoryData.quantity == 0)
                {
                    MessageBox.Show("This product cannot be activated because stock is 0.");
                    return;
                }

                productData.status = "Active";
                inventoryData.status = "Active";

                MessageBox.Show("Product moved back to Active list.");
            }
            else if (productData.status == "Deleted")
            {
                productData.status = "Inactive";
                inventoryData.status = "Inactive";
                productData.deletedDate = null;

                MessageBox.Show("Product restored to Inactive list.");
            }

            _db.SubmitChanges();
            LoadProducts(currentStatus);
        }

        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;

            if (btn == null)
                return;

            int productID = Convert.ToInt32(btn.Tag);

            var productData = _db.products.FirstOrDefault(p => p.productID == productID);
            var inventoryData = _db.inventories.FirstOrDefault(i => i.productID == productID);

            if (productData != null)
            {
                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to move this product to Deleted?",
                    "Delete Product",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (result == MessageBoxResult.Yes)
                {
                    productData.status = "Deleted";
                    productData.deletedDate = DateTime.Now;

                    if (inventoryData != null)
                    {
                        inventoryData.status = "Deleted";
                    }

                    _db.SubmitChanges();

                    MessageBox.Show("Product moved to Deleted list.");
                    LoadProducts(currentStatus);
                }
            }
        }
        private void LoadCategoryFilter()
        {
            var categories = _db.products
                .Where(p => p.type != null && p.type != "")
                .Select(p => p.type)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            categories.Insert(0, "All Categories");

            cmbCategoryFilter.ItemsSource = categories;
            cmbCategoryFilter.SelectedIndex = 0;
        }
        private void cmbCategoryFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadProducts(currentStatus);
        }
    }
}