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
	public class SveStochATS : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Stocks and Commodities - December 2011 - Applying The Put/Call Ratio Indicator";
				Name										= "SveStochATS";
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
				StochPeriod					= 30;
				StochSlow					= 5;
			}
			else if (State == State.Configure)
			{
				AddChartIndicator(SveStochIFT(StochPeriod, StochSlow));
			}
		}

		protected override void OnBarUpdate()
		{
			if (SveStochIFT(StochPeriod, StochSlow).IFTStoch[0] > 30)
			  	EnterLong();
			
			if (SveStochIFT(StochPeriod, StochSlow).IFTStoch[0] < 60 && Close[0] < SMA(165)[0])
				EnterShort();
			
			if (SveStochIFT(StochPeriod, StochSlow).IFTStoch[0] < 60)
				ExitLong();
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StochPeriod", Description="Number of bars used in the Stochastics calculation", Order=1, GroupName="Parameters")]
		public int StochPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StochSlow", Description="Number of bars used in stochastic smoothing", Order=2, GroupName="Parameters")]
		public int StochSlow
		{ get; set; }
		#endregion

	}
}
