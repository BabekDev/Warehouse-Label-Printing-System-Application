using Microsoft.Win32;
using System.Drawing;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WarehouseLabelPrintingSystem.Model;
using WarehouseLabelPrintingSystem.Utilities;
using WarehouseLabelPrintingSystem.ViewModel;
using System.IO;
using System.Windows.Documents;
using iTextSharp.text.pdf;
using iTextSharp.text;

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
            if (_product != null)
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

                    RotatePages(filePath, @"C:\Users\Babek-pc\Desktop\test.pdf", 90);
                }
            }
        }

        private void Print_Button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf";

            if (openFileDialog.ShowDialog() == true)
            {
                PrintLabels(openFileDialog.FileName);
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
                LocationPosition = new PointF(5f, 3f),
                BarcodePosition = new PointF(0f, 15f),
                BarcodeTextPosition = new PointF(6f, 27f),
                NotePosition = new PointF(10f, 0f)
            };

            label.GenerateLabel(filePath, _product.barcode!);
        }

        public void PrintLabels(string filePath)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
                {
                    FixedDocument fixedDoc = new FixedDocument();
                    Document document = new Document();
                    PdfReader pdfReader = new PdfReader(fileStream);
                    for (int pageNumber = 1; pageNumber <= pdfReader.NumberOfPages; pageNumber++)
                    {
                        // PdfImportedPage pdfImportedPage = printDialog.PrintableArea == PrintableArea.A4 ? printDialog.PrintableArea == PrintableArea.A3 : 0 ? pdfWriter.GetImportedPage(pdfReader, pageNumber) : pdfWriter.GetImportedPage(pdfReader, pageNumber);
                        PageContent pageContent = new PageContent();
                        FixedPage fixedPage = new FixedPage();
                        fixedPage.Width = printDialog.PrintableAreaWidth;
                        fixedPage.Height = printDialog.PrintableAreaHeight;
                        //fixedPage.Children.Add(pageContent);

                        ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                        fixedDoc.Pages.Add(pageContent);
                    }

                    printDialog.PrintDocument(fixedDoc.DocumentPaginator, "Print PDF");
                }
            }
        }

        private static void RotatePages(string pdfFilePath, string outputPath, int rotateDegree)
        {
            PdfReader reader = new(pdfFilePath);
            int pagesCount = reader.NumberOfPages;

            for (int n = 1; n <= pagesCount; n++)
            {
                PdfDictionary page = reader.GetPageN(n);
                PdfNumber rotate = page.GetAsNumber(PdfName.ROTATE);

                int rotation =
                        rotate == null ? rotateDegree : (rotate.IntValue + rotateDegree) % 360;

                page.Put(PdfName.ROTATE, new PdfNumber(rotation));
            }

            PdfStamper stamper = new(reader, new FileStream(outputPath, FileMode.Create));
            stamper.Close();
            reader.Close();
        }
    }
}
