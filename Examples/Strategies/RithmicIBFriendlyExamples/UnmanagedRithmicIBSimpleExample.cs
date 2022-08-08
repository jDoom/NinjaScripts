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
    public class UnmanagedRithmicIBSimpleExample : Strategy
    {
        private Order   longEntry, shortEntry, stopLossLong, stopLossShort, targetLong, targetShort;
        private string  oco;

        //private int     sumFilledLong; // This variable tracks the quantities of each execution making up the entry order
        //private int     sumFilledShort; // This variable tracks the quantities of each execution making up the entry order

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = @"This example demonstrates an Order/Execution/Position ordering agnostic strategy that will work well with all connection adapters.";
                Name                                        = "UnmanagedRithmicIBSimpleExample";
                Calculate                                   = Calculate.OnPriceChange;
                TraceOrders                                 = true;
                IsUnmanaged                                 = true;
                IsAdoptAccountPositionAware                 = true;
                // Disable this property for performance gains in Strategy Analyzer optimizations
                // See the Help Guide for additional information
                IsInstantiatedOnEachOptimizationIteration   = true;

                EntryDistance                               = 40;
                ProfitDistance                              = 80;
                StopDistance                                = 40;
            }
            else if (State == State.Realtime)
            {
                // convert any old historical order object references
                // to the new live order submitted to the real-time account
                if (shortEntry != null)
                    shortEntry = GetRealtimeOrder(shortEntry);
                if (longEntry != null)
                    longEntry = GetRealtimeOrder(longEntry);
                if (targetLong != null)
                    targetLong = GetRealtimeOrder(targetLong);
                if (targetShort != null)
                    targetShort = GetRealtimeOrder(targetShort);
                if (stopLossShort != null)
                    stopLossShort = GetRealtimeOrder(stopLossShort);
                if (stopLossLong != null)
                    stopLossLong = GetRealtimeOrder(stopLossLong);
            }
        }

        protected override void OnBarUpdate()
        {
            // Submit OCO entry limit orders if we currently don't have an entry order open
            if (Position.MarketPosition == MarketPosition.Flat)
            {
                /* The entry orders objects will take on a unique ID from our SubmitOrderUnmanaged() that we can use
                later for order identification purposes in the OnOrderUpdate() and OnExecution() methods. */
                if (State == State.Historical)
                    oco = DateTime.Now.ToString() + CurrentBar + "entry";
                else
                    oco = GetAtmStrategyUniqueId() + "entry";

                if (shortEntry == null)
                    SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, 20, High[0] + EntryDistance * TickSize, 0, oco, "Short limit entry");
                if (longEntry == null)
                    SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, 20, Low[0] - EntryDistance * TickSize, 0, oco, "Long limit entry");
            }
        }

        protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
            // Strategy Position changes by Executions. We can check if we are Long/Short here to protect our position based on the strategy Position.
            // If we want to have separate targets and stops based on separate entry orders, we need to have execution.Order, 
            // and thus get into a situation where order/execution/position ordering can leave an unprotected position with partial fills (When Order update comes after Execution)
            if (Position.MarketPosition == MarketPosition.Long && marketPosition == MarketPosition.Long)
            {
                if (stopLossLong == null && targetLong == null)
                {
                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "LongExits";
                    else
                        oco = GetAtmStrategyUniqueId() + "LongExits";

                    // Directly assign order objects for target and stop, to ensure target/stop Orders are assigned before we see additional Executions, which can result in multiple orders firing instead of updating the orders.
                    // We need to do this to be able to branch for ChangeOrders vs. new order submissions.
                    stopLossLong = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, Position.Quantity, 0, Position.AveragePrice - StopDistance * TickSize, oco, "StopLossLong");
                    targetLong = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, Position.Quantity, Position.AveragePrice + ProfitDistance * TickSize, 0, oco, "TargetLong");
                }
                else
                {
                    ChangeOrder(stopLossLong, Position.Quantity, 0, Position.AveragePrice - StopDistance * TickSize);
                    ChangeOrder(targetLong, Position.Quantity, Position.AveragePrice + ProfitDistance * TickSize, 0);
                }
            }

            if (Position.MarketPosition == MarketPosition.Short && marketPosition == MarketPosition.Short)
            {
                if (stopLossShort == null && targetShort == null)
                {
                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "LongExits";
                    else
                        oco = GetAtmStrategyUniqueId() + "LongExits";

                    // Directly assign order objects for target and stop, to ensure target/stop Orders are assigned before we see additional Executions, which can result in multiple orders firing instead of updating the orders.
                    // We need to do this to be able to branch for ChangeOrders vs. new order submissions.
                    stopLossShort = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, Position.Quantity, 0, Position.AveragePrice + StopDistance * TickSize, oco, "StopLossShort");
                    targetShort = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, Position.Quantity, Position.AveragePrice - ProfitDistance * TickSize, 0, oco, "TargetShort");
                }
                else
                {
                    ChangeOrder(stopLossShort, Position.Quantity, 0, Position.AveragePrice + StopDistance * TickSize);
                    ChangeOrder(targetShort, Position.Quantity, Position.AveragePrice - ProfitDistance * TickSize, 0);
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            // Assign Order objects here
            // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
            if (order.Name == "Short limit entry")
                shortEntry = order;
            else if (order.Name == "Long limit entry")
                longEntry = order;
            else if (order.Name == "StopLossLong")
                stopLossLong = order;
            else if (order.Name == "TargetLong")
                targetLong = order;
            else if (order.Name == "StopLossShort")
                stopLossShort = order;
            else if (order.Name == "TargetShort")
                targetShort = order;

            if (longEntry != null && longEntry == order)
            {
                // Reset the longTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry = null;

                // Null if filled
                if (order.OrderState == OrderState.Filled)
                    longEntry = null;
            }

            if (shortEntry != null && shortEntry == order)
            {
                // Reset the shortTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    shortEntry = null;

                // Null if filled
                if (order.OrderState == OrderState.Filled)
                    shortEntry = null;
            }

            //sets all targets and stops to null if one of them is canceled
            //PLEASE NOTE: setting IOrders to null ***DOES NOT*** cancel them
            if ((targetLong != null && targetLong == order)
                || (stopLossLong != null && stopLossLong == order)
                || (targetShort != null && targetShort == order)
                || (stopLossShort != null && stopLossShort == order)
                )
            {
                // Null if cancelled (these orders are OCO and will cancel on their own if another fills/cancels
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong = stopLossLong = targetShort = stopLossShort = null;

                // Null if filled (these orders are OCO and will cancel on their own if another fills/cancels
                if (order.OrderState == OrderState.Filled)
                    targetLong = stopLossLong = targetShort = stopLossShort = null;
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
