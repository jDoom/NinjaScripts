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
    public class UnmanagedRithmicIBFriendlyMultipleEntriesExample : Strategy
    {
        private Order shortEntry1       = null;
        private Order longEntry1        = null;
        private Order targetLong1       = null;
        private Order targetShort1      = null;
        private Order stopLossShort1    = null;
        private Order stopLossLong1     = null;

        private Order shortEntry2       = null;
        private Order longEntry2        = null;
        private Order targetLong2       = null;
        private Order targetShort2      = null;
        private Order stopLossShort2    = null;
        private Order stopLossLong2     = null;

        private string oco;

        private int sumFilledLong1 = 0; // This variable tracks the quantities of each execution making up the entry order
        private int sumFilledShort1 = 0; // This variable tracks the quantities of each execution making up the entry order
        private int sumFilledLong2 = 0; // This variable tracks the quantities of each execution making up the entry order
        private int sumFilledShort2 = 0; // This variable tracks the quantities of each execution making up the entry order

        private int entryQuantity1 = 10;
        private int entryQuantity2 = 10;

        private List<double> LongEntry1Prices;
        private List<double> ShortEntry1Prices;
        private List<double> LongEntry2Prices;
        private List<double> ShortEntry2Prices;

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description                                 = @"This example demonstrates an Order/Execution/Position ordering agnostic strategy that will work well with all connection adapters. Note: You need to calculate an entry's Average Fill Price from Executions";
                Name                                        = "UnmanagedRithmicIBFriendlyMultipleEntriesExample";
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
                IsUnmanaged                                 = true;
                IsAdoptAccountPositionAware                 = true;
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
                if (shortEntry1 != null)
                    shortEntry1 = GetRealtimeOrder(shortEntry1);
                if (longEntry1 != null)
                    longEntry1 = GetRealtimeOrder(longEntry1);
                if (targetLong1 != null)
                    targetLong1 = GetRealtimeOrder(targetLong1);
                if (targetShort1 != null)
                    targetShort1 = GetRealtimeOrder(targetShort1);
                if (stopLossShort1 != null)
                    stopLossShort1 = GetRealtimeOrder(stopLossShort1);
                if (stopLossLong1 != null)
                    stopLossLong1 = GetRealtimeOrder(stopLossLong1);

                if (shortEntry2 != null)
                    shortEntry2 = GetRealtimeOrder(shortEntry2);
                if (longEntry2 != null)
                    longEntry2 = GetRealtimeOrder(longEntry2);
                if (targetLong2 != null)
                    targetLong2 = GetRealtimeOrder(targetLong2);
                if (targetShort2 != null)
                    targetShort2 = GetRealtimeOrder(targetShort2);
                if (stopLossShort2 != null)
                    stopLossShort2 = GetRealtimeOrder(stopLossShort2);
                if (stopLossLong2 != null)
                    stopLossLong2 = GetRealtimeOrder(stopLossLong2);
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
                    oco = DateTime.Now.ToString() + CurrentBar + "entry1";
                else
                    oco = GetAtmStrategyUniqueId() + "entry1";

                if (shortEntry1 == null)
                    SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, entryQuantity1, High[0] + EntryDistance * TickSize, 0, oco, "Short limit entry 1");
                if (longEntry1 == null)
                    SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, entryQuantity1, Low[0] - EntryDistance * TickSize, 0, oco, "Long limit entry 1");

                if (State == State.Historical)
                    oco = DateTime.Now.ToString() + CurrentBar + "entry2";
                else
                    oco = GetAtmStrategyUniqueId() + "entry2";

                if (shortEntry2 == null)
                    SubmitOrderUnmanaged(0, OrderAction.SellShort, OrderType.Limit, entryQuantity2, High[0] + EntryDistance / 2 * TickSize, 0, oco, "Short limit entry 2");
                if (longEntry2 == null)
                    SubmitOrderUnmanaged(0, OrderAction.Buy, OrderType.Limit, entryQuantity2, Low[0] - EntryDistance / 2 * TickSize, 0, oco, "Long limit entry 2");
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
                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "LongExits1";
                    else
                        oco = GetAtmStrategyUniqueId() + "LongExits1";

                    stopLossLong1 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, sumFilledLong1, 0, averageEntryPrice - StopDistance * TickSize, oco, "StopLossLong 1");
                    targetLong1 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, sumFilledLong1, averageEntryPrice + ProfitDistance * TickSize, 0, oco, "TargetLong 1");
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
            if (execution.Name == "Short limit entry 1")
            {
                // We sum the quantities of each execution making up the entry order
                sumFilledShort1 += execution.Quantity;

                if (ShortEntry1Prices.IsNullOrEmpty())
                    ShortEntry1Prices = new List<double>();

                for (int i = 0; i < execution.Quantity; i++)
                    ShortEntry1Prices.Add(execution.Price);

                double averageEntryPrice = 0;
                for (int i = 0; i < ShortEntry1Prices.Count; i++)
                    averageEntryPrice += ShortEntry1Prices[i];
                averageEntryPrice /= ShortEntry1Prices.Count;

                if (stopLossShort1 == null && targetShort1 == null)
                {

                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "ShortExits1";
                    else
                        oco = GetAtmStrategyUniqueId() + "ShortExits1";

                    stopLossShort1 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, sumFilledShort1, 0, averageEntryPrice + StopDistance * TickSize, oco, "StopLossShort 1");
                    targetShort1 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, sumFilledShort1, averageEntryPrice - ProfitDistance * TickSize, 0, oco, "TargetShort 1");
                }
                else
                {
                    ChangeOrder(stopLossShort1, sumFilledShort1, 0, averageEntryPrice + StopDistance * TickSize);
                    ChangeOrder(targetShort1, sumFilledShort1, averageEntryPrice - ProfitDistance * TickSize, 0);
                }

                // Entry Order filled, and stops and targets submitted. Reset Price list and execution count, and running filled quantity
                if (sumFilledShort1 == entryQuantity1)
                {
                    // Move to Class?
                    sumFilledShort1 = 0;
                    ShortEntry1Prices.Clear();
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
                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "LongExits2";
                    else
                        oco = GetAtmStrategyUniqueId() + "LongExits2";

                    stopLossLong2 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.StopMarket, sumFilledLong2, 0, averageEntryPrice - StopDistance * TickSize, oco, "StopLossShort 2");
                    targetLong2 = SubmitOrderUnmanaged(0, OrderAction.Sell, OrderType.Limit, sumFilledLong2, averageEntryPrice + ProfitDistance * TickSize, 0, oco, "TargetShort 2");
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
            if (execution.Name == "Short limit entry 2")
            {
                // We sum the quantities of each execution making up the entry order
                sumFilledShort2 += execution.Quantity;

                if (ShortEntry2Prices.IsNullOrEmpty())
                    ShortEntry2Prices = new List<double>();

                for (int i = 0; i < execution.Quantity; i++)
                    ShortEntry2Prices.Add(execution.Price);

                double averageEntryPrice = 0;
                for (int i = 0; i < ShortEntry2Prices.Count; i++)
                    averageEntryPrice += ShortEntry2Prices[i];
                averageEntryPrice /= ShortEntry2Prices.Count;

                if (stopLossShort2 == null && targetShort2 == null)
                {
                    if (State == State.Historical)
                        oco = DateTime.Now.ToString() + CurrentBar + "ShortExits2";
                    else
                        oco = GetAtmStrategyUniqueId() + "ShortExits2";

                    stopLossShort2 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.StopMarket, sumFilledShort2, 0, averageEntryPrice + StopDistance * TickSize, oco, "StopLossShort 2");
                    targetShort2 = SubmitOrderUnmanaged(0, OrderAction.BuyToCover, OrderType.Limit, sumFilledShort2, averageEntryPrice - ProfitDistance * TickSize, 0, oco, "TargetShort 2");
                }
                else
                {
                    ChangeOrder(stopLossShort2, sumFilledShort2, 0, averageEntryPrice + StopDistance * TickSize);
                    ChangeOrder(targetShort2, sumFilledShort2, averageEntryPrice - ProfitDistance * TickSize, 0);
                }

                // Entry Order filled, and stops and targets submitted. Reset Price list and execution count, and running filled quantity
                if (sumFilledShort2 == entryQuantity2)
                {
                    // Move to Class?
                    sumFilledShort2 = 0;
                    ShortEntry2Prices.Clear();
                }
            }
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
            // Assign Order objects here
            // This is more reliable than assigning Order objects in OnBarUpdate, as the assignment is not guaranteed to be complete if it is referenced immediately after submitting
            if (order.Name == "Short limit entry 1")
                shortEntry1 = order;
            else if (order.Name == "Long limit entry 1")
                longEntry1 = order;
            else if (order.Name == "StopLossLong 1")
                stopLossLong1 = order;
            else if (order.Name == "TargetLong 1")
                targetLong1 = order;
            else if (order.Name == "StopLossShort 1")
                stopLossShort1 = order;
            else if (order.Name == "TargetShort 1")
                targetShort1 = order;

            if (longEntry1 != null && longEntry1 == order)
            {
                // Reset the longTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry1 = null;
                if (order.OrderState == OrderState.Filled)
                    longEntry1 = null;
            }

            if (shortEntry1 != null && shortEntry1 == order)
            {
                // Reset the shortTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    shortEntry1 = null;
                if (order.OrderState == OrderState.Filled)
                    shortEntry1 = null;
            }

            //sets all targets and stops to null if one of them is canceled
            //PLEASE NOTE: setting IOrders to null ***DOES NOT*** cancel them
            if ((targetLong1 != null && targetLong1 == order)
                || (stopLossLong1 != null && stopLossLong1 == order)
                || (targetShort1 != null && targetShort1 == order)
                || (stopLossShort1 != null && stopLossShort1 == order)
                )
            {
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong1 = stopLossLong1 = targetShort1 = stopLossShort1 = null;
                if (order.OrderState == OrderState.Filled)
                    targetLong1 = stopLossLong1 = targetShort1 = stopLossShort1 = null;
            }

            // Second entry biz

            if (order.Name == "Short limit entry 2")
                shortEntry2 = order;
            else if (order.Name == "Long limit entry 2")
                longEntry2 = order;
            else if (order.Name == "StopLossLong 2")
                stopLossLong2 = order;
            else if (order.Name == "TargetLong 2")
                targetLong2 = order;
            else if (order.Name == "StopLossShort 2")
                stopLossShort2 = order;
            else if (order.Name == "TargetShort 2")
                targetShort2 = order;

            if (longEntry2 != null && longEntry2 == order)
            {
                // Reset the longTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    longEntry2 = null;
                if (order.OrderState == OrderState.Filled)
                    longEntry2 = null;
            }

            if (shortEntry2 != null && shortEntry2 == order)
            {
                // Reset the shortTop object to null if order was cancelled without any fill
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    shortEntry2 = null;
                if (order.OrderState == OrderState.Filled)
                    shortEntry2 = null;
            }

            //sets all targets and stops to null if one of them is canceled
            //PLEASE NOTE: setting IOrders to null ***DOES NOT*** cancel them
            if ((targetLong2 != null && targetLong2 == order)
                || (stopLossLong2 != null && stopLossLong2 == order)
                || (targetShort2 != null && targetShort2 == order)
                || (stopLossShort2 != null && stopLossShort2 == order)
                )
            {
                // Reset to null if cancelled
                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
                    targetLong2 = stopLossLong2 = targetShort2 = stopLossShort2 = null;
                // Reset to null if filled. (We do not use the filled order object in OnExecutionUpdate, so we can null it
                if (order.OrderState == OrderState.Filled)
                    targetLong2 = stopLossLong2 = targetShort2 = stopLossShort2 = null;
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
