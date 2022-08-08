//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim, based on OneTick template by NinjaTrader_MichaelM
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
namespace NinjaTrader.NinjaScript.Indicators.DataManagementExamples
{
	public class BuySellVolumeOneTickExample : Indicator
	{
		private double	buys, sells;
		private int		lastBar;
		private bool	lastInTransition;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Demonstration of OF+ Best Practices in BuySellPressure";
				Name						= "BuySellVolumeOneTickExample";
				
				AddPlot(new Stroke(Brushes.DarkCyan,	2), PlotStyle.Bar, "BuyVolume");
				AddPlot(new Stroke(Brushes.Crimson,		2), PlotStyle.Bar, "SellVolume");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
			else if (State == State.DataLoaded)
			{
				buys	= 1;
				sells	= 1;
			}
		}
		
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
						ResetValues(false, false);
				}
				
				// We only set the value on the primary to preserve external programmatic access to plot as well as indicator-as-input cases
				SetValues(0);
				
				// If we are Calculate.OnBarClose or we are in Historical processing mode, we are already update to date on the 1 tick series so we reset here
				// We also check conditions if we are inside a transition bar for non-Calculate.OnBarClose modes, where we need to subtract last plot values instead of a full reset.
				if (Calculate == Calculate.OnBarClose || (lastBar != CurrentBars[0]) && ((State == State.Historical && indexOffset > 1) || (State == State.Realtime && indexOffset == 0)))
					ResetValues(false, false);
				else if (Calculate != Calculate.OnBarClose && State == State.Historical && indexOffset == 1)
					ResetValues(false, true);
				
				lastBar = CurrentBars[0];
				
			}
			else if (BarsInProgress == 1)
			{
				// The more granular series will open the new session so we have to reset any session related stuff here
				if (BarsArray[1].IsFirstBarOfSession)
					ResetValues(true, false);
				
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
			double	price			= BarsArray[1].GetClose(whatBar);
			
			// Accumulate volume
			if (price >= BarsArray[1].GetAsk(whatBar))
				buys += volume;	
			else if (price <= BarsArray[1].GetBid(whatBar))
				sells += volume;
			
			lastInTransition 	= inTransition;
		}
		
		private void SetValues(int barsAgo)
		{
			BuyVolume[barsAgo] = buys + sells;
			SellVolume[barsAgo] = sells;
		}
		
		private void ResetValues(bool isNewSession, bool isTransition)
		{
			if (isTransition)
			{
				// Perform subtraction for historical -> realtime transition here
				buys -= BuyVolume[0] - SellVolume[0];
				sells -= SellVolume[0];
			}
			else
			{
				// Regular reset of values for non-Transition bars can be done here
				buys = sells = 0;
			}
			
			if (isNewSession)
			{
				// Cumulative values (per session) would reset here
				// BuySellVolume is not gapless so we ignore in this example
			}
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyVolume
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellVolume
		{
			get { return Values[1]; }
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DataManagementExamples.BuySellVolumeOneTickExample[] cacheBuySellVolumeOneTickExample;
		public DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample()
		{
			return BuySellVolumeOneTickExample(Input);
		}

		public DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample(ISeries<double> input)
		{
			if (cacheBuySellVolumeOneTickExample != null)
				for (int idx = 0; idx < cacheBuySellVolumeOneTickExample.Length; idx++)
					if (cacheBuySellVolumeOneTickExample[idx] != null &&  cacheBuySellVolumeOneTickExample[idx].EqualsInput(input))
						return cacheBuySellVolumeOneTickExample[idx];
			return CacheIndicator<DataManagementExamples.BuySellVolumeOneTickExample>(new DataManagementExamples.BuySellVolumeOneTickExample(), input, ref cacheBuySellVolumeOneTickExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample()
		{
			return indicator.BuySellVolumeOneTickExample(Input);
		}

		public Indicators.DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample(ISeries<double> input )
		{
			return indicator.BuySellVolumeOneTickExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample()
		{
			return indicator.BuySellVolumeOneTickExample(Input);
		}

		public Indicators.DataManagementExamples.BuySellVolumeOneTickExample BuySellVolumeOneTickExample(ISeries<double> input )
		{
			return indicator.BuySellVolumeOneTickExample(input);
		}
	}
}

#endregion
