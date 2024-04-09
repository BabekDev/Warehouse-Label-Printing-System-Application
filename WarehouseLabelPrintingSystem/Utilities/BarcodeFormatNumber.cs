namespace WarehouseLabelPrintingSystem.Utilities
{
    public class BarcodeFormatNumber
    {
        public static string FormatNumber(string numberStr)
        {
            return numberStr.Substring(0, 1) + "  " + numberStr.Substring(1, 5) + "  " + numberStr.Substring(6);
        }
    }
}
