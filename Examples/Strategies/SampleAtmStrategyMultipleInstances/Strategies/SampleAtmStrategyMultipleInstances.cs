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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SampleAtmStrategyMultipleInstances : Strategy
	{

		private Dictionary<int, string> atmStrategyIds;
		private Dictionary<int, string> orderIds;
		private Dictionary<int, bool>	isAtmStrategyCreated;
		private bool conditionToEnter;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description	= NinjaTrader.Custom.Resource.NinjaScriptStrategyDescriptionSampleATMStrategy;
				Name		= "SampleAtmStrategyMultipleInstances";
				// This strategy has been designed to take advantage of performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration = false;
			}
			else if (State == State.DataLoaded)
			{
				atmStrategyIds	= new Dictionary<int, string>();
				orderIds		= new Dictionary<int, string>();
				isAtmStrategyCreated = new Dictionary<int, bool>();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;

			// HELP DOCUMENTATION REFERENCE: Please see the Help Guide section "Using ATM Strategies" under NinjaScript--> Educational Resources--> http://ninjatrader.com/support/helpGuides/nt8/en-us/using_atm_strategies.htm

			// Make sure this strategy does not execute against historical data
			if(State == State.Historical)
				return;
			
			if(atmStrategyIds.Count == 0)
			{
				atmStrategyIds.Add(atmStrategyIds.Count + 1, String.Empty);
				orderIds.Add(orderIds.Count + 1, String.Empty);
				isAtmStrategyCreated.Add(isAtmStrategyCreated.Count + 1, false);
			}
			
			conditionToEnter = Close[0] > Open[0];
			
			bool needToCreateNew = false;
			
			for (int i = 1; i <= atmStrategyIds.Count; i++)
			{
				if (isAtmStrategyCreated[i] && conditionToEnter)
					needToCreateNew = true;
				CheckStartNewAtmStrategy(i, "ATM 1");
			}
			
			if (needToCreateNew)
			{
				atmStrategyIds.Add(atmStrategyIds.Count + 1, String.Empty);
				orderIds.Add(orderIds.Count + 1, String.Empty);
				isAtmStrategyCreated.Add(isAtmStrategyCreated.Count + 1, false);
				
				CheckStartNewAtmStrategy(atmStrategyIds.Count, "ATM 1");
			}
		}
		
		private void CheckStartNewAtmStrategy(int i, string templateName)
		{
			// Submits an entry limit order at the current low price to initiate an ATM Strategy if both order id and strategy id are in a reset state
			// **** YOU MUST HAVE AN ATM STRATEGY TEMPLATE NAMED 'AtmStrategyTemplate' CREATED IN NINJATRADER (SUPERDOM FOR EXAMPLE) FOR THIS TO WORK ****
			if (orderIds[i].Length == 0 && atmStrategyIds[i].Length == 0 && conditionToEnter)
			{
				isAtmStrategyCreated[i] = false;  // reset atm strategy created check to false
				atmStrategyIds[i] = GetAtmStrategyUniqueId();
				orderIds[i] = GetAtmStrategyUniqueId();
				AtmStrategyCreate(OrderAction.Buy, OrderType.Limit, Low[0], 0, TimeInForce.Day, orderIds[i], templateName, atmStrategyIds[i], (atmCallbackErrorCode, atmCallBackId) => {
					//check that the atm strategy create did not result in error, and that the requested atm strategy matches the id in callback
					if (atmCallbackErrorCode == ErrorCode.NoError && atmCallBackId == atmStrategyIds[i])
						isAtmStrategyCreated[i] = true;
				});
			}

			// Check that atm strategy was created before checking other properties
			if (!isAtmStrategyCreated[i])
				return;

			// Check for a pending entry order
			if (orderIds[i].Length > 0)
			{
				string[] status = GetAtmStrategyEntryOrderStatus(orderIds[i]);

				// If the status call can't find the order specified, the return array length will be zero otherwise it will hold elements
				if (status.GetLength(0) > 0)
				{
					// Print out some information about the order to the output window
					Print("The entry order average fill price is: " + status[0]);
					Print("The entry order filled amount is: " + status[1]);
					Print("The entry order order state is: " + status[2]);

					// If the order state is terminal, reset the order id value
					if (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected")
						orderIds[i] = string.Empty;
				}
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyIds[i].Length > 0 && GetAtmStrategyMarketPosition(atmStrategyIds[i]) == Cbi.MarketPosition.Flat)
				atmStrategyIds[i] = string.Empty;

			if (atmStrategyIds[i].Length > 0)
			{
				// You can change the stop price
				if (GetAtmStrategyMarketPosition(atmStrategyIds[i]) != MarketPosition.Flat)
					AtmStrategyChangeStopTarget(0, Low[0] - 3 * TickSize, "STOP1", atmStrategyIds[i]);

				// Print some information about the strategy to the output window, please note you access the ATM strategy specific position object here
				// the ATM would run self contained and would not have an impact on your NinjaScript strategy position and PnL
				Print("The current ATM Strategy market position is: " + GetAtmStrategyMarketPosition(atmStrategyIds[i]));
				Print("The current ATM Strategy position quantity is: " + GetAtmStrategyPositionQuantity(atmStrategyIds[i]));
				Print("The current ATM Strategy average price is: " + GetAtmStrategyPositionAveragePrice(atmStrategyIds[i]));
				Print("The current ATM Strategy Unrealized PnL is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyIds[i]));
			}
		}
	}
}
