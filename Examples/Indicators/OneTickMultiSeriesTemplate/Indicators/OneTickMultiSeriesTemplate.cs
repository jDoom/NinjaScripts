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
	public class OneTickMultiSeriesTemplate : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"A template to show how to get the right ticks into the right bar using understood best practices for NinjaScript and without BarsPeriod specific handling.";
				Name										= "OneTickMultiSeriesTemplate";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Bar, "Bar Volume");
				AddPlot(new Stroke(Brushes.Goldenrod, 1), PlotStyle.Line, "Volume EMA(14)");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
		}
		
		private double 	barVolume;
		private double 	ema;
		private double 	lastEma;
		private int 	lastBar;
		private bool 	lastInTransition;
		
		protected override void OnBarUpdate()
		{
			if (BarsInProgress == 0)
			{
				// This lets us know what processing mode we are in
				// if indexOffset == 0 then we are in 'realtime processing mode'
				// if indexOffset is > 0 then we are in 'historical processing mode'
				int indexOffset = BarsArray[1].Count - 1 - CurrentBars[1];
				
				// If we are not Calculate.OnBarClose and we are in Realtime processing mode
				if (IsFirstTickOfBar && Calculate != Calculate.OnBarClose && (State == State.Realtime || BarsArray[0].IsTickReplay))
				{
					// We always get the last tick after the primary triggers OBU so we update the last bar
					if (CurrentBars[0] > 0)
						SetValues(1);
					
					// We have the last tick of the bar added so now we can reset
					if (BarsArray[0].IsTickReplay || State == State.Realtime && indexOffset == 0)
						ResetValues(false);
				}
				
				// We only set the value on the primary to preserve external programmatic access to plot as well as indicator-as-input cases
				SetValues(0);
				
				// If we are Calculate.OnBarClose or we are in Historical processing mode, we are already update to date on the 1 tick series so we reset here
				if (Calculate == Calculate.OnBarClose || (lastBar != CurrentBars[0] && (State == State.Historical || State == State.Realtime && indexOffset > 0)))
					ResetValues(false);
				
				lastBar = CurrentBars[0];
				
			}
			else if (BarsInProgress == 1)
			{
				// The more granular series will open the new session so we have to reset any session related stuff here
				if (BarsArray[1].IsFirstBarOfSession)
					ResetValues(true);
				
				// We only calculate values from the 1 tick series
				CalculateValues(false);
			}
		}
		
		private void CalculateValues(bool forceCurrentBar)
		{
			// This lets us know what processing mode we are in
			// if indexOffset == 0 and State is Realtime then we are in 'realtime processing mode'
			// if indexOffset is > 0 then we are in 'historical processing mode'
			int 	indexOffset 	= BarsArray[1].Count - 1 - CurrentBars[1];
			bool 	inTransition 	= State == State.Realtime && indexOffset > 1;
			
			// For Calculate.OnBarClose in realtime processing we have to advance the index on the tick series to not be one tick behind
			// The means, at the end of the 'transition' (where State is Realtime but we are still in historical processing mode) -> we have to calculate two ticks (CurrentBars[1] and CurrentBars[1] + 1)
			if (!inTransition && lastInTransition && !forceCurrentBar && Calculate == Calculate.OnBarClose)
				CalculateValues(true);
			
			bool 	useCurrentBar 	= State == State.Historical || inTransition || Calculate != Calculate.OnBarClose || forceCurrentBar;
			
			// This is where we decide what index to use
			int 	whatBar 		= useCurrentBar ? CurrentBars[1] : Math.Min(CurrentBars[1] + 1, BarsArray[1].Count - 1);
			
			// This is how we get the right tick values
			double 	volume 			= BarsArray[1].GetVolume(whatBar);
			
			barVolume 			+= volume;
			lastInTransition 	= inTransition;
		}
		
		private void SetValues(int barsAgo)
		{
			Values[0][barsAgo] = barVolume;
			
			// Normally you would calculate the EMA in CalculateValues method but in this case we want it to calculate off our bar volume, not the 1 tick volume
			if (lastBar != CurrentBars[0])
			{
				if (barsAgo == 0)
				{
					lastEma = ema;
					ema 	= CurrentBars[0] == 0 ? barVolume : barVolume * (2.0 / 15) + (1 - (2.0 / 15)) * lastEma;
				}
				else
				{
					ema 	= CurrentBars[0] == 0 ? barVolume : barVolume * (2.0 / 15) + (1 - (2.0 / 15)) * lastEma;
					lastEma = ema;
				}
			}
			
			Values[1][barsAgo] = ema;
		}
		
		private void ResetValues(bool isNewSession)
		{
			barVolume = 0;
			
			if (isNewSession)
			{
				// Cumulative values (per session) would reset here
				// EMA is not gapless so we ignore in this example
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OneTickMultiSeriesTemplate[] cacheOneTickMultiSeriesTemplate;
		public OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate()
		{
			return OneTickMultiSeriesTemplate(Input);
		}

		public OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate(ISeries<double> input)
		{
			if (cacheOneTickMultiSeriesTemplate != null)
				for (int idx = 0; idx < cacheOneTickMultiSeriesTemplate.Length; idx++)
					if (cacheOneTickMultiSeriesTemplate[idx] != null &&  cacheOneTickMultiSeriesTemplate[idx].EqualsInput(input))
						return cacheOneTickMultiSeriesTemplate[idx];
			return CacheIndicator<OneTickMultiSeriesTemplate>(new OneTickMultiSeriesTemplate(), input, ref cacheOneTickMultiSeriesTemplate);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate()
		{
			return indicator.OneTickMultiSeriesTemplate(Input);
		}

		public Indicators.OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate(ISeries<double> input )
		{
			return indicator.OneTickMultiSeriesTemplate(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate()
		{
			return indicator.OneTickMultiSeriesTemplate(Input);
		}

		public Indicators.OneTickMultiSeriesTemplate OneTickMultiSeriesTemplate(ISeries<double> input )
		{
			return indicator.OneTickMultiSeriesTemplate(input);
		}
	}
}

#endregion
