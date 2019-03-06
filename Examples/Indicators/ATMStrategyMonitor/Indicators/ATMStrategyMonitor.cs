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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ATMStrategyMonitor : Indicator
	{
		private Account myAcct;
		private Order shortEntry;
		private Order longEntry;
		private Order shortExit;
		private Order longExit;
		
		private double shortEntryPrice;
		private double longEntryPrice;
		
		private Dictionary<string, ATMStrat> ATMStrats;
		
		public class ATMStrat
		{
			public ATMStrat(string id)
			{
				Id = id;
			}
			
			public Order Entry;
			public Order Exit;
			public string Id;
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ATMStrategyMonitor";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.DataLoaded)
			{
				lock (Account.All)

              	myAcct = Account.All.FirstOrDefault(a => a.Name == "Sim101");
				myAcct.OrderUpdate += OnOrderUpdate;
				
				ATMStrats = new Dictionary<string, ATMStrat>();
			}
			else if (State == State.Terminated)
			{
				if(myAcct != null)
					myAcct.OrderUpdate -= OnOrderUpdate;
			}
		}
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
		{
			if (e.Order.Account.Strategies.Count > 0) 
		    { 		
		        AtmStrategy atmStrategy;
				StrategyBase stratbase;
		        lock (e.Order.Account.Strategies)
				{
					stratbase = e.Order.Account.Strategies.FirstOrDefault(s => { lock (s.Orders) return s.Orders.FirstOrDefault(order => order == e.Order) != null; });
		          	atmStrategy = stratbase as AtmStrategy;
				}
		        if (atmStrategy != null)
				{
		            Print(atmStrategy.Template + " " + atmStrategy.Id + " " + atmStrategy.Name + " "  + e.Order.Name + " " + e.OrderState + " " + e.Order.OrderType.ToString() + " " + e.Order.OrderId);
					Print(stratbase.Id);
					Print("");
					
					bool newATM = true;
					foreach (ATMStrat strat in ATMStrats.Values)
						if(strat.Id == atmStrategy.Id.ToString())
							newATM = false;
						
					if(newATM)
						ATMStrats.Add(atmStrategy.Id.ToString(), new ATMStrat(atmStrategy.Id.ToString()));
					
					// Entry Submitted
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Entry" && e.OrderState == OrderState.Submitted && e.Order.OrderId != null && e.Order.IsShort)
					{
						Print("ATM1 entered short");
						shortEntry = e.Order;
						longEntry = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Entry = e.Order;
					}
					
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Entry" && e.OrderState == OrderState.Submitted && e.Order.OrderId != null && e.Order.IsLong)
					{
						Print("ATM1 entered long");
						longEntry = e.Order;
						shortEntry = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Entry = e.Order;
					}
					
					// Entry Filled
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Entry" && e.OrderState == OrderState.Filled && e.Order.OrderId != null && e.Order.IsShort)
					{
						shortEntryPrice = e.AverageFillPrice;
						ATMStrats[atmStrategy.Id.ToString()].Entry = e.Order;
					}
					
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Entry" && e.OrderState == OrderState.Filled && e.Order.OrderId != null && e.Order.IsLong)
					{
						longEntryPrice = e.AverageFillPrice;
						ATMStrats[atmStrategy.Id.ToString()].Entry = e.Order;
					}
					
					// Stop Submitted
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Stop1" && e.OrderState == OrderState.Submitted && e.Order.OrderId != null && e.Order.IsLong)
					{
						Print("Stop assigned");
						shortExit = e.Order;
						longExit = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Exit = e.Order;
					}
					
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Stop1" && e.OrderState == OrderState.Submitted && e.Order.OrderId != null && e.Order.IsShort)
					{
						Print("Stop assigned");
						longExit = e.Order;
						shortExit = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Exit = e.Order;
					}
					
					// Stop Filled
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Stop1" && e.OrderState == OrderState.Filled && e.Order.OrderId != null && e.Order.IsLong)
					{
						Print(String.Format("Realized PnL: {0}", shortExit.AverageFillPrice - shortEntryPrice));
						shortExit = shortEntry = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Exit = e.Order;
					}
					
					if(atmStrategy.Template == "ATM1" && e.Order.Name == "Stop1" && e.OrderState == OrderState.Filled && e.Order.OrderId != null && e.Order.IsShort)
					{
						Print(String.Format("Realized PnL: {0}", longExit.AverageFillPrice - longEntryPrice));
						longExit = longEntry = null;
						
						ATMStrats[atmStrategy.Id.ToString()].Exit = e.Order;
					}
				}
				// Check if Orders are in a twerminal state and set to null
				if(shortEntry != null)
				{
					if(Order.IsTerminalState(shortEntry.OrderState))
						shortEntry = null;
				}
				if(longEntry != null)
				{
					if(Order.IsTerminalState(longEntry.OrderState))
						longEntry = null;
				}
				if(shortExit != null)
				{
					if(Order.IsTerminalState(shortExit.OrderState))
						shortExit = null;
				}
				if(longExit != null)
				{
					if(Order.IsTerminalState(longExit.OrderState))
						longExit = null;
				}
		    }
		}

		protected override void OnBarUpdate()
		{
			if(longExit != null)
				Print(String.Format("Long Unrealized PnL: {0}", Close[0] - longEntryPrice));
			if(shortExit != null)
				Print(String.Format("Short Unrealized PnL: {0}", shortEntryPrice - Close[0]));
			
			foreach(ATMStrat strat in ATMStrats.Values)
				Print(String.Format("ATM: {0} Realized PnL {1}", strat.Id, (strat.Exit.AverageFillPrice - strat.Entry.AverageFillPrice)));
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ATMStrategyMonitor[] cacheATMStrategyMonitor;
		public ATMStrategyMonitor ATMStrategyMonitor()
		{
			return ATMStrategyMonitor(Input);
		}

		public ATMStrategyMonitor ATMStrategyMonitor(ISeries<double> input)
		{
			if (cacheATMStrategyMonitor != null)
				for (int idx = 0; idx < cacheATMStrategyMonitor.Length; idx++)
					if (cacheATMStrategyMonitor[idx] != null &&  cacheATMStrategyMonitor[idx].EqualsInput(input))
						return cacheATMStrategyMonitor[idx];
			return CacheIndicator<ATMStrategyMonitor>(new ATMStrategyMonitor(), input, ref cacheATMStrategyMonitor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATMStrategyMonitor ATMStrategyMonitor()
		{
			return indicator.ATMStrategyMonitor(Input);
		}

		public Indicators.ATMStrategyMonitor ATMStrategyMonitor(ISeries<double> input )
		{
			return indicator.ATMStrategyMonitor(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATMStrategyMonitor ATMStrategyMonitor()
		{
			return indicator.ATMStrategyMonitor(Input);
		}

		public Indicators.ATMStrategyMonitor ATMStrategyMonitor(ISeries<double> input )
		{
			return indicator.ATMStrategyMonitor(input);
		}
	}
}

#endregion
