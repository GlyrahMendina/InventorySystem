using System;
using System.Linq;
using System.Windows;

namespace InventorySystem
{
    public partial class ProductFormWindow : Window
    {
        InventorySystemDataContext _db;
        private int? editingProductID = null;

        public ProductFormWindow()
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            dpManufacturingDate.SelectedDate = DateTime.Today;
            dpExpiryDate.SelectedDate = DateTime.Today.AddMonths(6);
        }

        public ProductFormWindow(int productID)
        {
            InitializeComponent();

            _db = new InventorySystemDataContext(
                Properties.Settings.Default.InventorySystemConnectionString
            );

            editingProductID = productID;
            txtTitle.Text = "Edit Product";

            LoadProductData();
        }

        private void LoadProductData()
        {
            var productData = _db.products.FirstOrDefault(p => p.productID == editingProductID);
            var inventoryData = _db.inventories.FirstOrDefault(i => i.productID == editingProductID);

            if (productData != null)
            {
                txtName.Text = productData.name;
                txtDescription.Text = productData.description;
                txtType.Text = productData.type;
                txtCost.Text = productData.cost.ToString();
                txtSellingPrice.Text = productData.sellingPrice.ToString();
                txtReorderLevel.Text = productData.reoderLevel.ToString();
            }

            if (inventoryData != null)
            {
                txtQuantity.Text = inventoryData.quantity.ToString();
                dpManufacturingDate.SelectedDate = inventoryData.manufacturingDate;
                dpExpiryDate.SelectedDate = inventoryData.expiryDate;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtName.Text.Length == 0 ||
                txtType.Text.Length == 0 ||
                txtCost.Text.Length == 0 ||
                txtSellingPrice.Text.Length == 0 ||
                txtQuantity.Text.Length == 0 ||
                txtReorderLevel.Text.Length == 0)
            {
                MessageBox.Show("Please complete all required fields.");
                return;
            }

            decimal cost;
            decimal sellingPrice;
            int quantity;
            int reorderLevel;

            if (!decimal.TryParse(txtCost.Text, out cost) ||
                !decimal.TryParse(txtSellingPrice.Text, out sellingPrice) ||
                !int.TryParse(txtQuantity.Text, out quantity) ||
                !int.TryParse(txtReorderLevel.Text, out reorderLevel))
            {
                MessageBox.Show("Please enter valid numbers for cost, price, quantity, and reorder level.");
                return;
            }

            if (editingProductID == null)
            {
                int newProductID = 1;

                if (_db.products.Any())
                {
                    newProductID = _db.products.Max(p => p.productID) + 1;
                }

                int newInventoryID = 1;

                if (_db.inventories.Any())
                {
                    newInventoryID = _db.inventories.Max(i => i.inventoryID) + 1;
                }

                product newProduct = new product
                {
                    productID = newProductID,
                    name = txtName.Text.Trim(),
                    description = txtDescription.Text.Trim(),
                    type = txtType.Text.Trim(),
                    cost = cost,
                    sellingPrice = sellingPrice,
                    reoderLevel = reorderLevel,
                    image = "",
                    status = "Active",
                    deletedDate = null
                };

                inventory newInventory = new inventory
                {
                    inventoryID = newInventoryID,
                    productID = newProductID,
                    quantity = quantity,
                    originalQuantity = quantity,
                    receivedDate = DateTime.Now,
                    manufacturingDate = dpManufacturingDate.SelectedDate.Value,
                    expiryDate = dpExpiryDate.SelectedDate.Value,
                    status = "Active"
                };

                _db.products.InsertOnSubmit(newProduct);
                _db.inventories.InsertOnSubmit(newInventory);
                _db.SubmitChanges();

                MessageBox.Show("Product added successfully.");
            }
            else
            {
                var productData = _db.products.FirstOrDefault(p => p.productID == editingProductID);
                var inventoryData = _db.inventories.FirstOrDefault(i => i.productID == editingProductID);

                if (productData != null)
                {
                    productData.name = txtName.Text.Trim();
                    productData.description = txtDescription.Text.Trim();
                    productData.type = txtType.Text.Trim();
                    productData.cost = cost;
                    productData.sellingPrice = sellingPrice;
                    productData.reoderLevel = reorderLevel;
                }

                if (inventoryData != null)
                {
                    inventoryData.quantity = quantity;
                    inventoryData.manufacturingDate = dpManufacturingDate.SelectedDate.Value;
                    inventoryData.expiryDate = dpExpiryDate.SelectedDate.Value;
                }

                _db.SubmitChanges();

                MessageBox.Show("Product updated successfully.");
            }

            DialogResult = true;
            Close();
        }
    }
}