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

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class RadialGradientBrushExample : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"RadialGradientBrushExample";
				Name										= "RadialGradientBrushExample";
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
				IsSuspendedWhileInactive					= false;				
			}
		}

		protected override void OnBarUpdate()
		{			

		}
		
		public override void OnRenderTargetChanged()
		{
			// Dispose and recreate our SharpDX Brushes or other device dependant resources on RenderTarget changes.
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Call base OnRender() method to paint defined Plots.
			base.OnRender(chartControl, chartScale);
			
			// Store previous AA mode
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode 	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode 							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Create Linear Graient Brush Properties
			SharpDX.Direct2D1.RadialGradientBrushProperties rgbProps = new SharpDX.Direct2D1.RadialGradientBrushProperties();
			rgbProps.Center = new SharpDX.Vector2(ChartPanel.W/2, ChartPanel.H/2);
			rgbProps.RadiusX = ChartPanel.W/2;
			rgbProps.RadiusY = ChartPanel.W/2;
			
			// Create Gradient Stop1 for the Gradient Stop Collection
			SharpDX.Direct2D1.GradientStop stop1;
			stop1.Color = SharpDX.Color.DarkSalmon;
			stop1.Position = 0;
			
			// Create Gradient Stop2 for the Gradient Stop Collection
			SharpDX.Direct2D1.GradientStop stop2;
			stop2.Color = SharpDX.Color.DarkGreen;
			stop2.Position = 1;
			
			// Create GradientStop array for GradientStopCollection
			SharpDX.Direct2D1.GradientStop[] rgbStops = new SharpDX.Direct2D1.GradientStop[] { stop1, stop2 };
			
			// Make our GradientStopCollection
			SharpDX.Direct2D1.GradientStopCollection rgbSGC = new SharpDX.Direct2D1.GradientStopCollection(RenderTarget, rgbStops);
			
			// Finally, create the LinearGradientBrush
			SharpDX.Direct2D1.RadialGradientBrush rgBrush = new SharpDX.Direct2D1.RadialGradientBrush(RenderTarget, rgbProps, rgbSGC);
			
			// Render Draw Method here
			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartPanel.W/2, ChartPanel. H/2), ChartPanel.W/2, ChartPanel. H/2), rgBrush);
			
			// This exmaple describes implementation in OnRender(), for more effieceny, dipose and recreate class level RenderTarget dependant objects in OnRederTargetStateChange()
			rgbSGC.Dispose();
			rgBrush.Dispose();
			
			// Reset AA mode.
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}	

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RadialGradientBrushExample[] cacheRadialGradientBrushExample;
		public RadialGradientBrushExample RadialGradientBrushExample()
		{
			return RadialGradientBrushExample(Input);
		}

		public RadialGradientBrushExample RadialGradientBrushExample(ISeries<double> input)
		{
			if (cacheRadialGradientBrushExample != null)
				for (int idx = 0; idx < cacheRadialGradientBrushExample.Length; idx++)
					if (cacheRadialGradientBrushExample[idx] != null &&  cacheRadialGradientBrushExample[idx].EqualsInput(input))
						return cacheRadialGradientBrushExample[idx];
			return CacheIndicator<RadialGradientBrushExample>(new RadialGradientBrushExample(), input, ref cacheRadialGradientBrushExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RadialGradientBrushExample RadialGradientBrushExample()
		{
			return indicator.RadialGradientBrushExample(Input);
		}

		public Indicators.RadialGradientBrushExample RadialGradientBrushExample(ISeries<double> input )
		{
			return indicator.RadialGradientBrushExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RadialGradientBrushExample RadialGradientBrushExample()
		{
			return indicator.RadialGradientBrushExample(Input);
		}

		public Indicators.RadialGradientBrushExample RadialGradientBrushExample(ISeries<double> input )
		{
			return indicator.RadialGradientBrushExample(input);
		}
	}
}

#endregion
