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
	[CategoryDefaultExpanded(false)]
	public class WCCI_AT : Strategy
	{
		private CCIForecasterV7DE ccif;
		
		private string	atmStrategyId			= string.Empty;
		private string	orderId					= string.Empty;
		private int 	entrybar;
		
		private int 		start_time; 	//Conversion variable for time to begin trading
		private int 		end_time; 		//Conversion variable for time to end trading
		private DateTime 	end_time1;
		private DateTime 	end_time2;
		private int 		start_time_1; 	//Conversion variable for time to begin trading
		private int 		end_time_1; 	//Conversion variable for time to end trading
		private DateTime 	end_time3;
		private DateTime 	end_time4;
		
		private string[] 	status;
		private int 		trade 				= 0;
		private int 		closeat 			= 0;
		
		private double 		projectedhigh;
		private double 		projectedlow;
		private double 		oldprojectedhigh 	= 0;
		private double 		oldprojectedlow 	= 0;
		
		private bool   isAtmStrategyCreated = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Woodies CCI AutoTrader";
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
				
				// Strategy Parameters
				ATMStrategy									= string.Empty;
				ATMAltStrategy								= string.Empty;
				TypeEntry									= OrderType.Limit;
				SnREntry									= OrderType.Market;
				LimitOffset									= 1;
				GstColor									= 1;
				StopOffset									= 1;
				TradeExitCross								= 100;
				ExitCandle									= false;
				Trade_Begin_Time1							= "9:30 AM";
				Trade_End_Time1								= "3:30 PM";
				Trade_Begin_Time2							= string.Empty;
				Trade_End_Time2								= "3:30 PM";
				Trade_ZLR									= true;
				Trade_Famir									= true;
				Trade_Vegas									= true;
				Trade_GB									= true;
				Trade_TT									= true;
				Trade_Ghost									= true;
				TakeLongTrades								= true;
				TakeShortTrades								= true;
				OffEndCandle								= true;
				SidewaysTrades								= false;
				StopReverse									= false;
				PrintStats									= true;
				
				// CCI Forecaster Parameters
				SideWinder0									= 60;
				RejectLevel									= 100;
				TrendLength									= 12;
				Periods										= 14;
				UsePointsAmount								= 10;
				UsePoints									= "Points";
				Extremes									= 185;
				FamirHookMin								= 5;
				FamirLimits									= 50;
				VTHFEPenetrationLevel						= 150;
				VMaxSwingPoint								= 150;
				VMaxSwingBars								= 10;
				GstColor									= 2.5;
				MinZLRPts									= 15;
				MinCCIPts									= 0;
				MaxSignalValue								= 120;
				MinZLRValue									= 0;
				ChopCrosses									= 4;
				UseJavierSideways							= false;
				Sidewaysboundary							= 100;
				SSZeroBand									= 33;
				SS100Band									= 33;
				SSCCITurboSeparation						= 75;
				MaxSSTurboValue								= 185;
			}
			else if (State == State.DataLoaded)
			{
				ccif = CCIForecasterV7DE(SideWinder0, RejectLevel, TrendLength, Periods, UsePointsAmount, UsePoints, Extremes, FamirHookMin, FamirLimits,
						VTHFEPenetrationLevel, VMaxSwingPoint, VMaxSwingBars, GstColor, MinZLRPts, MinCCIPts, MaxSignalValue, MinZLRValue, ChopCrosses, 
						UseJavierSideways, Sidewaysboundary, SSZeroBand, SS100Band, SSCCITurboSeparation, MaxSSTurboValue);
			}
		}

		protected override void OnBarUpdate()
        {
			// Make sure this strategy does not execute against historical data
			if (State == State.Historical) return;
			
			projectedhigh = Low[0]+BarsPeriod.Value*TickSize;
			projectedlow = High[0]-BarsPeriod.Value*TickSize;
			
			Print("================   Begin   ================");
			
 			start_time = ToTime(Convert.ToDateTime(Trade_Begin_Time1));//Convert String to Time for start time
			end_time = ToTime(Convert.ToDateTime(Trade_End_Time1).AddSeconds(-1));//ToTime(end_time2);//Finally convert end time to Time
			//Second trading session
			if(!string.IsNullOrEmpty(Trade_Begin_Time2))
			{	
				start_time_1 = ToTime(Convert.ToDateTime(Trade_Begin_Time2));
				end_time_1 = ToTime(Convert.ToDateTime(Trade_End_Time2).AddSeconds(-1));//ToTime(end_time2);//Finally convert end time to Time
			}
			
			Print(Time[0] + ",  " + Instrument.FullName + ",  " + "Bar #" + CurrentBar.ToString() +",  " + orderId.Length + ",  " + atmStrategyId.Length + "-" + (atmStrategyId.Length > 0 ? GetAtmStrategyMarketPosition(atmStrategyId).ToString() : "Flat") + ",  Trade Signal = " + trade + ",  Sideways Market?: " + ccif.SidewaysMkt[0] + ",  Trade in Sideways Market?: " + SidewaysTrades + 
			",  Start Time: " + start_time + ",  End Time: " + end_time + ",  Start Time 2: " + start_time_1 + ",  End Time 2: " + end_time_1);
			
			
			#region Check to see if there has been a 100 line cross or if our position has been closed
			if (atmStrategyId.Length > 0 && isAtmStrategyCreated)// Were we in a trade, and if so, are we still in it?  If not, reset orderId and atmStrategyId to String.Empty
			{
				if(orderId.Length >0) status = GetAtmStrategyEntryOrderStatus(orderId);
				//////////////////////////  100 Line Cross  //////////////////////////
				if((GetAtmStrategyMarketPosition(atmStrategyId)==MarketPosition.Long && CCI(14)[1] > TradeExitCross && CCI(14)[0] < TradeExitCross)
					||(GetAtmStrategyMarketPosition(atmStrategyId)==MarketPosition.Short && CCI(14)[1] < -TradeExitCross && CCI(14)[0] > -TradeExitCross))// Close the position on a 100 line cross
				{
					if(GetAtmStrategyMarketPosition(atmStrategyId)==MarketPosition.Long && projectedlow != oldprojectedlow)
					{	
						Print("");
						Print("");
						Print(String.Format("Time: {0} Low: {1} projectedlow: {2} Close: {3}", Time[0], Low[0], (projectedlow+TickSize), Close[0]));
						Print("");
						AtmStrategyChangeStopTarget(0, Math.Min(projectedlow-TickSize, Low[0]-TickSize), "STOP1", atmStrategyId);
						AtmStrategyChangeStopTarget(0, Math.Min(projectedlow-TickSize, Low[0]-TickSize), "STOP2", atmStrategyId);
						AtmStrategyChangeStopTarget(0, Math.Min(projectedlow-TickSize, Low[0]-TickSize), "STOP3", atmStrategyId);
						oldprojectedlow = projectedlow;
					}
					else if(GetAtmStrategyMarketPosition(atmStrategyId)==MarketPosition.Short && projectedhigh != oldprojectedhigh)
					{	
						Print("");
						Print("");
						Print(String.Format("Time: {0} High: {1} projectedhigh: {2}", Time[0], High[0], (projectedhigh+TickSize), Close[0]));
						Print("");
						AtmStrategyChangeStopTarget(0, Math.Max(projectedhigh+TickSize, High[0]+TickSize), "STOP1", atmStrategyId);
						AtmStrategyChangeStopTarget(0, Math.Max(projectedhigh+TickSize, High[0]+TickSize), "STOP2", atmStrategyId);
						AtmStrategyChangeStopTarget(0, Math.Max(projectedhigh+TickSize, High[0]+TickSize), "STOP3", atmStrategyId);
						oldprojectedhigh = projectedhigh;
					}
				}
				Print("The current ATM Strategy Market Position for " + Instrument.FullName + " is: " + GetAtmStrategyMarketPosition(atmStrategyId));
				Print("The current ATM Strategy Position Quantity for " + Instrument.FullName + " is: " + GetAtmStrategyPositionQuantity(atmStrategyId));
				Print("The current ATM Strategy Average Price for " + Instrument.FullName + " is: " + GetAtmStrategyPositionAveragePrice(atmStrategyId).ToString("f4"));
				Print("The current ATM Strategy Unrealized PnL for " + Instrument.FullName + " is: " + GetAtmStrategyUnrealizedProfitLoss(atmStrategyId).ToString("f4"));
				Print("The current ATM Strategy Realized PnL for " + Instrument.FullName + " is: " + GetAtmStrategyRealizedProfitLoss(atmStrategyId).ToString("f4")); 
				
				//Are we flat following a filled, cancelled or rejected order (don't do this if order is still pending)?
				if(GetAtmStrategyMarketPosition(atmStrategyId)==MarketPosition.Flat && (status[2] == "Filled" || status[2] == "Cancelled" || status[2] == "Rejected"))
				{	
					atmStrategyId = string.Empty;
					orderId = string.Empty;
					isAtmStrategyCreated = false;
					oldprojectedlow = 0;
					oldprojectedhigh = 0;
				}
			}
			#endregion
			
  			#region Set Signal Flag
			trade = 0;
			trade = ccif.WCCIPattern[0] * ccif.WCCIDirection[0];
			closeat = ccif.WCCICloseAt[0];
			switch(trade)
			{
				case 6:
					if(!Trade_Ghost || !TakeLongTrades)
						trade = 0;
					break;
				case 5:
					if(!Trade_TT 	|| !TakeLongTrades)
						trade = 0;
					break;
				case 4:
					if(!Trade_GB 	|| !TakeLongTrades)
						trade = 0;
					break;
				case 3:
					if(!Trade_Vegas || !TakeLongTrades)
						trade = 0;
					break;
				case 2:
					if(!Trade_Famir || !TakeLongTrades)
						trade = 0;
					break;
				case 1:
					if(!Trade_ZLR 	|| !TakeLongTrades)
						trade = 0;
					break;
				case -1:
					if(!Trade_ZLR	|| !TakeShortTrades)
						trade = 0;
					break;
				case -2:
					if(!Trade_Famir || !TakeShortTrades)
						trade = 0;
					break;
				case -3:
					if(!Trade_Vegas || !TakeShortTrades)
						trade = 0;
					break;
				case -4:
					if(!Trade_GB 	|| !TakeShortTrades)
						trade = 0;
					break;
				case -5:
					if(!Trade_TT 	|| !TakeShortTrades)
						trade = 0;
					break;
				case -6:
					if(!Trade_Ghost || !TakeShortTrades)
						trade = 0;
					break;
				default:
					break;
			}
			#endregion
						
			#region Order Entry
			if (orderId.Length == 0 && atmStrategyId.Length == 0 && trade > 0 && (ccif.SidewaysMkt[0] ? SidewaysTrades : 1==1)
				&& ( !string.IsNullOrEmpty(Trade_Begin_Time2) ? ((ToTime(Time[0]) >= start_time && ToTime(Time[0]) <= end_time) || (ToTime(Time[0]) >= start_time_1 && ToTime(Time[0]) <= end_time_1)) : (ToTime(Time[0]) >= start_time && ToTime(Time[0]) <= end_time))
				&& (!OffEndCandle ? Close[0]>Low[0] : 1==1))
			{
				entrybar = CurrentBar;
				atmStrategyId = GetAtmStrategyUniqueId();
				orderId = GetAtmStrategyUniqueId();
				
				AtmStrategyCreate(OrderAction.Buy, TypeEntry, (TypeEntry != OrderType.Market ? Close[0]+LimitOffset*TickSize : 0),
					(TypeEntry != OrderType.StopLimit ? 0 : (Close[0]+StopOffset*TickSize)), TimeInForce.Day, orderId,  
					(closeat == 1 ? ATMStrategy : ATMAltStrategy), atmStrategyId, (atmCallbackErrorCode, atmCallbackId) => 
					{
						if (atmCallbackId == atmStrategyId && atmCallbackErrorCode == Cbi.ErrorCode.NoError)
								isAtmStrategyCreated = true;
					});
				
				if(PrintStats)
				{
					Print(Instrument.FullName +","+ToDay(Time[0])+","+ToTime(Time[0])+","+(trade>1?"L":"S")+","+trade+","+Close[0]+","+(Close[0]==Low[0]?-1:1)+","+CCI(Periods)[0].ToString("f0")+","+(CCI(Periods)[0]-CCI(Periods)[1]).ToString("f0"));
				}
				else
				{
					Print(Time[0] + ",  " + Instrument.FullName + ",  " + orderId.Length + ",  " + atmStrategyId.Length + ",  Buy, " + TypeEntry + ",  " + (TypeEntry != OrderType.Market ? (Close[0]+LimitOffset*TickSize).ToString() : "0") 
					+ ",  " + (TypeEntry != OrderType.StopLimit ? "0" : (Close[0]+StopOffset*TickSize).ToString()) + ",   " + (closeat == 1 ? ATMStrategy : ATMAltStrategy));
				}
			}
			else if (orderId.Length == 0 && atmStrategyId.Length == 0 && trade < 0 && (ccif.SidewaysMkt[0] ? SidewaysTrades : 1==1)
				&& (!string.IsNullOrEmpty(Trade_Begin_Time2) ? ((ToTime(Time[0]) >= start_time && ToTime(Time[0]) <= end_time) || (ToTime(Time[0]) >= start_time_1 && ToTime(Time[0]) <= end_time_1)) : (ToTime(Time[0]) >= start_time && ToTime(Time[0]) <= end_time))
				&& (!OffEndCandle ? Close[0]<High[0] : 1==1))
			{	
				entrybar = CurrentBar;
				atmStrategyId = GetAtmStrategyUniqueId();
				orderId = GetAtmStrategyUniqueId();
				
				AtmStrategyCreate(OrderAction.SellShort, TypeEntry, (TypeEntry != OrderType.Market ? Close[0]-LimitOffset*TickSize : 0),
					(TypeEntry != OrderType.StopLimit ? 0 : (Close[0]-StopOffset*TickSize)), TimeInForce.Day, orderId, 
					(closeat == -1 ? ATMStrategy : ATMAltStrategy), atmStrategyId, (atmCallbackErrorCode, atmCallbackId) => 
					{
						if (atmCallbackId == atmStrategyId && atmCallbackErrorCode == Cbi.ErrorCode.NoError)
								isAtmStrategyCreated = true;
					});
				
				if(PrintStats)
				{
					Print(Instrument.FullName +","+ToDay(Time[0])+","+ToTime(Time[0])+","+(trade>1?"L":"S")+","+trade+","+Close[0]+","+(Close[0]==Low[0]?-1:1)+","+CCI(Periods)[0].ToString("f0")+","+(CCI(Periods)[0]-CCI(Periods)[1]).ToString("f0"));
				}
				else
				{
					Print(Time[0] + ",  " + Instrument.FullName + ",  " + orderId.Length + ",  " + atmStrategyId.Length + ",  SellShort, " + TypeEntry + ",  " + (TypeEntry != OrderType.Market ? (Close[0]-LimitOffset*TickSize).ToString() : "0") 
					+ ",  " + (TypeEntry != OrderType.StopLimit ? "0" : (Close[0]-StopOffset*TickSize).ToString()) + ",   " + (closeat == -1 ? ATMStrategy : ATMAltStrategy));
				}
			}
			#endregion

			//Cancel unfilled orders on new bar
			// Check for a pending entry order
			if (orderId.Length > 0 && isAtmStrategyCreated)
			{
				status = GetAtmStrategyEntryOrderStatus(orderId);
				Print("294   Check 1,   " + orderId + ",   Order Status:   " + status[2]);
				if(CurrentBar==entrybar+1 && (status[2] == "Accepted" || status[2] == "Working" || status[2] == "Pending"))
				{
					Print("292   Cancelled Order");
					AtmStrategyCancelEntryOrder(orderId);
					atmStrategyId = string.Empty;
					orderId = string.Empty;
					isAtmStrategyCreated = false;
				}
                
			} // If the strategy has terminated reset the strategy id
			else if (atmStrategyId.Length > 0 && GetAtmStrategyMarketPosition(atmStrategyId) == MarketPosition.Flat && isAtmStrategyCreated)
			{
				Print("302   reset the strategy id");
				atmStrategyId = string.Empty;
				orderId = string.Empty;
				isAtmStrategyCreated = false;
			}
			
			Print("================   End   ================");			
        }

		#region Properties
		
		#region General Properties
		[NinjaScriptProperty]
		[Display(Name="ATM Strategy", Description="ATM Strategy Name", Order=1, GroupName="Parameters")]
		public string ATMStrategy
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ATM Strategy, Off Candle", Description="ATM Strategy name for trades when the candle closes at the off end (Low for Longs and Highs for Shorts)", Order=2, GroupName="Parameters")]
		public string ATMAltStrategy
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ATM Strategy, Entry Order Type", Description="Type of order to be placed", Order=3, GroupName="Parameters")]
		public OrderType TypeEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ATM Strategy, SAR Order Type", Description="Type of order to be placed on Stop and Reverse", Order=4, GroupName="Parameters")]
		public OrderType SnREntry
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Order Offset, Limit", Description="Number of ticks for Limit Order from the trigger bar CLOSE (1 tick should equate to next bar open).", Order=5, GroupName="Parameters")]
		public int LimitOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Order Stop Offset, StopLimit", Description="Number of ticks for the StopLimit Order Stop price from the trigger bar projected High or Low (1 tick should equate to next bar open).", Order=7, GroupName="Parameters")]
		public int StopOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Exit Cross Line", Description="Exit the trade when the CCI crosses this line.", Order=8, GroupName="Parameters")]
		public int TradeExitCross
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Exit on cross, all candles", Description="Exit the trade when the CCI crosses the Exit Cross Line on any candle, both with and against the trade direction.  If True, any cross of 100 will exit the trade. If False, Only crosses on a candle against the trade direction will exit the trade.  In this case the stops will be moved to the high/low of the crossing candle.", Order=9, GroupName="Parameters")]
		public bool ExitCandle
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Session 1 Begin Time", Description="Start Time for trading session 1.", Order=10, GroupName="Parameters")]
		public string Trade_Begin_Time1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Session 1 End Time", Description="End Time for trading session 1.", Order=11, GroupName="Parameters")]
		public string Trade_End_Time1
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Session 2 Begin Time", Description="Start Time for trading session 2.  Leave blank if there is no 2nd session.", Order=12, GroupName="Parameters")]
		public string Trade_Begin_Time2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Session 2 End Time", Description="End Time for trading session 2.", Order=13, GroupName="Parameters")]
		public string Trade_End_Time2
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade ZLRs?", Description="Trade ZLR Pattern", Order=14, GroupName="Parameters")]
		public bool Trade_ZLR
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade Famirs?", Description="Trade Famir Pattern", Order=15, GroupName="Parameters")]
		public bool Trade_Famir
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade Vegas'?", Description="Trade Vegas Pattern", Order=16, GroupName="Parameters")]
		public bool Trade_Vegas
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade GB 100s?", Description="Trade GB Pattern", Order=17, GroupName="Parameters")]
		public bool Trade_GB
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade Tonys?", Description="Trade TT Pattern", Order=18, GroupName="Parameters")]
		public bool Trade_TT
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trade Ghosts?", Description="Trade Ghost Pattern", Order=19, GroupName="Parameters")]
		public bool Trade_Ghost
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Take Long Trades?", Description="Trade Long Trades", Order=20, GroupName="Parameters")]
		public bool TakeLongTrades
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Take Short Trades?", Description="Trade Short Trades", Order=21, GroupName="Parameters")]
		public bool TakeShortTrades
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Take Off Candle Trades?", Description="Trade on signals with close opposite to trade direction? (Longs on a low close or shorts on a high close)", Order=22, GroupName="Parameters")]
		public bool OffEndCandle
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Take Trades in sideways mkts?", Description="Trade during sideways markets?", Order=23, GroupName="Parameters")]
		public bool SidewaysTrades
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Stop and Reverse?", Description="Stop and Reverse on losing positions with new signal in opposite direction?  The reversal will be order type 'SAR Order Type'.", Order=24, GroupName="Parameters")]
		public bool StopReverse
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Print Stats?", Description="Print Entry and Exit CCI data?", Order=25, GroupName="Parameters")]
		public bool PrintStats
		{ get; set; }
		#endregion
		
		#region CCI Forecaster Properties
		[NinjaScriptProperty]
		[Display(Name="Sidewinder variable", Description="Sidewinder Variable.  Check with Woodies to determine the setting for bar types other than Range Bars.  It defaults to 60 for Range Bars.", Order=0, GroupName="CCI Parameters")]
		public int SideWinder0
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="CCI Trend Level", Description="CCI must go below this value before a ZLR or Vegas can be signaled", Order=0, GroupName="CCI Parameters")]
		public double RejectLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trend Length for GB100", Description="Number of bars for a trend to be in place for GB100 trades to trigger", Order=0, GroupName="CCI Parameters")]
		public int TrendLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="CCI Periods", Description="CCI Periods", Order=0, GroupName="CCI Parameters")]
		public int Periods
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Ghost, Use Points/Percent Amount", Description="Ghost, Use Points/Percent Amount", Order=0, GroupName="CCI Parameters")]
		public double UsePointsAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Ghost, Use Points or Percent", Description="Type of ZigZag deviation (Points or Percent)", Order=0, GroupName="CCI Parameters")]
		public string UsePoints
		{ get; set; }

		[NinjaScriptProperty]
		[Range(150, 350)]
		[Display(Name="Extremes CCI Level", Description="CCI must close above this value before before it is considered to have 'gone to extremes'.  This will also be the level at which a sideways market is reset.", Order=0, GroupName="CCI Parameters")]
		public double Extremes
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 10)]
		[Display(Name="Famir, CCI Points for Hook", Description="Famir minimum CCI hook points (default = 3)", Order=0, GroupName="CCI Parameters")]
		public double FamirHookMin
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Famir, Max CCI Level", Description="Famir CCI boundaries (default = 50)", Order=0, GroupName="CCI Parameters")]
		public double FamirLimits
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Penetration level before swing high/low", Description="Level that the CCI must penetrate through to reach the swing high/low.", Order=0, GroupName="CCI Parameters")]
		public double VTHFEPenetrationLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Max Swing after swing high/low", Description="Max high or low for swing back to trend after swing high/low (default = 100).  NOT SWING HIGH/LOW.", Order=0, GroupName="CCI Parameters")]
		public double VMaxSwingPoint
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Max Swing bars", Description="Max # of bars between swing high/low and trigger (default = 10).", Order=0, GroupName="CCI Parameters")]
		public int VMaxSwingBars
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Range(0, 10)]
		[Display(Name="Ghost trendline/peak gap", Description="The number of CCI points that must be between trend break line and a ghost right peak", Order=0, GroupName="CCI Parameters")]
		public double GstColor
		{ get; set; }			

		[NinjaScriptProperty]
		[Range(0, 75)]
		[Display(Name="ZLR, Minimum Point Change", Description="Minimum CCI point change before a ZLR can be signaled", Order=0, GroupName="CCI Parameters")]
		public double MinZLRPts
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Minimum CCI Point Change", Description="Minimum CCI point change for a Famir, Vegas, GB100 or Tony to trigger.", Order=0, GroupName="CCI Parameters")]
		public double MinCCIPts
		{ get; set; }

		[NinjaScriptProperty]
		[Range(75, 200)]
		[Display(Name="Maximum Signal Level", Description="Maximum CCI value for a Pattern to be signaled", Order=0, GroupName="CCI Parameters")]
		public double MaxSignalValue
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="ZLR, Minimum Level", Description="Minimum CCI value for a ZLR to be signaled", Order=0, GroupName="CCI Parameters")]
		public double MinZLRValue
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Sideways mkt crosses", Description="Numbers of 0 line crosses needed to declare a sideways market", Order=0, GroupName="CCI Parameters")]
		public int ChopCrosses
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Sideways mkt, Use Javier's alternate?", Description="Use Javier's alternate sideways market indicator?", Order=0, GroupName="CCI Parameters")]
		public bool UseJavierSideways
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Sideways Market Boundary", Description="Boundary limits between which sideways market crosses must remain.", Order=0, GroupName="CCI Parameters")]
		public int Sidewaysboundary
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="SS Zero Band", Description="Zero Line + or - Band", Order=0, GroupName="CCI Parameters")]
		public int SSZeroBand
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="SS 100 Band", Description="100 Line + or - Band", Order=0, GroupName="CCI Parameters")]
		public int SS100Band
		{ get; set; }

		[NinjaScriptProperty]
		[Range(50, int.MaxValue)]
		[Display(Name="SS CCI/Turbo Separation", Description="The separation between the previous bar CCI and Turbo", Order=0, GroupName="CCI Parameters")]
		public int SSCCITurboSeparation
		{ get; set; }

		[NinjaScriptProperty]
		[Range(75, 200)]
		[Display(Name="SS Maximum Turbo Level", Description="Maximum Turbo value for a Slingshot to be signaled", Order=0, GroupName="CCI Parameters")]
		public double MaxSSTurboValue
		{ get; set; }
		#endregion
		
		#endregion

	}
}
