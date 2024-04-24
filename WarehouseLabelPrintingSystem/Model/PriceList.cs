namespace WarehouseLabelPrintingSystem.Model
{
    public class PriceList
    {
        public string? customer_group_id { get; set; }
        public string? price_inc_vat { get; set; }
        public string? customer_group_name { get; set; }
        public bool customer_group_has_pricelist { get; set; }
        public bool customer_group_is_active { get; set; }
        public bool customer_group_is_deleted { get; set; }
    }
}
