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
	public class StrategyWithTradeOrganizer : Strategy
	{
		[XmlIgnore]
		public Dictionary<string, TradeOrganizer> TradeOrganizers;

        protected override void OnStateChange()
        {
			base.OnStateChange();
			if (State == State.DataLoaded)
			{
				TradeOrganizers = new Dictionary<string, TradeOrganizer>();
			}
            else if (State == State.Realtime)
            {
				foreach (TradeOrganizer tradeOrganizer in TradeOrganizers.Values)
               		tradeOrganizer.HandleRealtimeTransition();
            }
		}
		
		protected override void OnExecutionUpdate(Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time)
        {
			foreach (TradeOrganizer tradeOrganizer in TradeOrganizers.Values)
               	tradeOrganizer.HandleEntryExecution(execution);
        }

        protected override void OnOrderUpdate(Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError)
        {
			foreach (TradeOrganizer tradeOrganizer in TradeOrganizers.Values)
               	tradeOrganizer.UpdateOrderObjects(order);
        }
		
		/// <summary>
		/// Creates a TradeOrganizer object
		/// </summary>
		/// <param name="owner">Owning strategy used for order submissions and changes</param>
		/// <param name="entryName">Name of the Entry Order</param>
		/// <param name="targetDistanceTicks">Profit Target distance in ticks. Use 0 for no Profit Target</param>
		/// <param name="stopDistanceTicks">Stop Loss distance in ticks. Use 0 for no Stop Loss</param>
		/// <param name="isShort">Makes stop and target distance negative for short protective orders</param>
		/// <param name="barsInProgressIndex">Determines the data series in which the orders should be submitted. This should match the BarsInProgress index in which the entry order is submitted to.</param>
		/// <returns></returns>
		public void AddTradeOrganizer(StrategyBase owner, string entryName, int targetDistanceTicks, int stopDistanceTicks, bool isShort, int barsInProgressIndex)
		{
			TradeOrganizers.Add(entryName, new TradeOrganizer(owner, entryName, targetDistanceTicks, stopDistanceTicks, isShort, barsInProgressIndex));
		}
		
		public class TradeOrganizer
		{
			/// <summary>
			/// Creates a TradeOrganizer object
			/// </summary>
			/// <param name="owner">Owning strategy used for order submissions and changes</param>
			/// <param name="entryName">Name of the Entry Order</param>
			/// <param name="targetDistanceTicks">Profit Target distance in ticks. Use 0 for no Profit Target</param>
			/// <param name="stopDistanceTicks">Stop Loss distance in ticks. Use 0 for no Stop Loss</param>
			/// <param name="isShort">Makes stop and target distance negative for short protective orders</param>
			/// <param name="barsInProgressIndex">Determines the data series in which the orders should be submitted. This should match the BarsInProgress index in which the entry order is submitted to.</param>
			/// <returns></returns>
			public TradeOrganizer(StrategyBase owner, string entryName, int targetDistanceTicks, int stopDistanceTicks, bool isShort, int barsInProgressIndex)
			{
				entryPricesPerUnit = new List<double>();
				
				EntryName 		= entryName;
				TargetName 		= EntryName + " Profit Target";
				StopName 		= EntryName + " Stop Loss";
				
				targetDistance 	= Math.Abs(targetDistanceTicks);
				stopDistance	= Math.Abs(stopDistanceTicks);
				ownerStrategy	= owner;
				barsInProgress 	= barsInProgressIndex;
				
				if(isShort)
				{
					targetDistance 	= -targetDistance;
					stopDistance 	= -stopDistance;
				}
			}
			
			/// <summary>
			/// Submit Profit Target and Stop Loss if the executing order is the Entry order
			/// </summary>
			/// <param name="execution">Execution given from OnExecutionUpdate</param>
			/// <returns></returns>
			public void HandleEntryExecution(Execution execution)
			{
				if (execution.Name != EntryName)
					return;
				
                CurrentEntryFilled += execution.Quantity;

                for (int i = 0; i < execution.Quantity; i++)
                    entryPricesPerUnit.Add(execution.Price);

                for (int i = 0; i < entryPricesPerUnit.Count; i++)
                    AverageEntryPrice += entryPricesPerUnit[i];
                AverageEntryPrice /= entryPricesPerUnit.Count;
				
				if (ProfitTarget == null && StopLoss == null)
                {
					string oco = String.Empty;
					
					if (ownerStrategy.State == State.Historical)
                        oco = DateTime.Now.ToString() + ownerStrategy.CurrentBars[barsInProgress] + EntryName + "Exits";
                    else
                        oco = ownerStrategy.GetAtmStrategyUniqueId() + EntryName + "Exits";
					
					if (stopDistance != 0)
						if (ownerStrategy.IsUnmanaged)
							StopLoss = ownerStrategy.SubmitOrderUnmanaged(barsInProgress, OrderAction.Sell, OrderType.StopMarket, CurrentEntryFilled, 0, AverageEntryPrice - stopDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize, oco, StopName);
						else
                   			StopLoss = ownerStrategy.ExitLongStopMarket(barsInProgress, true, CurrentEntryFilled, AverageEntryPrice - stopDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize, StopName, EntryName);
					if (targetDistance != 0)
						if (ownerStrategy.IsUnmanaged)
							ProfitTarget = ownerStrategy.SubmitOrderUnmanaged(barsInProgress, OrderAction.Sell, OrderType.Limit, CurrentEntryFilled, AverageEntryPrice + targetDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize, 0, oco, TargetName);
						else
                    		ProfitTarget = ownerStrategy.ExitLongLimit(barsInProgress, true, CurrentEntryFilled, AverageEntryPrice + targetDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize, TargetName, EntryName);
                }
                else
                {
					if (StopLoss != null)
                    	ownerStrategy.ChangeOrder(StopLoss, CurrentEntryFilled, 0, AverageEntryPrice - stopDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize);
                    if (ProfitTarget != null)
						ownerStrategy.ChangeOrder(ProfitTarget, CurrentEntryFilled, AverageEntryPrice + targetDistance * ownerStrategy.Instruments[barsInProgress].MasterInstrument.TickSize, 0);
                }
			}
			
			/// <summary>
			/// Update Order objects within the helper class
			/// </summary>
			/// <param name="order">Order object given in OnOrderUpdate</param>
			/// <returns></returns>
			public void UpdateOrderObjects(Order order)
			{
				if (order.Name == EntryName)
	                Entry = order;
	            else if (order.Name == TargetName)
	                StopLoss = order;
	            else if (order.Name == StopName)
	                ProfitTarget = order;

	            // Null Entry order if filled or cancelled. We do not use the Order objects after the order is filled, so we can null it here
	            if (Entry != null && Entry == order)
	            {
	                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
	                    Entry = null;
	                if (order.OrderState == OrderState.Filled)
	                    Entry = null;
	            }

	            // Null Target and Stop orders if filled or cancelled. We do not use the Order objects after the order is filled, so we can null them here
	            if ((ProfitTarget != null && ProfitTarget == order)
	                || (StopLoss != null && StopLoss == order))
	            {
	                if (order.OrderState == OrderState.Cancelled && order.Filled == 0)
	                    ProfitTarget = StopLoss = null;
	                if (order.OrderState == OrderState.Filled)
					{
	                    ProfitTarget = StopLoss = null;
						CurrentEntryFilled = 0;
						AverageEntryPrice = 0;
						entryPricesPerUnit.Clear();
					}
	            }
			}
			
			/// <summary>
			/// Transition historical order objects to realtime order objects
			/// </summary>
			/// <returns></returns>
			public void HandleRealtimeTransition()
			{
				if (Entry != null)
                    Entry = ownerStrategy.GetRealtimeOrder(Entry);
                if (ProfitTarget != null)
                    ProfitTarget = ownerStrategy.GetRealtimeOrder(ProfitTarget);
                if (StopLoss != null)
                    StopLoss = ownerStrategy.GetRealtimeOrder(StopLoss);
			}
			
			/// <summary>
			/// Move Stop Loss to Average Entry Price
			/// </summary>
			/// <returns></returns>
			public void Breakeven()
			{
				if (StopLoss != null)
                   ownerStrategy.ChangeOrder(StopLoss, CurrentEntryFilled, 0, AverageEntryPrice);
			}
			
			/// <summary>
			/// Moves Stop Loss to price given
			/// </summary>
			/// <param name="price">Price to move stop</param>
			/// <returns></returns>
			public void MoveStopToPrice(double price)
			{
				if (StopLoss != null)
                   ownerStrategy.ChangeOrder(StopLoss, CurrentEntryFilled, 0, price);
			}
			
			/// <summary>
			/// Moves Profit Target to price given
			/// </summary>
			/// <param name="price">Price to move target</param>
			/// <returns></returns>
			public void MoveTargetToPrice(double price)
			{
				if (ProfitTarget != null)
                   ownerStrategy.ChangeOrder(ProfitTarget, CurrentEntryFilled, 0, price);
			}
			
			public string EntryName, TargetName, StopName;
			public int CurrentEntryFilled;
			public double AverageEntryPrice;
			public Order Entry, ProfitTarget, StopLoss;			
			
			private List<double> entryPricesPerUnit;
			private int targetDistance, stopDistance, barsInProgress;
			private StrategyBase ownerStrategy;
		}
	}
}
