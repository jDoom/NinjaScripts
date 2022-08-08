//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim
//
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

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.CustomRenderingExamples
{
	public class OnRenderDrawObjectsExample : Indicator
	{
		// Dictionary for keeping tyrack of DX brushes made from user defined brushes.
		private Dictionary<string, DXMediaMap> dxmBrushes;
		
		// Create a private Media Brush and have the getter/setter return the DXMediaMap Media Brush and update the Media Brush and DX Brush
        private Brush BrushToUse
        {
            get { return dxmBrushes["BrushToUse"].MediaBrush; }
            set { UpdateBrush(value, "BrushToUse", BrushToUseOpacity); }
        }
		
		// Create Series<bool> to tell if we want to draw something for a particular bar
		private Series<bool> DrawLineSeries;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "OnRenderDrawObjects";
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
				
				// Set defaults for our properties, will be assigned to our DX/Media Brush in State.DataLoaded
				BrushToUseBrush								= Brushes.Red;
				BrushToUseWidth								= 2;
				BrushToUseOpacity							= 100.0;
			}
			else if (State == State.DataLoaded)
			{
				// Create Series<bool> to tell if we want to draw something for a particular bar
				DrawLineSeries = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
				// Create oue brush management dictionary
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				// Add brushes to the dictionary
				foreach (string brushName in new string[] { "BrushToUse" } )
            	    dxmBrushes.Add(brushName, new DXMediaMap());
				
				// Set any Brushes we use for custom rendering to that of the User Defined Brush.
				BrushToUse = BrushToUseBrush;
			}
		}

		bool draw = false;
		protected override void OnBarUpdate()
		{
			// Some condition that we would want to signal that we want to draw for that bar.
			//if(Close[0] > Open[0])
				DrawLineSeries[0] = true;

			if (CurrentBar < 3)
				return;
			
			if (DrawLineSeries[3])
				DrawLineSeries[3] = false;
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			// Call base.OnRender() to ensure plots get rendered appropriately.
			base.OnRender(chartControl, chartScale);
			
            if (Bars == null || ChartControl == null)
                return;
			
			if (IsInHitTest)
				return;

			// Set AntiAlias mode for smoothing
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Limit Custom Rendering to ChartBarsd.FromIndex and ChartBars.ToIndex
            for (int idx = ChartBars.FromIndex; idx <= ChartBars.ToIndex; idx++)
            {
				// Reference Series objects with GetValueAt(idx).
				if(DrawLineSeries.GetValueAt(idx))
				{
					// Draw Line
					DrawLine("BrushToUse",
							ChartControl.GetXByBarIndex(ChartBars, idx),
							ChartPanel.Y,
							ChartControl.GetXByBarIndex(ChartBars, idx),
							ChartPanel.H,
							BrushToUseWidth,
							DashStyleHelper.Solid);
				}
			}
			
			//Reset AntiAlias mode
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}
		
		public override void OnRenderTargetChanged()
        {
            // Dispose and recreate our DX Brushes or other Device Dependant resources on RenderTarget changes
            try
            {
				if (dxmBrushes == null)
					return;
                foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
                {			
                    if (item.Value.DxBrush != null)
                        item.Value.DxBrush.Dispose();

                    if (RenderTarget != null && item.Value.MediaBrush != null && !RenderTarget.IsDisposed)
                        item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString(), LogLevel.Error);
            }
        }
		
		// Our DX/Media Brush management class.
		[Browsable(false)]
        public class DXMediaMap
        {
            public SharpDX.Direct2D1.Brush DxBrush;
            public System.Windows.Media.Brush MediaBrush;
        }

        private void SetOpacity(string brushName, double opacity)
        {
            if (dxmBrushes[brushName].MediaBrush == null)
                return;
			
			// Force opcatity to be in bounds
			opacity = Math.Min(100.0, Math.Max(0, opacity));

			// Clone any Frozen brush so it can be modified
            if (dxmBrushes[brushName].MediaBrush.IsFrozen)
                dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

			// Set Opacity and freeze brush.
            dxmBrushes[brushName].MediaBrush.Opacity = opacity / 100.0;
            dxmBrushes[brushName].MediaBrush.Freeze();
        }
		
		private void UpdateBrush(Brush mediaBrush, string brushName, double opacity)
        {
			// Set Media Brush to brush passed
            dxmBrushes[brushName].MediaBrush = mediaBrush;
			
			// Call SetOpacity() to clone, set opacity and freeze brush. (currently
            SetOpacity(brushName, opacity);
			
			// Dispose DX Brushes and other Device Dependant resources
            if (dxmBrushes[brushName].DxBrush != null)
                dxmBrushes[brushName].DxBrush.Dispose();
			
			// Recreate DX Brushes and other Device Dependant Resources here, making sure RenderTarget is not null or IsDisposed
            if (RenderTarget != null && !RenderTarget.IsDisposed)
			{
				dxmBrushes[brushName].DxBrush = dxmBrushes[brushName].MediaBrush.ToDxBrush(RenderTarget); 
			}
        }
		
		// Condense Line drawing code
		private void DrawLine(string brushName, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			// Set StrokeStyleProperties Dashstyle to that of the DashStyleHelper
			if (dashStyle == DashStyleHelper.Dash)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash;
			if (dashStyle == DashStyleHelper.DashDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot;
			if (dashStyle == DashStyleHelper.DashDotDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;
			if (dashStyle == DashStyleHelper.Dot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;
			if (dashStyle == DashStyleHelper.Solid)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint = new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint = new System.Windows.Point(x2, y2).ToVector2();
			
			// Draw the line
			RenderTarget.DrawLine(startPoint, endPoint, dxmBrushes[brushName].DxBrush, width, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping,
			strokeStyle.Dispose();
		}
		
		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BrushToUseBrush", Description="Color of down bars.", Order=1, GroupName="Parameters")]
		public Brush BrushToUseBrush
		{ get; set; }

		[Browsable(false)]
		public string BarColorDownSerializable
		{
			get { return Serialize.BrushToString(BrushToUseBrush); }
			set { BrushToUseBrush = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BrushToUse width", Description="Width of lines.", Order=2, GroupName="Parameters")]
		public int BrushToUseWidth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0, 100.0)]
		[Display(Name="BrushToUse Opacity", Description="Opacity of lines.", Order=3, GroupName="Parameters")]
		public double BrushToUseOpacity
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.OnRenderDrawObjectsExample[] cacheOnRenderDrawObjectsExample;
		public CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			return OnRenderDrawObjectsExample(Input, brushToUseBrush, brushToUseWidth, brushToUseOpacity);
		}

		public CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(ISeries<double> input, Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			if (cacheOnRenderDrawObjectsExample != null)
				for (int idx = 0; idx < cacheOnRenderDrawObjectsExample.Length; idx++)
					if (cacheOnRenderDrawObjectsExample[idx] != null && cacheOnRenderDrawObjectsExample[idx].BrushToUseBrush == brushToUseBrush && cacheOnRenderDrawObjectsExample[idx].BrushToUseWidth == brushToUseWidth && cacheOnRenderDrawObjectsExample[idx].BrushToUseOpacity == brushToUseOpacity && cacheOnRenderDrawObjectsExample[idx].EqualsInput(input))
						return cacheOnRenderDrawObjectsExample[idx];
			return CacheIndicator<CustomRenderingExamples.OnRenderDrawObjectsExample>(new CustomRenderingExamples.OnRenderDrawObjectsExample(){ BrushToUseBrush = brushToUseBrush, BrushToUseWidth = brushToUseWidth, BrushToUseOpacity = brushToUseOpacity }, input, ref cacheOnRenderDrawObjectsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			return indicator.OnRenderDrawObjectsExample(Input, brushToUseBrush, brushToUseWidth, brushToUseOpacity);
		}

		public Indicators.CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(ISeries<double> input , Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			return indicator.OnRenderDrawObjectsExample(input, brushToUseBrush, brushToUseWidth, brushToUseOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			return indicator.OnRenderDrawObjectsExample(Input, brushToUseBrush, brushToUseWidth, brushToUseOpacity);
		}

		public Indicators.CustomRenderingExamples.OnRenderDrawObjectsExample OnRenderDrawObjectsExample(ISeries<double> input , Brush brushToUseBrush, int brushToUseWidth, double brushToUseOpacity)
		{
			return indicator.OnRenderDrawObjectsExample(input, brushToUseBrush, brushToUseWidth, brushToUseOpacity);
		}
	}
}

#endregion
