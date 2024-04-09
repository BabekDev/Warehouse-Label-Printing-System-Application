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
        private readonly DuellAPIService _duellAPIService = new();

        public MainWindow()
        {
            InitializeComponent();

            ConnectApi();
        }

        private async void ConnectApi()
        {
            try
            {
                string? token = await _duellAPIService.GetAccessToken();
                string? jsonResponse = await _duellAPIService.GetProductList(token);

                Root? root = JsonConvert.DeserializeObject<Root>(jsonResponse!);

                if(root!.status)
                {
                    isConnection_text.Text = "Successful connection to the API";

                    IEnumerable<GridViewColumn> columns = CreateProductColumns();

                    GridView_Products.Columns.Clear();

                    foreach (var item in columns)
                    {
                        GridView_Products.Columns.Add(item);
                    }

                    ListView_Products.ItemsSource = root.products;
                }
                else
                {
                    isConnection_text.Foreground = new SolidColorBrush(Colors.Red);
                    isConnection_text.Text = "Failed to connect to API";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private void Print_Menu_Click(object sender, RoutedEventArgs e)
        {
            Product selectedProduct = (Product)ListView_Products.SelectedItem;

            if(selectedProduct != null)
            {
                PrintMenuWindow printMenu = new();
                printMenu.ShowDialog();
            }
        }

        private void Export_to_PDF_Click(object sender, RoutedEventArgs e)
        {
            Product selectedProduct = (Product)ListView_Products.SelectedItem;

            if (selectedProduct != null)
            {
                ExportToFileWindow exportToFileWindow = new(selectedProduct);
                exportToFileWindow.ShowDialog();
            }
        }

        private void ListView_Products_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(ListView_Products.SelectedItem != null)
            {
                Print_Menu.IsEnabled = true;
                Export_to_PDF.IsEnabled = true;
            }
        }

        private void ListView_Products_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private List<GridViewColumn> CreateProductColumns()
        {
            string[] desiredProperties = new string[] {
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
    }
}