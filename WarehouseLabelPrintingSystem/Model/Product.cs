using System.Text.RegularExpressions;

namespace WarehouseLabelPrintingSystem.Model
{
    public class Product
    {
        public string? product_id { get; set; }
        public object? related_product_id { get; set; }
        public string? product_name { get; set; }
        public string? product_number { get; set; }
        public string? description { get; set; }
        public string? barcode { get; set; }
        public string? product_image { get; set; }
        public object? brand_id { get; set; }
        public object? brand_name { get; set; }
        public string? category_id { get; set; }
        public string? category_name { get; set; }
        public string? supplier_id { get; set; }
        public string? supplier_name { get; set; }
        public string? supplier_code { get; set; }
        public string? vatrate_id { get; set; }
        public string? vatrate_percent { get; set; }
        public string? cost_price { get; set; }
        public string? price_inc_vat { get; set; }
        public object? special_price { get; set; }
        public bool has_takeaway_price { get; set; }
        public object? takeaway_price { get; set; }
        public object? special_takeaway_price { get; set; }
        public bool is_takeaway { get; set; }
        public object? prepare_time { get; set; }
        public bool allow_online_order { get; set; }
        public bool is_disable_discount { get; set; }
        public bool view_on_kitchen { get; set; }
        public bool view_on_webshop { get; set; }
        public object? commission { get; set; }
        public bool ask_qty { get; set; }
        public bool is_deleted { get; set; }
        public string? created_at { get; set; }
        public string? updated_at { get; set; }
        public bool has_pricelist { get; set; }
        public string? unit { get; set; }
        public bool ask_price { get; set; }
        public string? currency_code { get; set; }
        public bool use_stock { get; set; }
        public object? webshop_description { get; set; }
        public string? stock_price { get; set; }
        public bool has_serial_number { get; set; }
        public string? product_thumb_big { get; set; }
        public string? product_thumb_small { get; set; }
        public string? product_thumb_large { get; set; }
        public List<CustomField>? custom_fields { get; set; }
        public List<PriceList>? pricelist { get; set; }
        public List<object>? ingredients { get; set; }
        public List<object>? images { get; set; }

        public string location
        {
            get
            {
                var pattern = @"^\d+-\d+-[A-Za-z]-\d+$";
                var customField = custom_fields!.FirstOrDefault(cf => cf.value != null && Regex.IsMatch(cf.value, pattern));
                string? foundValue = customField?.value;

                if(foundValue != null)
                {
                    return foundValue;
                }    
                else
                {
                    return "No location";
                }
            }
        }
    }
}
