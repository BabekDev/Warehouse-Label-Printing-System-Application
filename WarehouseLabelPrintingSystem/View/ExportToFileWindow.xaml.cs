using Microsoft.Win32;
using System.Drawing;
using System.IO;
using System.Printing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;
using WarehouseLabelPrintingSystem.Model;
using WarehouseLabelPrintingSystem.Utilities;
using WarehouseLabelPrintingSystem.ViewModel;
using PdfSharp.Pdf.IO;

namespace WarehouseLabelPrintingSystem.View
{
    public partial class ExportToFileWindow : Window
    {
        private readonly Product _product;

        public ExportToFileWindow(Product product)
        {
            InitializeComponent();

            _product = product;

            LoadProductData();
        }

        private void LoadProductData()
        {
            if(_product != null )
            {
                Product_Id_Text.Text = $"Id: {_product.product_id}";
                Product_Number_Text.Text = $"Number: {_product.product_number}";
                Product_Barcode_Text.Text = $"Barcode: {_product.barcode}";
                Product_Unit_Text.Text = $"Unit: {_product.unit}";
                Product_Name_Text.Text = $"Name: {_product.product_name}";

                List<GridViewColumn> columns = CreateCustomFieldColumns();

                GridView_CustomField.Columns.Clear();

                foreach (var item in columns)
                {
                    GridView_CustomField.Columns.Add(item);
                }

                ListView_CustomFields.ItemsSource = _product.custom_fields;
            }
        }

        private void Save_to_File_Click(object sender, RoutedEventArgs e)
        {
            CustomField customField = (CustomField)ListView_CustomFields.SelectedItem;

            if (customField != null)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";

                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;
                    BarcodeGenerationAndSavingToPDF(filePath);
                }
            }
        }

        private void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                PrintPDF(openFileDialog.FileName);
            }
        }

        private void ListView_CustomFields_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListView_CustomFields.SelectedItem != null)
            {
                Save_to_File.IsEnabled = true;
                Print_Button.IsEnabled = true;
            }
        }

        private void ListView_CustomFields_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private List<GridViewColumn> CreateCustomFieldColumns()
        {
            string[] desiredProperties = new string[] {
                "id",
                "label",
                "value",
            };

            List<GridViewColumn> columns = new();

            foreach (var propertyName in desiredProperties)
            {
                PropertyInfo? property = typeof(CustomField).GetProperty(propertyName);

                if (property != null)
                {
                    GridViewColumn column = new()
                    {
                        Header = property.Name,
                        DisplayMemberBinding = new Binding(property.Name)
                    };
                    columns.Add(column);
                }
            }

            return columns;
        }

        private void BarcodeGenerationAndSavingToPDF(string filePath)
        {
            CustomField customField = (CustomField)ListView_CustomFields.SelectedItem;

            LabelViewModel label = new()
            {
                ProductNumber = _product.product_number,
                ProductName = _product.product_name,
                Unit = _product.unit,
                Location = customField.value,
                BarcodeText = BarcodeFormatNumber.FormatNumber(_product.barcode!),
                ProductNumberPosition = new PointF(11f, 0f),
                UnitPosition = new PointF(160f, -18f),
                ProductNamePosition = new PointF(10f, 8f),
                LocationPosition = new PointF(10f, 6f),
                BarcodePosition = new PointF(0f, 15f),
                BarcodeTextPosition = new PointF(6f, 25f),
                NotePosition = new PointF(10f, 0f)
            };

            label.GenerateLabel(filePath, _product.barcode!);
        }

        private void PrintPDF(string filePath)
        {
            try
            {
                PrintDialog printDialog = new();
                printDialog.PageRangeSelection = PageRangeSelection.AllPages;
                printDialog.UserPageRangeEnabled = true;

                if (printDialog.ShowDialog() == true)
                {
                    System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo
                    {
                        Verb = "print",
                        FileName = filePath,
                        CreateNoWindow = true,
                        WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                    };

                    System.Diagnostics.Process p = new()
                    {
                        StartInfo = info
                    };
                    p.Start();

                    p.WaitForInputIdle();

                    Thread.Sleep(3000);
                    if (false == p.CloseMainWindow())
                        p.Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while printing the PDF file: " + ex.Message);
            }
        }
    }
}
