using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace MountAndBlade_Warband_Map_Project
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private String[] availableLandmarkNames;

		public MainWindow()
		{
			InitializeComponent();

			String mapFile = "mb-w floris.xml";
			if (Debugger.IsAttached)
				mapFile = "../../../" + mapFile;

			if (!File.Exists(mapFile))
			{
				MessageBox.Show("Missing map XML layout file " + mapFile);
				this.Close();
				return;
			}

			this.MapDisplay.RenderLandmarks = false;
			this.MapDisplay.LoadFromConfiguration(mapFile);

			availableLandmarkNames = this.MapDisplay.MapLandmarks.Select(landmark => landmark.Name).ToArray();

			this.PrimarySearchTextbox.ItemsSource = availableLandmarkNames;
			this.PrimarySearchTextbox.PreviewKeyDown += PrimarySearchTextbox_KeyDown;
		}

		void PrimarySearchTextbox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter && PrimarySearchTextbox.Text != "")
			{
				GoButton_Click(null, null);
				PrimarySearchTextbox.Text = "";
			}
		}

		private void GoButton_Click(object sender, RoutedEventArgs e)
		{
			this.MapDisplay.MapViewScale = Math.Max(this.MapDisplay.MapViewScale, 0.6f);
			this.MapDisplay.GoToLandmark(PrimarySearchTextbox.Text);
		}
	}
}
