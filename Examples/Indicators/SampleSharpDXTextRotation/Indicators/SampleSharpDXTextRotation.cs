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
using SharpDX;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SampleSharpDXTextRotation : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SampleSharpDXTextRotation";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				DrawOnPricePanel 							= false;
				IsOverlay 									= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Create a Font, Brush and TextFormat to draw our text.
			// For more information on drawing Text, please see the source code for the Text DrawingTool
			var font = new Gui.Tools.SimpleFont("Arial", 20);
			SharpDX.Direct2D1.Brush tmpBrush = Brushes.Red.ToDxBrush(RenderTarget);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			
			// Create a TextLayout for our text to draw.
			var cachedTextLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "Hello, I am sideways text.", textFormat, 600, textFormat.FontSize);
			
			// Rotate the RenderTarget by setting the Matrix3x2 Transform property
			// Matrix3x2.Rotation() will return a rotated Matrix3x2 based off of the angle specified, and the center point where you draw the object 
			RenderTarget.Transform = Matrix3x2.Rotation(1.5708f, new Vector2(100,100));
			
			// Draw the text on the rotated RenderTarget
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(100, 100), cachedTextLayout, tmpBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			
			// Dispose of resources
			textFormat.Dispose();
			cachedTextLayout.Dispose();
			tmpBrush.Dispose();
			
			// Rotate the RenderTarget back
			RenderTarget.Transform = Matrix3x2.Identity;

			// Return rendering to base class
			base.OnRender(chartControl, chartScale);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleSharpDXTextRotation[] cacheSampleSharpDXTextRotation;
		public SampleSharpDXTextRotation SampleSharpDXTextRotation()
		{
			return SampleSharpDXTextRotation(Input);
		}

		public SampleSharpDXTextRotation SampleSharpDXTextRotation(ISeries<double> input)
		{
			if (cacheSampleSharpDXTextRotation != null)
				for (int idx = 0; idx < cacheSampleSharpDXTextRotation.Length; idx++)
					if (cacheSampleSharpDXTextRotation[idx] != null &&  cacheSampleSharpDXTextRotation[idx].EqualsInput(input))
						return cacheSampleSharpDXTextRotation[idx];
			return CacheIndicator<SampleSharpDXTextRotation>(new SampleSharpDXTextRotation(), input, ref cacheSampleSharpDXTextRotation);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleSharpDXTextRotation SampleSharpDXTextRotation()
		{
			return indicator.SampleSharpDXTextRotation(Input);
		}

		public Indicators.SampleSharpDXTextRotation SampleSharpDXTextRotation(ISeries<double> input )
		{
			return indicator.SampleSharpDXTextRotation(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleSharpDXTextRotation SampleSharpDXTextRotation()
		{
			return indicator.SampleSharpDXTextRotation(Input);
		}

		public Indicators.SampleSharpDXTextRotation SampleSharpDXTextRotation(ISeries<double> input )
		{
			return indicator.SampleSharpDXTextRotation(input);
		}
	}
}

#endregion
