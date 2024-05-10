using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WarehouseLabelPrintingSystem.Model;
using WarehouseLabelPrintingSystem.Services;
using WarehouseLabelPrintingSystem.Utilities;
using WarehouseLabelPrintingSystem.ViewModel;

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

            comboBox_labels.SelectedIndex = 0;

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

            LoadPrintersWithDefaultFirst();
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
                    comboBox_labels.Visibility = Visibility.Visible;
                    comboBox_print_list.Visibility = Visibility.Visible;
                    search_box_printer_name_title.Visibility = Visibility.Visible;

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

                var pattern = @"^\d+-\d+-[A-Za-z]-\d+$";
                var customField = selectedProduct.custom_fields!.FirstOrDefault(cf => cf.value != null && Regex.IsMatch(cf.value, pattern));
                string? foundValue = customField?.value;

                if (foundValue != null)
                {
                    string projectRoot = AppDomain.CurrentDomain.BaseDirectory;
                    string pdfFolderPath = Path.Combine(projectRoot, "PDF");

                    if (!Directory.Exists(pdfFolderPath))
                    {
                        Directory.CreateDirectory(pdfFolderPath);
                    }

                    string timestamp = $"{selectedProduct.product_number} - {comboBox_labels.Text}";
                    string fileName = $"{timestamp}.pdf";

                    string filePath = Path.Combine(pdfFolderPath, fileName);

                    try
                    {
                        BarcodeGenerationAndSavingToPDF(filePath, foundValue, selectedProduct);
                        _logger.LogInformation($"PDF file saved at: {filePath}");

                        LabelViewModel.PrintPdf(filePath, comboBox_print_list.Text, comboBox_labels.SelectedIndex);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving PDF: {ex.Message}");
                        MessageBox.Show("Error saving PDF: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    MessageBox.Show("The location of this product was not found.");
                }
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

        private void BarcodeGenerationAndSavingToPDF(string filePath, string location, Product product)
        {
            var commonLabelProperties = new
            {
                ProductNumber = product.product_number,
                ProductName = product.product_name,
                Unit = product.unit,
                Location = location,
                BarcodeText = BarcodeFormatNumber.FormatNumber(product.barcode!)
            };

            var labelPositions = new Dictionary<int, (PointF, PointF, PointF, PointF, PointF, PointF, PointF)>
            {
                [0] = (new PointF(5f, 15f), new PointF(10f, 120f), new PointF(10f, 75f),
                      new PointF(158f, 125f), new PointF(10f, 48f), new PointF(12f, 5f),
                      new PointF(10f, 0f)),

                [1] = (new PointF(4.5f, 4f), new PointF(2f, 20f), new PointF(2f, 0f),
                      new PointF(27f, 22f), new PointF(10f, 48f), new PointF(7f, 1.5f),
                      new PointF(10f, 0f))
            };

            if (labelPositions.TryGetValue(comboBox_labels.SelectedIndex, out var positions))
            {
                var label = new LabelViewModel(positions.Item1, positions.Item2, positions.Item3,
                                               positions.Item4, positions.Item5, positions.Item6,
                                               positions.Item7)
                {
                    ProductNumber = commonLabelProperties.ProductNumber,
                    ProductName = commonLabelProperties.ProductName,
                    Unit = commonLabelProperties.Unit,
                    Location = commonLabelProperties.Location,
                    BarcodeText = commonLabelProperties.BarcodeText
                };

                switch (comboBox_labels.SelectedIndex)
                {
                    case 0:
                        label.GenerateLabelSize208x148(filePath, product.barcode!);
                        break;

                    case 1:
                        label.GenerateLabelSize39x27(filePath, product.barcode!);
                        break;
                }
            }
        }

        private void LoadPrintersWithDefaultFirst()
        {
            var defaultPrinter = new PrinterSettings().PrinterName;

            var installedPrinters = PrinterSettings.InstalledPrinters.Cast<string>().ToList();

            if (installedPrinters.Contains(defaultPrinter))
            {
                comboBox_print_list.Items.Add(defaultPrinter);
                installedPrinters.Remove(defaultPrinter);
            }

            foreach (var printerName in installedPrinters)
            {
                comboBox_print_list.Items.Add(printerName);
            }

            if (comboBox_print_list.Items.Count > 0)
            {
                comboBox_print_list.SelectedIndex = 0;
            }
        }
    }
}
