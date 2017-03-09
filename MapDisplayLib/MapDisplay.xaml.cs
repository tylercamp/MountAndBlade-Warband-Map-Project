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

namespace MapDisplayLib
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class MapDisplayControl : UserControl
    {
		public MapLandmarkCollection MapLandmarks { get; private set; }

		public BitmapImage MapBitmapImage { get { return MapImageContainer.Source as BitmapImage; } }

		float mapViewScale = 1.0f;
		public float MapViewScale
		{
			get { return mapViewScale; }
			set
			{
				mapViewScale = value;
				mapViewScale = Math.Max(mapViewScale, MinMapViewScale);
				mapViewScale = Math.Min(1.0f, mapViewScale);
			}
		}

		//	Based on the size of the map and the size of the view area
		public float MinMapViewScale
		{
			get
			{
				if (this.MapBitmapImage == null)
					return 1.0f;

				return Math.Max((float)this.ActualWidth / (float)MapBitmapImage.PixelWidth, (float)this.ActualHeight / (float)MapBitmapImage.PixelHeight);
			}
		}

		//	get: Calculated from current view size and map transform.
		public Rect MapViewArea
		{
			get
			{
				var transform = CalculateMapTransform().Inverse;

				Point topLeft, bottomRight;
				topLeft = transform.Transform(new Point(0, 0));
				bottomRight = transform.Transform(new Point(this.ActualWidth, this.ActualHeight));

				return new Rect(topLeft, bottomRight);
			}
		}

		public Point MapViewPosition { get; set; }

		public bool RenderLandmarks { get; set; }

		public Transform CalculateMapTransform()
		{
			TransformGroup currentImageTransform = new TransformGroup();

			currentImageTransform.Children.Add(new TranslateTransform(-MapViewPosition.X, -MapViewPosition.Y));
			currentImageTransform.Children.Add(new ScaleTransform(MapViewScale, MapViewScale));
			currentImageTransform.Children.Add(new TranslateTransform(MapCanvas.ActualWidth / 2.0, MapCanvas.ActualHeight / 2.0));

			return currentImageTransform;
		}

		public void ResetMapViewPosition()
		{
			if (MapBitmapImage == null)
				return;

			MapViewScale = MinMapViewScale;
			MapViewPosition = new Point(MapBitmapImage.PixelWidth / 2.0, MapBitmapImage.PixelHeight / 2.0);

			this.UpdateMapTransform();
		}

		private Point lastMousePosition = new Point();
		public bool IsDragging { get; private set; }

		public bool IsReady
		{
			get
			{
				return this.IsLoaded &&
						MapBitmapImage != null &&
						this.ActualWidth != 0.0 && this.ActualHeight != 0.0;
			}
		}

		public void GoToLandmark(String landmarkName)
		{
			try
			{
				var landmark = MapLandmarks.First((l) => l.Name.ToLowerInvariant() == landmarkName.ToLowerInvariant());
				if (landmark != null)
					GoToLandmark(landmark);
			}
			catch (Exception e)
			{
				MessageBox.Show("Landmark " + landmarkName + " does not exist.");
			}
		}

		public void GoToLandmark(MapLandmark landmark)
		{
			MapViewPosition = new Point(landmark.Position.X, landmark.Position.Y);
			UpdateMapTransform();
		}

        public MapDisplayControl()
        {
			MapLandmarks = new MapLandmarkCollection();
			MapLandmarks.MapName = "Mount and Blade: Warband (Floris Mod)";

			this.DataContext = MapLandmarks;

            InitializeComponent();

			this.SizeChanged += MapDisplayControl_SizeChanged;
			this.MouseWheel += MapDisplayControl_MouseWheel;
			this.MouseMove += MapDisplayControl_MouseMove;
			this.MouseDown += MapDisplayControl_MouseDown;
			this.MouseUp += MapDisplayControl_MouseUp;

			this.MapLandmarks.CollectionChanged += MapLandmarks_CollectionChanged;

			IsDragging = false;
        }

		void MapLandmarks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			//	NOTE: RenderLandmarks assumed to never change
			if (!this.RenderLandmarks)
				return;

			if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
			{
				var labels = LayoutGrid.Children.OfType<Label>().ToArray();
				foreach (var label in labels)
					LayoutGrid.Children.Remove(label);
			}

			if (e.NewItems != null)
			{
				foreach (MapLandmark newLandmark in e.NewItems)
				{
					Label landmarkLabel = new Label();
					landmarkLabel.Content = newLandmark.Name;
					landmarkLabel.FontSize = 20.0;
					landmarkLabel.Foreground = Brushes.White;
					landmarkLabel.Margin = new Thickness(newLandmark.Position.X, newLandmark.Position.Y, 0, 0);
					LayoutGrid.Children.Add(landmarkLabel);
				}
			}

			if (e.OldItems != null)
			{
				foreach (MapLandmark oldLandmark in e.OldItems)
				{
					var labelForLandmark = LayoutGrid.Children.OfType<Label>().First((label) => (String)label.Content == oldLandmark.Name);
					LayoutGrid.Children.Remove(labelForLandmark);
				}
			}
		}

		void MapDisplayControl_MouseUp(object sender, MouseButtonEventArgs e)
		{
			IsDragging = false;
		}

		void MapDisplayControl_MouseDown(object sender, MouseButtonEventArgs e)
		{
			IsDragging = true;
			lastMousePosition = e.GetPosition(this);
		}

		void MapDisplayControl_MouseMove(object sender, MouseEventArgs e)
		{
			if (!IsDragging)
				return;

			if (e.LeftButton != MouseButtonState.Pressed)
			{
				IsDragging = false;
				return;
			}

			Transform currentTransform = CalculateMapTransform();
			Point currentPosition = e.GetPosition(this);

			Point currentWorldPosition = currentTransform.Inverse.Transform(currentPosition);
			Point previousWorldPosition = currentTransform.Inverse.Transform(lastMousePosition);

			Point newViewPosition = MapViewPosition;
			newViewPosition.X -= (currentWorldPosition.X - previousWorldPosition.X);
			newViewPosition.Y -= (currentWorldPosition.Y - previousWorldPosition.Y);

			MapViewPosition = newViewPosition;

			lastMousePosition = e.GetPosition(this);

			UpdateMapTransform();
		}

		void MapDisplayControl_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			const float scrollFactor = 0.2f * (1.0f / 120.0f);
			MapViewScale += MapViewScale * (scrollFactor * e.Delta);

			this.UpdateMapTransform();
		}

		public void SetMapImageFromFile(String mapImageFile)
		{
			MapLandmarks.MapImageFileName = mapImageFile;

			LoadMapImageFromFile(mapImageFile);
		}

		void MapDisplayControl_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			MapViewScale = Math.Max(MapViewScale, MinMapViewScale);

			this.UpdateMapTransform();
		}

		private void UpdateMapTransform()
		{
			if (MapBitmapImage == null)
				return;

			var correctedViewPosition = MapViewPosition;
			var currentArea = MapViewArea;
			
			if (currentArea.Left < 0)
				correctedViewPosition.X -= currentArea.Left;
			
			if (currentArea.Right > MapBitmapImage.PixelWidth)
				correctedViewPosition.X -= (currentArea.Right - MapBitmapImage.PixelWidth);
			
			if (currentArea.Top < 0)
				correctedViewPosition.Y -= currentArea.Top;

			if (currentArea.Bottom > MapBitmapImage.PixelHeight)
				correctedViewPosition.Y -= (currentArea.Bottom - MapBitmapImage.PixelHeight);

			MapViewPosition = correctedViewPosition;

			LayoutGrid.RenderTransform = CalculateMapTransform();
		}

		public void LoadFromConfiguration(String configurationFile)
		{
			MapLandmarks.Clear();

			MapLandmarks.LoadFromFile(configurationFile);
			LoadMapImageFromFile(MapLandmarks.MapImageFileName);
		}

		private void LoadMapImageFromFile(String fileName)
		{
			Task.Factory.StartNew(() =>
				{
					BitmapImage mapImage = new BitmapImage();
					mapImage.BeginInit();
					mapImage.UriSource = new Uri(System.IO.Path.GetFullPath(fileName));
					mapImage.CacheOption = BitmapCacheOption.OnLoad;
					mapImage.EndInit();

					mapImage.Freeze();

					Dispatcher.Invoke(() =>
						{
							this.MapImageContainer.Source = mapImage;

							LayoutGrid.Width = mapImage.PixelWidth;
							LayoutGrid.Height = mapImage.PixelHeight;

							this.ResetMapViewPosition();
						});
				});
		}
    }
}
