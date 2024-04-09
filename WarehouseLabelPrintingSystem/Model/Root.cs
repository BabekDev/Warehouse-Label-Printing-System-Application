namespace WarehouseLabelPrintingSystem.Model
{
    public class Root
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public int code { get; set; }
        public string? dateTimeBeforeQryExec { get; set; }
        public List<Product>? products { get; set; }
        public int total_count { get; set; }
    }
}
