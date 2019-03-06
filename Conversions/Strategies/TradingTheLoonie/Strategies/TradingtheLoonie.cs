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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;

using Correlation;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class TradingtheLoonie : Strategy
	{
		private BollingerBandDivergence bbd;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "TradingtheLoonie";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.Configure)
			{
				AddDataSeries("CL 01-18", BarsPeriodType.Day, 1);
				//AddDataSeries("CL 01-18", new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 1440 }, "Default 24 x 7");
			}
			else if (State == State.DataLoaded)
			{
				bbd = BollingerBandDivergence(20);
				AddChartIndicator(bbd);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsPeriod.BarsPeriodType != BarsPeriodType.Day && !(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value == 1440))
			{
				Draw.TextFixed(this, "NinjaScriptInfo", "This strategy must be applied to a daily series", TextPosition.BottomRight);
				return;
			}
			
			if(CurrentBars[0] < 1 || CurrentBars[1] < 1)
				return;

			// BUY		
			if (MAX(bbd, 3)[0] > 20 
				&& bbd[0] < bbd[1] 
				&& ROC(Closes[0], 2)[0] > 0 
				&& SMA(Closes[1], 40)[0] > SMA(Closes[1], 40)[2]
				&& Correlation(Closes[0], 20, CorrelationType.Pearson, 0)[0] > -.4)
			{
				EnterLong();
			}
						
			// SELL
			if ((CrossAbove(EMA(MACD(12, 26, 9), 9), MACD(12, 26, 9), 1) && Stochastics(1, 30, 3).K[0] > 85)
				|| (MIN(bbd, 3)[0] < -20 && ROC(Closes[1], 3)[0] < -3)
				|| (Closes[0][0] < MIN(Low, 15)[1] && Correlation(Closes[0], 60, CorrelationType.Pearson, 0)[0] < -.4))
			{
				ExitLong();
			}
			
						
			// SELL SHORT			
			if (MIN(bbd, 3)[0] < -20
				&& bbd[0] > bbd[1]
				&& ROC(Closes[0], 2)[0] < 0
				&& SMA(Closes[1], 40)[0] < SMA(Closes[1], 40)[2]
				&& Correlation(Closes[0], 20, CorrelationType.Pearson, 0)[0] > -.4)
			{
				EnterShort();
			}	
			
			// COVER
			if ((CrossAbove(MACD(12, 26, 9), EMA(MACD(12, 26, 9), 9), 1) && Stochastics(1, 30, 3).K[0] < 25 && Closes[1][0] >= (1 + 4 / 100) * MIN(Closes[1], 4)[0])
				|| (MAX(bbd, 3)[0] > 20 && ROC(Closes[1], 3)[0] > 4.5)
				|| (Closes[0][0] > MAX(High, 15)[1] && Correlation(Closes[0], 60, CorrelationType.Pearson, 0)[0] < -.4))
			{
				ExitShort();
			}
		}
	}
}
