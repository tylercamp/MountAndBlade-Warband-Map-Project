using MapDisplayLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MapEditor
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			//this.MapDisplay.SetMapImageFromFile(@"G:\Dropbox\Projects\MountAndBlade-Warband Map Project\MountAndBladeWarband-Floris.png");

			this.MapDisplay.MouseDoubleClick += MapDisplay_MouseDoubleClick;
		}

		void MapDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (!this.MapDisplay.IsReady)
				return;

			var mousePos = e.GetPosition(this.MapDisplay);

			LabelNameWindow nameWindow = new LabelNameWindow();
			if (nameWindow.ShowDialog().Value)
			{
				MapLandmark newLandmark = new MapLandmark();
				newLandmark.Name = nameWindow.EnteredText;

				var mapTransform = this.MapDisplay.CalculateMapTransform();
				var mapPoint = mapTransform.Inverse.Transform(mousePos);

				newLandmark.Position = new System.Drawing.Point((int)mapPoint.X, (int)mapPoint.Y);
				this.MapDisplay.MapLandmarks.Add(newLandmark);
			}
		}

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (this.MapDisplay.MapLandmarks.MapImageFileName == null)
				return;

			SaveFileDialog sfd = new SaveFileDialog();
			sfd.RestoreDirectory = true;
			sfd.InitialDirectory = System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(this.MapDisplay.MapLandmarks.MapImageFileName));
			sfd.Filter = "XML Files|*.xml";
			if (!sfd.ShowDialog().GetValueOrDefault(false))
				return;

			this.MapDisplay.MapLandmarks.SaveToFile(sfd.FileName);
		}

		private void Load_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog sfd = new OpenFileDialog();
			sfd.Filter = "XML Files|*.xml";
			if (!sfd.ShowDialog().GetValueOrDefault(false))
				return;

			this.MapDisplay.LoadFromConfiguration(sfd.FileName);
		}

		private void New_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
			if (!ofd.ShowDialog().GetValueOrDefault(false))
				return;

			MessageBox.Show("Enter the name of the map.");
			LabelNameWindow nameWindow = new LabelNameWindow();
			if (!nameWindow.ShowDialog().GetValueOrDefault(false))
				return;

			this.MapDisplay.MapLandmarks.Clear();
			this.MapDisplay.MapLandmarks.MapName = nameWindow.EnteredText;
			this.MapDisplay.SetMapImageFromFile(ofd.FileName);
		}
	}
}
