#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

using NinjaTrader.NinjaScript.AddOns; // Our DXMediaBrush and DXHelper classes reside in the AddOn namespace.

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	[CategoryDefaultExpanded(false)]
	public class SharpDXHelperExample : Indicator
	{	
		private DXMediaBrush DXMBrush;
		private DXHelper DXH;
		
		// Create Series<bool> to tell if we want to draw something for a particular bar
		private Series<bool> DrawSeries;
		
		// Hide parameters on the Indicator Label
		public override string DisplayName
		{ get { return Name; } } 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Demonstration script using the SharpDXHelper class for managed custom rendering";
				Name										= "SharpDXHelperExample";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				// Set defaults for our properties, can be assigned to our DX/Media Brush in states from State.DataLoaded to State.Historical
				LineBrush									= Brushes.RoyalBlue;				
				LineBrushWidth								= 2;
				LineBrushOpacity							= 50.0;
				
				RectangleBrush								= Brushes.LightGreen;
				RectangleBorderBrush						= Brushes.Green;
				RectangleBrushWidth							= 5;
				RectangleBrushOpacity						= 10.0;
				
				RoundedRectangleBrush						= Brushes.DarkGreen;
				RoundedRectangleBorderBrush					= Brushes.LimeGreen;
				RoundedRectangleBrushWidth					= 5;
				RoundedRectangleBrushOpacity				= 10.0;
				
				EllipseBrush								= Brushes.DarkRed;
				EllipseBrushWidth							= 5;
				EllipseBrushOpacity							= 40.0;
				
				string image1Path							= System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "SharpDXHelper.png");
				Image1Path									= System.IO.File.Exists(image1Path) ? image1Path : String.Empty;
				Image1Opacity								= 30.0f;
				
				string image2Path							= System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "SharpDXHelper2.png");
				Image2Path									= System.IO.File.Exists(image2Path) ? image2Path : String.Empty;
				Image2Opacity								= 20.0f;
			}
			else if (State == State.DataLoaded)
			{
				// Create a Series<bool> to tell if we want to draw something for a particular bar, must have MaximumBarsLookBack.Infinite.
				// This is more efficient than calculating what should be rendered during OnRender(), where we should only worry about how things should be rendered.
				DrawSeries = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
				// Create a list of brush names for us to manage with  the helper class' "DXMediaBrush" Dictionary
				string[] brushes = new string[] { "LineBrush" , "RectBrush" , "RectBorderBrush" , "RoundedRectBrush" , "RoundedRectBorderBrush" , "EllipseBrush" };
				
				// If we use resourses managed by the helper class (bitmaps and internally managed brushes created from Windows Media Brush references)
				// and want to use a timer to clear brushes after a certain period of time, ChartControl must be used with the constructor for the resource timer and cannot be null
				if (ChartControl != null)
				{
					// Create new instance of the Helper Class
					// We are using managed brushes in this example, and have a 5 minute refresh period
					// minutesToRefersh values of 0 or less will not create a timer for brush disposal
					// This is useful for cleaning up programmatically created brushes that may not always be reused thorught a NinjaScript's life.
					DXH = new DXHelper(true, ChartControl, 0);
					
					// Add the brushes we defined in our brushes list
					DXH.AddBrushes(brushes);
					
					// Set our brushes to the values we would like
					DXH.UpdateBrush(RenderTarget, "LineBrush", 				LineBrush, 						LineBrushOpacity);
					DXH.UpdateBrush(RenderTarget, "RectBrush", 				RectangleBrush, 				RectangleBrushOpacity);
					DXH.UpdateBrush(RenderTarget, "RectBorderBrush", 		RectangleBorderBrush, 			RectangleBrushOpacity);
					DXH.UpdateBrush(RenderTarget, "RoundedRectBrush", 		RoundedRectangleBrush, 			RoundedRectangleBrushOpacity);
					DXH.UpdateBrush(RenderTarget, "RoundedRectBorderBrush", RoundedRectangleBorderBrush, 	RoundedRectangleBrushOpacity);
					DXH.UpdateBrush(RenderTarget, "EllipseBrush",			EllipseBrush, 					EllipseBrushOpacity);
				}
				
				// Create a DXMediaBrush
				DXMBrush = new DXMediaBrush();
				DXMBrush.UpdateBrush(RenderTarget, LineBrush, LineBrushOpacity);
				
				// Apply Opacity to LineBrush (used for User Defined Media Brush Demonstration. See overlay line 5.)
				LineBrush 			= LineBrush.Clone();
				LineBrush.Opacity 	= LineBrushOpacity / 100.0;
            	LineBrush.Freeze();
			}
			else if (State == State.Terminated)
			{
				// Dispose of the resources created by our helper class.
				if(DXH != null)
					DXH.Dispose();
			}
		}

		protected override void OnBarUpdate()
		{
			// Some condition that we would want to signal that we want to draw for that bar.
			if(Close[0] > Open[0])
				DrawSeries[0] = true;
			else
				DrawSeries[0] = false;		
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			bool overlay = false;
			// Call base.OnRender() to ensure plots get rendered appropriately.
			base.OnRender(chartControl, chartScale);
			
            if (Bars == null || ChartControl == null)
                return;

			// Set AntiAlias mode for smoothing
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Check if we are in hit test for chartScale lines to ignore mouse clicks on these drawings
			if (!IsInHitTest)
			{
				double top 		= Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY(ChartPanel.Y));
				double bot 		= Instrument.MasterInstrument.RoundToTickSize(chartScale.GetValueByY(ChartPanel.H));
				double lastPos 	= chartScale.GetYByValue(bot-2*TickSize);
				int j = 0;
				
				// Loop through each tick level to draw lines with alternating DashStyles
				for (double i = bot-TickSize; i <= top; i+= TickSize)
				{
					// This check makes sure lines have a distance greater than 1 pixel.
					if (lastPos - chartScale.GetYByValue(i) > 1)
					{
						// Iterate our DashStyle type
						++j;
						
						// Draw our alternating tick lines
						DXH.DrawLine(RenderTarget,
									"LineBrush",
									ChartPanel.X,
									chartScale.GetYByValue(i),
									ChartPanel.W,
									chartScale.GetYByValue(i),
									LineBrushWidth,
									(DashStyleHelper)(j%4));
						
						lastPos = chartScale.GetYByValue(i);
					}	
				}
			}
			
			// Limit Custom Rendering to ChartBars.FromIndex and ChartBars.ToIndex
			// Each index can be used for rendering per bar
            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
				// Reference Series objects with GetValueAt(idx) for each up bar marked in our "DrawSeries".
				// It is best to determine what should be rendered outside of OnRender() as much as possible to reduce repeated calculations in OnRender().
				if (DrawSeries.GetValueAt(idx))
				{
					// Set a bool for overlay drawing to be done last
					overlay = true;
					
					//  DXMBrush Dictionary Reference for up bars
					DXH.DrawLine(RenderTarget,
								"LineBrush",
								ChartControl.GetXByBarIndex(ChartBars, idx)-0.5,
								chartScale.GetYByValue(Close.GetValueAt(idx))-0.5,
								ChartControl.GetXByBarIndex(ChartBars, idx)-0.5,
								chartScale.GetYByValue(Open.GetValueAt(idx))-0.5,
								(float)ChartControl.BarWidth*2,
								DashStyleHelper.Solid);
				}
				
				// Demonstrate DrawText using DXMediaBrush
				DXH.DrawText(		RenderTarget,
									idx.ToString(),
									new SimpleFont("Arial", 10),
									chartControl.GetXByBarIndex(ChartBars, idx),
									chartScale.GetYByValue(High.GetValueAt(idx) + 3 * TickSize),
									idx.ToString().Length*10,
									10,
									DXMBrush);
				
				// Demonstrate DrawText Layout using DXMediaBrush
				DXH.DrawTextLayout(	RenderTarget,
									ChartPanel,
									chartControl.GetXByBarIndex(ChartBars, idx),
									chartScale.GetYByValue(Low.GetValueAt(idx) - 3 * TickSize),
									idx.ToString(),
									new SimpleFont("Arial", 10),
									DXMBrush);
				
				// Demonstrate FillEllipse using DXMediaBrush Dictionary reference
				DXH.FillEllipse(	RenderTarget, 
									chartControl.GetXByBarIndex(ChartBars, idx),
									chartScale.GetYByValue((High.GetValueAt(idx) + 5 * TickSize)),
									(float)chartControl.BarWidth*2,
									(float)chartControl.BarWidth*2,
									"EllipseBrush");
				
				// Demonstrate DrawEllipse using DXMediaBrush Dictionary reference
				DXH.DrawEllipse(	RenderTarget,
									chartControl.GetXByBarIndex(ChartBars, idx),
									chartScale.GetYByValue(Low.GetValueAt(idx) - 5 * TickSize),
									(float)chartControl.BarWidth*2,
									(float)chartControl.BarWidth*2,
									"EllipseBrush",
									2,
									DashStyleHelper.Dash);				
			}
			
			// Draw items that should be overlayed once after all other drawing so the overlay remains on top and ignore mouse clicks
			if (overlay && !IsInHitTest)
			{
				// Draw some images
				DXH.DrawBitmap(RenderTarget, Image1Path, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, Image1Opacity);
				DXH.DrawBitmap(RenderTarget, Image2Path, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, Image2Opacity);
				
				// Draw a filled rectangle and a rectangle border
				DXH.FillRectangle(RenderTarget, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, "RectBrush");
				DXH.DrawRectangle(RenderTarget, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, "RectBorderBrush", 10, DashStyleHelper.Dash);
				
				// Draw a filled rounded rectangle and a rounded rectangle border
				DXH.FillRoundedRectangle(RenderTarget, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, ChartPanel.W/2, ChartPanel.H/2, "RoundedRectBrush");
				DXH.DrawRoundedRectangle(RenderTarget, ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H, ChartPanel.W/2, ChartPanel.H/2, "RoundedRectBorderBrush", 10, DashStyleHelper.Dash);
				
				// Line 1 DXMediaBrush Dictionary Reference
				DXH.DrawLine(RenderTarget,
							"LineBrush",
							ChartPanel.X,
							ChartPanel.Y,
							ChartPanel.W,
							ChartPanel.H,
							LineBrushWidth,
							DashStyleHelper.Solid);
				
				// Line 2 DXMediaBrush reference
				DXH.DrawLine(RenderTarget,
							DXMBrush,
							ChartPanel.X - 20,
							ChartPanel.Y,
							ChartPanel.W - 20,
							ChartPanel.H,
							LineBrushWidth,
							DashStyleHelper.Dash);
				
				// Line 3 DXMediaBrush.DxBrush reference from DXMediaBrush
				DXH.DrawLine(RenderTarget,
							DXMBrush.DxBrush,
							ChartPanel.X - 40,
							ChartPanel.Y,
							ChartPanel.W - 40,
							ChartPanel.H,
							LineBrushWidth,
							DashStyleHelper.DashDot);
				
				// Line 4 Windows Media Brush reference from DXMediaBrush
				DXH.DrawLine(RenderTarget,
							DXMBrush.MediaBrush,
							ChartPanel.X - 60,
							ChartPanel.Y,
							ChartPanel.W - 60,
							ChartPanel.H,
							LineBrushWidth,
							DashStyleHelper.DashDotDot);
				
				// Line 5 Windows Media Brush reference from User Defined Brush (Media Brushes must be cloned and refrozen for opacity to have effect)
				DXH.DrawLine(RenderTarget,
							LineBrush,
							ChartPanel.X - 80,
							ChartPanel.Y,
							ChartPanel.W - 80,
							ChartPanel.H,
							LineBrushWidth,
							DashStyleHelper.Solid);
			}
			
			//Reset AntiAlias mode
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}
		
		public override void OnRenderTargetChanged()
        {
			// Dispose and recreate our SharpDX Brushes or other device dependant resources on RenderTarget changes
			// Catch if RenderTarget is null
			if (RenderTarget == null)
				return;
			
			// Have the helper class dispose/recreate new SharpDX brushes
			if (DXH != null)
				DXH.RenderTargetChange(RenderTarget);
		
			// Any DXMediaBrush that we create on our own will need to be handled here.
			if (DXMBrush != null)
				DXMBrush.RenderTargetChange(RenderTarget);
        }

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Brush", Description="Color of Up bars and lines", Order=1, GroupName="1. Lines")]
		public Brush LineBrush
		{ get; set; }
		
		[Browsable(false)]
		public string UserDefinedBrushSerializable
		{
			get { return Serialize.BrushToString(LineBrush); }
			set { LineBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Brush Width", Description="Width of lines", Order=2, GroupName="1. Lines")]
		public int LineBrushWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Brush Opacity", Description="Opacity of lines", Order=3, GroupName="1. Lines")]
		public double LineBrushOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Brush", Description="Color of Rectangle", Order=4, GroupName="2. Rectangle")]
		public Brush RectangleBrush
		{ get; set; }
		
		[Browsable(false)]
		public string RectangleBrushSerializable
		{
			get { return Serialize.BrushToString(RectangleBrush); }
			set { RectangleBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Border Brush", Description="Color of Rectangle Border", Order=5, GroupName="2. Rectangle")]
		public Brush RectangleBorderBrush
		{ get; set; }
		
		[Browsable(false)]
		public string RectangleBorderBrushSerializable
		{
			get { return Serialize.BrushToString(RectangleBorderBrush); }
			set { RectangleBorderBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Brush Width", Description="Width of Rectangle Border", Order=6, GroupName="2. Rectangle")]
		public int RectangleBrushWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Brush Opacity", Description="Opacity of Rectanlge", Order=7, GroupName="2. Rectangle")]
		public double RectangleBrushOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Brush", Description="Color of Rounded Rectangle", Order=8, GroupName="3. Rounded Rectangle")]
		public Brush RoundedRectangleBrush
		{ get; set; }
		
		[Browsable(false)]
		public string RoundedRectangleBrushSerializable
		{
			get { return Serialize.BrushToString(RoundedRectangleBrush); }
			set { RoundedRectangleBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Border Brush", Description="Color of Rounded Rectangle Border", Order=9, GroupName="3. Rounded Rectangle")]
		public Brush RoundedRectangleBorderBrush
		{ get; set; }
		
		[Browsable(false)]
		public string RoundedRectangleBorderBrushSerializable
		{
			get { return Serialize.BrushToString(RoundedRectangleBorderBrush); }
			set { RoundedRectangleBorderBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Brush Width", Description="Width of Rounded Rectangle Border", Order=10, GroupName="3. Rounded Rectangle")]
		public int RoundedRectangleBrushWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Brush Opacity", Description="Opacity of Rectanlge", Order=11, GroupName="3. Rounded Rectangle")]
		public double RoundedRectangleBrushOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Brush", Description="Color of Ellipses", Order=12, GroupName="4. Ellipse")]
		public Brush EllipseBrush
		{ get; set; }
		
		[Browsable(false)]
		public string EllipseBrushSerializable
		{
			get { return Serialize.BrushToString(EllipseBrush); }
			set { EllipseBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Brush Width", Description="Width of Ellipses", Order=13, GroupName="4. Ellipse")]
		public int EllipseBrushWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Brush Opacity", Description="Opacity of Ellipses", Order=14, GroupName="4. Ellipse")]
		public double EllipseBrushOpacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.NinjaScript.AddOns.CustomFilePicker")]
		[Display(Name="Image1 File Path", Description="File Path for Image1", Order=15, GroupName="5. Images")]
		public string Image1Path
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Image1 Opacity", Description="Opacity of Image1", Order=16, GroupName="5. Images")]
		public float Image1Opacity
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.NinjaScript.AddOns.CustomFilePicker")]
		[Display(Name="Image2 File Path", Description="File Path for Image2", Order=17, GroupName="5. Images")]
		public string Image2Path
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="Image2 Opacity", Description="Opacity of Image2", Order=18, GroupName="5. Images")]
		public float Image2Opacity
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SharpDXHelperExample[] cacheSharpDXHelperExample;
		public SharpDXHelperExample SharpDXHelperExample(Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			return SharpDXHelperExample(Input, lineBrush, lineBrushWidth, lineBrushOpacity, rectangleBrush, rectangleBorderBrush, rectangleBrushWidth, rectangleBrushOpacity, roundedRectangleBrush, roundedRectangleBorderBrush, roundedRectangleBrushWidth, roundedRectangleBrushOpacity, ellipseBrush, ellipseBrushWidth, ellipseBrushOpacity, image1Path, image1Opacity, image2Path, image2Opacity);
		}

		public SharpDXHelperExample SharpDXHelperExample(ISeries<double> input, Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			if (cacheSharpDXHelperExample != null)
				for (int idx = 0; idx < cacheSharpDXHelperExample.Length; idx++)
					if (cacheSharpDXHelperExample[idx] != null && cacheSharpDXHelperExample[idx].LineBrush == lineBrush && cacheSharpDXHelperExample[idx].LineBrushWidth == lineBrushWidth && cacheSharpDXHelperExample[idx].LineBrushOpacity == lineBrushOpacity && cacheSharpDXHelperExample[idx].RectangleBrush == rectangleBrush && cacheSharpDXHelperExample[idx].RectangleBorderBrush == rectangleBorderBrush && cacheSharpDXHelperExample[idx].RectangleBrushWidth == rectangleBrushWidth && cacheSharpDXHelperExample[idx].RectangleBrushOpacity == rectangleBrushOpacity && cacheSharpDXHelperExample[idx].RoundedRectangleBrush == roundedRectangleBrush && cacheSharpDXHelperExample[idx].RoundedRectangleBorderBrush == roundedRectangleBorderBrush && cacheSharpDXHelperExample[idx].RoundedRectangleBrushWidth == roundedRectangleBrushWidth && cacheSharpDXHelperExample[idx].RoundedRectangleBrushOpacity == roundedRectangleBrushOpacity && cacheSharpDXHelperExample[idx].EllipseBrush == ellipseBrush && cacheSharpDXHelperExample[idx].EllipseBrushWidth == ellipseBrushWidth && cacheSharpDXHelperExample[idx].EllipseBrushOpacity == ellipseBrushOpacity && cacheSharpDXHelperExample[idx].Image1Path == image1Path && cacheSharpDXHelperExample[idx].Image1Opacity == image1Opacity && cacheSharpDXHelperExample[idx].Image2Path == image2Path && cacheSharpDXHelperExample[idx].Image2Opacity == image2Opacity && cacheSharpDXHelperExample[idx].EqualsInput(input))
						return cacheSharpDXHelperExample[idx];
			return CacheIndicator<SharpDXHelperExample>(new SharpDXHelperExample(){ LineBrush = lineBrush, LineBrushWidth = lineBrushWidth, LineBrushOpacity = lineBrushOpacity, RectangleBrush = rectangleBrush, RectangleBorderBrush = rectangleBorderBrush, RectangleBrushWidth = rectangleBrushWidth, RectangleBrushOpacity = rectangleBrushOpacity, RoundedRectangleBrush = roundedRectangleBrush, RoundedRectangleBorderBrush = roundedRectangleBorderBrush, RoundedRectangleBrushWidth = roundedRectangleBrushWidth, RoundedRectangleBrushOpacity = roundedRectangleBrushOpacity, EllipseBrush = ellipseBrush, EllipseBrushWidth = ellipseBrushWidth, EllipseBrushOpacity = ellipseBrushOpacity, Image1Path = image1Path, Image1Opacity = image1Opacity, Image2Path = image2Path, Image2Opacity = image2Opacity }, input, ref cacheSharpDXHelperExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SharpDXHelperExample SharpDXHelperExample(Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			return indicator.SharpDXHelperExample(Input, lineBrush, lineBrushWidth, lineBrushOpacity, rectangleBrush, rectangleBorderBrush, rectangleBrushWidth, rectangleBrushOpacity, roundedRectangleBrush, roundedRectangleBorderBrush, roundedRectangleBrushWidth, roundedRectangleBrushOpacity, ellipseBrush, ellipseBrushWidth, ellipseBrushOpacity, image1Path, image1Opacity, image2Path, image2Opacity);
		}

		public Indicators.SharpDXHelperExample SharpDXHelperExample(ISeries<double> input , Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			return indicator.SharpDXHelperExample(input, lineBrush, lineBrushWidth, lineBrushOpacity, rectangleBrush, rectangleBorderBrush, rectangleBrushWidth, rectangleBrushOpacity, roundedRectangleBrush, roundedRectangleBorderBrush, roundedRectangleBrushWidth, roundedRectangleBrushOpacity, ellipseBrush, ellipseBrushWidth, ellipseBrushOpacity, image1Path, image1Opacity, image2Path, image2Opacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SharpDXHelperExample SharpDXHelperExample(Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			return indicator.SharpDXHelperExample(Input, lineBrush, lineBrushWidth, lineBrushOpacity, rectangleBrush, rectangleBorderBrush, rectangleBrushWidth, rectangleBrushOpacity, roundedRectangleBrush, roundedRectangleBorderBrush, roundedRectangleBrushWidth, roundedRectangleBrushOpacity, ellipseBrush, ellipseBrushWidth, ellipseBrushOpacity, image1Path, image1Opacity, image2Path, image2Opacity);
		}

		public Indicators.SharpDXHelperExample SharpDXHelperExample(ISeries<double> input , Brush lineBrush, int lineBrushWidth, double lineBrushOpacity, Brush rectangleBrush, Brush rectangleBorderBrush, int rectangleBrushWidth, double rectangleBrushOpacity, Brush roundedRectangleBrush, Brush roundedRectangleBorderBrush, int roundedRectangleBrushWidth, double roundedRectangleBrushOpacity, Brush ellipseBrush, int ellipseBrushWidth, double ellipseBrushOpacity, string image1Path, float image1Opacity, string image2Path, float image2Opacity)
		{
			return indicator.SharpDXHelperExample(input, lineBrush, lineBrushWidth, lineBrushOpacity, rectangleBrush, rectangleBorderBrush, rectangleBrushWidth, rectangleBrushOpacity, roundedRectangleBrush, roundedRectangleBorderBrush, roundedRectangleBrushWidth, roundedRectangleBrushOpacity, ellipseBrush, ellipseBrushWidth, ellipseBrushOpacity, image1Path, image1Opacity, image2Path, image2Opacity);
		}
	}
}

#endregion
