#region Using declarations
using System;
using System.Windows;
using System.Windows.Controls.WpfPropertyGrid;
using System.Windows.Markup;
using Microsoft.Win32;
#endregion

namespace NinjaTrader.NinjaScript.AddOns
{
	public class CustomFilePicker : PropertyEditor
	{
		public CustomFilePicker()
		{
			InlineTemplate = CreateTemplate();
		}

		DataTemplate CreateTemplate()
		{
			const string xamlTemplate = 
@"
<DataTemplate >
	  <Grid>
		<Grid.ColumnDefinitions>
		  <ColumnDefinition Width=""30""/>
          <ColumnDefinition Width=""*""/>
		</Grid.ColumnDefinitions>

	<Button Grid.Column=""0"" Content=""..."" Padding=""0"" Margin=""0""
			  HorizontalAlignment=""Stretch"" VerticalAlignment=""Stretch""
			  HorizontalContentAlignment=""Center""
			  MinWidth=""30""
			  Width=""30""
			  MaxWidth=""30""
			  Command =""pg:PropertyEditorCommands.ShowDialogEditor""
			  CommandParameter=""{Binding}"" />

		<TextBox Grid.Column=""1"" 
				 Text=""{Binding StringValue}"" 
				 ToolTip=""{Binding Value}""/>

	
	  </Grid>
	</DataTemplate>
";


			ParserContext context = new ParserContext();
			context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
			context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
			context.XmlnsDictionary.Add("pg", "http://schemas.denisvuyka.wordpress.com/wpfpropertygrid");
			DataTemplate template = (DataTemplate)XamlReader.Parse(xamlTemplate, context);
			return template;
		}

		public override void ClearValue(PropertyItemValue propertyValue, IInputElement commandSource)
		{
			if (propertyValue == null || propertyValue.IsReadOnly)
			{
				return;
			}
			propertyValue.StringValue = string.Empty;
		}

		public override void ShowDialog(PropertyItemValue propertyValue, IInputElement commandSource)
		{
			PropertyGrid propGrid = commandSource as PropertyGrid;
			string lastPath = propertyValue.StringValue;
			if (propGrid == null) return;

			OpenFileDialog openFileDialog = new OpenFileDialog();
			openFileDialog.Filter = "All files (*.*)|*.*";
			openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			openFileDialog.ShowDialog();

			propertyValue.StringValue = openFileDialog.FileName != String.Empty ? openFileDialog.FileName : lastPath; // change this string and compile, the ui does not see this change
			propGrid.DoReload();
			propGrid.RaiseEvent(new PropertyValueChangedEventArgs(PropertyGrid.PropertyValueChangedEvent, propertyValue.ParentProperty, ""));
		}
	}
}