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
	public class LinearGradientBrushExample : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"LinearGradientBrushExample";
				Name										= "LinearGradientBrushExample";
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
			SharpDX.Direct2D1.LinearGradientBrushProperties lgbProps = new SharpDX.Direct2D1.LinearGradientBrushProperties();
			lgbProps.StartPoint = new SharpDX.Vector2(0, (float)chartScale.GetYByValue(2900));
			lgbProps.EndPoint = new SharpDX.Vector2(0, (float)chartScale.GetYByValue(2850));
			
			// Create Gradient Stop1 for the Gradient Stop Collection
			SharpDX.Direct2D1.GradientStop stop1;
			stop1.Color = SharpDX.Color.DarkSalmon;
			stop1.Position = 0;
			
			// Create Gradient Stop2 for the Gradient Stop Collection
			SharpDX.Direct2D1.GradientStop stop2;
			stop2.Color = SharpDX.Color.DarkGreen;
			stop2.Position = 1;
			
			// Create GradientStop array for GradientStopCollection
			SharpDX.Direct2D1.GradientStop[] lgbStops = new SharpDX.Direct2D1.GradientStop[] { stop1, stop2 };
			
			// Make our GradientStopCollection
			SharpDX.Direct2D1.GradientStopCollection lgbSGC = new SharpDX.Direct2D1.GradientStopCollection(RenderTarget, lgbStops);
			
			// Finally, create the LinearGradientBrush
			SharpDX.Direct2D1.LinearGradientBrush lgBrush = new SharpDX.Direct2D1.LinearGradientBrush(RenderTarget, lgbProps, lgbSGC);
			
			// Render Draw Method here
			RenderTarget.FillEllipse(new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(ChartPanel.W/2, ChartPanel. H/2), ChartPanel.W/2, ChartPanel. H/2), lgBrush);
			
			// This exmaple describes implementation in OnRender(), for more effieceny, dipose and recreate class level RenderTarget dependant objects in OnRederTargetStateChange()
			lgbSGC.Dispose();
			lgBrush.Dispose();
			
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
		private LinearGradientBrushExample[] cacheLinearGradientBrushExample;
		public LinearGradientBrushExample LinearGradientBrushExample()
		{
			return LinearGradientBrushExample(Input);
		}

		public LinearGradientBrushExample LinearGradientBrushExample(ISeries<double> input)
		{
			if (cacheLinearGradientBrushExample != null)
				for (int idx = 0; idx < cacheLinearGradientBrushExample.Length; idx++)
					if (cacheLinearGradientBrushExample[idx] != null &&  cacheLinearGradientBrushExample[idx].EqualsInput(input))
						return cacheLinearGradientBrushExample[idx];
			return CacheIndicator<LinearGradientBrushExample>(new LinearGradientBrushExample(), input, ref cacheLinearGradientBrushExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinearGradientBrushExample LinearGradientBrushExample()
		{
			return indicator.LinearGradientBrushExample(Input);
		}

		public Indicators.LinearGradientBrushExample LinearGradientBrushExample(ISeries<double> input )
		{
			return indicator.LinearGradientBrushExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinearGradientBrushExample LinearGradientBrushExample()
		{
			return indicator.LinearGradientBrushExample(Input);
		}

		public Indicators.LinearGradientBrushExample LinearGradientBrushExample(ISeries<double> input )
		{
			return indicator.LinearGradientBrushExample(input);
		}
	}
}

#endregion
