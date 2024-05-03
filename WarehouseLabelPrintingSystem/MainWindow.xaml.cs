using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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

        private ObservableCollection<Product> _products;
        public ICollectionView FilteredProducts { get; set; }

        private string _currentSortColumn = "";
        private ListSortDirection _currentSortDirection = ListSortDirection.Ascending;

        public MainWindow()
        {
            InitializeComponent();

            _logger = App.LoggerFactory!.CreateLogger<MainWindow>();
            _logger.LogInformation("MainWindow initialized.");

            ConnectToApi();

            _currentSortColumn = string.Empty;
            _currentSortDirection = ListSortDirection.Ascending;

            var productColumns = CreateProductColumns();
            var gridView = (GridView)ListView_Products.View;

            foreach (var column in productColumns)
            {
                gridView.Columns.Add(column);
            }

            _products = new ObservableCollection<Product>();

            FilteredProducts = CollectionViewSource.GetDefaultView(_products);
            FilteredProducts.Filter = ProductFilter;
            ListView_Products.ItemsSource = FilteredProducts;
        }

        private bool ProductFilter(object item)
        {
            if (item is Product product)
            {
                string searchNumber = search_box_number.Text ?? "";
                string searchName = search_box_name.Text ?? "";

                bool matchesNumber = product.product_number!.Contains(searchNumber, StringComparison.OrdinalIgnoreCase);
                bool matchesName = product.product_name!.Contains(searchName, StringComparison.OrdinalIgnoreCase);

                // The product matches if it satisfies both search conditions
                return matchesNumber && matchesName;
            }

            return false;
        }

        /// <summary>
        /// Asynchronously connects to the API and retrieves product information.
        /// </summary>
        private async void ConnectToApi()
        {
            try
            {
                _logger.LogInformation("Attempting to connect to the API...");
                string? token = await _apiService.GetAccessToken();
                string? jsonResponse = await _apiService.GetProductList(token);

                Root? root = JsonConvert.DeserializeObject<Root>(jsonResponse!);

                if (root != null && root.status)
                {
                    progress_connectionAPI.IsIndeterminate = false;
                    isConnection_text.Visibility = Visibility.Visible;
                    Export_to_PDF.Visibility = Visibility.Visible;
                    progress_connectionAPI.Visibility = Visibility.Collapsed;
                    text_connectionAPI.Visibility = Visibility.Collapsed;
                    search_box_number.Visibility = Visibility.Visible;
                    search_box_number_title.Visibility = Visibility.Visible;
                    search_box_name.Visibility = Visibility.Visible;
                    search_box_name_title.Visibility = Visibility.Visible;

                    _logger.LogInformation("Successfully connected to the API.");
                    isConnection_text.Text = "Successfully connected to the API";

                    var productColumns = CreateProductColumns();
                    GridView_Products.Columns.Clear();

                    foreach (var column in productColumns)
                    {
                        GridView_Products.Columns.Add(column);
                    }

                    _products.Clear();
                    foreach (var product in root.products!)
                    {
                        _products.Add(product);
                    }
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
            string[] desiredProperties = new[]
            {
            "product_id",
            "product_name",
            "product_number",
            "barcode",
            "unit"
        };

            var columns = new List<GridViewColumn>();

            foreach (var propertyName in desiredProperties)
            {
                var property = typeof(Product).GetProperty(propertyName);

                if (property != null)
                {
                    var column = new GridViewColumn
                    {
                        Header = propertyName, // Set the column header
                        DisplayMemberBinding = new Binding(propertyName),
                    };

                    columns.Add(column);
                }
            }

            return columns;
        }

        private void ListView_Products_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as FrameworkElement;

            while (clickedElement != null && clickedElement.GetType() != typeof(GridViewColumnHeader))
            {
                clickedElement = VisualTreeHelper.GetParent(clickedElement) as FrameworkElement;
            }

            if (clickedElement is GridViewColumnHeader header)
            {
                var columnHeader = header.Column.Header.ToString();

                if (_currentSortColumn == columnHeader)
                {
                    _currentSortDirection = _currentSortDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }
                else
                {
                    _currentSortColumn = columnHeader!;
                    _currentSortDirection = ListSortDirection.Ascending;
                }

                FilteredProducts.SortDescriptions.Clear();
                FilteredProducts.SortDescriptions.Add(new SortDescription(_currentSortColumn, _currentSortDirection));
                FilteredProducts.Refresh();
            }
        }

        private void search_box_number_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilteredProducts.Refresh();
        }

        private void search_box_name_TextChanged(object sender, TextChangedEventArgs e)
        {
            FilteredProducts.Refresh();
        }
    }
}