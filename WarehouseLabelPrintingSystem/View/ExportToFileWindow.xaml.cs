using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WarehouseLabelPrintingSystem.Model;
using WarehouseLabelPrintingSystem.Utilities;
using WarehouseLabelPrintingSystem.ViewModel;
using System.IO;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Logging;

namespace WarehouseLabelPrintingSystem.View
{
    /// <summary>
    /// Window for exporting product information to a PDF file and printing it.
    /// </summary>
    public partial class ExportToFileWindow : Window
    {
        private readonly Product _product;
        private readonly ILogger<ExportToFileWindow> _logger;

        public ExportToFileWindow(Product product)
        {
            InitializeComponent();
            _product = product;

            _logger = App.LoggerFactory!.CreateLogger<ExportToFileWindow>();
            _logger.LogInformation("ExportToFileWindow initialized");

            LoadProductData();
        }

        /// <summary>
        /// Loads product data and updates UI components with product details.
        /// </summary>
        private void LoadProductData()
        {
            if (_product == null) return;

            Product_Id_Text.Text = $"Id: {_product.product_id}";
            Product_Number_Text.Text = $"Number: {_product.product_number}";
            Product_Barcode_Text.Text = $"Barcode: {_product.barcode}";
            Product_Unit_Text.Text = $"Unit: {_product.unit}";
            Product_Name_Text.Text = $"Name: {_product.product_name}";

            // Load custom field columns into the grid view
            List<GridViewColumn> columns = CreateCustomFieldColumns();
            GridView_CustomField.Columns.Clear();
            columns.ForEach(column => GridView_CustomField.Columns.Add(column));

            // Set custom fields as the data source for the list view
            ListView_CustomFields.ItemsSource = _product.custom_fields;
        }

        /// <summary>
        /// Event handler for the "Save to File" button. Exports the selected product information to a PDF file.
        /// </summary>
        private void Save_to_File_Click(object sender, RoutedEventArgs e)
        {
            var selectedField = (CustomField?)ListView_CustomFields.SelectedItem;

            if (selectedField != null)
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF Files (*.pdf)|*.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    try
                    {
                        BarcodeGenerationAndSavingToPDF(filePath);
                        _logger.LogInformation($"PDF file saved at: {filePath}");

                        RotatePages(filePath, filePath, 90);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error saving PDF: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for the "Print" button. Prints a selected PDF file.
        /// </summary>
        private void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    LabelViewModel.PrintPdf(filePath);
                    _logger.LogInformation($"PDF file printed: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error printing PDF: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Event handler for when the selection in the custom fields list view changes.
        /// Enables the "Save to File" and "Print" buttons if an item is selected.
        /// </summary>
        private void ListView_CustomFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isItemSelected = ListView_CustomFields.SelectedItem != null;
            Save_to_File.IsEnabled = isItemSelected;
            Print_Button.IsEnabled = isItemSelected;
        }

        /// <summary>
        /// Creates grid view columns for the custom fields.
        /// </summary>
        private static List<GridViewColumn> CreateCustomFieldColumns()
        {
            var desiredProperties = new string[] { "id", "label", "value" };

            var columns = new List<GridViewColumn>();
            foreach (var propertyName in desiredProperties)
            {
                var property = typeof(CustomField).GetProperty(propertyName);

                if (property != null)
                {
                    var column = new GridViewColumn
                    {
                        Header = property.Name,
                        DisplayMemberBinding = new Binding(property.Name)
                    };
                    columns.Add(column);
                }
            }

            return columns;
        }

        /// <summary>
        /// Generates a PDF with product information and barcode.
        /// </summary>
        private void BarcodeGenerationAndSavingToPDF(string filePath)
        {
            var selectedField = (CustomField?)ListView_CustomFields.SelectedItem;

            var label = new LabelViewModel
            {
                ProductNumber = _product.product_number,
                ProductName = _product.product_name,
                Unit = _product.unit,
                Location = selectedField?.value,
                BarcodeText = BarcodeFormatNumber.FormatNumber(_product.barcode!)
            };

            label.GenerateLabel(filePath, _product.barcode!);
        }

        /// <summary>
        /// Rotates the pages of a given PDF file.
        /// </summary>
        private static void RotatePages(string pdfFilePath, string outputPath, int rotateDegree)
        {
            using var reader = new PdfReader(pdfFilePath);
            using var stamper = new PdfStamper(reader, new FileStream(outputPath, FileMode.Create));

            for (int n = 1; n <= reader.NumberOfPages; n++)
            {
                var page = reader.GetPageN(n);
                var rotate = page.GetAsNumber(PdfName.ROTATE);
                int newRotation = rotate == null ? rotateDegree : (rotate.IntValue + rotateDegree) % 360;
                page.Put(PdfName.ROTATE, new PdfNumber(newRotation));
            }
        }
    }
}
