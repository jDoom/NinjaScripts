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

using Correlation;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class Correlation : Indicator
	{
		#region Variables
		private const int Instrument1BIP=0, Instrument2BIP=1;
		private CorrelationType corrType = CorrelationType.Pearson;
		private SyncedDataSeries Instrument2BIPClose;
		private ISeries<double> SecondaryISeries;
		private Series<double> SecondarySeries;
		#endregion
		
		#region SynchronizationMethods
/*
  Synchronization of Multi-Instrument Indicators & Strategies - For NinjaTrader 7
  Kevin Doren - Version 4/9/2011

  7/29/2010:  Added "SynchronizedBarCountToday" field
  9/8/2010:   Added "SyncedGet" method
  4/1/2011:   Added "LastDataTime" field, streamlined & simplified some code, added check for time-based Period
  4/9/2011:   Fixed problem with syncing daily bars with different session end times while COBC=false w/ historical data
			
  Multi-instrument indicators (and strategies, for the same concepts apply) by their nature are looking
  at relationships between instruments.  Any calculations or comparisons we do need to be time-synchronized - we want
  to be using data that occurred at the same time.  For certain kinds of analysis, such as intraday
  analysis of heavily-traded instruments during regular trading hours on historical data, the techniques
  shown here probably aren't necessary.  But for real time analysis, especially with instruments that
  might have missing bars, synchronization techniques are neeeded to avoid the misleading results that
  come from comparing data that occurred at different times when the analysis only has meaning if they occurred at the same time.

  There are 3 scenarios: Historical, real-time with (CalculateOnBarClose == true), 
  and real-time with (CalculateOnBarClose == false).  Each has its issues.

  The simplest case is Historical.  In this case, NT7 will synchronize the bars, which means that when
  OnBarUpdate is called for a Bar Series with a particular timestamp, any other Bar Series which
  has a bar with the same timestamp will have that bar set to be the current bar.  This means that
  Closes[PrimaryInstrument][0] and Closes[SyncedInstrument][0] will both point to bars with the same timestamp IF Closes[1] has a
  bar with that timestamp.  If not, Close[SyncedInstrument][0] will point to the most recent bar preceding it.
  So even Close[PrimaryInstrument][0] and Close[SyncedInstrument][0] are not necessarily synchronized.  As you look back in time,
  it can get even more out of sync, because bars might be missing from either series.

  For an indicator like correlation, the calculation makes no sense unless the bars are all synchronized.
  The solution implemented here is to create 2 additional Data Series, each synchronized to the Primary 
  Instrument, because the indicator output is synced to the Primary Instrument.
  The first (base class) Data Series ("SyncedDataSeries") is a synchronized copy of the secondary instrument, Closes[SyncedInstrument].
  The second Data Series is a BoolSeries field (".Synced") which flags the bars that are synced.
		
  In other words, Closes[PrimaryInstrument][x] and SyncedDataSeries class SyncedDataSeries[x] will refer to the same time,
  and the field SyncedDataSeries.Synced[x] will be set to true if a tick from SyncedInstrument occurred during that bar.

  When operating with Historical data, if a synchronized bar exists, it will be Closes[SyncedInstrument][0].
  In real time, with (CalculateOnBarClose==false), it gets more complicated, because NT7 is event-driven,
  which mean that bars don't close because of time.  They close because a tick arrives with a timestamp of
  a later bar, at which time OnBarUpdate is called and the new bar starts forming.  This could take a long time.
  So when we are processing a bar for the primary instrument, we need to also check the unclosed bar of the secondary
  instrument, which we can do by referencing Times[1][-1]; this is safe to do because we know the bar's time
  has expired - we are running OnBarUpdate for the primary instrument for the same timestamp, which can
  only happen because a tick arrived for a later bar.

  Also, in real time, it's possible that the synced instrument has received bars after the time of the
  bar we are currently processing.  A synchronized bar could exist at Closes[SyncedInstrument][1], Closes[SyncedInstrument][2], or earlier.
  So we need to scan back through the seconday instrument's closes to see if the Synced bar is there.
  
  The (base class) Data Series "SyncedDataSeries" is filled from OnBarUpdate for the primary instrument, and it will hold the
  synchronized bars we found using the above techniques.  We set a flag in the "Synced" data series to show which
  locations are synced.  Routines which require lookbacks can be written to perform their calculations using only synced data.

  There is an additional issue when using daily bars.  As of this writing, daily bars carry a timestamp of the end of the
  NT7 session template.  This can change depending on which session template is applied to the instrument, either in the
  instrument manager or in the chart properties.  There is no issue if both instruments have the same session template,
  then their daily bars will have the same timestamp and thus be synchronized.  But if the secondary instrument has a timestamp
  later than the primary instrument, the days will be out of sync unless we strip away the times and just use the dates.
*/		
		public class SyncedDataSeries : Series<double>
		{
			public IndicatorBase IndBase;
			public int PrimaryInstrument;			// BarsArray Index of primary instrument (usually 0 in indicators)
													//   but could be higher than 0 for multi-instrument, multi-timeframe indicators
			public int SyncedInstrument;			// BarsArray Index of secondary instrument we want to synchronize to primary instrument
			public ISeries<double> DataArray;			// Output Data Series synced to primary instrument, hold synchronized copy of secondary instrument
			public Series<bool> Synced;				// BoolSeries output, synced with primary instrument, holds flags
													//   true=output (DataArray) holds synchronized value, false = output does not hold synchronized value
			public int SynchronizedBarCount;		// Total number of synchronized bars held in output, but some or all may be inaccessible if MaximumBarsLookBack is set to 256
			public int SynchronizedBarCountToday;	// Total number of synchronized bars so far today
			public bool DataStarted;
			private bool dailyBars;
//			private bool lastSyncWasToUnclosedBar;
			public DateTime LastDataTime;			// Time of the most recent data received from syncedInstrument
			private DateTime primaryInstrumentCurrentBarTime;
			private DateTime syncedInstrumentCurrentBarTime;
			private int primaryInstrumentCurrentBar;
			private int syncedInstrumentCurrentBar;
			
			public SyncedDataSeries (IndicatorBase indicator, int primaryInstrument, int syncedInstrument, ISeries<double> dataArray) :
				this (indicator, primaryInstrument, syncedInstrument, dataArray, indicator.MaximumBarsLookBack)
			{
			}
			public SyncedDataSeries (IndicatorBase indicator, int primaryInstrument, int syncedInstrument, ISeries<double> dataArray, MaximumBarsLookBack maxLookback) : base (indicator, maxLookback)
			{
				IndBase = indicator;
				PrimaryInstrument = primaryInstrument;
				SyncedInstrument = syncedInstrument;
				SynchronizedBarCount = 0;
				SynchronizedBarCountToday = 0;
				DataArray = dataArray;
				DataStarted = false;
//				lastSyncWasToUnclosedBar = false;
				LastDataTime = DateTime.MinValue;
				primaryInstrumentCurrentBarTime = DateTime.MinValue;
				syncedInstrumentCurrentBarTime = DateTime.MinValue;
				primaryInstrumentCurrentBar = -1;
				syncedInstrumentCurrentBar = -1;
				
				switch (IndBase.BarsPeriods[PrimaryInstrument].BarsPeriodType)
				{
					case BarsPeriodType.Day:
					case BarsPeriodType.Week:
					case BarsPeriodType.Month:
					case BarsPeriodType.Year:
						dailyBars = true;
						break;
					case BarsPeriodType.Minute:
					case BarsPeriodType.Second:
					{
						dailyBars = false;
						break;
					}
					default:
						throw new ArgumentException("SyncedDataSeries: Instrument Period must be time-based (Minute,Day,Week,Month,Year,or Second).");
						break;
				}
				if (IndBase.BarsPeriods[PrimaryInstrument].BarsPeriodType != IndBase.BarsPeriods[SyncedInstrument].BarsPeriodType)
					throw new ArgumentException("SyncedDataSeries: Primary and Synced Intruments must both have the same Period Type.");
				if (IndBase.BarsPeriods[PrimaryInstrument].Value != IndBase.BarsPeriods[SyncedInstrument].Value)
					throw new ArgumentException("SyncedDataSeries: Primary and Synced Intruments must both have the same Period Value.");
				
				if (PrimaryInstrument == 0)
					Synced = new Series<bool>(IndBase);  // We are syncing to the primary instrument of the instantiating indicator
				else
					throw new ArgumentOutOfRangeException ("PrimaryInstrument", "SyncedDataSeries: PrimaryInstrument must = 0 if no SyncedIndicator base is given.");
			}
	
			public SyncedDataSeries (IndicatorBase indicator, int primaryInstrument, int syncedInstrument, ISeries<double> dataArray, IndicatorBase syncedIndicator) :
				this (indicator, primaryInstrument, syncedInstrument, dataArray, syncedIndicator, indicator.MaximumBarsLookBack)
			{
			}
			public SyncedDataSeries (IndicatorBase indicator, int primaryInstrument, int syncedInstrument, ISeries<double> dataArray, IndicatorBase syncedIndicator, MaximumBarsLookBack maxLookback) :base (syncedIndicator, maxLookback)
			{	
				IndBase = indicator;
				PrimaryInstrument = primaryInstrument;
				SyncedInstrument = syncedInstrument;
				SynchronizedBarCount = 0;
				SynchronizedBarCountToday = 0;
				DataArray = dataArray;
				DataStarted = false;
//				lastSyncWasToUnclosedBar = false;
				LastDataTime = DateTime.MinValue;
				primaryInstrumentCurrentBarTime = DateTime.MinValue;
				syncedInstrumentCurrentBarTime = DateTime.MinValue;
				primaryInstrumentCurrentBar = -1;
				syncedInstrumentCurrentBar = -1;
				
				switch (IndBase.BarsPeriods[PrimaryInstrument].BarsPeriodType)
				{
					case BarsPeriodType.Day:
					case BarsPeriodType.Week:
					case BarsPeriodType.Month:
					case BarsPeriodType.Year:
						dailyBars = true;
						break;
					case BarsPeriodType.Minute:
					case BarsPeriodType.Second:
					{
						dailyBars = false;
						break;
					}
					default:
						throw new ArgumentException("SyncedDataSeries: Instrument Period must be time-based (Minute,Day,Week,Month,Year,or Second).");
						break;
				}
				if (IndBase.BarsPeriods[PrimaryInstrument].BarsPeriodType != IndBase.BarsPeriods[SyncedInstrument].BarsPeriodType)
					throw new ArgumentException("SyncedDataSeries: Primary and Synced Intruments must both have the same Period Type.");
				if (IndBase.BarsPeriods[PrimaryInstrument].Value != IndBase.BarsPeriods[SyncedInstrument].Value)
					throw new ArgumentException("SyncedDataSeries: Primary and Synced Intruments must both have the same Period Value.");
				
				Synced = new Series<bool>(syncedIndicator);	// Not syncing to primary instrument of instantiating indicator;
															// So create a new BoolSeries synced to the master instrument
			}
			
			public double SyncedGet(int syncedBarsBack)
			{
				int i;
				if (syncedBarsBack >= SynchronizedBarCount)
					return 0;			// not enough synchronized bars - return 0 rather than throw exception
				return SyncedGet(syncedBarsBack, 0, out i);
			}
			
			public double SyncedGet(int syncedBarsBack, int initIndex, out int index)
			{
				index = initIndex;
				while (!Synced[index])  //Find most recent synced Bar
					index++;
				
				for (int counter=0; counter < Math.Min(syncedBarsBack,SynchronizedBarCount); counter++)
				{  //looking back past the first synced bar, keep looking for more
					index++;
					while (!Synced[index])  //Find previous synced Bar
						index++;
				}
				return base[index];
			}
			
			private void setBarAsSynced(int getDataFromBarsBack)
			{
				Synced[0] = true;
				SynchronizedBarCount++;
				SynchronizedBarCountToday++;
				LastDataTime = IndBase.Times[SyncedInstrument][getDataFromBarsBack];
				base[0] = DataArray[getDataFromBarsBack];
			}
			
			public void Synchronize()
			{
				if ((IndBase.BarsInProgress != PrimaryInstrument) && (IndBase.BarsInProgress != SyncedInstrument))
						return;	// Bars being processed are not for us
				
				if (IndBase.Calculate == Calculate.OnBarClose)
				{
					if (IndBase.BarsInProgress == PrimaryInstrument) // Primary Instrument
					{
						primaryInstrumentCurrentBarTime = dailyBars ? IndBase.Times[PrimaryInstrument][0].Date : IndBase.Times[PrimaryInstrument][0];
						syncedInstrumentCurrentBar = IndBase.BarsArray[SyncedInstrument].CurrentBar;
						
						if (!DataStarted)
						{
							if (syncedInstrumentCurrentBar == -1)
							{
								Synced[0] = false;
								return;						// return if no data yet from synced instrument
							}
							else
								DataStarted = true;
						}
						if (IndBase.BarsArray[PrimaryInstrument].IsFirstBarOfSession)
							SynchronizedBarCountToday = 0;	// it's a new session, zero today's synced bar count
							
						//	Scan through SyncedInstrument's bars, try to find one in sync with the current Primary instrument bar
						//	Start the scan with the unclosed bar [-1] (necessary in real time); if it has the same time as the Primary instrument's Time[0] (which is now closed), it should be done forming
						int barsBack = 0;
						/*if (syncedInstrumentCurrentBar == IndBase.BarsArray[SyncedInstrument].Count-1)
							barsBack = 0;
						else
							barsBack = 0; // -1
						*/
						//Print("Hi");
						
						while ((barsBack < syncedInstrumentCurrentBar) && primaryInstrumentCurrentBarTime < (dailyBars ? IndBase.Times[SyncedInstrument][barsBack].Date : IndBase.Times[SyncedInstrument][barsBack]))
								barsBack ++;  // ignore bars that occur in the future (this can happen in real time if primary instrument is missing bars)
						if (primaryInstrumentCurrentBarTime == (dailyBars ? IndBase.Times[SyncedInstrument][barsBack].Date : IndBase.Times[SyncedInstrument][barsBack]))
							{	// Found a synchronized bar
								setBarAsSynced(barsBack);
//								if (barsBack == -1)
//									lastSyncWasToUnclosedBar = true;
							}
						else	// No synchronized bar found
							{
								Synced[0] = false;
								base[0] = DataArray[barsBack];	// set output to most recent bar that isn't in the future
								LastDataTime = IndBase.Times[SyncedInstrument][barsBack];
							}
					}
					
					else // (IndBase.BarsInProgress == SyncedInstrument)
					{
						// Normally we don't have to do anything when (BarsInProgress == SyncedInstrument) if (CalculateOnBarClose == true)
						// Method output is synced with, and handled by, primary instrument when (BarsInProgress == PrimaryInstrument)
						//
						// Sometimes in a real-time OnBarUpdate for PrimaryIntrument, we find that SyncedInstrument has an unclosed bar (index of [-1]) with the same timestamp,
						// so we use it, which is safe to do because PrimaryInstrument's bar has already closed, so we know that the end of the bar period has passed.
						//
						if (!DataStarted)
						{
							if (IndBase.BarsArray[PrimaryInstrument].CurrentBar == -1)
								return;							// return if no data yet from Primary instrument
							else
								DataStarted = true;
						}	
//						if (lastSyncWasToUnclosedBar)
//						{
//							lastSyncWasToUnclosedBar = false;   // if there were any late arriving ticks, they are now included in the bar that just closed
//							int barsBack=0;
//							while (!Synced[barsBack])			// Scan for most recent synced bar
//								barsBack++;
//							base.Set(barsBack,DataArray[0]);	// Store the final value, in case it has changed since we stored the value from the unclosed bar
//						}
					}
				}
				else  // (CalculateOnBarClose == false)
				{
					if (IndBase.BarsInProgress == PrimaryInstrument) // Primary Instrument
					{
						if (!DataStarted)
						{	
							if (syncedInstrumentCurrentBar == -1)
							{
								Synced[0] = false;
								base.Reset();
								return;							// return if no data yet from synced instrument
							}
							else
								DataStarted = true;
						}		
						if (IndBase.IsFirstTickOfBar)				// First tick of bar, need to set state of sync flag
						{
							primaryInstrumentCurrentBarTime = dailyBars ? IndBase.Times[PrimaryInstrument][0].Date : IndBase.Times[PrimaryInstrument][0];
							primaryInstrumentCurrentBar = IndBase.BarsArray[PrimaryInstrument].CurrentBar;
							syncedInstrumentCurrentBar = IndBase.BarsArray[SyncedInstrument].CurrentBar;
							syncedInstrumentCurrentBarTime = dailyBars ? IndBase.Times[SyncedInstrument][0].Date : IndBase.Times[SyncedInstrument][0];

							if (IndBase.BarsArray[PrimaryInstrument].IsFirstBarOfSession)
								SynchronizedBarCountToday = 0;	// it's a new session, zero today's synced bar count
							
							//	When synchronizing daily bars if syncedInstrument has a later session end time than primaryInstrument
							//  if COBC == false, when processing historical data, OnBarUpdate will be called for primaryInstrument
							//  while the syncedInstrument shows the previous day in its current bar.  We have to peek ahead to
							//  the next bar (-1 index) to see if the date matches
							if (   (dailyBars == true)  
								&& (syncedInstrumentCurrentBar != IndBase.BarsArray[SyncedInstrument].Count-1) 
								&& (primaryInstrumentCurrentBarTime == IndBase.Times[SyncedInstrument][-1].Date))
							{
								setBarAsSynced(-1);
								return;
							}
							
							if (primaryInstrumentCurrentBarTime == syncedInstrumentCurrentBarTime)
								setBarAsSynced(0);
							else
							{
								if ((syncedInstrumentCurrentBar > 0) && (primaryInstrumentCurrentBarTime == (dailyBars ? IndBase.Times[SyncedInstrument][1].Date : IndBase.Times[SyncedInstrument][1])))
									setBarAsSynced(1);
								else
								{
									Synced[0] = false;
									base[0] = DataArray[0];  // store most recent value from prior bars, in case no updates come for rest of bar
									LastDataTime = syncedInstrumentCurrentBarTime;
								}
							}
						}
					}
					else // (IndBase.BarsInProgress == SyncedInstrument)
					{
						if (syncedInstrumentCurrentBar != IndBase.BarsArray[SyncedInstrument].CurrentBar)
						{	// if SyncedInstrument's CurrentBar has changed, it's the first tick of the bar, need to save CurrentBar and Time
							syncedInstrumentCurrentBar = IndBase.BarsArray[SyncedInstrument].CurrentBar;
							syncedInstrumentCurrentBarTime = dailyBars ? IndBase.Times[SyncedInstrument][0].Date : IndBase.Times[SyncedInstrument][0];
						}
						
						if (!DataStarted)
						{
							if (IndBase.BarsArray[PrimaryInstrument].CurrentBar == -1)
								return;						// return if no data yet from primary instrument
							else
								DataStarted = true;
						}
//						if (primaryInstrumentCurrentBarTime == syncedInstrumentCurrentBarTime)
						if (((!dailyBars) && (IndBase.Times[SyncedInstrument][0] == IndBase.Times[PrimaryInstrument][0]))
							|| (dailyBars && (ToDay(IndBase.Times[SyncedInstrument][0]) == ToDay(IndBase.Times[PrimaryInstrument][0]))))
						{
							if (!Synced[0])
								setBarAsSynced(0);
							else
								base[0] = DataArray[0];
						}
					}
				}
			}
		}
		#endregion
		
		#region MathematicalMethods	
		private double SyncedSMA(ISeries<double> inputSeries, Series<bool> synced, int sMAPeriod)
			//	SyncedSMA: Calculate SMA of a Data Series, using only valid (synchonized) bars
			//
			//	Inputs:
			//		inputSeries: DataSeries on which to calculate SMA
			//		synced: BoolSeries, true for each bar which holds a valid (synchronized) value to use in SMA
			//		sMAPeriod: Period for SMA calculation; there must be >= sMAPeriod synced bars in the input series, or it will throw an exception
			//
		{
			double sMAsum = 0;
			int counter = 0;
			for (int barsBack = 0; counter < sMAPeriod; barsBack++)
			{
				if (barsBack > 255)  // NT now throws an exception, so this statement isn't strictly necessary; left it in to issue helpful error message
					if (MaximumBarsLookBack == MaximumBarsLookBack.TwoHundredFiftySix)
						throw new IndexOutOfRangeException("Index out of range: Try changing 'Maximum bars look back' parameter to 'Infinite'");
					
				if (synced[barsBack])
				{
					sMAsum += inputSeries[barsBack];
					counter++;
				}
			}
			return (sMAsum / System.Convert.ToDouble(sMAPeriod));
		}
		
		private double SyncedCorrelation(PriceSeries inputSeries1, SyncedDataSeries inputSeries2, int corrPeriod)
			//  SyncedCorrelation: Calculates the Pearson Product Moment Correlation of 2 Data Series, using only the bars which are synchronized
			//  Note: Correlations may differ from Excel if NinjaTrader is set to dividend-adjust historical prices
			//
			//	Inputs:
			//		inputSeries1: 1st DataSeries, typically Closes[x]
			//		inputSeries2: SyncedDataSeries, synced to InputSeries1; some bars (flagged by BoolSeries "Synced") hold valid (synchronized) values
			//		corrPeriod: Period for Correlation calculation; there must be >= CorrPeriod synced bars in the input series, or it will throw an exception
			//
		{
			double sum1 = 0; 
			double sum2 = 0;
			double sum3 = 0;
			int index1=0;
			double mean1, mean2, variance1, variance2, denominator;
			mean1 = SyncedSMA(inputSeries1,inputSeries2.Synced,corrPeriod);
			mean2 = SyncedSMA(inputSeries2,inputSeries2.Synced,corrPeriod);
			
			for (int counter = 0; counter < corrPeriod; counter++)
			{
				while (!inputSeries2.Synced[index1])  //Find next synced Bar
					index1++;
				variance1 = inputSeries1[index1] - mean1;
				variance2 = inputSeries2[index1] - mean2;
				sum1 += variance1 * variance2;
				sum2 += variance1 * variance1;
				sum3 += variance2 * variance2;
				index1++;
			}
			denominator = Math.Sqrt(sum2 * sum3);
			if (denominator == 0)
				return (0);  // avoid divide-by-zero error;  not really the correct result (actual correlation is undefined)
			else
				return (sum1 / denominator);
		}
		
		private double KDSMA(double[] inputSeries, int sMAPeriod)
		{   
			double sMAsum = 0;
			for (int index = 0; index < sMAPeriod; index++)
				sMAsum += inputSeries[index];
			return (sMAsum / System.Convert.ToDouble(sMAPeriod));
		}
		
		private double KDCorrelation(double[] inputSeries1, double[] inputSeries2, int corrPeriod)
			//  Note: Correlations may differ from Excel if NinjaTrader is set to dividend-adjust historical prices
		{
			double sum1 = 0; 
			double sum2 = 0;
			double sum3 = 0;
			double mean1, mean2, variance1, variance2, denominator;
			mean1 = KDSMA(inputSeries1,corrPeriod);
			mean2 = KDSMA(inputSeries2,corrPeriod);
			
			for (int index = 0; index < corrPeriod; index++)
			{
				variance1 = inputSeries1[index] - mean1;
				variance2 = inputSeries2[index] - mean2;
				sum1 += variance1 * variance2;
				sum2 += variance1 * variance1;
				sum3 += variance2 * variance2;
			}	
			denominator = Math.Sqrt(sum2 * sum3);
			if (denominator == 0)
				return (0);  // avoid divide-by-zero error;  not really the correct result (actual correlation is undefined)
			else
				return (sum1 / denominator);
		}
		
		
		private double SyncedPctChangeCorrelation(PriceSeries inputSeries1, SyncedDataSeries inputSeries2, int corrPeriod, int changeLookback)
		{
			int index=0;
			double[] pctChangeArray1 = new double[corrPeriod];
			double[] pctChangeArray2 = new double[corrPeriod];
			
			while (!inputSeries2.Synced[index])  //Find most recent synced Bar
				index++;
			double val1 = inputSeries1[index];
			double val2 = inputSeries2[index];
			index++;
			
			for (int counter=corrPeriod-1; counter >= 0; counter--)
			{
				int changeIndex;
				while (!inputSeries2.Synced[index])  //Find previous synced Bar
					index++;
				double chg2 = inputSeries2.SyncedGet(changeLookback-1,index,out changeIndex);  // this will scan back more synced bars if changeLookback > 0
				double chg1 = inputSeries1[changeIndex];
				pctChangeArray1[counter] = (val1 - chg1) / chg1;
				pctChangeArray2[counter] = (val2 - chg2) / chg2;
				val1 = inputSeries1[index];
				val2 = inputSeries2[index];
				index++;
			}		
			
//			return(KDRankCorrelation(pctChangeArray1,pctChangeArray2,corrPeriod));	// maybe rank correlation of PctChange would be better?
			return(KDCorrelation(pctChangeArray1,pctChangeArray2,corrPeriod));
		}
		
		private void KDResidual(double[] inputSeries, int slopePeriod, double[] residualArray)
		{	
			double meanx, variance1, variance2;
			double meany = KDSMA(inputSeries,slopePeriod);
			double sum1 = 0;
			double sum2 = 0;
			meanx = System.Convert.ToDouble(slopePeriod - 1) / 2;
			for (int index = 0; index < slopePeriod; index++)
					{
						variance1 = System.Convert.ToDouble(index) - meanx;
						sum1 += variance1 *(inputSeries[index] - meany);
						sum2 += variance1 * variance1;
					}
			double slope = sum1/sum2;
			double intercept = meany - (slope * meanx);
			for (int index = 0; index < slopePeriod; index ++)
				residualArray[index] = inputSeries[index] - (intercept + (slope * index));
		}
		
		private double SyncedResidualCorrelation(PriceSeries inputSeries1, SyncedDataSeries inputSeries2, int corrPeriod)
		//	Correlation of residuals (residual = error = difference between actual values and linear regression line)
		{
			int index1=0;
			double[] residualArray1 = new double[corrPeriod];
			double[] residualArray2 = new double[corrPeriod];
			double[] dataArray1 = new double[corrPeriod];
			double[] dataArray2 = new double[corrPeriod];
			for (int counter=corrPeriod-1; counter >= 0; counter--)
			{
				while (!inputSeries2.Synced[index1])  //Find next synced Bar
					index1++;
				dataArray1[counter] = inputSeries1[index1];
				dataArray2[counter] = inputSeries2[index1];
				index1++;
			}
			KDResidual(dataArray1,corrPeriod,residualArray1);
			KDResidual(dataArray2,corrPeriod,residualArray2);
//			return(KDRankCorrelation(residualArray1,residualArray2,corrPeriod));	// maybe rank correlation of residuals would be better?
			return(KDCorrelation(residualArray1,residualArray2,corrPeriod));
		}
		
		
		private void RankSort(double[] dataArray, double[] indexArray, int period)
		{
			int index1;
			int[] indexArray2 = new int[period];
			for (index1=0; index1<period; index1++)
				indexArray2[index1] = index1;
			Array.Sort(dataArray,indexArray2);
		
			for (index1=0; index1<period; index1++)
			{
				int index2 = index1+1;
				while ((index2 < period) && (dataArray[index1] == dataArray[Math.Min(index2,period-1)]))
					index2 = index2+1;  //look for series of identical values
				double avgIndex = System.Convert.ToDouble(index1 + index2 + 1) / 2; // Average & convert to 1 origin
				for (int index3 = index2-1; index3 >= index1; index3--)
					indexArray[indexArray2[index3]] = avgIndex;
				index1 = index2-1;
			}
		}
		
		private double KDRankCorrelation(double[] dataArray1, double[] dataArray2, int corrPeriod)
			//  KDRankCorrelation: Calculate the Spearman Rank Correlation of 2 Data Arrays
		{
			double[] indexArray1 = new double[corrPeriod];
			double[] indexArray2 = new double[corrPeriod];
			RankSort(dataArray1,indexArray1,corrPeriod);
			RankSort(dataArray2,indexArray2,corrPeriod);
			return(KDCorrelation(indexArray1,indexArray2,corrPeriod));
		}
		
		private double SyncedRankCorrelation(PriceSeries inputSeries1, SyncedDataSeries inputSeries2, int corrPeriod)
			//  SyncedRankCorrelation: Calculate the Spearman Rank Correlation of 2 Data Series, using only the bars which are synchronized
			//
			//	Inputs:
			//		inputSeries1: 1st DataSeries, typically Closes[x]
			//		inputSeries2: SyncedDataSeries, synced to InputSeries1; some bars (flagged by BoolSeries "Synced") hold valid (synchronized) values
			//		corrPeriod: Period for Correlation calculation; there must be >= CorrPeriod synced bars in the input series, or it will throw an exception
			//
		{
			int index1=0;
			double[] dataArray1 = new double[corrPeriod];
			double[] dataArray2 = new double[corrPeriod];
			for (int counter=corrPeriod-1; counter >= 0; counter--)
			{
				while (!inputSeries2.Synced[index1])  //Find next synced Bar
					index1++;
				dataArray1[counter] = inputSeries1[index1];
				dataArray2[counter] = inputSeries2[index1];
				index1++;
			}
			return(KDRankCorrelation(dataArray1,dataArray2,corrPeriod));
		}
		
		#endregion
		
		
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Correlation of Two Instruments";
				Name										= "Correlation";
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
				//Symbol2										= string.Empty;
				Period										= 100;
				PeriodPctChange								= 1;
				AddPlot(Brushes.CornflowerBlue, "Name");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("CL 01-18", BarsPeriodType.Day, 1);
				//AddDataSeries("CL 01-18", new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 1440 }, "Default 24 x 7");
			}
			else if (State == State.DataLoaded)
			{			
				Instrument2BIPClose = new SyncedDataSeries(this, Instrument1BIP, Instrument2BIP, Closes[Instrument2BIP]);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsPeriod.BarsPeriodType != BarsPeriodType.Day && !(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value == 1440))
			{
				Draw.TextFixed(this, "NinjaScriptInfo", "This indicator must be applied to a daily series", TextPosition.BottomRight);
				return;
			}
				
			try
			{
				Instrument2BIPClose.Synchronize();
			}
			catch (Exception ex)
			{
				Print(ex.ToString());
			}
			if (BarsInProgress == Instrument1BIP) // Instrument 1
			{
				if (Instrument2BIPClose.SynchronizedBarCount > (CorrType == CorrelationType.PctChange ? Period + PeriodPctChange : Period))	// No output if not enough synchronized bars for calculation
					if (Instrument2BIPClose.Synced[0])  // We have a new synchronized pair of closes; do the correlation calculation
						switch (CorrType)
						{
							case CorrelationType.Pearson:
								Value[0] = SyncedCorrelation(Closes[Instrument1BIP],Instrument2BIPClose,Period); 
								break;
							case CorrelationType.Rank:
								Value[0] = SyncedRankCorrelation(Closes[Instrument1BIP],Instrument2BIPClose,Period);
								break;
							case CorrelationType.Residual:
								Value[0] = SyncedResidualCorrelation(Closes[Instrument1BIP],Instrument2BIPClose,Period);
								break;
							case CorrelationType.PctChange:
								Value[0] = SyncedPctChangeCorrelation(Closes[Instrument1BIP],Instrument2BIPClose,Period,PeriodPctChange);
								break;
						}
					else
						if (Value.IsValidDataPoint(1))
							Value[0] = Value[1];	// Current bar is not synchronized; Use the last correlation again as the current one
													// First time through is guaranteed to be synced; if we get here Value[1] is always defined
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Correlation Lookback Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="CorrType", Description="Pearson(Standard), Rank(Spearman),\nResidual, Percent Change", Order=2, GroupName="Parameters")]
		public CorrelationType CorrType
        {
            get { return corrType; }
            set 
			{ 
				corrType = value; 
				if (Plots.Length > 0)
					switch (corrType)
					{
						case CorrelationType.Pearson:
							Plots[0].Brush = Brushes.CornflowerBlue;
							break;
						case CorrelationType.Rank:
							Plots[0].Brush = Brushes.Red;
							break;
						case CorrelationType.Residual:
							Plots[0].Brush = Brushes.ForestGreen;
							break;
						case CorrelationType.PctChange:
							Plots[0].Brush = Brushes.Purple;
							break;
					}
			}
        }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="PeriodPctChange", Description="Lookback Bars for PctChange", Order=3, GroupName="Parameters")]
		public int PeriodPctChange
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Default
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

namespace Correlation
{
	public enum CorrelationType
	{
		Pearson,
		Rank,
		Residual,
		PctChange
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Correlation[] cacheCorrelation;
		public Correlation Correlation(int period, CorrelationType corrType, int periodPctChange)
		{
			return Correlation(Input, period, corrType, periodPctChange);
		}

		public Correlation Correlation(ISeries<double> input, int period, CorrelationType corrType, int periodPctChange)
		{
			if (cacheCorrelation != null)
				for (int idx = 0; idx < cacheCorrelation.Length; idx++)
					if (cacheCorrelation[idx] != null && cacheCorrelation[idx].Period == period && cacheCorrelation[idx].CorrType == corrType && cacheCorrelation[idx].PeriodPctChange == periodPctChange && cacheCorrelation[idx].EqualsInput(input))
						return cacheCorrelation[idx];
			return CacheIndicator<Correlation>(new Correlation(){ Period = period, CorrType = corrType, PeriodPctChange = periodPctChange }, input, ref cacheCorrelation);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Correlation Correlation(int period, CorrelationType corrType, int periodPctChange)
		{
			return indicator.Correlation(Input, period, corrType, periodPctChange);
		}

		public Indicators.Correlation Correlation(ISeries<double> input , int period, CorrelationType corrType, int periodPctChange)
		{
			return indicator.Correlation(input, period, corrType, periodPctChange);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Correlation Correlation(int period, CorrelationType corrType, int periodPctChange)
		{
			return indicator.Correlation(Input, period, corrType, periodPctChange);
		}

		public Indicators.Correlation Correlation(ISeries<double> input , int period, CorrelationType corrType, int periodPctChange)
		{
			return indicator.Correlation(input, period, corrType, periodPctChange);
		}
	}
}

#endregion
