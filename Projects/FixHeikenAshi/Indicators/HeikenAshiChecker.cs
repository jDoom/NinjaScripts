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
	public class HeikenAshiChecker : Indicator
	{
		private int mismatchCount;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Apply to Heiken Ashi BarsType to compare with indicator result";
				Name										= "HeikenAshiChecker";
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
			}
			else if (State == State.Configure)
			{
				AddDataSeries(new BarsPeriod {BarsPeriodType = BarsPeriod.BaseBarsPeriodType, Value = BarsPeriod.BaseBarsPeriodValue} ); 
			}
			else if (State == State.Historical)
			{
				ClearOutputWindow();
				Print(String.Format("{0} {1}", Instrument.FullName, BarsPeriod.ToString()));
				Print("Historical processing started. Mismatches will be printed.");
			}
			else if (State == State.Realtime)
			{
				Print(String.Format("Historical processing complete. Checked: {0} Mismatches: {1}", CurrentBar, mismatchCount));
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0)
				return;
			
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1)
				return;
			
			if (Times[0][0] != Times[1][0])
				return;
			
			if (Opens[0][0] != HeikenAshi8Rounded(BarsArray[1]).HAOpen[0])
			{
				mismatchCount++;
				Print(String.Format("Open inequal at {0} {1} Checked: {2} Mismatches: {3}", CurrentBar, Time[0], CurrentBar, mismatchCount));
			}
			
			if (Highs[0][0] != HeikenAshi8Rounded(BarsArray[1]).HAHigh[0])
			{
				mismatchCount++;
				Print(String.Format("High inequal at {0} {1} Checked: {2} Mismatches: {3}", CurrentBar, Time[0], CurrentBar, mismatchCount));
			}
			
			if (Lows[0][0] != HeikenAshi8Rounded(BarsArray[1]).HALow[0])
			{
				mismatchCount++;
				Print(String.Format("Low inequal at {0} {1} Checked: {2} Mismatches: {3}", CurrentBar, Time[0], CurrentBar, mismatchCount));
			}
			
			if (Closes[0][0] != HeikenAshi8Rounded(BarsArray[1]).HAClose[0])
			{
				mismatchCount++;
				Print(String.Format("Close inequal at {0} {1} Checked: {2} Mismatches: {3}", CurrentBar, Time[0], CurrentBar, mismatchCount));
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HeikenAshiChecker[] cacheHeikenAshiChecker;
		public HeikenAshiChecker HeikenAshiChecker()
		{
			return HeikenAshiChecker(Input);
		}

		public HeikenAshiChecker HeikenAshiChecker(ISeries<double> input)
		{
			if (cacheHeikenAshiChecker != null)
				for (int idx = 0; idx < cacheHeikenAshiChecker.Length; idx++)
					if (cacheHeikenAshiChecker[idx] != null &&  cacheHeikenAshiChecker[idx].EqualsInput(input))
						return cacheHeikenAshiChecker[idx];
			return CacheIndicator<HeikenAshiChecker>(new HeikenAshiChecker(), input, ref cacheHeikenAshiChecker);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HeikenAshiChecker HeikenAshiChecker()
		{
			return indicator.HeikenAshiChecker(Input);
		}

		public Indicators.HeikenAshiChecker HeikenAshiChecker(ISeries<double> input )
		{
			return indicator.HeikenAshiChecker(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HeikenAshiChecker HeikenAshiChecker()
		{
			return indicator.HeikenAshiChecker(Input);
		}

		public Indicators.HeikenAshiChecker HeikenAshiChecker(ISeries<double> input )
		{
			return indicator.HeikenAshiChecker(input);
		}
	}
}

#endregion
