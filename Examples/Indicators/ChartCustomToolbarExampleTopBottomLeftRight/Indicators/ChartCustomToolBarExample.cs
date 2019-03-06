#region Using declarations
using System;
using System.Windows;
using System.Windows.Media;
using NinjaTrader.Gui.Chart;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ChartCustomToolBarExample : Indicator
	{
		private System.Windows.Media.SolidColorBrush		activeBackgroundDarkGray;
		private System.Windows.Media.SolidColorBrush		backGroundMediumGray;
		private System.Windows.Controls.Grid				chartGrid;
		private NinjaTrader.Gui.Chart.ChartTab				chartTab;
		private NinjaTrader.Gui.Chart.ChartTrader			chartTrader;
		private NinjaTrader.Gui.Chart.Chart					chartWindow;
		private System.Windows.Media.SolidColorBrush		controlLightGray;
		private System.Windows.Controls.Grid				leftInnerGrid;
		private System.Windows.Controls.Menu				leftMenu1;
		private NinjaTrader.Gui.Tools.NTMenuItem			leftMenu1Item1SubItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			leftMenu1Item1SubItem2;
		private System.Windows.Controls.Menu				leftMenu2;
		private System.Windows.Controls.MenuItem			leftMenu2Item1;
		private System.Windows.Controls.Grid				rightInnerGrid;
		private System.Windows.Controls.Menu				rightMenu1;
		private NinjaTrader.Gui.Tools.NTMenuItem			rightMenu1Item1SubItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			rightMenu1Item1SubItem2;
		private System.Windows.Controls.Menu				rightMenu2;
		private System.Windows.Controls.MenuItem			rightMenu2Item1;
		private bool										panelActive;
		private int											tabControlStartColumn;
		private int											tabControlStartRow;
		private int											tabControlEndColumn;
		private int											tabControlEndRow;
		private System.Windows.Controls.TabItem				tabItem;
		private System.Windows.Media.SolidColorBrush		textColor;
		private System.Windows.Controls.Menu				topMenu;
		private NinjaTrader.Gui.Tools.NTMenuItem			topMenuItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			topMenuItem1SubItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			topMenuItem1SubItem2;
		private System.Windows.Controls.MenuItem			topMenuItem2;
		private System.Windows.Controls.Menu				botMenu;
		private NinjaTrader.Gui.Tools.NTMenuItem			botMenuItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			botMenuItem1SubItem1;
		private NinjaTrader.Gui.Tools.NTMenuItem			botMenuItem1SubItem2;
		private System.Windows.Controls.MenuItem			botMenuItem2;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Enter the description for your new custom Indicator here.";
				Name								= "ChartCustomToolBarExample";
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				IsSuspendedWhileInactive			= true;
			}
			else if (State == State.DataLoaded)
			{
				activeBackgroundDarkGray			= new System.Windows.Media.SolidColorBrush(Color.FromRgb(30, 30, 30));
				activeBackgroundDarkGray.Freeze();
				backGroundMediumGray				= new System.Windows.Media.SolidColorBrush(Color.FromRgb(45, 45, 47));
				backGroundMediumGray.Freeze();
				controlLightGray					= new System.Windows.Media.SolidColorBrush(Color.FromRgb(64, 63, 69));
				controlLightGray.Freeze();
				textColor							= new System.Windows.Media.SolidColorBrush(Color.FromRgb(204, 204, 204));
				textColor.Freeze();
			}
			else if (State == State.Historical)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						CreateWPFControls();
					}));
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						DisposeWPFControls();
					}));
				}
			}
		}

		protected void CreateWPFControls()
		{
			chartWindow	= System.Windows.Window.GetWindow(ChartControl.Parent) as NinjaTrader.Gui.Chart.Chart;
			chartGrid	= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;
			chartTrader	= chartWindow.FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader;

			//chartGrid.Background = BackBrush;

			// upper tool bar objects
			// upper tool bar menu
			topMenu = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				BorderBrush			= controlLightGray,
				Padding				= new System.Windows.Thickness(0),
				Margin				= new System.Windows.Thickness(0),
				VerticalAlignment	= VerticalAlignment.Center				
			};

			Style mmiStyle = Application.Current.TryFindResource("MainMenuItem") as Style;

			topMenuItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Menu 1     v",
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(1),
				Style				= mmiStyle,
				VerticalAlignment	= VerticalAlignment.Center
			};

			topMenuItem1SubItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				BorderThickness		= new System.Windows.Thickness(0),
				Foreground			= textColor,
				Header				= "Submenu Item 1"
			};

			topMenuItem1SubItem1.Click += TopMenuItem1SubItem1_Click;
			topMenuItem1.Items.Add(topMenuItem1SubItem1);

			topMenuItem1SubItem2 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Submenu Item 2"
			};
			topMenuItem1SubItem2.Click += TopMenuItem1SubItem2_Click;
			topMenuItem1.Items.Add(topMenuItem1SubItem2);

			topMenu.Items.Add(topMenuItem1);
			
			// upper tool bar button, has text and image
			topMenuItem2 = new System.Windows.Controls.MenuItem()
			{
				Background			= controlLightGray,
				FontSize			= 12,
				Foreground			= textColor,
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(5, 0, 5, 0)				
			};

			// this stackpanel allows us to place text and a picture horizontally in topMenuItem2
			System.Windows.Controls.StackPanel topMenuItem2StackPanel = new System.Windows.Controls.StackPanel()
			{
				Orientation			= System.Windows.Controls.Orientation.Horizontal,
				VerticalAlignment	= VerticalAlignment.Top,
				HorizontalAlignment	= HorizontalAlignment.Right
			};

			System.Windows.Controls.TextBlock newTextBlock = new System.Windows.Controls.TextBlock()
			{
				HorizontalAlignment	= HorizontalAlignment.Right,
				Margin				= new System.Windows.Thickness(0, 0, 2, 0),
				Text				= "B1 ",
				ToolTip				= "Button 1",
				VerticalAlignment	= VerticalAlignment.Top
			};

			topMenuItem2StackPanel.Children.Add(newTextBlock);

			// check to see if an image exists in Documents\bin\Custom\Indicators called B1.png.
			// if its there, include this with the button
			System.Windows.Media.Imaging.BitmapImage buttonImage = new System.Windows.Media.Imaging.BitmapImage();

			try
			{
				buttonImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(NinjaTrader.Core.Globals.UserDataDir + @"bin\Custom\Indicators\B1.png"));
			}
			catch (Exception e) { }

			System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image();

			if (buttonImage != null)
			{
				imageControl = new System.Windows.Controls.Image()
				{
					Source		= buttonImage,
					Height		= 10,
					Width		= 10
				};
			}

			if (buttonImage != null)
				topMenuItem2StackPanel.Children.Add(imageControl);

			topMenuItem2.Header = topMenuItem2StackPanel;
			topMenuItem2.Click += TopMenuItem2_Click;
			topMenu.Items.Add(topMenuItem2);
			
			// bottom toolbar objects
			botMenu = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				BorderBrush			= controlLightGray,
				Padding				= new System.Windows.Thickness(0),
				Margin				= new System.Windows.Thickness(0),
				VerticalAlignment	= VerticalAlignment.Center				
			};

			Style mmiStyleBot = Application.Current.TryFindResource("MainMenuItem") as Style;

			botMenuItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Menu 1     v",
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(1),
				Style				= mmiStyleBot,
				VerticalAlignment	= VerticalAlignment.Center
			};

			botMenuItem1SubItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				BorderThickness		= new System.Windows.Thickness(0),
				Foreground			= textColor,
				Header				= "Submenu Item 1"
			};

			botMenuItem1SubItem1.Click += BotMenuItem1SubItem1_Click;
			botMenuItem1.Items.Add(botMenuItem1SubItem1);

			botMenuItem1SubItem2 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Submenu Item 2"
			};
			botMenuItem1SubItem2.Click += BotMenuItem1SubItem2_Click;
			botMenuItem1.Items.Add(botMenuItem1SubItem2);

			botMenu.Items.Add(botMenuItem1);
			
			// upper tool bar button, has text and image
			botMenuItem2 = new System.Windows.Controls.MenuItem()
			{
				Background			= controlLightGray,
				FontSize			= 12,
				Foreground			= textColor,
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(5, 0, 5, 0)				
			};

			// this stackpanel allows us to place text and a picture horizontally in topMenuItem2
			System.Windows.Controls.StackPanel botMenuItem2StackPanel = new System.Windows.Controls.StackPanel()
			{
				Orientation			= System.Windows.Controls.Orientation.Horizontal,
				VerticalAlignment	= VerticalAlignment.Top,
				HorizontalAlignment	= HorizontalAlignment.Right
			};

			System.Windows.Controls.TextBlock newTextBlockBot = new System.Windows.Controls.TextBlock()
			{
				HorizontalAlignment	= HorizontalAlignment.Right,
				Margin				= new System.Windows.Thickness(0, 0, 2, 0),
				Text				= "B1 ",
				ToolTip				= "Button 1",
				VerticalAlignment	= VerticalAlignment.Top
			};

			botMenuItem2StackPanel.Children.Add(newTextBlockBot);

			// check to see if an image exists in Documents\bin\Custom\Indicators called B1.png.
			// if its there, include this with the button
			buttonImage = new System.Windows.Media.Imaging.BitmapImage();

			try
			{
				buttonImage = new System.Windows.Media.Imaging.BitmapImage(new Uri(NinjaTrader.Core.Globals.UserDataDir + @"bin\Custom\Indicators\B1.png"));
			}
			catch (Exception e) { }

			imageControl = new System.Windows.Controls.Image();

			if (buttonImage != null)
			{
				imageControl = new System.Windows.Controls.Image()
				{
					Source		= buttonImage,
					Height		= 10,
					Width		= 10
				};
			}

			if (buttonImage != null)
				botMenuItem2StackPanel.Children.Add(imageControl);

			botMenuItem2.Header = botMenuItem2StackPanel;
			botMenuItem2.Click += BotMenuItem2_Click;
			botMenu.Items.Add(botMenuItem2);

			// left toolbar objects
			// each vertical object needs its own menu
			leftInnerGrid = new System.Windows.Controls.Grid();

			leftInnerGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			leftInnerGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

			leftInnerGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			leftInnerGrid.RowDefinitions[1].Height = new GridLength(30);

			leftMenu1 = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0)
			};

			leftMenu2 = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0)
			};

			// this allows us to make our menus stack vertically
			System.Windows.Controls.VirtualizingStackPanel VerticalStackPanel = new System.Windows.Controls.VirtualizingStackPanel()
			{
				Background			= activeBackgroundDarkGray,
				HorizontalAlignment	= HorizontalAlignment.Stretch,
				Orientation			= System.Windows.Controls.Orientation.Vertical,
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			NinjaTrader.Gui.Tools.NTMenuItem leftMenu1Item1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "M2 v",
				HorizontalAlignment	= HorizontalAlignment.Stretch,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0),/*,
				Style				= Application.Current.TryFindResource("MainMenuItem") as Style*/
				ToolTip				= "Menu 2",
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			leftMenu1Item1SubItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				BorderThickness		= new System.Windows.Thickness(0),
				Foreground			= textColor,
				Header				= "Submenu Item 1"
			};

			leftMenu1Item1SubItem1.Click += LeftMenu1Item1SubItem1_Click;
			leftMenu1Item1.Items.Add(leftMenu1Item1SubItem1);

			NinjaTrader.Gui.Tools.NTMenuItem leftMenu1Item1SubItem2 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Submenu Item 2"
			};

			leftMenu1Item1SubItem2.Click += LeftMenu1Item1SubItem2_Click;
			leftMenu1Item1.Items.Add(leftMenu1Item1SubItem2);

			leftMenu1.Items.Add(leftMenu1Item1);
			VerticalStackPanel.Children.Add(leftMenu1);

			leftMenu2Item1 = new System.Windows.Controls.MenuItem()
			{
				Background			= controlLightGray,
				FontSize			= 12,
				Foreground			= textColor,
				Header				= "B2",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0),
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			leftMenu2Item1.Click += LeftMenu2Item1_Click;
			leftMenu2.Items.Add(leftMenu2Item1);

			VerticalStackPanel.Children.Add(leftMenu2);
			leftInnerGrid.Children.Add(VerticalStackPanel);
			
			// right panel
			rightInnerGrid = new System.Windows.Controls.Grid();

			rightInnerGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			rightInnerGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);

			rightInnerGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition());
			rightInnerGrid.RowDefinitions[1].Height = new GridLength(30);

			rightMenu1 = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0)
			};

			rightMenu2 = new System.Windows.Controls.Menu()
			{
				Background			= activeBackgroundDarkGray,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0)
			};

			// this allows us to make our menus stack vertically
			System.Windows.Controls.VirtualizingStackPanel VerticalStackPanelRight = new System.Windows.Controls.VirtualizingStackPanel()
			{
				Background			= activeBackgroundDarkGray,
				HorizontalAlignment	= HorizontalAlignment.Stretch,
				Orientation			= System.Windows.Controls.Orientation.Vertical,
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			NinjaTrader.Gui.Tools.NTMenuItem rightMenu1Item1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "M2 v",
				HorizontalAlignment	= HorizontalAlignment.Stretch,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0),/*,
				Style				= Application.Current.TryFindResource("MainMenuItem") as Style*/
				ToolTip				= "Menu 2",
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			rightMenu1Item1SubItem1 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				BorderThickness		= new System.Windows.Thickness(0),
				Foreground			= textColor,
				Header				= "Submenu Item 1"
			};

			rightMenu1Item1SubItem1.Click += RightMenu1Item1SubItem1_Click;
			rightMenu1Item1.Items.Add(rightMenu1Item1SubItem1);

			NinjaTrader.Gui.Tools.NTMenuItem rightMenu1Item1SubItem2 = new Gui.Tools.NTMenuItem()
			{
				Background			= controlLightGray,
				Foreground			= textColor,
				Header				= "Submenu Item 2"
			};

			rightMenu1Item1SubItem2.Click += RightMenu1Item1SubItem2_Click;
			rightMenu1Item1.Items.Add(rightMenu1Item1SubItem2);

			rightMenu1.Items.Add(rightMenu1Item1);
			VerticalStackPanelRight.Children.Add(rightMenu1);

			rightMenu2Item1 = new System.Windows.Controls.MenuItem()
			{
				Background			= controlLightGray,
				FontSize			= 12,
				Foreground			= textColor,
				Header				= "B2",
				HorizontalAlignment = HorizontalAlignment.Stretch,
				Margin				= new System.Windows.Thickness(0),
				Padding				= new System.Windows.Thickness(0),
				VerticalAlignment	= VerticalAlignment.Stretch
			};

			rightMenu2Item1.Click += RightMenu2Item1_Click;
			rightMenu2.Items.Add(rightMenu2Item1);

			VerticalStackPanelRight.Children.Add(rightMenu2);
			rightInnerGrid.Children.Add(VerticalStackPanelRight);
			
			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void DisposeWPFControls()
		{
			if (topMenuItem1SubItem1 != null)
				topMenuItem1SubItem1.Click -= TopMenuItem1SubItem1_Click;

			if (topMenuItem1SubItem2 != null)
				topMenuItem1SubItem2.Click -= TopMenuItem1SubItem2_Click;

			if (topMenuItem2 != null)
				topMenuItem2.Click -= TopMenuItem2_Click;
			
			if (botMenuItem1SubItem1 != null)
				botMenuItem1SubItem1.Click -= BotMenuItem1SubItem1_Click;

			if (botMenuItem1SubItem2 != null)
				botMenuItem1SubItem2.Click -= BotMenuItem1SubItem2_Click;

			if (botMenuItem2 != null)
				botMenuItem2.Click -= BotMenuItem2_Click;

			if (leftMenu1Item1SubItem1 != null)
				leftMenu1Item1SubItem1.Click -= LeftMenu1Item1SubItem1_Click;

			if (leftMenu1Item1SubItem2 != null)
				leftMenu1Item1SubItem2.Click -= LeftMenu1Item1SubItem2_Click;

			if (leftMenu2Item1 != null)
				leftMenu2Item1.Click -= LeftMenu2Item1_Click;
			
			if (rightMenu1Item1SubItem1 != null)
				rightMenu1Item1SubItem1.Click -= RightMenu1Item1SubItem1_Click;

			if (rightMenu1Item1SubItem2 != null)
				rightMenu1Item1SubItem2.Click -= RightMenu1Item1SubItem2_Click;

			if (rightMenu2Item1 != null)
				rightMenu2Item1.Click -= RightMenu2Item1_Click;

			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			RemoveWPFControls();
		}

		protected override void OnBarUpdate() { }

		protected void InsertWPFControls()
		{
			if (panelActive)
				return;

			if (chartGrid.RowDefinitions.Count == 0)
				chartGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

			tabControlStartColumn	= System.Windows.Controls.Grid.GetColumn(chartWindow.MainTabControl);
			tabControlStartRow		= System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl);

			chartGrid.ColumnDefinitions.Insert(tabControlStartColumn, new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(30) });
			chartGrid.RowDefinitions.Insert(tabControlStartRow, new System.Windows.Controls.RowDefinition() { Height = new GridLength(20) });

			// including the chartTabControl move all items right of the chart and below the chart to the right one column and down one row
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) >= tabControlStartColumn)
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) + 1);

				if (System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) >= tabControlStartRow)
					System.Windows.Controls.Grid.SetRow(chartGrid.Children[i], System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) + 1);
				
				if(System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) >= tabControlEndColumn)
					tabControlEndColumn = System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]);
				
				if(System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) >= tabControlEndRow)
					tabControlEndRow = System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]);
			}
			
			// Now that grids have moved, check Chart Trader position to properly add a right panel
			int chartTraderStartColumn = System.Windows.Controls.Grid.GetColumn(chartTrader);
			int chartTraderStartRow = System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl);
			
			// Add our Column on the right
			chartGrid.ColumnDefinitions.Insert(chartTraderStartColumn + 2, new System.Windows.Controls.ColumnDefinition() { Width = new GridLength(30) });
			
			// Shift all items to the right of Chart Trader
			for (int i = 0; i < chartGrid.Children.Count; i++)
				if (System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > chartTraderStartColumn)
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) + 1);
			
			// Finally, add the bottom row.
			chartGrid.RowDefinitions.Insert(tabControlEndRow+1, new System.Windows.Controls.RowDefinition() { Height = new GridLength(20) });
			

			// set the columns and rows for our new items
			System.Windows.Controls.Grid.SetColumn(topMenu, System.Windows.Controls.Grid.GetColumn(chartWindow.MainTabControl));
			System.Windows.Controls.Grid.SetRow(topMenu, tabControlStartRow);

			System.Windows.Controls.Grid.SetColumn(leftInnerGrid, tabControlStartColumn);
			System.Windows.Controls.Grid.SetRow(leftInnerGrid, System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl));
			
			System.Windows.Controls.Grid.SetColumn(botMenu, System.Windows.Controls.Grid.GetColumn(chartWindow.MainTabControl));
			System.Windows.Controls.Grid.SetRow(botMenu, tabControlEndRow+1);

			System.Windows.Controls.Grid.SetColumn(rightInnerGrid, System.Windows.Controls.Grid.GetColumn(chartTrader) + 2);
			System.Windows.Controls.Grid.SetRow(rightInnerGrid, System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl));
			
			chartGrid.Children.Add(topMenu);
			chartGrid.Children.Add(leftInnerGrid);
				
			chartGrid.Children.Add(botMenu);
			chartGrid.Children.Add(rightInnerGrid);

			// let the script know the panel is active
			panelActive = true;
		}

		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;
			
			// Work backwards, remove the rows/columns that were added last
			
			// Remove Bottom
			if (botMenu != null)
			{
				chartGrid.RowDefinitions.RemoveAt(System.Windows.Controls.Grid.GetRow(botMenu));
				chartGrid.Children.Remove(botMenu);
			}
			
			int tabControlRow = System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl);
			Print(tabControlRow);
			
			for (int i = 0; i < chartGrid.Children.Count; i++)
				if (System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > tabControlRow + 1)
					System.Windows.Controls.Grid.SetRow(chartGrid.Children[i], System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) - 1);
			
			// Remove Right
			if (rightInnerGrid != null)
			{
				chartGrid.ColumnDefinitions.RemoveAt(System.Windows.Controls.Grid.GetColumn(rightInnerGrid));
				chartGrid.Children.Remove(rightInnerGrid);
			}
			
			int chartTraderStartColumn = System.Windows.Controls.Grid.GetColumn(chartTrader);
			
			for (int i = 0; i < chartGrid.Children.Count; i++)
				if (System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > chartTraderStartColumn + 2)
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) - 1);

			// Remove Top and Left
			if (leftInnerGrid != null)
			{
				chartGrid.ColumnDefinitions.RemoveAt(System.Windows.Controls.Grid.GetColumn(leftInnerGrid));
				chartGrid.Children.Remove(leftInnerGrid);
			}

			if (topMenu != null)
			{
				chartGrid.RowDefinitions.RemoveAt(System.Windows.Controls.Grid.GetRow(topMenu));
				chartGrid.Children.Remove(topMenu);
			}			
			
			// if the childs column is 1 (so we can move it to 0) and the column is to the right of the column we are removing, shift it left
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) > System.Windows.Controls.Grid.GetColumn(leftInnerGrid))
					System.Windows.Controls.Grid.SetColumn(chartGrid.Children[i], System.Windows.Controls.Grid.GetColumn(chartGrid.Children[i]) - 1);

				if (System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > System.Windows.Controls.Grid.GetRow(topMenu))
					System.Windows.Controls.Grid.SetRow(chartGrid.Children[i], System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) - 1);
			}

			panelActive = false;
		}

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}

		protected void TopMenuItem1SubItem1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M1I1 - Top menu subitem 1 selected", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void TopMenuItem1SubItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M1I2 - Top menu subitem 2 selected", TextPosition.BottomLeft, Brushes.ForestGreen, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void TopMenuItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "B1 - Top button clicked", TextPosition.BottomLeft, Brushes.OrangeRed, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// only invalidate the chart so that the text box will appear even if there is no incoming data
			ChartControl.InvalidateVisual();
		}
		
		protected void BotMenuItem1SubItem1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M1I1 - Bottom menu subitem 1 selected", TextPosition.BottomLeft, Brushes.Green, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void BotMenuItem1SubItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M1I2 - Bottom menu subitem 2 selected", TextPosition.BottomLeft, Brushes.ForestGreen, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void BotMenuItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "B1 - Bottom button clicked", TextPosition.BottomLeft, Brushes.OrangeRed, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			// only invalidate the chart so that the text box will appear even if there is no incoming data
			ChartControl.InvalidateVisual();
		}
		
		protected void LeftMenu1Item1SubItem1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M2I1 - Left menu subitem 1 selected", TextPosition.BottomLeft, Brushes.DarkMagenta, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void LeftMenu1Item1SubItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M2I2 - Left menu subitem 2 selected", TextPosition.BottomLeft, Brushes.DarkOrchid, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void LeftMenu2Item1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "B2 - Left button clicked", TextPosition.BottomLeft, Brushes.MediumTurquoise, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}
		
		protected void RightMenu1Item1SubItem1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M2I1 - Right menu subitem 1 selected", TextPosition.BottomLeft, Brushes.DarkMagenta, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void RightMenu1Item1SubItem2_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "M2I2 - Right menu subitem 2 selected", TextPosition.BottomLeft, Brushes.DarkOrchid, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}

		protected void RightMenu2Item1_Click(object sender, RoutedEventArgs e)
		{
			Draw.TextFixed(this, "infobox", "B2 - Right button clicked", TextPosition.BottomLeft, Brushes.MediumTurquoise, new Gui.Tools.SimpleFont("Arial", 25), Brushes.Transparent, Brushes.Transparent, 100);
			ChartControl.InvalidateVisual();
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChartCustomToolBarExample[] cacheChartCustomToolBarExample;
		public ChartCustomToolBarExample ChartCustomToolBarExample()
		{
			return ChartCustomToolBarExample(Input);
		}

		public ChartCustomToolBarExample ChartCustomToolBarExample(ISeries<double> input)
		{
			if (cacheChartCustomToolBarExample != null)
				for (int idx = 0; idx < cacheChartCustomToolBarExample.Length; idx++)
					if (cacheChartCustomToolBarExample[idx] != null &&  cacheChartCustomToolBarExample[idx].EqualsInput(input))
						return cacheChartCustomToolBarExample[idx];
			return CacheIndicator<ChartCustomToolBarExample>(new ChartCustomToolBarExample(), input, ref cacheChartCustomToolBarExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChartCustomToolBarExample ChartCustomToolBarExample()
		{
			return indicator.ChartCustomToolBarExample(Input);
		}

		public Indicators.ChartCustomToolBarExample ChartCustomToolBarExample(ISeries<double> input )
		{
			return indicator.ChartCustomToolBarExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChartCustomToolBarExample ChartCustomToolBarExample()
		{
			return indicator.ChartCustomToolBarExample(Input);
		}

		public Indicators.ChartCustomToolBarExample ChartCustomToolBarExample(ISeries<double> input )
		{
			return indicator.ChartCustomToolBarExample(input);
		}
	}
}

#endregion
