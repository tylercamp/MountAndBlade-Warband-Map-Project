using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Windows.Media.Imaging;

/*
 * Used to crop/resize the large amounts of screenshots used to create gamemap.png. Go to Program.FillProcessingSteps to configure.
 * 
 * 
 * 
 * My process for map generation was taking a bunch of screenshots, moving the camera vertically, and grouping image sets together via vertical group.
 *	i.e. start at the top right, screenshot, move down a bit, screenshot, etc. until I hit the bottom. That set of screenshots would be one group. I'd
 *	move to the left a bit, and repeat the process.
 *	
 * Once I had all of the screenshot groups (came to ~6 groups of ~7 images per group), I used Photoshop's PhotoMerge to merge each group (slice) of the
 *	map. I would then photomerge each vertical slice to create the overall map. To top it off, I then photomerged extra screenshots to fill in the
 *	corners/edges that I had missed.
 *	
 * I tried multiple times to use all of the screenshots at once, but Photoshop just took way too long.
 * 
 */

namespace ImageProcessing
{
	interface ImageProcessor
	{
		Bitmap ProcessImage(Bitmap image);
	}

	class ResizeProcessor : ImageProcessor
	{
		enum SizingMethod
		{
			Absolute,
			Percent,
			Unknown
		}

		SizingMethod method = SizingMethod.Unknown;

		int newWidth, newHeight;
		float percent;

		public ResizeProcessor(int newWidth, int newHeight)
		{
			method = SizingMethod.Absolute;
			this.newWidth = newWidth;
			this.newHeight = newHeight;
		}

		public ResizeProcessor(float percent)
		{
			method = SizingMethod.Percent;
			this.percent = percent;
		}

		public Bitmap ProcessImage(Bitmap image)
		{
			int newImageWidth, newImageHeight;
			switch (method)
			{
				case SizingMethod.Absolute:
					newImageWidth = newWidth;
					newImageHeight = newHeight;
					break;
				case SizingMethod.Percent:
					newImageWidth = (int)(image.Width * percent);
					newImageHeight = (int)(image.Height * percent);
					break;
				default:
					throw new Exception();
			}

			return new Bitmap(image, newImageWidth, newImageHeight);
		}
	}

	class CropProcessor : ImageProcessor
	{
		Rectangle targetArea;

		public CropProcessor(Rectangle targetArea)
		{
			this.targetArea = targetArea;
		}

		public Bitmap ProcessImage(Bitmap image)
		{
			int newWidth, newHeight;
			newWidth = Math.Min(image.Width - targetArea.X, targetArea.Width);
			newHeight = Math.Min(image.Height - targetArea.Y, targetArea.Height);

			Bitmap result = new Bitmap(newWidth, newHeight);

			using (Graphics graphics = Graphics.FromImage(result))
			{
				graphics.DrawImage(image, new Rectangle(0, 0, newWidth, newHeight), targetArea, GraphicsUnit.Pixel);
			}

			return result;
		}
	}

	class Program
	{
		/* CONFIGURATION */
		static void FillProcessingSteps(List<ImageProcessor> target)
		{
			//	Crop off M&B UI (my own screenshot resolution was 2560x1440)
			target.Add(new CropProcessor(targetArea: new Rectangle(0, 0, 2560, 1200)));

			//	Resize for Photoshop performance
			//target.Add(new ResizeProcessor(0.5f));
		}





		static async Task RunProcessingList(String targetFile, String outputFile, List<ImageProcessor> processors)
		{
			await Task.Factory.StartNew((Action)delegate()
			{
				Bitmap currentOutput = Bitmap.FromFile(targetFile) as Bitmap;
				foreach (var processor in processors)
					currentOutput = processor.ProcessImage(currentOutput);
				currentOutput.Save(outputFile);
			});
		}

		[STAThread]
		static void Main(string[] args)
		{
			Console.SetWindowSize(100, 10);

			//	Assign to 'null' to allow the user to pick their own folder
			String sourceFolder = @"C:\Users\Tyler\Documents\Mount&Blade Warband\Screenshots\Floris Expanded Mod Pack 2.54";
			String outputFolderName = "output";

			String[] fileIgnoreList = new String[] { "gamemap.png" };

			if (sourceFolder == null)
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				if (fbd.ShowDialog() != DialogResult.OK)
					return;

				sourceFolder = fbd.SelectedPath;
			}

			String outputFolder = Path.Combine(sourceFolder, outputFolderName);
			if (!Directory.Exists(outputFolder))
				Directory.CreateDirectory(outputFolder);



			List<ImageProcessor> steps = new List<ImageProcessor>();
			FillProcessingSteps(steps);

			List<Task> processingTasks = new List<Task>();

			var files = Directory.EnumerateFiles(sourceFolder);
			foreach (var fullFilePath in files)
			{
				if (fileIgnoreList.Contains(Path.GetFileName(fullFilePath)))
					continue;

				String outputFile = Path.Combine(outputFolder, Path.GetFileNameWithoutExtension(fullFilePath) + ".png");
				processingTasks.Add(RunProcessingList(fullFilePath, outputFile, steps));
			}

			foreach (var task in processingTasks)
				task.Wait();

			Console.WriteLine("Done.");
		}
	}
}
