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

#endregion

//This namespace holds Bars types in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.BarsTypes
{
	/// <summary>
	/// Modded for NT8b9!!!!
	/// </summary>
	public class UniRenkoBarsType : BarsType
	{
		double barOpen;
        double barMax;
        double barMin;
		double fakeOpen=0;

		int    barDirection=0;
		double openOffset=0;
		double trendOffset=0;
		double reversalOffset=0;

		bool   maxExceeded=false;
		bool   minExceeded=false;

		double tickSize=0.01;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Universal renko bar type. Ported by Sim22, 2015. simterann22@gmx.com";
				Name								= "UniRenko";
				BarsPeriod							= new BarsPeriod { BarsPeriodType = (BarsPeriodType) 501, BarsPeriodTypeName = "UniRenkoBarsType(501)", Value = 1 };
				BuiltFrom							= BarsPeriodType.Tick;
				DaysToLoad							= 3;
				IsIntraday							= true;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("BaseBarsPeriodType",			true));
				//Properties.Remove(Properties.Find("BaseBarsPeriodValue",		true));
				Properties.Remove(Properties.Find("PointAndFigurePriceType",	true));
				Properties.Remove(Properties.Find("ReversalType",				true));
				
				SetPropertyName("Value", "Tick Trend");
				SetPropertyName("Value2", "Tick Reversal");
				SetPropertyName("BaseBarsPeriodValue",  "Open Offset");
				
				Name = "UniR T" + BarsPeriod.Value +"R" + BarsPeriod.Value2; // +"O" + BarsPeriod.BaseBarsPeriodValue;
				/// I kept the UI name short b/c as of NT8b6 the name space at the toolbar is very short. You would not see the values. Sim22.
			}
		}

		public override int GetInitialLookBackDays(BarsPeriod barsPeriod, TradingHours tradingHours, int barsBack)
		{		
			return 3;
		}

		protected override void OnDataPoint(Bars bars, double open, double high, double low, double close, DateTime time, long volume, bool isBar, double bid, double ask)
		{
			///Beta 9 addition!
			// build a session iterator from the bars object being updated
		  if (SessionIterator == null)
		    SessionIterator = new SessionIterator(bars);
		 
		  // check if we are in a new trading session based on the trading hours selected by the user
		  bool isNewSession = SessionIterator.IsNewSession(time, isBar);
		 
		  // calculate the new trading day
		  if (isNewSession)
		    SessionIterator.CalculateTradingDay(time, isBar);
		  
		  ///End Beta 9 addition
		  
		  
			//### First Bar
		  if (bars.Count == 0 || (bars.IsResetOnNewTradingDay && isNewSession))
			{
				tickSize = bars.Instrument.MasterInstrument.TickSize;
				trendOffset = bars.BarsPeriod.Value * tickSize;
				reversalOffset = bars.BarsPeriod.Value2 * tickSize;
				//bars.BarPeriod.BaseBarsPeriodValue = bars.BarsPeriod.Value;	//### Remove to customize OpenOffset
				openOffset = Math.Ceiling((double)bars.BarsPeriod.BaseBarsPeriodValue * 1) * tickSize;
				
				barOpen = close;
                barMax  = barOpen + (trendOffset * barDirection);
                barMin  = barOpen - (trendOffset * barDirection);
				
				AddBar(bars, barOpen, barOpen, barOpen, barOpen, time, volume);
			}
			//### Subsequent Bars
			else
			{
                maxExceeded  = bars.Instrument.MasterInstrument.Compare(close, barMax) > 0 ? true : false;
                minExceeded  = bars.Instrument.MasterInstrument.Compare(close, barMin) < 0 ? true : false;

                	//### Defined Range Exceeded?
                if ( maxExceeded || minExceeded )
                {
                    double thisClose = maxExceeded ? Math.Min(close, barMax) : minExceeded ? Math.Max(close, barMin) : close;
                    barDirection     = maxExceeded ? 1 : minExceeded ? -1 : 0;
                    fakeOpen = thisClose - (openOffset * barDirection);		//### Fake Open is halfway down the bar

                    	//### Close Current Bar
                    UpdateBar(bars, (maxExceeded ? thisClose : bars.GetHigh(bars.Count - 1)), (minExceeded ? thisClose : bars.GetLow(bars.Count - 1)), thisClose, time, volume);

                    	//### Add New Bar
					barOpen = close;
					barMax  = thisClose + ((barDirection>0 ? trendOffset : reversalOffset) );
					barMin  = thisClose - ((barDirection>0 ? reversalOffset : trendOffset) );

					AddBar(bars, fakeOpen, (maxExceeded ? thisClose : fakeOpen), (minExceeded ? thisClose : fakeOpen), thisClose, time, volume);
                }
                	//### Current Bar Still Developing
                else
                {
                    UpdateBar(bars, (close > bars.GetHigh(bars.Count - 1) ? close : bars.GetHigh(bars.Count - 1)), (close < bars.GetLow(bars.Count - 1) ? close : bars.GetLow(bars.Count - 1)), close, time, volume);
                }	
				
			}
			bars.LastPrice = close;
			
		}

		public override void ApplyDefaultBasePeriodValue(BarsPeriod period)
		{
			
		}

		public override void ApplyDefaultValue(BarsPeriod period)
		{
			//period.BarsPeriodTypeName 	= "UniRenko";
			period.Value 				= 2;
			period.Value2 				= 6;
			period.BaseBarsPeriodValue	= 2;
		}

		public override string ChartLabel(DateTime dateTime)
		{
			return dateTime.ToString("T", Core.Globals.GeneralOptions.CurrentCulture);
		}

		public override double GetPercentComplete(Bars bars, DateTime now)
		{
			//return 1.0d;
			return 0;
		}
	}
}
