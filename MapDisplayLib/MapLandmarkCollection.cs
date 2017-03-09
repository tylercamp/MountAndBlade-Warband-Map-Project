using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Xml;
using System.Drawing;
using System.Windows;
using System.IO;

namespace MapDisplayLib
{
	public class MapLandmarkCollection : ObservableCollection<MapLandmark>
	{
		public String MapName { get; set; }
		public String MapImageFileName { get; set; }

		public void LoadFromFile(String landmarkFile)
		{
			XmlDocument document = new XmlDocument();
			document.Load(landmarkFile);
			var root = document.DocumentElement;

			this.MapName = root.Attributes["Name"].Value;
			this.MapImageFileName = Path.Combine(Path.GetDirectoryName(landmarkFile), root.Attributes["ImageFile"].Value);

			foreach (XmlNode landmarkNode in root.ChildNodes)
			{
				try
				{
					var newMapLandmark = new MapLandmark();
					newMapLandmark.Name = landmarkNode.Attributes["Name"].Value;

					var newPosition = new System.Drawing.Point();
					newPosition.X = int.Parse(landmarkNode.Attributes["X"].Value);
					newPosition.Y = int.Parse(landmarkNode.Attributes["Y"].Value);

					newMapLandmark.Position = newPosition;

					this.Add(newMapLandmark);
				}
				catch (Exception e)
				{
					String message = String.Format("Unable to parse data for landmark:\n{0}\n\nError Message: {1}", landmarkNode.OuterXml, e.Message);
					MessageBox.Show(message);
				}
			}
		}

		protected override void InsertItem(int index, MapLandmark item)
		{
			if (this.Any(landmark => landmark.Name == item.Name))
			{
				throw new Exception("Duplicate MapLandmark entry");
			}

			base.InsertItem(index, item);
		}

		public void SaveToFile(String targetFileName)
		{
			XmlDocument document = new XmlDocument();

			XmlNode rootNode = document.CreateElement("Map");
			rootNode.Attributes.Append(document.CreateAttribute("Name")).Value = this.MapName;

			//	Set the image URI to be relative to the location of the file we're generating
			Uri mapImageUri = new Uri(this.MapImageFileName);
			Uri targetFolderUri = new Uri(Path.GetDirectoryName(Path.GetFullPath(targetFileName)) + "\\");
			rootNode.Attributes.Append(document.CreateAttribute("ImageFile")).Value = targetFolderUri.MakeRelativeUri(mapImageUri).ToString();

			document.AppendChild(rootNode);

			foreach (var landmark in this.Items)
			{
				XmlNode landmarkNode = document.CreateElement("Landmark");
				landmarkNode.Attributes.Append(document.CreateAttribute("Name")).Value = landmark.Name;
				landmarkNode.Attributes.Append(document.CreateAttribute("X")).Value = landmark.Position.X.ToString();
				landmarkNode.Attributes.Append(document.CreateAttribute("Y")).Value = landmark.Position.Y.ToString();

				rootNode.AppendChild(landmarkNode);
			}

			if (File.Exists(targetFileName))
				File.Delete(targetFileName);
			document.Save(targetFileName);
		}
	}
}
