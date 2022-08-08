//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim, NinjaTrader_BrandonH
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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies.OrderManagementExamples
{
    public class ManagedRithmicIBFriendlyMultipleEntriesExample : Strategy
    {        
        private Order longEntry1        = null;
        private Order targetLong1       = null;
        private Order stopLossLong1     = null;

        private Order longEntry2        = null;
        private Order targetLong2       = null;
        private Order stopLossLong2     = null;

        private int sumFilledLong1 = 0; // This variable tracks the quantities of each execution making up the entry order
        private int sumFilledLong2 = 0; // This variable tracks the quantities of each execution making up the entry order

        private int entryQuantity1 = 10;
        private int entryQuantity2 = 10;

        private List<double> LongEntry1Prices;
        private List<double> LongEntry2Prices;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = @"This example demonstrates an Order/Execution/Position ordering agnostic strategy that will work well with all connection adapters. Note: You need to calculate an entry's Average Fill Price from Executions";
                Name                                        = "ManagedRithmicIBFriendlyMultipleEntriesExample";
                Calculate                                   = Calculate.OnBarClose;
                EntriesPerDirection                         = 2;
                EntryHandling                               = EntryHandling.UniqueEntries;
                TraceOrders                                 = true;
                IsAdoptAccountPositionAware                 = true;

                EntryDistance 								= 40;
                ProfitDistance 								= 80;
                StopDistance 								= 40;
            }
            else if (State == State.Realtime)
            {
                // convert any old historical order object references
                // to the new live order submitted to the real-time account

                if (longEntry1 != null)
                    longEntry1 = GetRealtimeOrder(longEntry1);
                if (targetLong1 != null)
                    targetLong1 = GetRealtimeOrder(targetLong1);
                if (stopLossLong1 != null)
                    stopLossLong1 = GetRealtimeOrder(stopLossLong1);

                if (longEntry2 != null)
                    longEntry2 = GetRealtimeOrder(longEntry2);
                if (targetLong2 != null)
                    targetLong2 = GetRealtimeOrder(targetLong2);
                if (stopLossLong2 != null)
                    stopLossLong2 = GetRealtimeOrder(stopLossLong2);
            }
        }

        protected override void OnBarUpdate()
        {
            // Submit entry limit orders if we currently don't have an entry order open
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                if (longEntry1 == null)
				{
					EnterLongLimit(0, true, entryQuantity1, Low[0] - EntryDistance * TickSize,"Long limit entry 1");
				}
                    
                if (longEntry2 == null)
				{
					EnterLongLimit(0, true, entryQuantity2, Low[0] - EntryDistance / 2 * TickSize, "Long limit entry 2");
				}
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            /* We advise monitoring OnExecution to trigger submission of stop/target orders instead of OnOrderUpdate()
            since OnExecution() is called after OnOrderUpdate() which ensures your strategy has received the execution
            which is used for internal signal tracking. */
            if (execution.Name == "Long limit entry 1")
            {
                // We sum the quantities of each execution making up the entry order
                sumFilledLong1 += execution.Quantity;

                if (LongEntry1Prices.IsNullOrEmpty())
                    LongEntry1Prices = new List<double>();

                for (int i = 0; i < execution.Quantity; i++)
                    LongEntry1Prices.Add(execution.Price);

                double averageEntryPrice = 0;
                for (int i = 0; i < LongEntry1Prices.Count; i++)
                    averageEntryPrice += LongEntry1Prices[i];
                averageEntryPrice /= LongEntry1Prices.Count;


                if (stopLossLong1 == null && targetLong1 == null)
                {
                    stopLossLong1 = ExitLongStopMarket(0, true, sumFilledLong1, averageEntryPrice - StopDistance * TickSize, "StopLossLong 1", "Long limit entry 1");
					
                    targetLong1 = ExitLongLimit(0, true, sumFilledLong1, averageEntryPrice + ProfitDistance * TickSize, "TargetLong 1", "Long limit entry 1");
                }
                else
                {
                    ChangeOrder(stopLossLong1, sumFilledLong1, 0, averageEntryPrice - StopDistance * TickSize);
                    ChangeOrder(targetLong1, sumFilledLong1, averageEntryPrice + ProfitDistance * TickSize, 0);
                }

                // Entry Order filled, and stops and targets submitted. Reset Price list and execution count, and running filled quantity
                if (sumFilledLong1 == entryQuantity1)
                {
                    // Move to Class?
                    sumFilledLong1 = 0;
                    LongEntry1Prices.Clear();
                }
            }

            // Second entry biz
            if (execution.Name == "Long limit entry 2")
            {
                sumFilledLong2 += execution.Quantity;

                if (LongEntry2Prices.IsNullOrEmpty())
                    LongEntry2Prices = new List<double>();

                for (int i = 0; i < execution.Quantity; i++)
                    LongEntry2Prices.Add(execution.Price);

                double averageEntryPrice = 0;
                for (int i = 0; i < LongEntry2Prices.Count; i++)
                    averageEntryPrice += LongEntry2Prices[i];
                averageEntryPrice /= LongEntry2Prices.Count;

                if (stopLossLong2 == null && targetLong2 == null)
                {
                    stopLossLong2 = ExitLongStopMarket(0, true, sumFilledLong2, averageEntryPrice - StopDistance * TickSize, "StopLossShort 2", "Long limit entry 2");
                    targetLong2 = ExitLongLimit(0, true, sumFilledLong2, averageEntryPrice + ProfitDistance * TickSize, "TargetShort 2", "Long limit entry 2");
                }
                else
                {
                    ChangeOrder(stopLossLong2, sumFilledLong2, 0, averageEntryPrice - StopDistance * TickSize);
                    ChangeOrder(targetLong2, sumFilledLong2, averageEntryPrice + ProfitDistance * TickSize, 0);
                }

                // Entry Order filled, and stops and targets submitted. Reset Price list and execution count, and running filled quantity
                if (sumFilledLong2 == entryQuantity2)
                {
                    // Move to Class?
                    sumFilledLong2 = 0;
                    LongEntry2Prices.Clear();
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            // Assign Order objects here
            // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
            if (order.Name == "Long limit entry 1")
                longEntry1 = order;
            else if (order.Name == "StopLossLong 1")
                stopLossLong1 = order;
            else if (order.Name == "TargetLong 1")
                targetLong1 = order;

            if (longEntry1 != null && longEntry1 == order)
            {
                // Reset the longTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry1 = null;
                if (order.OrderState == OrderState.Filled)
                    longEntry1 = null;
            }

            //sets all targets and stops to null if one of them is canceled
            //PLEASE NOTE: setting IOrders to null ***DOES NOT*** cancel them
            if ((targetLong1 != null && targetLong1 == order)
                || (stopLossLong1 != null && stopLossLong1 == order))
            {
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong1 = stopLossLong1 = null;
                if (order.OrderState == OrderState.Filled)
                    targetLong1 = stopLossLong1 = null;
            }

            // Second entry biz

            if (order.Name == "Long limit entry 2")
                longEntry2 = order;
            else if (order.Name == "StopLossLong 2")
                stopLossLong2 = order;
            else if (order.Name == "TargetLong 2")
                targetLong2 = order;

            if (longEntry2 != null && longEntry2 == order)
            {
                // Reset the longTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry2 = null;
                if (order.OrderState == OrderState.Filled)
                    longEntry2 = null;
            }

            //sets all targets and stops to null if one of them is canceled
            //PLEASE NOTE: setting IOrders to null ***DOES NOT*** cancel them
            if ((targetLong2 != null && targetLong2 == order)
                || (stopLossLong2 != null && stopLossLong2 == order))
            {
                // Reset to null if cancelled
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong2 = stopLossLong2 = null;
                // Reset to null if filled. (We do not use the filled order object in OnExecutionUpdate, so we can null it
                if (order.OrderState == OrderState.Filled)
                    targetLong2 = stopLossLong2 = null;
            }
        }

        #region Properties
        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "EntryDistance", Description = "Distance of Entry Orders from High/Low", Order = 1, GroupName = "Parameters")]
        public int EntryDistance
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "ProfitDistance", Description = "Distance of Profit Target from AvgEntryPrice", Order = 2, GroupName = "Parameters")]
        public int ProfitDistance
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "StopDistance", Description = "Distance of Stop Loss from AvgEntryPrice", Order = 3, GroupName = "Parameters")]
        public int StopDistance
        { get; set; }
        #endregion
    }
}
