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
using System.Windows.Shapes;

namespace MapEditor
{
	/// <summary>
	/// Interaction logic for LabelNameWindow.xaml
	/// </summary>
	public partial class LabelNameWindow : Window
	{
		public String EnteredText { get; set; }

		public LabelNameWindow()
		{
			InitializeComponent();

			this.Closing += LabelNameWindow_Closing;
			this.DataContext = this;

			this.textbox.Focus();
		}

		void LabelNameWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (this.DialogResult == null)
				this.DialogResult = false;
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;
		}

		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.Button_Click(null, null);
		}
	}
}
