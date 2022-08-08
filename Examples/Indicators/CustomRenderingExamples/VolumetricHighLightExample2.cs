//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
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
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.CustomRenderingExamples
{
	public class VolumetricHighLightExample2 : Indicator
	{
		private BarsTypes.VolumetricBarsType		barsType;
		private Series<Dictionary<double, bool>>	MySeriesDictMatchBidAsk;
		private SharpDX.Direct2D1.Brush				hilightBrush, regBrush;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= "This example demonstrates how you can create logic to decorate Volumetric Bar's price levels. This example favors calculating what should be drawn in OnBarUpdate, and leaves only the rendering work to be done in OnRender. This appraoch is more memory intensive than CPU intensive.";
				Name						= "VolumetricHighLightExample2";
				Calculate					= Calculate.OnBarClose;
				DisplayInDataBox			= false;
				IsOverlay					= true;
				IsChartOnly					= true;
				IsSuspendedWhileInactive	= true;
				ScaleJustification			= ScaleJustification.Right;
			}
			else if (State == State.DataLoaded)
			{
				// We want our highlights to be behind the ChartBars
				SetZOrder(-1);
				
				// Make sure we are working with Volumetric bars
				barsType = Bars.BarsSeries.BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
		        if (barsType == null)
				{
					ChartControl.Dispatcher.InvokeAsync(new Action(() =>
					{      
						NinjaTrader.Gui.Tools.NTMessageBoxSimple.Show(System.Windows.Window.GetWindow(ChartControl.OwnerChart as System.Windows.DependencyObject), 
						"This example is for use on Volumetric Bars only", "VolumetricHighLightExample2", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.None);
					}));
					SetState(State.Terminated);
				}
				
				// Make a Series<Dictionary<double, bool>>, something that tells what we would need to draw, so we can perform data based calculations in OnBarUpdate, and just leave rendering code in OnRender.
				MySeriesDictMatchBidAsk = new Series<Dictionary<double, bool>>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{	
			// Create a Dictionary<double, bool> for each bar. This tracks each price level and signals if we should draw something on a specific price level in the bar
			Dictionary<double, bool> thisBarMatchingBidAskDict	= new Dictionary<double, bool>();
			
			// Loop through the price levels of the bar for deeper bar analysis. In this case, we will see if bid/ask volume matches
			//for (double tick = Low[0]; tick <= High[0]; tick += TickSize * BarsPeriod.Value)
			for (double tick = High[0]; tick >= Low[0]; tick -= TickSize)
			{
				// If volume matches, set the bool in our dictionary for that price level
				if (barsType.Volumes[CurrentBar].GetBidVolumeForPrice(tick) == barsType.Volumes[CurrentBar].GetAskVolumeForPrice(tick))
					thisBarMatchingBidAskDict.Add(tick, true);
				else
					thisBarMatchingBidAskDict.Add(tick, false);
			}
			
			// After looping through the price levels of the bar, assign our completed dictionary to the Series.
			MySeriesDictMatchBidAsk[0] = thisBarMatchingBidAskDict;
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{			
			// The width of the Volumetric price row (includes bid/ask side)
			float width	= Math.Max(chartControl.Properties.BarDistance - chartControl.Properties.BarDistance * 0.05f - 5, 0);
			
			// Loop through all bars on chart
			for (int i = ChartBars.FromIndex; i <= ChartBars.ToIndex; i++)
			{
				// Assign bar low/high to local variables to save calculation time looking up prices
				double	low		= Low.GetValueAt(i);
				double	high	= High.GetValueAt(i);
				
				// Loop through all ticks from Low to High on each bar
				for (double tick = low; tick <= high; tick += TickSize * BarsPeriod.Value)
				//for (double tick = High[0]; tick >= Low[0]; tick -= TickSize)
				{
					// Calculate size of each price row depending on Tick Per Level
					double	boxSizeinTicks		= (int) Math.Round(Math.Min(BarsPeriod.Value, (high + TickSize - tick) / TickSize));
					// Calculate x coordinate (shift by half of our width from bar X coordinate)
					float	x					= chartControl.GetXByBarIndex(ChartBars, i) - width * 0.5f;
					// Calculate y coordinate (shift half of TickSize from the current tick/price level in this bar)
					float	y					= chartScale.GetYByValue(tick - TickSize * 0.5);
					// Calculate height (difference of price level's y coordinate and ending y coordinate with boxSizeinTicks adjustment)
					float	height				= chartScale.GetYByValue(tick + TickSize * (boxSizeinTicks - 0.5)) - y;
					// Create the rectangle for the price level
					SharpDX.RectangleF barRect	= new RectangleF(x, y, width, height);
					
					// Look up the price level in our Series<Dictionary<double, bool>>, if we should highlight or not.
					if (MySeriesDictMatchBidAsk.IsValidDataPointAt(i) && MySeriesDictMatchBidAsk.GetValueAt(i)[tick])
						RenderTarget.FillRectangle(barRect, hilightBrush);
					else
						RenderTarget.FillRectangle(barRect, regBrush);
				}
			}
		}
		
		public override void OnRenderTargetChanged()
		{
			// Dispose brushes on RenderTarget changes if they are not null (These SharpdDX brushes will be null and created on first OnRenderTargetChange after State.Configure, when RenderTarget is not null)
			if (regBrush != null)
				regBrush.Dispose();
			if (hilightBrush != null)
				hilightBrush.Dispose();
			
			// Create brushes on RenderTarget changes when RenderTarget is not null. After final State.Terminated when script ends, RenderTarget is null, and brushes are left as disposed.
			if (RenderTarget != null)
			{
				regBrush		= Brushes.Orange.ToDxBrush(RenderTarget);
				hilightBrush	= Brushes.RoyalBlue.ToDxBrush(RenderTarget);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.VolumetricHighLightExample2[] cacheVolumetricHighLightExample2;
		public CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2()
		{
			return VolumetricHighLightExample2(Input);
		}

		public CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2(ISeries<double> input)
		{
			if (cacheVolumetricHighLightExample2 != null)
				for (int idx = 0; idx < cacheVolumetricHighLightExample2.Length; idx++)
					if (cacheVolumetricHighLightExample2[idx] != null &&  cacheVolumetricHighLightExample2[idx].EqualsInput(input))
						return cacheVolumetricHighLightExample2[idx];
			return CacheIndicator<CustomRenderingExamples.VolumetricHighLightExample2>(new CustomRenderingExamples.VolumetricHighLightExample2(), input, ref cacheVolumetricHighLightExample2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2()
		{
			return indicator.VolumetricHighLightExample2(Input);
		}

		public Indicators.CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2(ISeries<double> input )
		{
			return indicator.VolumetricHighLightExample2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2()
		{
			return indicator.VolumetricHighLightExample2(Input);
		}

		public Indicators.CustomRenderingExamples.VolumetricHighLightExample2 VolumetricHighLightExample2(ISeries<double> input )
		{
			return indicator.VolumetricHighLightExample2(input);
		}
	}
}

#endregion
