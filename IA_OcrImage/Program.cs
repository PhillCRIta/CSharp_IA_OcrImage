using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Patagames.Pdf;
using Patagames.Pdf.Net;


class Program
{
	static void Main(string[] args)
	{
		//string pdfPath = @"xxxx";
		//string outputDirectory = @"xxxx";
		//PdfSplitterConvetBitmap.SplitPdfToJpg(pdfPath, outputDirectory);


		List<List<string>> testoPagine = new List<List<string>>();

		FileInfo[] dir = new DirectoryInfo(@"\tempout").GetFiles("*.jpg");
		foreach (FileInfo file in dir)
		{
			List<Rectangle> rectanglePoints = new List<Rectangle>();
			//for each file send a Azure to get OCR
			List<string> righePagina = AnalyzeImage(file.FullName, ref rectanglePoints);
			InsertRectangleOnImage(file, rectanglePoints, Path.GetFileNameWithoutExtension(file.Name));
			testoPagine.Add(righePagina);
		}
		string pathOutFile = @"\tempout\txtOut.txt";
		foreach (List<string> listaRighe in testoPagine)
			File.AppendAllLines(pathOutFile, listaRighe);



		// Load the image from a file
		

	}

	private static void InsertRectangleOnImage(FileInfo imagePath, List<Rectangle> rectanglePoints, string name)
	{
		using (Image image = Image.FromFile(imagePath.FullName))
		{
			// Create a graphics object from the image
			using (Graphics graphics = Graphics.FromImage(image))
			{
				foreach (Rectangle rectanglePoint in rectanglePoints)
				{
					// Create a red pen to draw the rectangle
					using (Pen redPen = new Pen(Color.Red, 4))
					{
						// Define the rectangle dimensions
						int x = rectanglePoint.X;
						int y = rectanglePoint.Y;
						int width = rectanglePoint.Width;
						int height = rectanglePoint.Height;
						// Draw the rectangle
						graphics.DrawRectangle(redPen, x, y, width, height);
					}
				}
			}
			// Save the modified image to a new file
			string outputPath = @"\tempout\"+name+"_OUT.jpg";
			image.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
		}
	}

	static List<string> AnalyzeImage(string file, ref List<Rectangle> rectanglePoints)
	{
		string endpoint = "https://xxx.cognitiveservices.azure.com/";
		string key = "xxx";

		ImageAnalysisClient client = new ImageAnalysisClient(
			new Uri(endpoint),
			new AzureKeyCredential(key));

		BinaryData imageBin = BinaryData.FromBytes(File.ReadAllBytes(file));

		ImageAnalysisResult result = client.Analyze(
			imageBin,
			VisualFeatures.Caption | VisualFeatures.Read,
			new ImageAnalysisOptions { GenderNeutralCaption = true });

		List<string> righePagina = new List<string>();

		Console.WriteLine("Image analysis results:");
		Console.WriteLine(" Caption:");
		Console.WriteLine($"   '{result.Caption.Text}', Confidence {result.Caption.Confidence:F4}");
		Console.WriteLine(" Read:");
		foreach (DetectedTextBlock block in result.Read.Blocks)
		{
			foreach (DetectedTextLine line in block.Lines)
			{
				List<float> confidence = new List<float>();
				Console.WriteLine($"   Line: '{line.Text}', Bounding Polygon: [{string.Join(" ", line.BoundingPolygon)}]");
				foreach (DetectedTextWord word in line.Words)
				{
					confidence.Add(word.Confidence);
					Console.WriteLine($"     Word: '{word.Text}', Confidence {word.Confidence.ToString("#.####")}, Bounding Polygon: [{string.Join(" ", word.BoundingPolygon)}]");
					Rectangle r = new Rectangle();
					r.X = word.BoundingPolygon.Select(k => k.X).Min();
					r.Width = word.BoundingPolygon.Select(k=>k.X).Max()- word.BoundingPolygon.Select(k => k.X).Min();
					r.Y = word.BoundingPolygon.Select(k => k.Y).Min();
					r.Height = word.BoundingPolygon.Select(k => k.Y).Max() - word.BoundingPolygon.Select(k => k.Y).Min();
					rectanglePoints.Add(r);
				}
				if (confidence.Average() < 0.5)
					righePagina.Add("*******" + line.Text + "*******");
				else
					righePagina.Add(line.Text);
			}
			righePagina.Add(System.Environment.NewLine);
			righePagina.Add(System.Environment.NewLine);
			righePagina.Add(System.Environment.NewLine);
			righePagina.Add(System.Environment.NewLine);
		}
		return righePagina;
	}
}