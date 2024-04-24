using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using WarehouseLabelPrintingSystem.Model;
using WarehouseLabelPrintingSystem.Services;
using WarehouseLabelPrintingSystem.View;

namespace WarehouseLabelPrintingSystem
{
    public partial class MainWindow : Window
    {
        // Service for API interaction
        private readonly DuellAPIService _apiService = new();

        // Logger for this window
        private readonly ILogger<MainWindow> _logger;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize logger for this window
            _logger = App.LoggerFactory!.CreateLogger<MainWindow>();
            _logger.LogInformation("MainWindow initialized.");

            // Connect to the API and load products
            ConnectToApi();
        }

        /// <summary>
        /// Asynchronously connects to the API and retrieves product information.
        /// </summary>
        private async void ConnectToApi()
        {
            try
            {
                _logger.LogInformation("Attempting to connect to the API...");

                // Get the access token and retrieve product list
                string? token = await _apiService.GetAccessToken();
                string? jsonResponse = await _apiService.GetProductList(token);

                // Deserialize the JSON response
                Root? root = JsonConvert.DeserializeObject<Root>(jsonResponse!);

                // Check if the connection and data retrieval were successful
                if (root != null && root.status)
                {
                    _logger.LogInformation("Successfully connected to the API.");
                    isConnection_text.Text = "Successfully connected to the API";

                    // Create columns for the GridView and display products
                    IEnumerable<GridViewColumn> productColumns = CreateProductColumns();
                    GridView_Products.Columns.Clear();

                    foreach (var column in productColumns)
                    {
                        GridView_Products.Columns.Add(column);
                    }

                    ListView_Products.ItemsSource = root.products;
                }
                else
                {
                    _logger.LogWarning("Failed to connect to the API or retrieve product data.");
                    isConnection_text.Foreground = new SolidColorBrush(Colors.Red);
                    isConnection_text.Text = "Failed to connect to the API";
                }
            }
            catch (Exception ex)
            {
                // Log the error and display a message to the user
                _logger.LogError($"Error connecting to the API: {ex.Message}", ex);
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the Export to PDF button click. Opens a window to export the selected product to a PDF.
        /// </summary>
        private void Export_to_PDF_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Export to PDF button clicked.");

            Product? selectedProduct = (Product?)ListView_Products.SelectedItem;

            if (selectedProduct != null)
            {
                _logger.LogInformation($"Exporting product '{selectedProduct.product_name}' to PDF.");

                var exportWindow = new ExportToFileWindow(selectedProduct);
                exportWindow.ShowDialog();
            }
        }

        /// <summary>
        /// Handles the ListView selection change event. Enables or disables the Export to PDF button based on the selection.
        /// </summary>
        private void ListView_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var isProductSelected = ListView_Products.SelectedItem != null;
            Export_to_PDF.IsEnabled = isProductSelected;

            if (isProductSelected)
            {
                var selectedProduct = (Product)ListView_Products.SelectedItem!;
                _logger.LogInformation($"Product '{selectedProduct.product_name}' selected.");
            }
        }

        /// <summary>
        /// Creates the columns for displaying products in a GridView.
        /// </summary>
        /// <returns>A list of GridViewColumn objects.</returns>
        private List<GridViewColumn> CreateProductColumns()
        {
            _logger.LogInformation("Creating product columns.");

            string[] desiredProperties = new[]
            {
                "product_id",
                "product_name",
                "product_number",
                "barcode",
                "unit"
            };

            List<GridViewColumn> columns = new();

            foreach (var propertyName in desiredProperties)
            {
                PropertyInfo? property = typeof(Product).GetProperty(propertyName);

                if (property != null)
                {
                    var column = new GridViewColumn
                    {
                        Header = property.Name,
                        DisplayMemberBinding = new Binding(property.Name)
                    };
                    columns.Add(column);

                    _logger.LogInformation($"Added column '{property.Name}' to GridView.");
                }
            }

            return columns;
        }
    }
}