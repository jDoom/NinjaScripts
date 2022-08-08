//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim
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
    public class ManagedRithmicIBFriendlySimpleExample : Strategy
    {
        private Order longEntry1    = null;
        private Order targetLong1   = null;
        private Order stopLossLong1 = null;

        private int sumFilledLong1  = 0; // This variable tracks the quantities of each execution making up the entry order
        private int entryQuantity1  = 100;

        private List<double> LongEntry1Prices;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = @"This example demonstrates an Order/Execution/Position ordering agnostic strategy that will work well with all connection adapters.";
                Name                                        = "ManagedRithmicIBFriendlySimpleExample";
                Calculate                                   = Calculate.OnBarClose;
                EntriesPerDirection                         = 1;
                EntryHandling                               = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy                = true;
                ExitOnSessionCloseSeconds                   = 30;
                IsFillLimitOnTouch                          = false;
                MaximumBarsLookBack                         = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution                         = OrderFillResolution.Standard;
                Slippage                                    = 0;
                StartBehavior                               = StartBehavior.WaitUntilFlat;
                TimeInForce                                 = TimeInForce.Gtc;
                TraceOrders                                 = true;
                RealtimeErrorHandling                       = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling                          = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade                         = 20;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration   = true;

                EntryDistance = 40;
                ProfitDistance = 80;
                StopDistance = 40;
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
            }
        }

        protected override void OnBarUpdate()
        {
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                if (longEntry1 == null)
                    EnterLongLimit(0, true, entryQuantity1, Low[0] - EntryDistance * TickSize, "Long limit entry 1");
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            // Strategy Position changes by Executions. We can check if we are Long/Short here to protect our position based on the strategy Position.
            // If we want to have separate targets and stops based on separate entry orders, we need to have execution.Order, 
            // and thus get into a situation where order/execution/position ordering can leave an unprotected position with partial fills (When Order update comes after Execution)

            // Below checks that we got into a Long Strategy Position, and the marketPosition of that execution is a Long/Buy execution.
            // So we are entering a position we bought, and will not confuse a Short/Sell execution when the Strategy Position is Long (For example, if we were selling to close a long position.)
            if (Position.MarketPosition == MarketPosition.Long && marketPosition == MarketPosition.Long)
            {
                if (stopLossLong1 == null && targetLong1 == null)
                {
                    // Directly assign order objects from the method's return value. This prevents us from overprotecting the position by making sure our code changes the orders, instead of submitting new orders
                    stopLossLong1 = ExitLongStopMarket(0, true, sumFilledLong1, Position.AveragePrice - StopDistance * TickSize, "StopLossLong1", "Long limit entry 1");
                    targetLong1 = ExitLongLimit(0, true, sumFilledLong1, Position.AveragePrice + ProfitDistance * TickSize, "TargetLong1", "Long limit entry 1");
                }
                else
                {
                    ChangeOrder(stopLossLong1, Position.Quantity, 0, Position.AveragePrice - StopDistance * TickSize);
                    ChangeOrder(targetLong1, Position.Quantity, Position.AveragePrice + ProfitDistance * TickSize, 0);
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            // Note: This approach differs from SampleOnOrderUpdate in that Order objects are nulled in OnOrderUpdate when we see they are filled. This is because we do not need to hold onto the filled orders in OnExecutionUpdate

            // Assign Order objects here
            // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
            if (order.Name == "Long limit entry 1")
                longEntry1 = order;
            else if (order.Name == "StopLossLong1")
                stopLossLong1 = order;
            else if (order.Name == "TargetLong1")
                targetLong1 = order;

            // Null Entry order if filled or cancelled. We do not use the Order objects after the order is filled, so we can null it here
            if (longEntry1 != null && longEntry1 == order)
            {
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry1 = null;
                if (order.OrderState == OrderState.Filled)
                    longEntry1 = null;
            }

            // Null Target and Stop orders if filled or cancelled. We do not use the Order objects after the order is filled, so we can null them here
            if ((targetLong1 != null && targetLong1 == order)
                || (stopLossLong1 != null && stopLossLong1 == order))
            {
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong1 = stopLossLong1 = null;
                if (order.OrderState == OrderState.Filled)
                    targetLong1 = stopLossLong1 = null;
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
