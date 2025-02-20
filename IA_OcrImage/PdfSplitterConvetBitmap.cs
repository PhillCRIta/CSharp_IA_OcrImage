using Patagames.Pdf;
using Patagames.Pdf.Net;
using System.Drawing.Imaging;

internal class PdfSplitterConvetBitmap
{
	internal static void SplitPdfToJpg(string pdfPath, string outputDirectory)
	{
		// Ensure the output directory exists
		if (!Directory.Exists(outputDirectory))
		{
			Directory.CreateDirectory(outputDirectory);
		}
		// Load the PDF document
		using (var document = PdfDocument.Load(pdfPath))
		{
			for (int pageNumber = 0; pageNumber < document.Pages.Count; pageNumber++)
			{
				// Render the page to a bitmap
				string path = RenderPage(document, pageNumber);
				Console.WriteLine($"Page {pageNumber} saved in " + path);
			}
		}
	}
	internal static string RenderPage(PdfDocument document, int pageNumber)
	{
		string pathOut = string.Format(@"\test001_pdf_page_{0}.jpg", pageNumber);
		// Render the page to a bitmap with a specific DPI
		const int dpi = 300;
		var size = document.GetPageSizeByIndex(pageNumber);
		var width = (int)(size.Width * dpi / 72);
		var height = (int)(size.Height * dpi / 72);

		//var bitmap = new Bitmap(width, height);
		//using (var graphics = Graphics.FromImage(bitmap))
		//{
		//	graphics.Clear(Color.White);
		//	graphics.DrawImage(document.Pages[pageNumber].Render(), 0, 0);
		//	//pageNumber, width, height, dpi, dpi, PdfRenderFlags.Annotations
		//}
		//Create a bitmap
		using (PdfBitmap bmp = new PdfBitmap(width, height, true))
		{
			//Fill background
			bmp.FillRect(0, 0, width, height, FS_COLOR.White);
			//Render contents in a page to a drawing surface specified by a coordinate pair, a width, and a height.
			document.Pages[pageNumber].Render(bmp, 0, 0, width, height,
				Patagames.Pdf.Enums.PageRotate.Normal,
				Patagames.Pdf.Enums.RenderFlags.FPDF_ANNOT);
			//Get .Net image and save it into file
			bmp.GetImage().Save(pathOut, ImageFormat.Jpeg);
		}
		return pathOut;
	}
}