namespace WarehouseLabelPrintingSystem.Utilities
{
    /// <summary>
    /// Provides functionality for formatting barcode numbers with specific spacing.
    /// </summary>
    public static class BarcodeFormatNumber
    {
        /// <summary>
        /// Formats a barcode number with specific spacing.
        /// </summary>
        /// <param name="barcode">The original barcode number as a string.</param>
        /// <returns>The formatted barcode number, or an error message if the input is invalid.</returns>
        public static string FormatNumber(string barcode)
        {
            // Ensure the barcode is long enough to apply formatting
            if (string.IsNullOrWhiteSpace(barcode) || barcode.Length < 7)
            {
                // Log an appropriate message and return an error string or empty string
                return "Error: Barcode is invalid or too short for formatting.";
            }

            try
            {
                // Format the barcode with custom spacing
                string formattedBarcode =
                    barcode.Substring(0, 1) + "  " + // Add double space after the first character
                    barcode.Substring(1, 5) + "  " + // Add double space after the next 5 characters
                    barcode.Substring(6);           // Keep the remaining characters as-is

                return formattedBarcode;
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions, such as index out of range
                return $"Error: Unable to format barcode. Details: {ex.Message}";
            }
        }
    }
}
