using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Drawing;
using System.IO;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using Rectangle = iTextSharp.text.Rectangle;
using System.Diagnostics;
using Image = iTextSharp.text.Image;

namespace WarehouseLabelPrintingSystem.ViewModel
{
    /// <summary>
    /// ViewModel for creating labels with product information and barcodes.
    /// </summary>
    public class LabelViewModel
    {
        public string? ProductNumber { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }
        public string? Location { get; set; }
        public string? BarcodeText { get; set; }

        // Positions for various label elements
        public PointF BarcodePosition { get; set; }
        public PointF ProductNumberPosition { get; set; }
        public PointF ProductNamePosition { get; set; }
        public PointF UnitPosition { get; set; }
        public PointF LocationPosition { get; set; }
        public PointF BarcodeTextPosition { get; set; }
        public PointF NotePosition { get; set; }

        public LabelViewModel()
        {
            // Initialize default positions for elements
            BarcodePosition = new PointF(5f, 15f);
            ProductNumberPosition = new PointF(10f, 120f);
            ProductNamePosition = new PointF(10f, 75f);
            UnitPosition = new PointF(158f, 125f);
            LocationPosition = new PointF(10f, 48f);
            BarcodeTextPosition = new PointF(12f, 5f);
            NotePosition = new PointF(10f, 0f);
        }

        /// <summary>
        /// Generates a label PDF with the specified barcode and file path.
        /// </summary>
        /// <param name="filePath">Path to save the PDF file.</param>
        /// <param name="barcodeStr">Barcode string to be included in the label.</param>
        public void GenerateLabel(string filePath, string barcodeStr)
        {
            try
            {
                // Set up the PDF document with specific dimensions and margins
                Document document = new(new Rectangle(208f, 148f), 0, 0, 0, 0);
                PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

                document.Open();

                PdfContentByte canvas = writer.DirectContent;

                // Set the font for ProductName
                var productNameFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

                // Define an area for ProductName with wrapping text
                var productNameWidth = 188f;  // Width of the area
                var productNameHeight = 40f;  // Height for text wrapping

                Rectangle productNameRect = new(
                    ProductNamePosition.X,
                    ProductNamePosition.Y,
                    ProductNamePosition.X + productNameWidth,
                    ProductNamePosition.Y + productNameHeight
                );

                // Add ProductName with text wrapping
                ColumnText ct = new(canvas);
                ct.SetSimpleColumn(
                    new Phrase(ProductName, productNameFont),
                    productNameRect.Left,
                    productNameRect.Bottom,
                    productNameRect.Right,
                    productNameRect.Top,
                    15f, // Leading between lines
                    Element.ALIGN_LEFT
                );

                ct.Go();

                // Place other elements using ColumnText with predefined positions
                ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_LEFT,
                    new Phrase(ProductNumber, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25, BaseColor.BLACK)),
                    ProductNumberPosition.X,
                    ProductNumberPosition.Y,
                    0
                );

                ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_LEFT,
                    new Phrase(Unit, FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK)),
                    UnitPosition.X,
                    UnitPosition.Y,
                    0
                );

                ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_LEFT,
                    new Phrase(Location, FontFactory.GetFont(FontFactory.HELVETICA, 25, BaseColor.BLACK)),
                    LocationPosition.X,
                    LocationPosition.Y,
                    0
                );

                // Generate the barcode using ZXing and add it to the PDF
                var barcodeWriter = new BarcodeWriter
                {
                    Format = BarcodeFormat.EAN_13,
                    Options = new EncodingOptions
                    {
                        Width = 200,
                        Height = 30,
                        NoPadding = true,
                        PureBarcode = true
                    }
                };

                var barcodeImage = barcodeWriter.Write(barcodeStr);
                var barcode = Image.GetInstance(barcodeImage, System.Drawing.Imaging.ImageFormat.Bmp);
                barcode.SetAbsolutePosition(BarcodePosition.X, BarcodePosition.Y);

                canvas.AddImage(barcode);

                // Add the barcode text with specific position
                ColumnText.ShowTextAligned(
                    canvas,
                    Element.ALIGN_LEFT,
                    new Phrase(BarcodeText, FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK)),
                    BarcodeTextPosition.X,
                    BarcodeTextPosition.Y,
                    0
                );

                document.Close(); // Close the document to save changes
            }
            catch (Exception ex)
            {
                // Handle exceptions and provide meaningful feedback
                Console.WriteLine($"Error while generating label PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Prints the specified PDF file.
        /// </summary>
        /// <param name="filePath">Path to the PDF file to be printed.</param>
        public static void PrintPdf(string filePath)
        {
            // Information about the process to start printing
            ProcessStartInfo psi = new()
            {
                FileName = filePath,
                Verb = "print",
                CreateNoWindow = true,
                UseShellExecute = true,
            };

            try
            {
                // Start the printing process
                var process = Process.Start(psi)!;
                process.WaitForExit(); // Wait for the process to complete
            }
            catch (Exception ex)
            {
                // Handle exceptions during the printing process
                Console.WriteLine($"Error while printing PDF: {ex.Message}");
            }
        }
    }
}
