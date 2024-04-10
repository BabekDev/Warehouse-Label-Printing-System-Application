using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Drawing;
using System.IO;
using ZXing.Common;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Windows.Xps.Packaging;

namespace WarehouseLabelPrintingSystem.ViewModel
{
    public class LabelViewModel
    {
        public string? ProductNumber { get; set; }
        public string? ProductName { get; set; }
        public string? Unit { get; set; }
        public string? Location { get; set; }
        public string? BarcodeText { get; set; }
        public PointF BarcodePosition { get; set; }
        public PointF ProductNumberPosition { get; set; }
        public PointF ProductNamePosition { get; set; }
        public PointF UnitPosition { get; set; }
        public PointF LocationPosition { get; set; }
        public PointF BarcodeTextPosition { get; set; }
        public PointF NotePosition { get; set; }

        public LabelViewModel()
        {
            // Default constructor
        }

        public void GenerateLabel(string filePath, string barcodeStr)
        {
            // Create a new document
            Document document = new(new iTextSharp.text.Rectangle(208f, 148f), 0, 0, -5, 0);
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            document.Open();

            // Add barcode
            BarcodeWriter writerEan = new()
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

            // Add product information
            iTextSharp.text.Font productNumberFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25, BaseColor.BLACK);
            iTextSharp.text.Font productNameFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);
            iTextSharp.text.Font productUnitFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, BaseColor.BLACK);
            iTextSharp.text.Font productLocationFont = FontFactory.GetFont(FontFactory.HELVETICA, 25, BaseColor.BLACK);
            iTextSharp.text.Font productBarcodeTextFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, BaseColor.BLACK);

            document.Add(new Paragraph(ProductNumber, productNumberFont) { SpacingBefore = ProductNumberPosition.Y, IndentationLeft = ProductNumberPosition.X });
            document.Add(new Paragraph(Unit, productUnitFont) { SpacingBefore = UnitPosition.Y, IndentationLeft = UnitPosition.X });
            document.Add(new Paragraph(ProductName, productNameFont) { SpacingBefore = ProductNamePosition.Y, IndentationLeft = ProductNamePosition.X });
            document.Add(new Paragraph(Location, productLocationFont) { SpacingBefore = LocationPosition.Y, IndentationLeft = LocationPosition.X });

            var barcodeImage = writerEan.Write(barcodeStr);
            iTextSharp.text.Image barcode = iTextSharp.text.Image.GetInstance(barcodeImage, System.Drawing.Imaging.ImageFormat.Bmp);
            barcode.SetAbsolutePosition(BarcodePosition.X, BarcodePosition.Y);
            document.Add(barcode);

            document.Add(new Paragraph(BarcodeText, productBarcodeTextFont) { SpacingBefore = BarcodeTextPosition.Y, IndentationLeft = BarcodeTextPosition.X });

            document.Close();
        }
    }
}
