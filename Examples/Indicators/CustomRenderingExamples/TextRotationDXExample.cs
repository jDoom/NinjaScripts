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
using SharpDX;
#endregion

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.CustomRenderingExamples
{
	public class TextRotationDXExample : Indicator
	{
		private SimpleFont						font;
		private SharpDX.Direct2D1.Brush			textBrush;
		private SharpDX.DirectWrite.TextFormat	textFormat;
		private SharpDX.DirectWrite.TextLayout	cachedTextLayout;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "SharpDXTextRotationExample";
				Calculate									= Calculate.OnBarClose;
				DrawOnPricePanel 							= false;
				IsOverlay 									= true;
			}
			else if (State == State.DataLoaded)
			{
				font				= new SimpleFont("Arial", 20);
				textFormat			= font.ToDirectWriteTextFormat();
				// Create a TextLayout for our text to draw.
				cachedTextLayout	= new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, "Hello, I am sideways text.", textFormat, 600, textFormat.FontSize);
			}
			else if (State == State.Terminated)
			{
				// Dispose of resources
				if (textFormat != null)
					textFormat.Dispose();

				if (cachedTextLayout != null)
					cachedTextLayout.Dispose();

				if (textBrush != null)
					textBrush.Dispose();
			}
		}

		protected override void OnBarUpdate() { }
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{	
			// Rotate the RenderTarget by setting the Matrix3x2 Transform property
			// Matrix3x2.Rotation() will return a rotated Matrix3x2 based off of the angle specified, and the center point where you draw the object 
			RenderTarget.Transform	= Matrix3x2.Rotation(1.5708f, new Vector2(100,100));
			
			// Draw the text on the rotated RenderTarget
			RenderTarget.DrawTextLayout(new SharpDX.Vector2(100, 100), cachedTextLayout, textBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
						
			// Rotate the RenderTarget back
			RenderTarget.Transform	= Matrix3x2.Identity;

			// Return rendering to base class
			base.OnRender(chartControl, chartScale);
		}

		public override void OnRenderTargetChanged()
		{
			// Create a Font, Brush and TextFormat to draw our text.
			// For more information on drawing Text, please see the source code for the Text DrawingTool

			if (textBrush != null)
				textBrush.Dispose();

			if (RenderTarget != null)
			{
				textBrush = Brushes.Red.ToDxBrush(RenderTarget);
			}	
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.TextRotationDXExample[] cacheTextRotationDXExample;
		public CustomRenderingExamples.TextRotationDXExample TextRotationDXExample()
		{
			return TextRotationDXExample(Input);
		}

		public CustomRenderingExamples.TextRotationDXExample TextRotationDXExample(ISeries<double> input)
		{
			if (cacheTextRotationDXExample != null)
				for (int idx = 0; idx < cacheTextRotationDXExample.Length; idx++)
					if (cacheTextRotationDXExample[idx] != null &&  cacheTextRotationDXExample[idx].EqualsInput(input))
						return cacheTextRotationDXExample[idx];
			return CacheIndicator<CustomRenderingExamples.TextRotationDXExample>(new CustomRenderingExamples.TextRotationDXExample(), input, ref cacheTextRotationDXExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.TextRotationDXExample TextRotationDXExample()
		{
			return indicator.TextRotationDXExample(Input);
		}

		public Indicators.CustomRenderingExamples.TextRotationDXExample TextRotationDXExample(ISeries<double> input )
		{
			return indicator.TextRotationDXExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.TextRotationDXExample TextRotationDXExample()
		{
			return indicator.TextRotationDXExample(Input);
		}

		public Indicators.CustomRenderingExamples.TextRotationDXExample TextRotationDXExample(ISeries<double> input )
		{
			return indicator.TextRotationDXExample(input);
		}
	}
}

#endregion
