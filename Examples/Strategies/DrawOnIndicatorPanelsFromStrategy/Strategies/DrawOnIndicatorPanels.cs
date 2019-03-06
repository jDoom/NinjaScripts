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
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class DrawOnIndicatorPanels : Strategy
	{
		private MACD MACD1;
		private MACD MACD2;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "DrawOnIndicatorPanels";
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
				DrawOnPricePanel = false;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			else if (State == State.DataLoaded)
			{
				MACD1 = MACD(12, 26, 9);
				AddChartIndicator(MACD1);
				MACD1.DrawOnPricePanel = false;
				MACD1.IsOverlay = false;
				
				MACD2 = MACD(13, 26, 9);
				AddChartIndicator(MACD2);
				MACD2.DrawOnPricePanel = false;
				MACD2.IsOverlay = false;
			}
		}

		protected override void OnBarUpdate()
		{
			Draw.Dot(this, "Strategy"+CurrentBar, true, 0, High[0] + 10 * TickSize, Brushes.RoyalBlue, false);
			Draw.Dot(MACD1, "MACD1"+CurrentBar, true, 0, MACD1.Default[0] + 1, Brushes.RoyalBlue, false);
			Draw.Dot(MACD2, "MACD2"+CurrentBar, true, 0, MACD2.Diff[0] + 1, Brushes.RoyalBlue, false);
		}
	}
}
