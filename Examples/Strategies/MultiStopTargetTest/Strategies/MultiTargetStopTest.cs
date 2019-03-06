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
	public class MultiTargetStopTest : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "MultiTargetStopTest";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 3;
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
				
			}
		}

		protected override void OnBarUpdate()
		{
			if(State == State.Historical)
				return;
			
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				// Reset Stops and Targets when we are flat before we enter a new position.
				SetProfitTarget("LongEntry1", CalculationMode.Ticks, 20);
				SetProfitTarget("LongEntry2", CalculationMode.Ticks, 20);
				SetProfitTarget("LongEntry3", CalculationMode.Ticks, 20);
				SetStopLoss("LongEntry1", CalculationMode.Ticks, 10, false);
				SetStopLoss("LongEntry2", CalculationMode.Ticks, 10, false);
				SetStopLoss("LongEntry3", CalculationMode.Ticks, 10, false);
				
				SetProfitTarget("ShortEntry1", CalculationMode.Ticks, 20);
				SetProfitTarget("ShortEntry2", CalculationMode.Ticks, 20);
				SetProfitTarget("ShortEntry3", CalculationMode.Ticks, 20);
				SetStopLoss("ShortEntry1", CalculationMode.Ticks, 10, false);
				SetStopLoss("ShortEntry2", CalculationMode.Ticks, 10, false);
				SetStopLoss("ShortEntry3", CalculationMode.Ticks, 10, false);
				
				if(Close[0] > Open[0])
				{
					EnterShort("ShortEntry1");
					EnterShort("ShortEntry2");
					EnterShort("ShortEntry3");
				}
				if(Open[0] > Close[0])
				{
					EnterLong("LongEntry1");
					EnterLong("LongEntry2");
					EnterLong("LongEntry3");
				}
			}
			
			if (Position.MarketPosition == MarketPosition.Long)
			{
				if(Close[0] >= Position.AveragePrice + 5 * TickSize)
				{
					SetStopLoss("LongEntry1", CalculationMode.Price, Position.AveragePrice, false);
					SetStopLoss("LongEntry2", CalculationMode.Price, Position.AveragePrice, false);
					SetStopLoss("LongEntry3", CalculationMode.Price, Position.AveragePrice, false);
				}
				
				if(Close[0] >= Position.AveragePrice + 10 * TickSize)
				{
					SetStopLoss("LongEntry1", CalculationMode.Price, Position.AveragePrice + 5 * TickSize, false);
					SetStopLoss("LongEntry2", CalculationMode.Price, Position.AveragePrice + 5 * TickSize, false);
					SetStopLoss("LongEntry3", CalculationMode.Price, Position.AveragePrice + 5 * TickSize, false);
				}
			}
			
			if (Position.MarketPosition == MarketPosition.Short)
			{
				if (Close[0] <= Position.AveragePrice - 5 * TickSize)
				{
					SetStopLoss("ShortEntry1", CalculationMode.Price, Position.AveragePrice, false);
					SetStopLoss("ShortEntry2", CalculationMode.Price, Position.AveragePrice, false);
					SetStopLoss("ShortEntry3", CalculationMode.Price, Position.AveragePrice, false);
				}
				
				if (Close[0] <= Position.AveragePrice - 10 * TickSize)
				{
					SetStopLoss("ShortEntry1", CalculationMode.Price, Position.AveragePrice - 5 * TickSize, false);
					SetStopLoss("ShortEntry2", CalculationMode.Price, Position.AveragePrice - 5 * TickSize, false);
					SetStopLoss("ShortEntry3", CalculationMode.Price, Position.AveragePrice - 5 * TickSize, false);
				}
			}
		}
	}
}
