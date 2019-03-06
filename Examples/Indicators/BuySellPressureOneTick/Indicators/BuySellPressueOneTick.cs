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
	public class BuySellPressureOneTick : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Demonstration of OF+ Best Practices in BuySellPressure";
				Name										= "BuySellPressureOneTick";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				
				AddPlot(Brushes.DarkCyan, "BuyPressure");
				AddPlot(Brushes.Crimson, "SellPressure");
				
				AddLine(Brushes.DimGray, 75,	"Upper");
				AddLine(Brushes.DimGray, 25,	"Lower");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
		}
		
		private double		buys 	= 1;
		private double 		sells 	= 1;

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
			BuyPressure[barsAgo] = (buys / (buys + sells)) * 100;
			SellPressure[barsAgo] = (sells / (buys + sells)) * 100;
		}
		
		private void ResetValues(bool isNewSession)
		{
			buys = sells = 1;
			
			if (isNewSession)
			{
				// Cumulative values (per session) would reset here
				// EMA is not gapless so we ignore in this example
			}
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyPressure
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellPressure
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
		private BuySellPressureOneTick[] cacheBuySellPressureOneTick;
		public BuySellPressureOneTick BuySellPressureOneTick()
		{
			return BuySellPressureOneTick(Input);
		}

		public BuySellPressureOneTick BuySellPressureOneTick(ISeries<double> input)
		{
			if (cacheBuySellPressureOneTick != null)
				for (int idx = 0; idx < cacheBuySellPressureOneTick.Length; idx++)
					if (cacheBuySellPressureOneTick[idx] != null &&  cacheBuySellPressureOneTick[idx].EqualsInput(input))
						return cacheBuySellPressureOneTick[idx];
			return CacheIndicator<BuySellPressureOneTick>(new BuySellPressureOneTick(), input, ref cacheBuySellPressureOneTick);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BuySellPressureOneTick BuySellPressureOneTick()
		{
			return indicator.BuySellPressureOneTick(Input);
		}

		public Indicators.BuySellPressureOneTick BuySellPressureOneTick(ISeries<double> input )
		{
			return indicator.BuySellPressureOneTick(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BuySellPressureOneTick BuySellPressureOneTick()
		{
			return indicator.BuySellPressureOneTick(Input);
		}

		public Indicators.BuySellPressureOneTick BuySellPressureOneTick(ISeries<double> input )
		{
			return indicator.BuySellPressureOneTick(input);
		}
	}
}

#endregion
