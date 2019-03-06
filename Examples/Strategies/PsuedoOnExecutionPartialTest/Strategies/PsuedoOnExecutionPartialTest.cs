// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Strategies
{
	public class PsuedoOnExecutionPartialTest : Strategy
	{
		private Order entryOrder1						= null; // This variable holds an object representing our entry order
		private Order stopOrder1                        = null; // This variable holds an object representing our stop loss order
		private Order targetOrder1                      = null; // This variable holds an object representing our profit target order

		private Order entryOrder2						= null; // This variable holds an object representing our entry order
		private Order stopOrder2                        = null; // This variable holds an object representing our stop loss order
		private Order targetOrder2                      = null; // This variable holds an object representing our profit target order
		
		private int Quantity1							= 2000000;
		private int Quantity2							= 2000000;
		
		private Order queuedOrder1 = null;
		private bool orderQueued1 = false;
		private DateTime queuedTime1;
		
		private Order queuedOrder2 = null;
		private bool orderQueued2 = false;
		private DateTime queuedTime2;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description								= @"Sample Using OnOrderUpdate() and OnExecution() methods to submit protective orders";
				Name                                    = "PsuedoOnExecutionPartialTest";
				Calculate                               = Calculate.OnBarClose;
				EntriesPerDirection                     = 2;
				EntryHandling                           = EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy            = true;
				ExitOnSessionCloseSeconds               = 30;
				IsFillLimitOnTouch                      = false;
				MaximumBarsLookBack                     = MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution                     = OrderFillResolution.Standard;
				Slippage                                = 0;
				StartBehavior                           = StartBehavior.WaitUntilFlat;
				TimeInForce                             = TimeInForce.Gtc;
				TraceOrders                             = true;
				RealtimeErrorHandling                   = RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling                      = StopTargetHandling.ByStrategyPosition;
				BarsRequiredToTrade                     = 20;
			}
			else if (State == State.Realtime)
			{
				// one time only, as we transition from historical
			    // convert any old historical order object references
			    // to the new live order submitted to the real-time account
			    if (entryOrder1 != null)
			        entryOrder1 = GetRealtimeOrder(entryOrder1);
				if (stopOrder1 != null)
			        stopOrder1 = GetRealtimeOrder(stopOrder1);
				if (targetOrder1 != null)
			        targetOrder1 = GetRealtimeOrder(targetOrder1);
				
				if (entryOrder2 != null)
			        entryOrder2 = GetRealtimeOrder(entryOrder2);
				if (stopOrder2 != null)
			        stopOrder2 = GetRealtimeOrder(stopOrder2);
				if (targetOrder2 != null)
			        targetOrder2 = GetRealtimeOrder(targetOrder2);
			}
		}

		protected override void OnBarUpdate()
		{
			if(State == State.Historical)
				return;
			// Submit an entry market order if we currently don't have an entry order open and are past the BarsRequiredToTrade bars amount
			if (entryOrder1 == null && CurrentBar > BarsRequiredToTrade)
			{
				/* Enter Long. We will assign the resulting Order object to entryOrder1 in OnOrderUpdate() */
				EnterLong(Quantity1, "MyEntry1");
			}
			
			// Separate entry condition because we want to be sure entry methods are submitted if they are null
			if (entryOrder2 == null && CurrentBar > BarsRequiredToTrade)
			{
				/* Enter Long. We will assign the resulting Order object to entryOrder2 in OnOrderUpdate() */
				EnterLong(Quantity2, "MyEntry2");
			}
		}

		protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
		{
			// Handle entry orders here. The entryOrder object allows us to identify that the order that is calling the OnOrderUpdate() method is the entry order.
			// Assign entryOrder in OnOrderUpdate() to ensure the assignment occurs when expected.
            // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not gauranteed to be complete if it is referenced immediately after submitting
			if (order.Name == "MyEntry1")
    		{	
        		entryOrder1 = order;
				
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
				{
					queuedOrder1 = order;
					orderQueued1 = true;
					queuedTime1 = time;
				}

                // Reset the entryOrder object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
					entryOrder1 = null;
			}
			
			// Assign second order entry
			if (order.Name == "MyEntry2")
    		{	
        		entryOrder2 = order;
				
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
				{
					queuedOrder2 = order;
					orderQueued2 = true;
					queuedTime2 = time;
				}

                // Reset the entryOrder object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
					entryOrder2 = null;
			}
		}
		
		private void psuedoExecutionUpdate(Order order, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
		{
			/* We advise monitoring OnExecution to trigger submission of stop/target orders instead of OnOrderUpdate() since OnExecution() is called after OnOrderUpdate()
			which ensures your strategy has received the execution which is used for internal signal tracking. */
			if (entryOrder1 != null && entryOrder1 == order)
			{
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled || (order.OrderState == OrderState.Cancelled && order.Filled > 0))
				{
					// Stop-Loss order 4 ticks below our entry price
					stopOrder1 = ExitLongStopMarket(0, true, order.Filled, 0.1, "MyStop1", "MyEntry1");

					// Target order 8 ticks above our entry price
					//targetOrder1 = ExitLongLimit(0, true, order.Filled, order.AverageFillPrice + 80 * TickSize, "MyTarget", "MyEntry1");

					// Resets the entryOrder object to null after the order has been filled
					if (order.OrderState != OrderState.PartFilled)
						entryOrder1 = null;
				}
			}
			
			// Place profit target and stop loss for second entry
			if (entryOrder2 != null && entryOrder2 == order)
			{
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled || (order.OrderState == OrderState.Cancelled && order.Filled > 0))
				{
					// Stop-Loss order 4 ticks below our entry price
					stopOrder2 = ExitLongStopMarket(0, true, order.Filled, 0.1, "MyStop2", "MyEntry2");

					// Target order 8 ticks above our entry price
					//targetOrder2 = ExitLongLimit(0, true, order.Filled, order.AverageFillPrice , "MyTarget", "MyEntry2");

					// Resets the entryOrder object to null after the order has been filled
					if (order.OrderState != OrderState.PartFilled)
						entryOrder2 = null;
				}
			}

			// Reset our stop order and target orders' Order objects after our position is closed. (1st Entry)
			if ((stopOrder1 != null && stopOrder1 == order) || (targetOrder1 != null && targetOrder1 == order))
			{
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
				{
					stopOrder1 = null;
					targetOrder1 = null;
				}
			}
			
			// Reset our stop order and target orders' Order objects after our position is closed. (2nd Entry)
			if ((stopOrder2 != null && stopOrder2 == order) || (targetOrder2 != null && targetOrder2 == order))
			{
				if (order.OrderState == OrderState.Filled || order.OrderState == OrderState.PartFilled)
				{
					stopOrder2 = null;
					targetOrder2 = null;
				}
			}
		}

		protected override void OnPositionUpdate(Position position, double averagePrice, int quantity, MarketPosition marketPosition)
		{
			if (orderQueued1)
			{
				psuedoExecutionUpdate(queuedOrder1, queuedOrder1.AverageFillPrice, queuedOrder1.Filled, queuedOrder1.IsLong ? MarketPosition.Long: MarketPosition.Short, queuedOrder1.OrderId, queuedTime1);
				orderQueued1 = false;
				queuedOrder1 = null;
			}
			
			if (orderQueued2)
			{
				psuedoExecutionUpdate(queuedOrder2, queuedOrder2.AverageFillPrice, queuedOrder2.Filled, queuedOrder2.IsLong ? MarketPosition.Long: MarketPosition.Short, queuedOrder2.OrderId, queuedTime2);
				orderQueued2 = false;
				queuedOrder2 = null;
			}
		}

		#region Properties
		#endregion
	}
}