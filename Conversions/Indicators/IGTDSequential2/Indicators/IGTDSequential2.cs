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

using System.Globalization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class IGTDSequential2 : Indicator
	{
		#region Private Variables
		private Series<double> _dsTrueHigh;
		private Series<double> _dsTrueLow;
		private Series<double> _dsYHigh;
		private Series<double> _dsYLow;
		private Series<double> _dsYPixelOffsetHigh;
		private Series<double> _dsYPixelOffsetLow;
		
		private DashStyleHelper _dsTDST;
		private TDSequentialPlotType2 _TDSequentialPlotType2;
		private TDTerminationCount2 _TDTerminationCount2;
		private TDType2 _tdType2Previous;
		private TDSetupHelper2 _TDSetupHelper2ToRecycle;
		
		private bool _boolBearishTDPriceFlipOccurred;
		private bool _boolBullishTDPriceFlipOccurred;
		private bool _boolIsComboCountdownBuyInProgress;
		private bool _boolIsComboCountdownSellInProgress;
		private bool _boolIsSequentialCountdownBuyInProgress; 
		private bool _boolIsSequentialCountdownSellInProgress;
		private bool _boolWriteLOGOn;
		private bool _boolOR;
		private bool _boolQ1;
		
		private List<TDSetupHelper2> _alTDSetupHelper;
		private List<TDSTHelper2> _alTDSTHelperBuy2; 
		private List<TDSTHelper2> _alTDSTHelperSell2;
		
		private double _doublePointOffset;
		private double _doubleYLocation;
		private double _doubleTrueHigh;
		private double _doubleTrueLow;
		private double _doubleTerminationPrice;
		
		private int _intID;
		private int _intSetupRecycleCount;
		private int _intIndex;
		private int _intSetupCount;
		private int _intArrowCount;
		private int _intObjectCount;
		private int _intYPixelOffset;
		
		private static int _intIDtoFind = 0;
		#endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "IGTDSequential2";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				// General Parameters
				PlotSetupCountDownBuy						= true;
				PlotSetupCountDownSell						= true;
				PlotTDST									= true;
				PlotSequentialCountdownBuy					= true;
				PlotSequentialCountdownSell					= true;
				PlotComboCountdownBuy						= false;
				PlotComboCountdownSell						= false;
				PixelOffset									= 13;
				
				// Setup Coutndown Parameters
				SetupBars									= 9;
				SetupLookBackBars							= 4;
				PlotSetupCountDownAfterMinBars				= true;
				_TDSequentialPlotType2 						= TDSequentialPlotType2.DrawText;
				SetupFontColor								= Brushes.Green;
				SetupFontSize								= 8;
				SetupBackgroundColorBar1					= Brushes.Yellow;
				SetupFontColorBar1							= Brushes.Green;
				SetupFontSizeBar1							= 10;
				SetupBackgroundColorBar9					= Brushes.Yellow;
				SetupFontColorBar9							= Brushes.Green;
				SetupFontSizeBar9							= 10;
				DrawPerfectSignalArrow						= true;
				PerfectSignalBuyColor						= Brushes.Green;
				PerfectSignalSellColor						= Brushes.Red;
				CancelHaHC_LbLC								= false;
				CancelHaHH_LbLL								= false;
				CancelCaHC_CbLC								= false;
				CancelCaHH_CbLL								= false;
				CancelCaHTH_CbLTL							= false;
				
				// Setup TDST Parameters
				TDSTMaxNumber								= 0;
				_dsTDST 									= DashStyleHelper.Dash;
				TDSTResistanceColor							= Brushes.Red;
				TDSTSupportColor							= Brushes.Green;
				Width										= 2;
				BuyQ1OR										= true;
				SellQ1OR									= true;
				
				// Sequential Coutndown Parameters
				SequentialBars								= 13;
				SequentialLookBackBars						= 2;
				SequentialBar8VersusBar5Qualifier			= true;
				SequentialBar13VersusBar8Qualifier			= true;
				SequentialRecycleBars						= 22;
				_TDTerminationCount2 						= TDTerminationCount2.Close;
				SequentialFontColor							= Brushes.Blue;
				SequentialFontSize							= 8;
				SequentialBackgroundColorBar1				= Brushes.Yellow;
				SequentialFontColorBar1						= Brushes.Blue;
				SequentialFontSizeBar1						= 10;
				SequentialBackgroundColorBar13				= Brushes.Yellow;
				SequentialFontColorBar13					= Brushes.Blue;
				SequentialFontSizeBar13						= 10;
				SequentialBackgroundColorBarRecycle			= Brushes.Yellow;
				SequentialFontColorBarRecycle				= Brushes.Blue;
				SequentialFontSizeBarRecycle				= 13;
				SequentialCancelReverseSetup				= true;
				SequentialCancelTDST						= true;
				
				// Combo Countdown Parameters
				ComboBars									= 13;
				ComboLookBackBars							= 2;
				ComboRecycleBars							= 22;
				ComboFontColor								= Brushes.Black;
				ComboFontSize								= 8;
				ComboBackgroundColorBar1					= Brushes.Yellow;
				ComboFontColorBar1							= Brushes.Black;
				ComboFontSizeBar1							= 10;
				ComboBackgroundColorBar13					= Brushes.Yellow;
				ComboFontColorBar13							= Brushes.Black;
				ComboFontSizeBar13							= 10;
				ComboBackgroundColorBarRecycle				= Brushes.Yellow;
				ComboFontColorBarRecycle					= Brushes.Black;
				ComboFontSizeBarRecycle						= 13;
				
				// Private defaults
				_tdType2Previous 							= TDType2.Null;
				
				_boolBearishTDPriceFlipOccurred 			= false;
				_boolBullishTDPriceFlipOccurred 			= false;
				_boolIsComboCountdownBuyInProgress 			= false; 
				_boolIsComboCountdownSellInProgress 		= false;
				_boolIsSequentialCountdownBuyInProgress 	= false; 
				_boolIsSequentialCountdownSellInProgress 	= false;
				_boolWriteLOGOn 							= false; // todo
				_boolOR 									= false;
				_boolQ1 									= false;
				
				
				_doublePointOffset 							= 0.0;
				_doubleYLocation 							= 0.0;
				_doubleTrueHigh 							= 0.0;
				_doubleTrueLow 								= 0.0;
				_doubleTerminationPrice 					= 0.0;
				
				_intID			 							= 0;
				_intSetupRecycleCount 						= 0;
				_intIndex 									= 0;
				_intSetupCount 								= 0;
				_intArrowCount 								= 0;
				_intObjectCount 							= 0;
				_intYPixelOffset 							= 0;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{
				_alTDSetupHelper = new List<TDSetupHelper2>();
				_alTDSTHelperBuy2 = new List<TDSTHelper2>();
				_alTDSTHelperSell2 = new List<TDSTHelper2>();
				
				_dsTrueHigh = new Series<double>(this);
				_dsTrueLow = new Series<double>(this);
				_dsYHigh = new Series<double>(this);
				_dsYLow = new Series<double>(this);
				_dsYPixelOffsetHigh = new Series<double>(this);
				_dsYPixelOffsetLow = new Series<double>(this);

				_TDSetupHelper2ToRecycle = new TDSetupHelper2(-1, TDType2.Null, false);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < SetupBars*2) return;
			
			HouseKeeping();
			
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray()) // iterate through copy of the array
			{
				if (tdsh.Cancelled == false) Process(tdsh);	
			}
			
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray()) // iterate through copy of the array
			{
				if (tdsh.Tags != null) Cleanup(tdsh);	
			}
			
			// TDST
			if (PlotTDST) ProcessTDST();
		}
		
		private void RemoveDrawObjectsByTag(string tag)
		{
			RemoveDrawObject("Text"+tag);
			RemoveDrawObject("Line"+tag);
			RemoveDrawObject("Dot"+tag);
			RemoveDrawObject("ArrowUp"+tag);
			RemoveDrawObject("ArrowDown"+tag);
		}
		
		#region HouseKeeping
		private void HouseKeeping()
		{
			//if (_doublePointOffset == 0) _doublePointOffset = SMA(Range(), SetupBars)[0] * 0.21;
			_doublePointOffset = SMA(Range(), SetupBars)[0] * 0.21;
			if (_doublePointOffset < 0.002 && Instrument.MasterInstrument.InstrumentType == InstrumentType.Forex) _doublePointOffset = 0.002;
			if (_doublePointOffset < 0.34 && Instrument.MasterInstrument.InstrumentType == InstrumentType.Future) _doublePointOffset = 0.34;
			if (_doublePointOffset < 0.34 && Instrument.MasterInstrument.InstrumentType == InstrumentType.Stock) _doublePointOffset = 0.34;
			_boolBearishTDPriceFlipOccurred = IsBearishFlip;
			_boolBullishTDPriceFlipOccurred = IsBullishFlip;
			_boolIsComboCountdownBuyInProgress = false;
			_boolIsComboCountdownSellInProgress = false;
			_boolIsSequentialCountdownBuyInProgress = false;
			_boolIsSequentialCountdownSellInProgress = false;

			if (_boolBearishTDPriceFlipOccurred)
			{
				_intID++;
				_alTDSetupHelper.Add(new TDSetupHelper2(_intID, TDType2.Buy, PlotSetupCountDownAfterMinBars));
			}
			else if (_boolBullishTDPriceFlipOccurred)
			{
				_intID++;
				_alTDSetupHelper.Add(new TDSetupHelper2(_intID, TDType2.Sell, PlotSetupCountDownAfterMinBars));
			}
			
			if (CurrentBar == 0)
			{
				_dsTrueHigh[0] = (0);
				_dsTrueLow[0] = (0);
			}
			else
			{
				_dsTrueHigh[0] = (High[0] > Close[1] ? High[0] : Close[1]);
				_dsTrueLow[0] = (Low[0] < Close[1] ? Low[0] : Close[1]);
			}
			
			_dsYHigh[0] = (High[0]);
			_dsYLow[0] = (Low[0]);
			_dsYPixelOffsetHigh[0] = (0);
			_dsYPixelOffsetLow[0] = (0);
		}
		#endregion
		
		#region Cleanup
		private void Cleanup(TDSetupHelper2 tdsh)
		{
			if (tdsh.Cancelled ||
				(tdsh.Completed && tdsh.SearchForPerfectSignal == false && tdsh.PlotSetupCountDownAfterMinBars == false && 
				(tdsh.SequentialCountdownCancelled || tdsh.SequentialCountdownCompleted) && (tdsh.ComboCountdownCancelled || tdsh.ComboCountdownCompleted)))
			{
				Int32 int1 = _alTDSetupHelper.IndexOf(tdsh);
				if (tdsh.Cancelled)
				{
					for (int int2 = 0; int2 < tdsh.Tags.Length; int2++)
					{
						if (tdsh.Tags[int2] != null)
						{
							RemoveDrawObjectsByTag(tdsh.Tags[int2]); // remove td setup from chart
							if (tdsh.Type == _tdType2Previous) _intSetupRecycleCount--;
						}
						else
							break;
					}
					_alTDSetupHelper.RemoveAt(int1);
				}
				else
				{
					TDSetupHelper2 tdsh2 = _alTDSetupHelper[int1];
					tdsh2.Tags = null;
					_alTDSetupHelper[int1] = tdsh2;
				}
			}
		}
		#endregion
		
		#region Process
		private void Process(TDSetupHelper2 tdsh)
		{
			_intIDtoFind = tdsh.ID;
			_intIndex = _alTDSetupHelper.FindIndex(FindByID);

			if (tdsh.Type == TDType2.Buy)
			{
				_doubleYLocation = Low[0];
			}
			else if (tdsh.Type == TDType2.Sell)
			{
				_doubleYLocation = High[0];
			}

			// td setup cancellation rules
			if (tdsh.InProgress)
			{
				tdsh.Cancelled = (tdsh.Type == TDType2.Buy && _boolBullishTDPriceFlipOccurred) || // bullish td price flip (page 4)
					(tdsh.Type == TDType2.Sell && _boolBearishTDPriceFlipOccurred); // bearish td price flip (page 2)
			
				tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && Close[0] >= Close[SetupLookBackBars]) || // td buy setup failed (page 3)
					(tdsh.Type == TDType2.Sell && Close[0] <= Close[SetupLookBackBars]); // td sell setup failed (page 5)
				
				if (CancelHaHC_LbLC && tdsh.Count > 1) // New Market Timing Techniques (page 50)
				{
					tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && High[0] > MAX(Close, tdsh.Count)[0]) || 
					(tdsh.Type == TDType2.Sell && Low[0] < MIN(Close, tdsh.Count)[0]);
				}
				
				if (CancelHaHH_LbLL && tdsh.Count > 1) // New Market Timing Techniques (page 50)
				{
					tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && High[0] > MAX(High, tdsh.Count)[0]) || 
					(tdsh.Type == TDType2.Sell && Low[0] < MIN(Low, tdsh.Count)[0]);
				}
				
				if (CancelCaHC_CbLC && tdsh.Count > 1) // New Market Timing Techniques (page 50)
				{
					tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && Close[0] > MAX(Close, tdsh.Count)[0]) || 
					(tdsh.Type == TDType2.Sell && Close[0] < MIN(Close, tdsh.Count)[0]);
				}
				
				if (CancelCaHH_CbLL && tdsh.Count > 1) // New Market Timing Techniques (page 50)
				{
					tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && Close[0] > MAX(High, tdsh.Count)[0]) || 
					(tdsh.Type == TDType2.Sell && Close[0] < MIN(Low, tdsh.Count)[0]);
				}
				
				if (CancelCaHTH_CbLTL && tdsh.Count > 1) // New Market Timing Techniques (page 50)
				{
					tdsh.Cancelled = tdsh.Cancelled || 
					(tdsh.Type == TDType2.Buy && Close[0] > MAX(_dsTrueHigh, tdsh.Count)[0]) || 
					(tdsh.Type == TDType2.Sell && Close[0] < MIN(_dsTrueLow, tdsh.Count)[0]);
				}
			}
			else if (tdsh.Completed && tdsh.PlotSetupCountDownAfterMinBars)
			{
				tdsh.PlotSetupCountDownAfterMinBars = (tdsh.Type == TDType2.Buy && _boolBullishTDPriceFlipOccurred) || // bullish td price flip (page 4)
					(tdsh.Type == TDType2.Sell && _boolBearishTDPriceFlipOccurred); // bearish td price flip (page 2)
			
				tdsh.PlotSetupCountDownAfterMinBars = tdsh.PlotSetupCountDownAfterMinBars || 
					(tdsh.Type == TDType2.Buy && Close[0] >= Close[SetupLookBackBars]) || // td buy setup failed (page 3)
					(tdsh.Type == TDType2.Sell && Close[0] <= Close[SetupLookBackBars]); // td sell setup failed (page 5)
				
				tdsh.PlotSetupCountDownAfterMinBars = !tdsh.PlotSetupCountDownAfterMinBars;
			}
			
			if (tdsh.Cancelled == false && (tdsh.InProgress || (tdsh.Completed && tdsh.PlotSetupCountDownAfterMinBars)))
			{
				tdsh = ProcessSetupCountDown(tdsh);
				//if (_intSetupCount >= 2 && _intSetupRecycleCount >= SequentialRecycleBars) // (page 22 of Bloomberg DeMark Indicators book)
				if (_intSetupCount >= 2 && tdsh.Count >= SequentialRecycleBars) // (Kay Way)
				{
					//RecycleSequentialCountdown(_TDSetupHelper2ToRecycle);
					RecycleSequentialCountdown(tdsh.CountdownToRecycle as TDSetupHelper2);
					_intSetupCount--;
					_intSetupRecycleCount = _intSetupRecycleCount - tdsh.Count;
				}
			} 
			
			if (tdsh.Completed) 
			{
				if (tdsh.SearchForPerfectSignal) tdsh = PerfectSignalTest(tdsh);
				if ((tdsh.Type == TDType2.Buy && PlotSequentialCountdownBuy) || (tdsh.Type == TDType2.Sell && PlotSequentialCountdownSell)) tdsh = ProcessSequentialCountdown(tdsh);
				if ((tdsh.Type == TDType2.Buy && PlotComboCountdownBuy) || (tdsh.Type == TDType2.Sell && PlotComboCountdownSell)) tdsh = ProcessComboCountdown(tdsh);

				_boolIsComboCountdownBuyInProgress = _boolIsComboCountdownBuyInProgress == false && tdsh.Type == TDType2.Buy && tdsh.ComboCountdownInProgress;
				_boolIsComboCountdownSellInProgress = _boolIsComboCountdownSellInProgress == false && tdsh.Type == TDType2.Sell && tdsh.ComboCountdownInProgress;;
				_boolIsSequentialCountdownBuyInProgress = _boolIsSequentialCountdownBuyInProgress == false && tdsh.Type == TDType2.Buy && tdsh.SequentialCountdownInProgress;
				_boolIsSequentialCountdownSellInProgress = _boolIsSequentialCountdownSellInProgress == false && tdsh.Type == TDType2.Sell && tdsh.SequentialCountdownInProgress;
			}
			
			if (_boolWriteLOGOn)
			{
				string strMessage = _intIndex.ToString() + "-" +
					tdsh.Type.ToString() + 
					"- INTERNAL: " + _boolIsComboCountdownBuyInProgress.ToString() + " " + 
									 _boolIsComboCountdownSellInProgress.ToString() + " " + 
									 _boolIsSequentialCountdownBuyInProgress.ToString() + " " + 
									 _boolIsSequentialCountdownSellInProgress.ToString() + " " +
									 _intSetupRecycleCount.ToString() + " " +
					"- SETUP: " + tdsh.Cancelled.ToString() + " " + tdsh.Completed.ToString() + " " + tdsh.InProgress.ToString() + " Count " + tdsh.Count.ToString() +
					"- SEQUENTIAL: " + tdsh.SequentialCountdownCancelled.ToString() + " " + tdsh.SequentialCountdownCompleted.ToString() + " " + tdsh.SequentialCountdownInProgress.ToString() + " bar8 " + tdsh.SequentialBar8Close.ToString() +
					"- COMBO: " + tdsh.ComboCountdownCancelled.ToString() + " " + tdsh.ComboCountdownCompleted.ToString() + " " + tdsh.ComboCountdownInProgress.ToString() + " Count " + tdsh.ComboCount.ToString() +
					"";
				Log(strMessage, LogLevel.Information);
			}
			
			_alTDSetupHelper[_intIndex] = tdsh;
		}
		
		// Explicit predicate delegate 
		private static bool FindByID(TDSetupHelper2 tdsh)
		{
			return tdsh.ID == _intIDtoFind;
		}	
		#endregion
		
		#region Setup Countdown
		/// <summary>
		/// (page 9 of Bloomberg DeMark Indicators book)
		/// </summary>
		private TDSetupHelper2 PerfectSignalTest(TDSetupHelper2 tdsh)
		{
			if (tdsh.Type == TDType2.Buy)
			{
				if ((Low[0] <= tdsh.Bar6Low && Low[0] <= tdsh.Bar7Low) || 
					(tdsh.Bar8or9Low <= tdsh.Bar6Low && tdsh.Bar8or9Low <= tdsh.Bar7Low)) 
				{
					Draw.ArrowUp(this,"ArrowUp"+_intArrowCount++.ToString(), false, 0, _dsYLow[0] + _doublePointOffset * -1.5, PerfectSignalBuyColor);
					tdsh.SearchForPerfectSignal = false;
				}
			}
			else
			{
				if ((High[0] >= tdsh.Bar6High && High[0] >= tdsh.Bar7High) || 
					(tdsh.Bar8or9High >= tdsh.Bar6High && tdsh.Bar8or9High >= tdsh.Bar7High)) 
				{
					Draw.ArrowDown(this,"ArrowDown"+_intArrowCount++.ToString(), false, 0, _dsYHigh[0] + _doublePointOffset * 1.5, PerfectSignalSellColor);
					tdsh.SearchForPerfectSignal = false;
				}
			}
			return tdsh;
		}
		
		private TDSetupHelper2 ProcessSetupCountDown(TDSetupHelper2 tdsh)
		{
			tdsh.Count++;
			if (tdsh.Type == _tdType2Previous) _intSetupRecycleCount++;
			// Bar 1
			if (tdsh.Count == 1) 
			{
				TDSetupDrawText(tdsh, SetupBackgroundColorBar1, SetupFontColorBar1, SetupFontSizeBar1);
				tdsh.Tags.SetValue(_intObjectCount.ToString(), tdsh.Count-1);
				tdsh.Bar1High = High[0];
				tdsh.Bar1Low = Low[0];
			}
			// Bar N (normally 9)
			else if (tdsh.Count == SetupBars) 
			{
				TDSetupDrawText(tdsh, SetupBackgroundColorBar9, SetupFontColorBar9, SetupFontSizeBar9);
				tdsh.Tags.SetValue(_intObjectCount.ToString(), tdsh.Count-1);
				
				if (tdsh.Type == TDType2.Buy)
				{
					tdsh.Bar6Low = Low[3];
					tdsh.Bar7Low = Low[2];
					
					if (Low[0] < Low[1])
						tdsh.Bar8or9Low = Low[0]; // bar N low (normally 9)
					else
						tdsh.Bar8or9Low = Low[1]; // bar N-1 low (normally 8)
				}
				else
				{
					tdsh.Bar6High = High[3];
					tdsh.Bar7High = High[2];
					
					if (High[0] > High[1])
						tdsh.Bar8or9High = High[0]; // bar N high (normally 9)
					else
						tdsh.Bar8or9High = High[1]; // bar N-1 high (normally 8)
				}
				
				// TDST Helper
				if (PlotTDST && ((tdsh.Type == TDType2.Buy && PlotSetupCountDownBuy) || (tdsh.Type == TDType2.Sell && PlotSetupCountDownSell)))
				{
					if (TDSTMaxNumber > 0)
					{
						if (tdsh.Type == TDType2.Buy && _alTDSTHelperBuy2.Count >= TDSTMaxNumber)
						{
                    		RemoveDrawObjectsByTag(_alTDSTHelperBuy2[_alTDSTHelperBuy2.Count - TDSTMaxNumber].Tag); // remove oldest line from chart
						}
						if (tdsh.Type == TDType2.Sell && _alTDSTHelperSell2.Count >= TDSTMaxNumber)
						{
                    		RemoveDrawObjectsByTag(_alTDSTHelperSell2[_alTDSTHelperSell2.Count - TDSTMaxNumber].Tag); // remove oldest line from chart
						}
					}

					if (tdsh.Type == TDType2.Buy)
					{
						_alTDSTHelperBuy2.Add(new TDSTHelper2(tdsh.Type,
							false,
							"TDSTBuyBar1" + CurrentBar.ToString(), 
							tdsh.StartBar, // - intBar, // start bar
							tdsh.Type == TDType2.Buy ? tdsh.TrueHigh : tdsh.TrueLow, // start Y
							tdsh.StartBar, // - intBar, // end bar
							tdsh.Type == TDType2.Buy ? tdsh.TrueHigh : tdsh.TrueLow, // end Y
							(tdsh.Type == TDType2.Buy ? TDSTResistanceColor : TDSTSupportColor),
							DashStyleHelper,
							Width));
					}
					else if (tdsh.Type == TDType2.Sell)
					{
						_alTDSTHelperSell2.Add(new TDSTHelper2(tdsh.Type,
							false,
							"TDSTSellBar1" + CurrentBar.ToString(), 
							tdsh.StartBar, // - intBar, // start bar
							tdsh.Type == TDType2.Buy ? tdsh.TrueHigh : tdsh.TrueLow, // start Y
							tdsh.StartBar, // - intBar, // end bar
							tdsh.Type == TDType2.Buy ? tdsh.TrueHigh : tdsh.TrueLow, // end Y
							(tdsh.Type == TDType2.Buy ? TDSTResistanceColor : TDSTSupportColor),
							DashStyleHelper,
							Width));
					}
				}
			
				tdsh.Completed = true;
				tdsh.InProgress = false;
				tdsh.SearchForPerfectSignal = DrawPerfectSignalArrow && ((tdsh.Type == TDType2.Buy && PlotSetupCountDownBuy) || (tdsh.Type == TDType2.Sell && PlotSetupCountDownSell));
				
				tdsh.SequentialCountdownCancelled = (tdsh.Type == TDType2.Buy && PlotSequentialCountdownBuy == false) || (tdsh.Type == TDType2.Sell && PlotSequentialCountdownSell == false);
				tdsh.SequentialCountdownCompleted = (tdsh.Type == TDType2.Buy && PlotSequentialCountdownBuy == false) || (tdsh.Type == TDType2.Sell && PlotSequentialCountdownSell == false);
				tdsh.SequentialCountdownInProgress = (tdsh.Type == TDType2.Buy && PlotSequentialCountdownBuy) || (tdsh.Type == TDType2.Sell && PlotSequentialCountdownSell);
				
				tdsh.ComboCountdownCancelled = (tdsh.Type == TDType2.Buy && PlotComboCountdownBuy == false) || (tdsh.Type == TDType2.Sell && PlotComboCountdownSell == false);
				tdsh.ComboCountdownCompleted = (tdsh.Type == TDType2.Buy && PlotComboCountdownBuy == false) || (tdsh.Type == TDType2.Sell && PlotComboCountdownSell == false);
				tdsh.ComboCountdownInProgress = (tdsh.Type == TDType2.Buy && PlotComboCountdownBuy) || (tdsh.Type == TDType2.Sell && PlotComboCountdownSell);
				
				if (tdsh.ComboCountdownInProgress) tdsh = StartComboCountdown(tdsh);

                // Cancel Reverse Setup - Start
                // When a setup which is in the opposite direction to a countdown has reached the minimum required count. 
                // This cancellation works only when "REVERSE SETUP CANCEL" is true. So in cases where "reverse setup cancel" is true and 
                // a buy setup has reached the minimum required count (default count is 9), ALL sell countdowns in progress are cancelled. 
                // Likewise when a sell setup has reached the minimum required count, ALL buy countdowns in progress are cancelled.
				if (SequentialCancelReverseSetup)
				{
					if (PlotSequentialCountdownBuy && tdsh.Type == TDType2.Sell) CancelSequentialCountdownBuys(tdsh.ID);
					if (PlotSequentialCountdownSell && tdsh.Type == TDType2.Buy) CancelSequentialCountdownSells(tdsh.ID);
					if (PlotComboCountdownBuy && tdsh.Type == TDType2.Sell) CancelComboCountdownBuys(tdsh.ID);
					if (PlotComboCountdownSell && tdsh.Type == TDType2.Buy) CancelComboCountdownSells(tdsh.ID);
				}
                // Cancel Reverse Setup - End
				
				if (tdsh.Type != _tdType2Previous)
				{
					_intSetupCount = 0;
					_intSetupRecycleCount = SetupBars;
					_tdType2Previous = tdsh.Type;
				}
				tdsh.CountdownToRecycle = _TDSetupHelper2ToRecycle;
				_TDSetupHelper2ToRecycle = tdsh;
				_intSetupCount++;
			}
			// Bar 2 - N-1
			else
			{
				if (_TDSequentialPlotType2 == TDSequentialPlotType2.DrawDots)
					TDSetupDrawDot(tdsh);
				else
					TDSetupDrawText(tdsh, SetupFontColor, SetupFontSize);
				tdsh.Tags.SetValue(_intObjectCount.ToString(), tdsh.Count-1);
			}
			
			if (tdsh.InProgress)
			{
				if (High[0] > tdsh.HighestHigh) tdsh.HighestHigh = High[0];
				if (Low[0] < tdsh.LowestLow) tdsh.LowestLow = Low[0];
				
				_doubleTrueHigh = High[0] > Close[1] ? High[0] : Close[1];
				if (tdsh.Type == TDType2.Buy && _doubleTrueHigh > tdsh.TrueHigh)
				{
					tdsh.StartBar = CurrentBar;
					tdsh.TrueHigh = _doubleTrueHigh;
				}
				_doubleTrueLow = Low[0] < Close[1] ? Low[0] : Close[1];
				if (tdsh.Type == TDType2.Sell && _doubleTrueLow < tdsh.TrueLow)
				{
					tdsh.StartBar = CurrentBar;
					tdsh.TrueLow = _doubleTrueLow;
				}
			}
			
			return tdsh;
		}

		private void TDSetupDrawDot(TDSetupHelper2 tdsh)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy && PlotSetupCountDownBuy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				Draw.Dot(this, "Dot"+_intObjectCount.ToString(), true, 0, _dsYLow[0], Brushes.Lime);
			}
			else if (tdsh.Type == TDType2.Sell && PlotSetupCountDownSell)
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				Draw.Dot(this, "Dot"+_intObjectCount.ToString(), true, 0, _dsYHigh[0], Brushes.Red);
			}
		}
		
		private void TDSetupDrawText(TDSetupHelper2 tdsh, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy && PlotSetupCountDownBuy == false) return;
			if (tdsh.Type == TDType2.Sell && PlotSetupCountDownSell == false) return;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}

			Draw.Text(this,
				"Text"+_intObjectCount.ToString(),
				false, 
				tdsh.Count.ToString(), 
				0, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center,
				Brushes.Transparent,
				Brushes.Transparent,
				0); // areaOpacity
		}
		
		private void TDSetupDrawText(TDSetupHelper2 tdsh, Brush colorBackground, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy && PlotSetupCountDownBuy == false) return;
			if (tdsh.Type == TDType2.Sell && PlotSetupCountDownSell == false) return;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}

			Draw.Text(this,
				"Text"+_intObjectCount.ToString(),
				false, 
				tdsh.Count.ToString(), 
				0, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center,
				colorBackground,
				colorBackground,
				80); // areaOpacity
		}
		#endregion
		
		#region Setup Process TDST
		private void ProcessTDST()
		{
			TDSTHelper2 tdst;

            Int32 intStart = _alTDSTHelperBuy2.Count > 2 ? _alTDSTHelperBuy2.Count - 2 : 0;

            for (Int32 int1 = intStart; int1 < _alTDSTHelperBuy2.Count; int1++)
			{
				tdst = _alTDSTHelperBuy2[int1];
				if (tdst.Completed == false)
				{
					_boolQ1 = Close[2] < Close[3] &&
						Close[1] > Close[2] &&
						Close[1] > tdst.StartY &&
						High[1] > Open[1];
					_boolOR = High[0] > Open[0] &&
						Open[0] > Close[1];
					
					if (BuyQ1OR && _boolQ1 && _boolOR) 
					{
						Draw.Line(this,"Line"+tdst.Tag, false, CurrentBar - tdst.StartBar, tdst.StartY, 0, tdst.EndY, tdst.Brush, DashStyleHelper.Solid, tdst.Width);
						tdst.Completed = true;
						_alTDSTHelperBuy2[int1] = tdst;
					}
					else
					{
						Draw.Line(this,"Line"+tdst.Tag, false, CurrentBar - tdst.StartBar, tdst.StartY, 0, tdst.EndY, tdst.Brush, tdst.DashStyleHelper, tdst.Width);
					}
				}
			}

            intStart = _alTDSTHelperSell2.Count > 2 ? _alTDSTHelperSell2.Count - 2 : 0;

            for (Int32 int1 = intStart; int1 < _alTDSTHelperSell2.Count; int1++)
			{
				tdst = _alTDSTHelperSell2[int1];
				if (tdst.Completed == false)
				{
					_boolQ1 = Close[2] > Close[3] &&
						Close[1] < Close[2] &&
						Close[1] < tdst.StartY &&
						Low[1] < Open[1];
					_boolOR = Low[0] < Open[0] &&
						Open[0] < Close[1];
					
					if (SellQ1OR && _boolQ1 && _boolOR) 
					{
						Draw.Line(this, "Line"+tdst.Tag, false, CurrentBar - tdst.StartBar, tdst.StartY, 0, tdst.EndY, tdst.Brush, DashStyleHelper.Solid, tdst.Width);
						tdst.Completed = true;
						_alTDSTHelperSell2[int1] = tdst;
					}
					else
					{
						Draw.Line(this, "Line"+tdst.Tag, false, CurrentBar - tdst.StartBar, tdst.StartY, 0, tdst.EndY, tdst.Brush, tdst.DashStyleHelper, tdst.Width);
					}
				}
			}
		}
		#endregion
		
		#region Sequential Countdown
		/// <summary>
		/// 
		/// </summary>
		/// <param name="intID"></param>
		private void CancelSequentialCountdownBuys(Int32 intID)
		{
			Int32 int1 = 0;
			TDSetupHelper2 tdsh2;
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray())
			{
				if (tdsh.ID > intID) break;
				if (tdsh.Type == TDType2.Buy)
				{
					tdsh2 = (TDSetupHelper2)_alTDSetupHelper[int1];
					if (tdsh.SequentialCountdownInProgress)
					{
						RemoveCurrentSequentialCountdown(tdsh); // page 21 of Bloomberg DeMark Indicators book
						tdsh2.SequentialCountdownCancelled = true;
						tdsh2.SequentialCountdownInProgress = false;
					}
					tdsh2.SearchForPerfectSignal = false; // setup in the opposite direction, stop any perfection search
					_alTDSetupHelper[int1] = tdsh2;
				}
				int1++;
			}
		}
		
		private void CancelSequentialCountdownSells(Int32 intID)
		{
			Int32 int1 = 0;
			TDSetupHelper2 tdsh2;
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray())
			{
				if (tdsh.ID > intID) break;
				if (tdsh.Type == TDType2.Sell)
				{
					tdsh2 = (TDSetupHelper2)_alTDSetupHelper[int1];
					if (tdsh.SequentialCountdownInProgress)
					{						
						RemoveCurrentSequentialCountdown(tdsh); // page 34 of Bloomberg DeMark Indicators book
						tdsh2.SequentialCountdownCancelled = true;
						tdsh2.SequentialCountdownInProgress = false;
					}
					tdsh2.SearchForPerfectSignal = false; // setup in the opposite direction, stop any perfection search					
					_alTDSetupHelper[int1] = tdsh2;
				}
				int1++;
			}
		}
		
		private TDSetupHelper2 ProcessSequentialCountdown(TDSetupHelper2 tdsh)
		{
			if (tdsh.SequentialCountdownCancelled || tdsh.SequentialCountdownCompleted) return tdsh;

            // Cancel TDST - Start
            // Each setup which reaches the minimum required count gives rise to 1 countdown and 1 TDST Line. 
            // In the case of a buy setup which has reached the minimum required count, a TDST Buy Line is drawn and a Buy Countdown commences. 
            // At any point during the progress of this Buy Countdown, a true low is formed above the TDST Buy Line that commenced 
            // drawing at the same time as the Buy Countdown, the Buy Countdown is cancelled BUT all other Buy Countdowns which may be in 
            // progress continue on. With the sell countdown, a true high must be formed below the TDST Sell Line for 
            // the sell countdown in question to be cancelled. In this case, countdowns cancellations refer to a specific countdown.
			if (SequentialCancelTDST)
			{
				if (tdsh.Type == TDType2.Buy)
				{
					_doubleTrueLow = Low[0] < Close[1] ? Low[0] : Close[1];
					if (_doubleTrueLow > tdsh.TrueHigh)
					{
						RemoveCurrentSequentialCountdown(tdsh);
						_intIDtoFind = tdsh.ID;
						_intIndex = _alTDSetupHelper.FindIndex(FindByID);
						tdsh.SequentialCountdownCancelled = true;
						tdsh.SequentialCountdownInProgress = false;
						_alTDSetupHelper[_intIndex] = tdsh;
					}
				}
				else
				{
					_doubleTrueHigh = High[0] > Close[1] ? High[0] : Close[1];
					if (_doubleTrueHigh < tdsh.TrueLow)
					{
						RemoveCurrentSequentialCountdown(tdsh);
						_intIDtoFind = tdsh.ID;
						_intIndex = _alTDSetupHelper.FindIndex(FindByID);
						tdsh.SequentialCountdownCancelled = true;
						tdsh.SequentialCountdownInProgress = false;
						_alTDSetupHelper[_intIndex] = tdsh;
					}
				}
			}
            // Cancel TDST - End

			if ( _TDTerminationCount2 == TDTerminationCount2.Open && tdsh.SequentialCount == SequentialBars - 1)
			{		
				if (tdsh.Type == TDType2.Buy)
				{
					_doubleTerminationPrice = Open[0] > Close[0] ? Close[0] : Open[0];
				}
				else
				{
					_doubleTerminationPrice = Open[0] > Close[0] ? Open[0] : Close[0];
				}
			}
			else
			{
				_doubleTerminationPrice = Close[0];
			}

            if ((tdsh.Type == TDType2.Buy && _doubleTerminationPrice <= Low[SequentialLookBackBars]) || // czpiaor
                (tdsh.Type == TDType2.Sell && _doubleTerminationPrice >= High[SequentialLookBackBars])) // czpiaor
			{
				if (tdsh.SequentialCount < SequentialBars - 1)
				{
					if (SequentialBar8VersusBar5Qualifier && tdsh.SequentialCount == 7)
					{
						if ((tdsh.Type == TDType2.Buy && Low[0] <= tdsh.SequentialBar5Close) ||
							(tdsh.Type == TDType2.Sell && High[0] >= tdsh.SequentialBar5Close))
						{
							tdsh.SequentialCount++;
							tdsh.SequentialBar8Close = Close[0];
						}
						if ((tdsh.Type == TDType2.Buy && _boolIsSequentialCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsSequentialCountdownSellInProgress == false))
						{
							if (tdsh.SequentialCount == 8)
							{
								TDSequentialDrawText(tdsh, SequentialFontColor, SequentialFontSize);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
							else
							{
								TDSequentialDrawText(tdsh, "+", SequentialFontColor, SequentialFontSizeBar13);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
						}
					}
					else
					{
						tdsh.SequentialCount++;
						if ((tdsh.Type == TDType2.Buy && _boolIsSequentialCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsSequentialCountdownSellInProgress == false))
						{
							if (tdsh.SequentialCount == 1)
							{
								TDSequentialDrawText(tdsh, SequentialBackgroundColorBar1, SequentialFontColorBar1, SequentialFontSizeBar1);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
							else
							{
								TDSequentialDrawText(tdsh, SequentialFontColor, SequentialFontSize);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
						}
						if (tdsh.SequentialCount == 5) tdsh.SequentialBar5Close = Close[0];
						if (tdsh.SequentialCount == 8) tdsh.SequentialBar8Close = Close[0];
					}
				}
				else if (tdsh.SequentialCount == SequentialBars - 1) // additional requirement for bar 13 (page 19 of Bloomberg DeMark Indicators book)
				{
					if (SequentialBar13VersusBar8Qualifier)
					{
						if ((tdsh.Type == TDType2.Buy && Low[0] <= tdsh.SequentialBar8Close) ||
							(tdsh.Type == TDType2.Sell && High[0] >= tdsh.SequentialBar8Close))
						{
							tdsh.SequentialCount++;
							if ((tdsh.Type == TDType2.Buy && _boolIsSequentialCountdownBuyInProgress == false) ||
								(tdsh.Type == TDType2.Sell && _boolIsSequentialCountdownSellInProgress == false))
							{
								tdsh = TDSequentialDrawText(tdsh, SequentialBackgroundColorBar1, SequentialFontColorBar13, SequentialFontSizeBar13);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
							tdsh.SequentialCountdownCompleted = true;
							tdsh.SequentialCountdownInProgress = false;
						}
						else
						{
							if ((tdsh.Type == TDType2.Buy && _boolIsSequentialCountdownBuyInProgress == false) ||
								(tdsh.Type == TDType2.Sell && _boolIsSequentialCountdownSellInProgress == false))
							{
								TDSequentialDrawText(tdsh, "+", SequentialFontColor, SequentialFontSizeBar13);
								tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
							}
						}
					}
					else
					{
						tdsh.SequentialCount++;
						if ((tdsh.Type == TDType2.Buy && _boolIsSequentialCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsSequentialCountdownSellInProgress == false))
						{
							tdsh = TDSequentialDrawText(tdsh, SequentialBackgroundColorBar1, SequentialFontColorBar13, SequentialFontSizeBar13);
							tdsh.SequentialTags.SetValue(_intObjectCount.ToString(), tdsh.SequentialTagsIndex++); 
						}
						tdsh.SequentialCountdownCompleted = true;
						tdsh.SequentialCountdownInProgress = false;
					}
				}
			}

			return tdsh;
		}
		
		private string RemoveCurrentSequentialCountdown(TDSetupHelper2 tdsh)
		{
			string strTag13 = string.Empty;
			for (int int2 = 0; int2 < tdsh.SequentialTags.Length - 1; int2++)
			{
				if (tdsh.SequentialTags[int2] == null)
				{
					break;
				}
				else if (tdsh.SequentialTags[int2 + 1] == null && tdsh.SequentialCountdownCompleted)
				{
					strTag13 = tdsh.SequentialTags[int2];
					break;
				}
				RemoveDrawObjectsByTag(tdsh.SequentialTags[int2]); // remove td sequential countdown from chart
			}
			return strTag13;
		}
		
		private void RecycleSequentialCountdown(TDSetupHelper2 tdsh)
		{
			if (tdsh == null || tdsh.ID < 0) return;
			string strBar13Tag = RemoveCurrentSequentialCountdown(tdsh);
			if (strBar13Tag.Length > 0)
			{
				Draw.Text(this,
					"Text"+strBar13Tag,
					false, 
					"R", 
					CurrentBar - tdsh.SequentialBar13, 
					tdsh.SequentialBar13YLocation, 
					tdsh.SequentialBar13YPixelOffsetLow,
					SequentialFontColorBarRecycle,
					new SimpleFont("GenericSansSerif", SequentialFontSizeBarRecycle), 
					TextAlignment.Center, 
					Brushes.Transparent, 
					Brushes.Transparent, 
					0); 
			}
			tdsh.SequentialCountdownCompleted = true;
			tdsh.SequentialCountdownInProgress = false;
		}
		
		private void TDSequentialDrawText(TDSetupHelper2 tdsh, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				tdsh.SequentialCount.ToString(), 
				0, 
				_doubleYLocation, 
				_intYPixelOffset,
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				Brushes.Transparent, 
				Brushes.Transparent, 
				0); 
		}
		
		private TDSetupHelper2 TDSequentialDrawText(TDSetupHelper2 tdsh, Brush colorBackground, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				tdsh.SequentialCount.ToString(), 
				0, 
				_doubleYLocation, 
				_intYPixelOffset,
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				colorBackground, 
				colorBackground, 
				80); 
			tdsh.SequentialBar13 = CurrentBar;
			tdsh.SequentialBar13YLocation = _doubleYLocation;
			tdsh.SequentialBar13YPixelOffsetLow = _intYPixelOffset;
			return tdsh;
		}
		
		private void TDSequentialDrawText(TDSetupHelper2 tdsh, string strText, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				strText, 
				0, 
				_doubleYLocation, 
				_intYPixelOffset,
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				Brushes.Transparent, 
				Brushes.Transparent, 
				0); 
		}
		
		private void TDSequentialDrawText(TDSetupHelper2 tdsh, string strText, int intBarsAgo, Brush colorBackground, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[0] = (_dsYLow[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[0];
				_dsYPixelOffsetLow[0] = (_dsYPixelOffsetLow[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[0] = (_dsYHigh[0] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[0];
				_dsYPixelOffsetHigh[0] = (_dsYPixelOffsetHigh[0] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[0];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				strText, 
				intBarsAgo, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				colorBackground,
				colorBackground, 
				80); 
		}
		#endregion
	
		#region Combo Countdown
		private void CancelComboCountdownBuys(Int32 intID)
		{
			Int32 int1 = 0;
			TDSetupHelper2 tdsh2;
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray())
			{
				if (tdsh.ID > intID) break;
				if (tdsh.Type == TDType2.Buy)
				{
					tdsh2 = (TDSetupHelper2)_alTDSetupHelper[int1];
					if (tdsh.ComboCountdownInProgress)
					{
						RemoveCurrentComboCountdown(tdsh); // page 21 of Bloomberg DeMark Indicators book
						tdsh2.ComboCountdownCancelled = true;
						tdsh2.ComboCountdownInProgress = false;
					}
					tdsh2.SearchForPerfectSignal = false; // setup in the opposite direction, stop any perfection search
					_alTDSetupHelper[int1] = tdsh2;
				}
				int1++;
			}
		}
		
		private void CancelComboCountdownSells(Int32 intID)
		{
			Int32 int1 = 0;
			TDSetupHelper2 tdsh2;
			foreach (TDSetupHelper2 tdsh in _alTDSetupHelper.ToArray())
			{
				if (tdsh.ID > intID) break;
				if (tdsh.Type == TDType2.Sell)
				{
					tdsh2 = (TDSetupHelper2)_alTDSetupHelper[int1];
					if (tdsh.ComboCountdownInProgress)
					{						
						RemoveCurrentComboCountdown(tdsh); // page 34 of Bloomberg DeMark Indicators book
						tdsh2.ComboCountdownCancelled = true;
						tdsh2.ComboCountdownInProgress = false;
					}
					tdsh2.SearchForPerfectSignal = false; // setup in the opposite direction, stop any perfection search					
					_alTDSetupHelper[int1] = tdsh2;
				}
				int1++;
			}
		}
		
		private TDSetupHelper2 ProcessComboCountdown(TDSetupHelper2 tdsh)
		{
			if (tdsh.ComboCountdownCancelled || tdsh.ComboCountdownCompleted) return tdsh;
			
			if ((tdsh.Type == TDType2.Buy && (Close[0] <= Low[ComboLookBackBars] && Low[0] <= tdsh.LastComboLow && Close[0] < tdsh.LastComboClose && Close[0] < Close[1])) ||
				(tdsh.Type == TDType2.Sell && (Close[0] >= High[ComboLookBackBars] && High[0] >= tdsh.LastComboHigh && Close[0] > tdsh.LastComboClose && Close[0] > Close[1])))
			{
				tdsh.ComboCount++;
				if (tdsh.ComboCount == 1)
				{
					if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
						(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
					{
						TDComboDrawText(tdsh, 0, ComboBackgroundColorBar1, ComboFontColorBar1, ComboFontSizeBar1);
						tdsh.ComboTags.SetValue(_intObjectCount.ToString(), tdsh.ComboCount-1); 
					}
				}
				else if (tdsh.ComboCount == ComboBars)
				{
					if (_intSetupCount >= 2 && _intSetupRecycleCount >= ComboRecycleBars) // (page 22 of Bloomberg DeMark Indicators book)
					{
						RecycleComboCountdown(tdsh);
						_intSetupCount--;
						_intSetupRecycleCount = _intSetupRecycleCount - tdsh.Count;
						tdsh.ComboCountdownCompleted = true;
						tdsh.ComboCountdownInProgress = false;
					}
					else if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
					{
						TDComboDrawText(tdsh, 0, ComboBackgroundColorBar13, ComboFontColorBar13, ComboFontSizeBar13);
						tdsh.ComboTags.SetValue(_intObjectCount.ToString(), tdsh.ComboCount-1); 
					}
					tdsh.ComboCountdownCompleted = true;
					tdsh.ComboCountdownInProgress = false;
				}
				else
				{
					if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
						(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
					{
						TDComboDrawText(tdsh, 0, ComboFontColor, ComboFontSize);
						tdsh.ComboTags.SetValue(_intObjectCount.ToString(), tdsh.ComboCount-1); 
					}
				}
				tdsh.LastComboClose = Close[0];
				tdsh.LastComboHigh = High[0];
				tdsh.LastComboLow = Low[0];
			}
			
			if (tdsh.Type == TDType2.Buy)
			{
				if (Low[0] > tdsh.Bar1High)
				{
					RemoveCurrentComboCountdown(tdsh);
					tdsh.ComboCountdownCancelled = true;
					tdsh.ComboCountdownInProgress = false;
				}
			}
			else
			{
				if (High[0] < tdsh.Bar1Low)
				{
					RemoveCurrentComboCountdown(tdsh);
					tdsh.ComboCountdownCancelled = true;
					tdsh.ComboCountdownInProgress = false;
				}
			}
			
			return tdsh;
		}
		
		private void RecycleComboCountdown(TDSetupHelper2 tdsh)
		{
			RemoveCurrentComboCountdown(tdsh);
			if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
				(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
			{			
				TDComboDrawText(tdsh, "R", 0, ComboBackgroundColorBarRecycle, ComboFontColorBarRecycle, ComboFontSizeBarRecycle);
			}
		}

		private void RemoveCurrentComboCountdown(TDSetupHelper2 tdsh)
		{
			for (int int2 = 0; int2 < tdsh.ComboTags.Length; int2++)
			{
				if (tdsh.ComboTags[int2] != null)
					RemoveDrawObjectsByTag(tdsh.ComboTags[int2]); // remove td setup from chart
				else
					break;
			}
		}

		private TDSetupHelper2 StartComboCountdown(TDSetupHelper2 tdsh)
		{
			for (int index = 8; index >= 1; index--) 
			{
				if (tdsh.Type == TDType2.Buy)
				{
					if ((index == 8 && Close[index] <= Low[index+ComboLookBackBars] && Close[index] < Close[index+ComboLookBackBars-1]) ||
						(Close[index] <= Low[index+ComboLookBackBars] && Low[index] <= tdsh.LastComboLow && Close[index] < tdsh.LastComboClose && Close[index] < Close[index+ComboLookBackBars-1]))
					{
						tdsh.ComboCount++;
						if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
						{
							if (tdsh.ComboCount == 1)
							{
								TDComboDrawText(tdsh, index, ComboBackgroundColorBar1, ComboFontColorBar1, ComboFontSizeBar1);
							}
							else
							{
								TDComboDrawText(tdsh, index, ComboFontColor, ComboFontSize);
							}
							tdsh.ComboTags.SetValue(_intObjectCount.ToString(), tdsh.ComboCount-1); 
						}
						tdsh.LastComboClose = Close[index];
						tdsh.LastComboLow = Low[index];
					}
				}
				else
				{
					if ((index == 8 && Close[index] >= High[index+ComboLookBackBars] && Close[index] > Close[index+ComboLookBackBars-1]) ||
						(Close[index] >= High[index+ComboLookBackBars] && High[index] >= tdsh.LastComboHigh && Close[index] > tdsh.LastComboClose && Close[index] > Close[index+ComboLookBackBars-1]))
					{
						tdsh.ComboCount++;
						if ((tdsh.Type == TDType2.Buy && _boolIsComboCountdownBuyInProgress == false) ||
							(tdsh.Type == TDType2.Sell && _boolIsComboCountdownSellInProgress == false))
						{
							if (tdsh.ComboCount == 1)
							{
								TDComboDrawText(tdsh, index, ComboBackgroundColorBar1, ComboFontColorBar1, ComboFontSizeBar1);
							}
							else
							{
								TDComboDrawText(tdsh, index, ComboFontColor, ComboFontSize);
							}
							tdsh.ComboTags.SetValue(_intObjectCount.ToString(), tdsh.ComboCount-1); 
						}
						tdsh.LastComboClose = Close[index];
						tdsh.LastComboHigh = High[index];
					}
				}
			}
			return tdsh;
		}
		
		private void TDComboDrawText(TDSetupHelper2 tdsh, int intBarsAgo, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[intBarsAgo] = (_dsYLow[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[intBarsAgo];
				_dsYPixelOffsetLow[intBarsAgo] = (_dsYPixelOffsetLow[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[intBarsAgo];
			}
			else
			{
				_dsYHigh[intBarsAgo] = (_dsYHigh[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[intBarsAgo];
				_dsYPixelOffsetHigh[intBarsAgo] = (_dsYPixelOffsetHigh[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[intBarsAgo];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				tdsh.ComboCount.ToString(), 
				intBarsAgo, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				Brushes.Transparent, 
				Brushes.Transparent, 
				0); 
		}
		
		private void TDComboDrawText(TDSetupHelper2 tdsh, int intBarsAgo, Brush colorBackground, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[intBarsAgo] = (_dsYLow[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[intBarsAgo];
				_dsYPixelOffsetLow[intBarsAgo] = (_dsYPixelOffsetLow[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[0];
			}
			else
			{
				_dsYHigh[intBarsAgo] = (_dsYHigh[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[intBarsAgo];
				_dsYPixelOffsetHigh[intBarsAgo] = (_dsYPixelOffsetHigh[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[intBarsAgo];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				tdsh.ComboCount.ToString(), 
				intBarsAgo, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				colorBackground, 
				colorBackground, 
				80); 
		}
		
		private void TDComboDrawText(TDSetupHelper2 tdsh, string strText, int intBarsAgo, Brush colorBackground, Brush colorColor, int intFontSize)
		{
			_intObjectCount++;
			if (tdsh.Type == TDType2.Buy)
			{
				_dsYLow[intBarsAgo] = (_dsYLow[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = Low[intBarsAgo];
				_dsYPixelOffsetLow[intBarsAgo] = ( _dsYPixelOffsetLow[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetLow[intBarsAgo];
			}
			else
			{
				_dsYHigh[intBarsAgo] = (_dsYHigh[intBarsAgo] + _doublePointOffset * tdsh.Multiplier);
				_doubleYLocation = High[intBarsAgo];
				_dsYPixelOffsetHigh[intBarsAgo] = (_dsYPixelOffsetHigh[intBarsAgo] + PixelOffset * tdsh.Multiplier);
				_intYPixelOffset = (int)_dsYPixelOffsetHigh[intBarsAgo];
			}
			Draw.Text(this,
				"Text"+_intObjectCount.ToString(), 
				false, 
				strText, 
				intBarsAgo, 
				_doubleYLocation, 
				_intYPixelOffset, // yPixelOffset
				colorColor, 
				new SimpleFont("GenericSansSerif", intFontSize), 
				TextAlignment.Center, 
				colorBackground,
				colorBackground, 
				80); 
		}
		#endregion
		
		#region Private Properties
		private bool BearishTDPriceFlipOccurred
		{
			get { return _boolBearishTDPriceFlipOccurred; }
			set { _boolBearishTDPriceFlipOccurred = value; }
		}
		private bool BullishTDPriceFlipOccurred
		{
			get { return _boolBullishTDPriceFlipOccurred; }
			set { _boolBullishTDPriceFlipOccurred = value; }
		}
		/// <summary>
		/// (page 2 of Bloomberg DeMark Indicators book)
		/// </summary>
		/// <returns></returns>
		private bool IsBearishFlip
		{
			get { return Close[0] < Close[SetupLookBackBars] && Close[1] > Close[SetupLookBackBars + 1]; }
		}
		/// <summary>
		/// (page 4 of Bloomberg DeMark Indicators book)
		/// </summary>
		/// <returns></returns>
		private bool IsBullishFlip
		{
			get { return Close[0] > Close[SetupLookBackBars] && Close[1] < Close[SetupLookBackBars + 1]; }
		}
		/// <summary>
		/// (page 3 of Bloomberg DeMark Indicators book)
		/// </summary>
		/// <returns></returns>
		private bool IsBarTDBuySetup
		{
			get
			{
				// need to be a for loop
				bool boolReturn = true;
				for (Int32 int1 = 0; int1 < SetupBars; int1++)
				{
					boolReturn = Close[int1] >= Close[SetupLookBackBars + int1];
					if (boolReturn == false) break;
				}
				return boolReturn;
			}
		}
		/// <summary>
		/// (page 4 of Bloomberg DeMark Indicators book)
		/// </summary>
		/// <returns></returns>
		private bool IsBarTDSellSetup
		{
			get
			{
				// need to be a for loop
				bool boolReturn = true;
				for (Int32 int1 = 0; int1 < SetupBars; int1++)
				{
					boolReturn = Close[int1] <= Close[SetupLookBackBars + int1];
					if (boolReturn == false) break;
				}
				return boolReturn;
			}
		}
		#endregion

		#region Public Properties
		[NinjaScriptProperty]
		[Display(Name="Plot Setup Countdown Buy", Description="Plot Setup Countdown Buy.", Order=1, GroupName="Parameters")]
		public bool PlotSetupCountDownBuy
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot Setup Countdown Sell", Description="Plot Setup Countdown Sell.", Order=2, GroupName="Parameters")]
		public bool PlotSetupCountDownSell
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot TDST", Description="Plot TDST.", Order=3, GroupName="Parameters")]
		public bool PlotTDST
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot Sequential Countdown Buy", Description="Plot Sequential Countdown Buy.", Order=4, GroupName="Parameters")]
		public bool PlotSequentialCountdownBuy
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot Sequential Countdown Sell", Description="Plot Sequential Countdown Sell.", Order=5, GroupName="Parameters")]
		public bool PlotSequentialCountdownSell
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot Combo Countdown Buy", Description="Plot Combo Countdown Buy.", Order=6, GroupName="Parameters")]
		public bool PlotComboCountdownBuy
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Plot Combo Countdown Sell", Description="Plot Combo Countdown Sell.", Order=7, GroupName="Parameters")]
		public bool PlotComboCountdownSell
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Y Pixel Offset", Description="Y Pixel Offset.", Order=8, GroupName="Parameters")]
		public int PixelOffset
		{ get; set; }

		#region Setup Countdown
		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Bars needed", Description="Number of bars needed (minimum of 5 bars).", Order=9, GroupName="Setup Countdown")]
		public int SetupBars
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Look back bars", Description="Look back bars (minimum of 2 bars).", Order=10, GroupName="Setup Countdown")]
		public int SetupLookBackBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Continue countdown after minimum bars", Description="Continue countdown after minimum bars.", Order=11, GroupName="Setup Countdown")]
		public bool PlotSetupCountDownAfterMinBars
		{ get; set; }
		
		[TypeConverter(typeof(FriendlyEnumConverter))] // Converts the enum to string values
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Enums normally automatically get a combo box, but we need to apply this specific editor so default value is automatically selected
		[Display(Name="Plot type", Description="Plot type.", Order=12, GroupName="Setup Countdown")]
		public TDSequentialPlotType2 PlotType
        {
            get { return _TDSequentialPlotType2; }
            set { _TDSequentialPlotType2 = value; }
        }

        public class FriendlyEnumConverter : TypeConverter
	    {
	        // Set the values to appear in the combo box
	        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	        {
	            List<string> values = new List<string>() { "DrawText", "DrawDots" };

	            return new StandardValuesCollection(values);
	        }

	        // map the value from "Friendly" string to MyEnum type
	        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	        {
	            string stringVal = value.ToString();
	            switch (stringVal)
	            {
	                case "DrawText":
	                return TDSequentialPlotType2.DrawText;
	                case "DrawDots":
	                return TDSequentialPlotType2.DrawDots;
	            }
	            return TDSequentialPlotType2.DrawText;
	        }

	        // map the MyEnum type to "Friendly" string
	        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	        {
	            TDSequentialPlotType2 stringVal = (TDSequentialPlotType2) Enum.Parse(typeof(TDSequentialPlotType2), value.ToString());
	            switch (stringVal)
	            {
	                case TDSequentialPlotType2.DrawText:
	                return "DrawText";
	                case TDSequentialPlotType2.DrawDots:
	                return "DrawDots";
	            }
	            return string.Empty;
	        }

	        // required interface members needed to compile
	        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	        { return true; }

	        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	        { return true; }

	        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	        { return true; }

	        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	        { return true; }
	    }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color", Description="Font color.", Order=13, GroupName="Setup Countdown")]
		public Brush SetupFontColor
		{ get; set; }

		[Browsable(false)]
		public string SetupFontColorSerializable
		{
			get { return Serialize.BrushToString(SetupFontColor); }
			set { SetupFontColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size", Description="Font size.", Order=14, GroupName="Setup Countdown")]
		public int SetupFontSize
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 1", Description="Background color bar 1.", Order=15, GroupName="Setup Countdown")]
		public Brush SetupBackgroundColorBar1
		{ get; set; }

		[Browsable(false)]
		public string SetupBackgroundColorBar1Serializable
		{
			get { return Serialize.BrushToString(SetupBackgroundColorBar1); }
			set { SetupBackgroundColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 1", Description="Font color bar 1.", Order=16, GroupName="Setup Countdown")]
		public Brush SetupFontColorBar1
		{ get; set; }

		[Browsable(false)]
		public string SetupFontColorBar1Serializable
		{
			get { return Serialize.BrushToString(SetupFontColorBar1); }
			set { SetupFontColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 1", Description="Font size.", Order=17, GroupName="Setup Countdown")]
		public int SetupFontSizeBar1
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 9", Description="Background color bar 9.", Order=18, GroupName="Setup Countdown")]
		public Brush SetupBackgroundColorBar9
		{ get; set; }

		[Browsable(false)]
		public string SetupBackgroundColorBar9Serializable
		{
			get { return Serialize.BrushToString(SetupBackgroundColorBar9); }
			set { SetupBackgroundColorBar9 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 9", Description="Font color bar 9.", Order=19, GroupName="Setup Countdown")]
		public Brush SetupFontColorBar9
		{ get; set; }

		[Browsable(false)]
		public string SetupFontColorBar9Serializable
		{
			get { return Serialize.BrushToString(SetupFontColorBar9); }
			set { SetupFontColorBar9 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 9", Description="Font size.", Order=20, GroupName="Setup Countdown")]
		public int SetupFontSizeBar9
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Draw perfect signal arrow", Description="Draw perfect signal arrow.", Order=21, GroupName="Setup Countdown")]
		public bool DrawPerfectSignalArrow
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Perfect signal buy color", Description="Perfect signal buy color.", Order=22, GroupName="Setup Countdown")]
		public Brush PerfectSignalBuyColor
		{ get; set; }

		[Browsable(false)]
		public string PerfectSignalBuyColorSerializable
		{
			get { return Serialize.BrushToString(PerfectSignalBuyColor); }
			set { PerfectSignalBuyColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Perfect signal sell color", Description="Perfect signal sell color.", Order=23, GroupName="Setup Countdown")]
		public Brush PerfectSignalSellColor
		{ get; set; }

		[Browsable(false)]
		public string PerfectSignalSellColorSerializable
		{
			get { return Serialize.BrushToString(PerfectSignalSellColor); }
			set { PerfectSignalSellColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Cancel if HaHC or LbLC", Description="Cancel if High above Highest Close of the entire Buy Setup or Low below Lowest Close of the entire Sell Setup.", Order=24, GroupName="Setup Countdown")]
		public bool CancelHaHC_LbLC
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel if HaHH or LbLL", Description="Cancel if High above Highest High of the entire Buy Setup or Low below Lowest Low of the entire Sell Setup.", Order=25, GroupName="Setup Countdown")]
		public bool CancelHaHH_LbLL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel if CaHC or CbLC", Description="Cancel if Close above Highest Close of the entire Buy Setup or Close below Lowest Close of the entire Sell Setup.", Order=26, GroupName="Setup Countdown")]
		public bool CancelCaHC_CbLC
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel if CaHH or CbLL", Description="Cancel if Close above Highest High of the entire Buy Setup or Close below Lowest Low of the entire Sell Setup.", Order=27, GroupName="Setup Countdown")]
		public bool CancelCaHH_CbLL
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel if CaHTH or CbLTL", Description="Cancel if Close above Highest True High of the entire Buy Setup or Close below Lowest True Low of the entire Sell Setup.", Order=28, GroupName="Setup Countdown")]
		public bool CancelCaHTH_CbLTL
		{ get; set; }
		#endregion

		#region Setup TDST
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Maximun number", Description="Maximun number on chart, use 0 to plot all.", Order=29, GroupName="Setup TDST")]
		public int TDSTMaxNumber
		{ get; set; }
		
		[TypeConverter(typeof(FriendlyEnumConverter2))] // Converts the enum to string values
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Enums normally automatically get a combo box, but we need to apply this specific editor so default value is automatically selected
		[Display(Name="Dash Style", Description="Dash Style.", Order=30, GroupName="Setup TDST")]
		public DashStyleHelper DashStyleHelper
        {
            get { return _dsTDST; }
            set { _dsTDST = value; }
        }

        public class FriendlyEnumConverter2 : TypeConverter
	    {
	        // Set the values to appear in the combo box
	        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	        {
	            List<string> values = new List<string>() { "Solid", "Dash", "Dot", "DashDot", "DashDotDot" };

	            return new StandardValuesCollection(values);
	        }

	        // map the value from "Friendly" string to MyEnum type
	        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	        {
	            string stringVal = value.ToString();
	            switch (stringVal)
	            {
	                case "Solid":
	                return DashStyleHelper.Solid;
	                case "Dash":
	                return DashStyleHelper.Dash;
					case "Dot":
	                return DashStyleHelper.Dot;
	                case "DashDot":
	                return DashStyleHelper.DashDot;
	                case "DashDotDot":
	                return DashStyleHelper.DashDotDot;
	            }
	            return DashStyleHelper.Solid;
	        }

	        // map the MyEnum type to "Friendly" string
	        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	        {
	            DashStyleHelper stringVal = (DashStyleHelper) Enum.Parse(typeof(DashStyleHelper), value.ToString());
	            switch (stringVal)
	            {
	                case DashStyleHelper.Solid:
	                return "Solid";
	                case DashStyleHelper.Dash:
	                return "Dash";
					case DashStyleHelper.Dot:
	                return "Dot";
	                case DashStyleHelper.DashDot:
	                return "DashDot";
					case DashStyleHelper.DashDotDot:
	                return "DashDotDot";
	            }
	            return string.Empty;
	        }

	        // required interface members needed to compile
	        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	        { return true; }

	        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	        { return true; }

	        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	        { return true; }

	        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	        { return true; }
	    }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Resistance Color", Description="Resistance Color.", Order=31, GroupName="Setup TDST")]
		public Brush TDSTResistanceColor
		{ get; set; }

		[Browsable(false)]
		public string TDSTResistanceColorSerializable
		{
			get { return Serialize.BrushToString(TDSTResistanceColor); }
			set { TDSTResistanceColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Support Color", Description="Support Color.", Order=32, GroupName="Setup TDST")]
		public Brush TDSTSupportColor
		{ get; set; }

		[Browsable(false)]
		public string TDSTSupportColorSerializable
		{
			get { return Serialize.BrushToString(TDSTSupportColor); }
			set { TDSTSupportColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Width", Description="Width (0-8).", Order=33, GroupName="Setup TDST")]
		public int Width
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Buy Q1OR", Description="Buy Q1OR.", Order=34, GroupName="Setup TDST")]
		public bool BuyQ1OR
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Sell Q1OR", Description="Sell Q1OR.", Order=35, GroupName="Setup TDST")]
		public bool SellQ1OR
		{ get; set; }
		#endregion
		
		#region Sequential Countdown
		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Bars needed", Description="Number of bars needed (minimum of 5 bars).", Order=36, GroupName="Sequential Countdown")]
		public int SequentialBars
		{ get; set; }

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Look back bars", Description="Look back bars (minimum of 2 bars).", Order=37, GroupName="Sequential Countdown")]
		public int SequentialLookBackBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Bar 8 versus bar 5 qualifier", Description="Bar 8 versus bar 5 qualifier.", Order=38, GroupName="Sequential Countdown")]
		public bool SequentialBar8VersusBar5Qualifier
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Bar 13 versus bar 8 qualifier", Description="Bar 13 versus bar 8 qualifier.", Order=39, GroupName="Sequential Countdown")]
		public bool SequentialBar13VersusBar8Qualifier
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Recycle count", Description="Number of Setup bars needed for a recycle.", Order=40, GroupName="Sequential Countdown")]
		public int SequentialRecycleBars
		{ get; set; }
		
		[TypeConverter(typeof(FriendlyEnumConverter3))] // Converts the enum to string values
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Enums normally automatically get a combo box, but we need to apply this specific editor so default value is automatically selected
		[Display(Name="Termination count", Description="This refers to the 13th count where instead of comparing the close vs the low of 2 bars earlier, you compare the open vs the low of 2 bars earlier.", Order=41, GroupName="Sequential Countdown")]
		public TDTerminationCount2 TerminationCount
        {
            get { return _TDTerminationCount2; }
            set { _TDTerminationCount2 = value; }
        }

        public class FriendlyEnumConverter3 : TypeConverter
	    {
	        // Set the values to appear in the combo box
	        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	        {
	            List<string> values = new List<string>() { "Close", "Open" };

	            return new StandardValuesCollection(values);
	        }

	        // map the value from "Friendly" string to MyEnum type
	        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	        {
	            string stringVal = value.ToString();
	            switch (stringVal)
	            {
	                case "Close":
	                return TDTerminationCount2.Close;
	                case "Open":
	                return TDTerminationCount2.Open;
	            }
	            return DashStyleHelper.Solid;
	        }

	        // map the MyEnum type to "Friendly" string
	        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	        {
	            TDTerminationCount2 stringVal = (TDTerminationCount2) Enum.Parse(typeof(TDTerminationCount2), value.ToString());
	            switch (stringVal)
	            {
	                case TDTerminationCount2.Close:
	                return "Close";
	                case TDTerminationCount2.Open:
	                return "Open";
	            }
	            return string.Empty;
	        }

	        // required interface members needed to compile
	        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	        { return true; }

	        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	        { return true; }

	        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
	        { return true; }

	        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	        { return true; }
	    }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color", Description="Font color.", Order=42, GroupName="Sequential Countdown")]
		public Brush SequentialFontColor
		{ get; set; }

		[Browsable(false)]
		public string SequentialFontColorSerializable
		{
			get { return Serialize.BrushToString(SequentialFontColor); }
			set { SequentialFontColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size", Description="Font size.", Order=43, GroupName="Sequential Countdown")]
		public int SequentialFontSize
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 1", Description="Background color bar 1.", Order=44, GroupName="Sequential Countdown")]
		public Brush SequentialBackgroundColorBar1
		{ get; set; }

		[Browsable(false)]
		public string SequentialBackgroundColorBar1Serializable
		{
			get { return Serialize.BrushToString(SequentialBackgroundColorBar1); }
			set { SequentialBackgroundColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 1", Description="Font color bar 1.", Order=45, GroupName="Sequential Countdown")]
		public Brush SequentialFontColorBar1
		{ get; set; }

		[Browsable(false)]
		public string SequentialFontColorBar1Serializable
		{
			get { return Serialize.BrushToString(SequentialFontColorBar1); }
			set { SequentialFontColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 1", Description="Font size bar 1.", Order=46, GroupName="Sequential Countdown")]
		public int SequentialFontSizeBar1
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 13", Description="Background color bar 13.", Order=47, GroupName="Sequential Countdown")]
		public Brush SequentialBackgroundColorBar13
		{ get; set; }

		[Browsable(false)]
		public string SequentialBackgroundColorBar13Serializable
		{
			get { return Serialize.BrushToString(SequentialBackgroundColorBar13); }
			set { SequentialBackgroundColorBar13 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 13", Description="Font color bar 13.", Order=48, GroupName="Sequential Countdown")]
		public Brush SequentialFontColorBar13
		{ get; set; }

		[Browsable(false)]
		public string SequentialFontColorBar13Serializable
		{
			get { return Serialize.BrushToString(SequentialFontColorBar13); }
			set { SequentialFontColorBar13 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 13", Description="Font size bar 13.", Order=49, GroupName="Sequential Countdown")]
		public int SequentialFontSizeBar13
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar Recycle", Description="Background color bar Recycle.", Order=50, GroupName="Sequential Countdown")]
		public Brush SequentialBackgroundColorBarRecycle
		{ get; set; }

		[Browsable(false)]
		public string SequentialBackgroundColorBarRecycleSerializable
		{
			get { return Serialize.BrushToString(SequentialBackgroundColorBarRecycle); }
			set { SequentialBackgroundColorBarRecycle = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar Recycle", Description="Font color bar Recycle.", Order=51, GroupName="Sequential Countdown")]
		public Brush SequentialFontColorBarRecycle
		{ get; set; }

		[Browsable(false)]
		public string SequentialFontColorBarRecycleSerializable
		{
			get { return Serialize.BrushToString(SequentialFontColorBarRecycle); }
			set { SequentialFontColorBarRecycle = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar Recycle", Description="Font size bar Recycle.", Order=52, GroupName="Sequential Countdown")]
		public int SequentialFontSizeBarRecycle
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel if reverse setup", Description="This option cancel countdowns (sequential and combo) in progress once the minimum required count of a setup in the opposite direction is met.", Order=53, GroupName="Sequential Countdown")]
		public bool SequentialCancelReverseSetup
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Cancel TDST", Description="This option cancel sequential countdowns in progress if a true low is formed above the TDST Buy Line for a Buy Countdown or a true high is formed below the TDST Sell Line for a Sell Countdown.", Order=54, GroupName="Sequential Countdown")]
		public bool SequentialCancelTDST
		{ get; set; }
		#endregion

		#region Combo Countdown
		[NinjaScriptProperty]
		[Range(8, int.MaxValue)]
		[Display(Name="Bars needed", Description="Number of bars needed (minimum of 8 bars).", Order=55, GroupName="Combo Countdown")]
		public int ComboBars
		{ get; set; }

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Look back bars", Description="Look back bars (minimum of 2 bars).", Order=56, GroupName="Combo Countdown")]
		public int ComboLookBackBars
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Recycle count", Description="Number of Setup bars needed for a recycle.", Order=57, GroupName="Combo Countdown")]
		public int ComboRecycleBars
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color", Description="Font color.", Order=58, GroupName="Combo Countdown")]
		public Brush ComboFontColor
		{ get; set; }

		[Browsable(false)]
		public string ComboFontColorSerializable
		{
			get { return Serialize.BrushToString(ComboFontColor); }
			set { ComboFontColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size", Description="Font size.", Order=59, GroupName="Combo Countdown")]
		public int ComboFontSize
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 1", Description="Background color bar 1.", Order=60, GroupName="Combo Countdown")]
		public Brush ComboBackgroundColorBar1
		{ get; set; }

		[Browsable(false)]
		public string ComboBackgroundColorBar1Serializable
		{
			get { return Serialize.BrushToString(ComboBackgroundColorBar1); }
			set { ComboBackgroundColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 1", Description="Font color bar 1.", Order=61, GroupName="Combo Countdown")]
		public Brush ComboFontColorBar1
		{ get; set; }

		[Browsable(false)]
		public string ComboFontColorBar1Serializable
		{
			get { return Serialize.BrushToString(ComboFontColorBar1); }
			set { ComboFontColorBar1 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 1", Description="Font size bar 1.", Order=62, GroupName="Combo Countdown")]
		public int ComboFontSizeBar1
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar 13", Description="Background color bar 13.", Order=63, GroupName="Combo Countdown")]
		public Brush ComboBackgroundColorBar13
		{ get; set; }

		[Browsable(false)]
		public string ComboBackgroundColorBar13Serializable
		{
			get { return Serialize.BrushToString(ComboBackgroundColorBar13); }
			set { ComboBackgroundColorBar13 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar 13", Description="Font color bar 13.", Order=64, GroupName="Combo Countdown")]
		public Brush ComboFontColorBar13
		{ get; set; }

		[Browsable(false)]
		public string ComboFontColorBar13Serializable
		{
			get { return Serialize.BrushToString(ComboFontColorBar13); }
			set { ComboFontColorBar13 = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar 13", Description="Font size bar 13.", Order=65, GroupName="Combo Countdown")]
		public int ComboFontSizeBar13
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BG color bar Recycle", Description="Background color bar Recycle.", Order=66, GroupName="Combo Countdown")]
		public Brush ComboBackgroundColorBarRecycle
		{ get; set; }

		[Browsable(false)]
		public string ComboBackgroundColorBarRecycleSerializable
		{
			get { return Serialize.BrushToString(ComboBackgroundColorBarRecycle); }
			set { ComboBackgroundColorBarRecycle = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Font color bar Recycle", Description="Font color bar Recycle.", Order=67, GroupName="Combo Countdown")]
		public Brush ComboFontColorBarRecycle
		{ get; set; }

		[Browsable(false)]
		public string ComboFontColorBarRecycleSerializable
		{
			get { return Serialize.BrushToString(ComboFontColorBarRecycle); }
			set { ComboFontColorBarRecycle = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="Font size bar Recycle", Description="Font size bar Recycle.", Order=68, GroupName="Combo Countdown")]
		public int ComboFontSizeBarRecycle
		{ get; set; }
		#endregion
		#endregion

	}
}

public enum TDSequentialPlotType2
{
	DrawDots,
	DrawText
}

public enum TDTerminationCount2
{
	Close,
	Open
}

public enum TDType2
{
	Buy,
	Sell,
	Null
}

public class TDSetupHelper2 
{
	public bool Cancelled;
	public bool Completed;
	public bool ComboCountdownCancelled;
	public bool ComboCountdownCompleted;
	public bool ComboCountdownInProgress;
	public bool InProgress;
	public bool PlotSetupCountDownAfterMinBars;
	public bool SearchForPerfectSignal;
	public bool SequentialCountdownCancelled;
	public bool SequentialCountdownCompleted;
    public bool SequentialCountdownInProgress;
    public bool SequentialCountdownIsRecycle;
    public bool SequentialCountdownRecycled;
	
	public double Bar1High; 
	public double Bar6High; 
	public double Bar7High; 
	public double Bar8or9High; 
	public double Bar1Low; 
	public double Bar6Low; 
	public double Bar7Low; 
	public double Bar8or9Low;

	public double LastComboClose;
	public double LastComboHigh;
	public double LastComboLow;

	public double Multiplier;
	public double SequentialBar5Close; 
	public double SequentialBar8Close; 
	public double SequentialBar13YLocation; 
	
	public int StartBar; 
	public double HighestHigh; 
	public double LowestLow; 
	public double TrueHigh; 
	public double TrueLow; 

	public int ComboCount;
	public int Count;
	public int ID;
	public int SequentialBar13;
	public int SequentialBar13YPixelOffsetLow; 
	public int SequentialCount;
	public int SequentialTagsIndex;

	public string[] ComboTags; 
	public string[] SequentialTags;
	public string[] Tags;

	public object CountdownToRecycle;
	public TDType2 Type;

	public TDSetupHelper2(Int32 intID, TDType2 TDType2, bool boolPlotSetupCountDownAfterMinBars)
	{
		Type = TDType2;

		Cancelled = false;
		Completed = false;
		ComboCountdownCancelled = false;
		ComboCountdownCompleted = false;
		ComboCountdownInProgress = false;
		InProgress = true;
		PlotSetupCountDownAfterMinBars = boolPlotSetupCountDownAfterMinBars;
		SearchForPerfectSignal = false;
		SequentialCountdownCancelled = false;
		SequentialCountdownCompleted = false;
        SequentialCountdownInProgress = false;
        SequentialCountdownIsRecycle = false;
        SequentialCountdownRecycled = false;

		Bar1High = 0.0; 
		Bar6High = 0.0; 
		Bar7High = 0.0; 
		Bar8or9High = 0.0;
		Bar1Low = 0.0; 
		Bar6Low = 0.0; 
		Bar7Low = 0.0; 
		Bar8or9Low = 0.0;
		
		if (TDType2 == TDType2.Buy)
		{
			LastComboClose = double.MaxValue;
			LastComboHigh = 0.0;
			LastComboLow = double.MaxValue;
			Multiplier = -1.0;			
		}
		else
		{
			LastComboClose = double.MinValue;
			LastComboHigh = double.MinValue;
			LastComboLow = 0.0;
			Multiplier = 1.0;			
		}

		SequentialBar5Close = 0.0;
		SequentialBar8Close = 0.0;
		SequentialBar13YLocation = 0.0;
		StartBar = 0;
		HighestHigh = double.MinValue;
		LowestLow = double.MaxValue;
		TrueHigh = double.MinValue;
		TrueLow = double.MaxValue;
		
		ComboCount = 0;		
		Count = 0;
		ID = intID;
		SequentialBar13 = 0;
		SequentialBar13YPixelOffsetLow = 0;
		SequentialCount = 0;
		SequentialTagsIndex = 0;
		
		ComboTags = new String[144];
		SequentialTags = new String[144];
		Tags = new String[144];
		
		CountdownToRecycle = null;
	}
}

public struct TDSTHelper2 
{
	public TDType2 Type;
	public bool Completed;
	public string Tag;
	public int StartBar;
	public double StartY;
	public int EndBar;
	public double EndY;
	public Brush Brush;
	public DashStyleHelper DashStyleHelper;
	public int Width;
	
	public TDSTHelper2(TDType2 TDType2, bool boolCompleted, string strTag, int intStartBar, double doubleStartY, int intEndBar, double doubleEndY, Brush colorTDST, DashStyleHelper dsTDST, int intWidth) 
	{
		Type = TDType2;
		Completed = boolCompleted;
		Tag = strTag;
		StartBar = intStartBar;    
		StartY = doubleStartY;    
		EndBar = intEndBar;    
		EndY = doubleEndY;    
		Brush = colorTDST;    
		DashStyleHelper = dsTDST;    
		Width = intWidth;    
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IGTDSequential2[] cacheIGTDSequential2;
		public IGTDSequential2 IGTDSequential2(bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			return IGTDSequential2(Input, plotSetupCountDownBuy, plotSetupCountDownSell, plotTDST, plotSequentialCountdownBuy, plotSequentialCountdownSell, plotComboCountdownBuy, plotComboCountdownSell, pixelOffset, setupBars, setupLookBackBars, plotSetupCountDownAfterMinBars, setupFontColor, setupFontSize, setupBackgroundColorBar1, setupFontColorBar1, setupFontSizeBar1, setupBackgroundColorBar9, setupFontColorBar9, setupFontSizeBar9, drawPerfectSignalArrow, perfectSignalBuyColor, perfectSignalSellColor, cancelHaHC_LbLC, cancelHaHH_LbLL, cancelCaHC_CbLC, cancelCaHH_CbLL, cancelCaHTH_CbLTL, tDSTMaxNumber, tDSTResistanceColor, tDSTSupportColor, width, buyQ1OR, sellQ1OR, sequentialBars, sequentialLookBackBars, sequentialBar8VersusBar5Qualifier, sequentialBar13VersusBar8Qualifier, sequentialRecycleBars, sequentialFontColor, sequentialFontSize, sequentialBackgroundColorBar1, sequentialFontColorBar1, sequentialFontSizeBar1, sequentialBackgroundColorBar13, sequentialFontColorBar13, sequentialFontSizeBar13, sequentialBackgroundColorBarRecycle, sequentialFontColorBarRecycle, sequentialFontSizeBarRecycle, sequentialCancelReverseSetup, sequentialCancelTDST, comboBars, comboLookBackBars, comboRecycleBars, comboFontColor, comboFontSize, comboBackgroundColorBar1, comboFontColorBar1, comboFontSizeBar1, comboBackgroundColorBar13, comboFontColorBar13, comboFontSizeBar13, comboBackgroundColorBarRecycle, comboFontColorBarRecycle, comboFontSizeBarRecycle);
		}

		public IGTDSequential2 IGTDSequential2(ISeries<double> input, bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			if (cacheIGTDSequential2 != null)
				for (int idx = 0; idx < cacheIGTDSequential2.Length; idx++)
					if (cacheIGTDSequential2[idx] != null && cacheIGTDSequential2[idx].PlotSetupCountDownBuy == plotSetupCountDownBuy && cacheIGTDSequential2[idx].PlotSetupCountDownSell == plotSetupCountDownSell && cacheIGTDSequential2[idx].PlotTDST == plotTDST && cacheIGTDSequential2[idx].PlotSequentialCountdownBuy == plotSequentialCountdownBuy && cacheIGTDSequential2[idx].PlotSequentialCountdownSell == plotSequentialCountdownSell && cacheIGTDSequential2[idx].PlotComboCountdownBuy == plotComboCountdownBuy && cacheIGTDSequential2[idx].PlotComboCountdownSell == plotComboCountdownSell && cacheIGTDSequential2[idx].PixelOffset == pixelOffset && cacheIGTDSequential2[idx].SetupBars == setupBars && cacheIGTDSequential2[idx].SetupLookBackBars == setupLookBackBars && cacheIGTDSequential2[idx].PlotSetupCountDownAfterMinBars == plotSetupCountDownAfterMinBars && cacheIGTDSequential2[idx].SetupFontColor == setupFontColor && cacheIGTDSequential2[idx].SetupFontSize == setupFontSize && cacheIGTDSequential2[idx].SetupBackgroundColorBar1 == setupBackgroundColorBar1 && cacheIGTDSequential2[idx].SetupFontColorBar1 == setupFontColorBar1 && cacheIGTDSequential2[idx].SetupFontSizeBar1 == setupFontSizeBar1 && cacheIGTDSequential2[idx].SetupBackgroundColorBar9 == setupBackgroundColorBar9 && cacheIGTDSequential2[idx].SetupFontColorBar9 == setupFontColorBar9 && cacheIGTDSequential2[idx].SetupFontSizeBar9 == setupFontSizeBar9 && cacheIGTDSequential2[idx].DrawPerfectSignalArrow == drawPerfectSignalArrow && cacheIGTDSequential2[idx].PerfectSignalBuyColor == perfectSignalBuyColor && cacheIGTDSequential2[idx].PerfectSignalSellColor == perfectSignalSellColor && cacheIGTDSequential2[idx].CancelHaHC_LbLC == cancelHaHC_LbLC && cacheIGTDSequential2[idx].CancelHaHH_LbLL == cancelHaHH_LbLL && cacheIGTDSequential2[idx].CancelCaHC_CbLC == cancelCaHC_CbLC && cacheIGTDSequential2[idx].CancelCaHH_CbLL == cancelCaHH_CbLL && cacheIGTDSequential2[idx].CancelCaHTH_CbLTL == cancelCaHTH_CbLTL && cacheIGTDSequential2[idx].TDSTMaxNumber == tDSTMaxNumber && cacheIGTDSequential2[idx].TDSTResistanceColor == tDSTResistanceColor && cacheIGTDSequential2[idx].TDSTSupportColor == tDSTSupportColor && cacheIGTDSequential2[idx].Width == width && cacheIGTDSequential2[idx].BuyQ1OR == buyQ1OR && cacheIGTDSequential2[idx].SellQ1OR == sellQ1OR && cacheIGTDSequential2[idx].SequentialBars == sequentialBars && cacheIGTDSequential2[idx].SequentialLookBackBars == sequentialLookBackBars && cacheIGTDSequential2[idx].SequentialBar8VersusBar5Qualifier == sequentialBar8VersusBar5Qualifier && cacheIGTDSequential2[idx].SequentialBar13VersusBar8Qualifier == sequentialBar13VersusBar8Qualifier && cacheIGTDSequential2[idx].SequentialRecycleBars == sequentialRecycleBars && cacheIGTDSequential2[idx].SequentialFontColor == sequentialFontColor && cacheIGTDSequential2[idx].SequentialFontSize == sequentialFontSize && cacheIGTDSequential2[idx].SequentialBackgroundColorBar1 == sequentialBackgroundColorBar1 && cacheIGTDSequential2[idx].SequentialFontColorBar1 == sequentialFontColorBar1 && cacheIGTDSequential2[idx].SequentialFontSizeBar1 == sequentialFontSizeBar1 && cacheIGTDSequential2[idx].SequentialBackgroundColorBar13 == sequentialBackgroundColorBar13 && cacheIGTDSequential2[idx].SequentialFontColorBar13 == sequentialFontColorBar13 && cacheIGTDSequential2[idx].SequentialFontSizeBar13 == sequentialFontSizeBar13 && cacheIGTDSequential2[idx].SequentialBackgroundColorBarRecycle == sequentialBackgroundColorBarRecycle && cacheIGTDSequential2[idx].SequentialFontColorBarRecycle == sequentialFontColorBarRecycle && cacheIGTDSequential2[idx].SequentialFontSizeBarRecycle == sequentialFontSizeBarRecycle && cacheIGTDSequential2[idx].SequentialCancelReverseSetup == sequentialCancelReverseSetup && cacheIGTDSequential2[idx].SequentialCancelTDST == sequentialCancelTDST && cacheIGTDSequential2[idx].ComboBars == comboBars && cacheIGTDSequential2[idx].ComboLookBackBars == comboLookBackBars && cacheIGTDSequential2[idx].ComboRecycleBars == comboRecycleBars && cacheIGTDSequential2[idx].ComboFontColor == comboFontColor && cacheIGTDSequential2[idx].ComboFontSize == comboFontSize && cacheIGTDSequential2[idx].ComboBackgroundColorBar1 == comboBackgroundColorBar1 && cacheIGTDSequential2[idx].ComboFontColorBar1 == comboFontColorBar1 && cacheIGTDSequential2[idx].ComboFontSizeBar1 == comboFontSizeBar1 && cacheIGTDSequential2[idx].ComboBackgroundColorBar13 == comboBackgroundColorBar13 && cacheIGTDSequential2[idx].ComboFontColorBar13 == comboFontColorBar13 && cacheIGTDSequential2[idx].ComboFontSizeBar13 == comboFontSizeBar13 && cacheIGTDSequential2[idx].ComboBackgroundColorBarRecycle == comboBackgroundColorBarRecycle && cacheIGTDSequential2[idx].ComboFontColorBarRecycle == comboFontColorBarRecycle && cacheIGTDSequential2[idx].ComboFontSizeBarRecycle == comboFontSizeBarRecycle && cacheIGTDSequential2[idx].EqualsInput(input))
						return cacheIGTDSequential2[idx];
			return CacheIndicator<IGTDSequential2>(new IGTDSequential2(){ PlotSetupCountDownBuy = plotSetupCountDownBuy, PlotSetupCountDownSell = plotSetupCountDownSell, PlotTDST = plotTDST, PlotSequentialCountdownBuy = plotSequentialCountdownBuy, PlotSequentialCountdownSell = plotSequentialCountdownSell, PlotComboCountdownBuy = plotComboCountdownBuy, PlotComboCountdownSell = plotComboCountdownSell, PixelOffset = pixelOffset, SetupBars = setupBars, SetupLookBackBars = setupLookBackBars, PlotSetupCountDownAfterMinBars = plotSetupCountDownAfterMinBars, SetupFontColor = setupFontColor, SetupFontSize = setupFontSize, SetupBackgroundColorBar1 = setupBackgroundColorBar1, SetupFontColorBar1 = setupFontColorBar1, SetupFontSizeBar1 = setupFontSizeBar1, SetupBackgroundColorBar9 = setupBackgroundColorBar9, SetupFontColorBar9 = setupFontColorBar9, SetupFontSizeBar9 = setupFontSizeBar9, DrawPerfectSignalArrow = drawPerfectSignalArrow, PerfectSignalBuyColor = perfectSignalBuyColor, PerfectSignalSellColor = perfectSignalSellColor, CancelHaHC_LbLC = cancelHaHC_LbLC, CancelHaHH_LbLL = cancelHaHH_LbLL, CancelCaHC_CbLC = cancelCaHC_CbLC, CancelCaHH_CbLL = cancelCaHH_CbLL, CancelCaHTH_CbLTL = cancelCaHTH_CbLTL, TDSTMaxNumber = tDSTMaxNumber, TDSTResistanceColor = tDSTResistanceColor, TDSTSupportColor = tDSTSupportColor, Width = width, BuyQ1OR = buyQ1OR, SellQ1OR = sellQ1OR, SequentialBars = sequentialBars, SequentialLookBackBars = sequentialLookBackBars, SequentialBar8VersusBar5Qualifier = sequentialBar8VersusBar5Qualifier, SequentialBar13VersusBar8Qualifier = sequentialBar13VersusBar8Qualifier, SequentialRecycleBars = sequentialRecycleBars, SequentialFontColor = sequentialFontColor, SequentialFontSize = sequentialFontSize, SequentialBackgroundColorBar1 = sequentialBackgroundColorBar1, SequentialFontColorBar1 = sequentialFontColorBar1, SequentialFontSizeBar1 = sequentialFontSizeBar1, SequentialBackgroundColorBar13 = sequentialBackgroundColorBar13, SequentialFontColorBar13 = sequentialFontColorBar13, SequentialFontSizeBar13 = sequentialFontSizeBar13, SequentialBackgroundColorBarRecycle = sequentialBackgroundColorBarRecycle, SequentialFontColorBarRecycle = sequentialFontColorBarRecycle, SequentialFontSizeBarRecycle = sequentialFontSizeBarRecycle, SequentialCancelReverseSetup = sequentialCancelReverseSetup, SequentialCancelTDST = sequentialCancelTDST, ComboBars = comboBars, ComboLookBackBars = comboLookBackBars, ComboRecycleBars = comboRecycleBars, ComboFontColor = comboFontColor, ComboFontSize = comboFontSize, ComboBackgroundColorBar1 = comboBackgroundColorBar1, ComboFontColorBar1 = comboFontColorBar1, ComboFontSizeBar1 = comboFontSizeBar1, ComboBackgroundColorBar13 = comboBackgroundColorBar13, ComboFontColorBar13 = comboFontColorBar13, ComboFontSizeBar13 = comboFontSizeBar13, ComboBackgroundColorBarRecycle = comboBackgroundColorBarRecycle, ComboFontColorBarRecycle = comboFontColorBarRecycle, ComboFontSizeBarRecycle = comboFontSizeBarRecycle }, input, ref cacheIGTDSequential2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IGTDSequential2 IGTDSequential2(bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			return indicator.IGTDSequential2(Input, plotSetupCountDownBuy, plotSetupCountDownSell, plotTDST, plotSequentialCountdownBuy, plotSequentialCountdownSell, plotComboCountdownBuy, plotComboCountdownSell, pixelOffset, setupBars, setupLookBackBars, plotSetupCountDownAfterMinBars, setupFontColor, setupFontSize, setupBackgroundColorBar1, setupFontColorBar1, setupFontSizeBar1, setupBackgroundColorBar9, setupFontColorBar9, setupFontSizeBar9, drawPerfectSignalArrow, perfectSignalBuyColor, perfectSignalSellColor, cancelHaHC_LbLC, cancelHaHH_LbLL, cancelCaHC_CbLC, cancelCaHH_CbLL, cancelCaHTH_CbLTL, tDSTMaxNumber, tDSTResistanceColor, tDSTSupportColor, width, buyQ1OR, sellQ1OR, sequentialBars, sequentialLookBackBars, sequentialBar8VersusBar5Qualifier, sequentialBar13VersusBar8Qualifier, sequentialRecycleBars, sequentialFontColor, sequentialFontSize, sequentialBackgroundColorBar1, sequentialFontColorBar1, sequentialFontSizeBar1, sequentialBackgroundColorBar13, sequentialFontColorBar13, sequentialFontSizeBar13, sequentialBackgroundColorBarRecycle, sequentialFontColorBarRecycle, sequentialFontSizeBarRecycle, sequentialCancelReverseSetup, sequentialCancelTDST, comboBars, comboLookBackBars, comboRecycleBars, comboFontColor, comboFontSize, comboBackgroundColorBar1, comboFontColorBar1, comboFontSizeBar1, comboBackgroundColorBar13, comboFontColorBar13, comboFontSizeBar13, comboBackgroundColorBarRecycle, comboFontColorBarRecycle, comboFontSizeBarRecycle);
		}

		public Indicators.IGTDSequential2 IGTDSequential2(ISeries<double> input , bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			return indicator.IGTDSequential2(input, plotSetupCountDownBuy, plotSetupCountDownSell, plotTDST, plotSequentialCountdownBuy, plotSequentialCountdownSell, plotComboCountdownBuy, plotComboCountdownSell, pixelOffset, setupBars, setupLookBackBars, plotSetupCountDownAfterMinBars, setupFontColor, setupFontSize, setupBackgroundColorBar1, setupFontColorBar1, setupFontSizeBar1, setupBackgroundColorBar9, setupFontColorBar9, setupFontSizeBar9, drawPerfectSignalArrow, perfectSignalBuyColor, perfectSignalSellColor, cancelHaHC_LbLC, cancelHaHH_LbLL, cancelCaHC_CbLC, cancelCaHH_CbLL, cancelCaHTH_CbLTL, tDSTMaxNumber, tDSTResistanceColor, tDSTSupportColor, width, buyQ1OR, sellQ1OR, sequentialBars, sequentialLookBackBars, sequentialBar8VersusBar5Qualifier, sequentialBar13VersusBar8Qualifier, sequentialRecycleBars, sequentialFontColor, sequentialFontSize, sequentialBackgroundColorBar1, sequentialFontColorBar1, sequentialFontSizeBar1, sequentialBackgroundColorBar13, sequentialFontColorBar13, sequentialFontSizeBar13, sequentialBackgroundColorBarRecycle, sequentialFontColorBarRecycle, sequentialFontSizeBarRecycle, sequentialCancelReverseSetup, sequentialCancelTDST, comboBars, comboLookBackBars, comboRecycleBars, comboFontColor, comboFontSize, comboBackgroundColorBar1, comboFontColorBar1, comboFontSizeBar1, comboBackgroundColorBar13, comboFontColorBar13, comboFontSizeBar13, comboBackgroundColorBarRecycle, comboFontColorBarRecycle, comboFontSizeBarRecycle);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IGTDSequential2 IGTDSequential2(bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			return indicator.IGTDSequential2(Input, plotSetupCountDownBuy, plotSetupCountDownSell, plotTDST, plotSequentialCountdownBuy, plotSequentialCountdownSell, plotComboCountdownBuy, plotComboCountdownSell, pixelOffset, setupBars, setupLookBackBars, plotSetupCountDownAfterMinBars, setupFontColor, setupFontSize, setupBackgroundColorBar1, setupFontColorBar1, setupFontSizeBar1, setupBackgroundColorBar9, setupFontColorBar9, setupFontSizeBar9, drawPerfectSignalArrow, perfectSignalBuyColor, perfectSignalSellColor, cancelHaHC_LbLC, cancelHaHH_LbLL, cancelCaHC_CbLC, cancelCaHH_CbLL, cancelCaHTH_CbLTL, tDSTMaxNumber, tDSTResistanceColor, tDSTSupportColor, width, buyQ1OR, sellQ1OR, sequentialBars, sequentialLookBackBars, sequentialBar8VersusBar5Qualifier, sequentialBar13VersusBar8Qualifier, sequentialRecycleBars, sequentialFontColor, sequentialFontSize, sequentialBackgroundColorBar1, sequentialFontColorBar1, sequentialFontSizeBar1, sequentialBackgroundColorBar13, sequentialFontColorBar13, sequentialFontSizeBar13, sequentialBackgroundColorBarRecycle, sequentialFontColorBarRecycle, sequentialFontSizeBarRecycle, sequentialCancelReverseSetup, sequentialCancelTDST, comboBars, comboLookBackBars, comboRecycleBars, comboFontColor, comboFontSize, comboBackgroundColorBar1, comboFontColorBar1, comboFontSizeBar1, comboBackgroundColorBar13, comboFontColorBar13, comboFontSizeBar13, comboBackgroundColorBarRecycle, comboFontColorBarRecycle, comboFontSizeBarRecycle);
		}

		public Indicators.IGTDSequential2 IGTDSequential2(ISeries<double> input , bool plotSetupCountDownBuy, bool plotSetupCountDownSell, bool plotTDST, bool plotSequentialCountdownBuy, bool plotSequentialCountdownSell, bool plotComboCountdownBuy, bool plotComboCountdownSell, int pixelOffset, int setupBars, int setupLookBackBars, bool plotSetupCountDownAfterMinBars, Brush setupFontColor, int setupFontSize, Brush setupBackgroundColorBar1, Brush setupFontColorBar1, int setupFontSizeBar1, Brush setupBackgroundColorBar9, Brush setupFontColorBar9, int setupFontSizeBar9, bool drawPerfectSignalArrow, Brush perfectSignalBuyColor, Brush perfectSignalSellColor, bool cancelHaHC_LbLC, bool cancelHaHH_LbLL, bool cancelCaHC_CbLC, bool cancelCaHH_CbLL, bool cancelCaHTH_CbLTL, int tDSTMaxNumber, Brush tDSTResistanceColor, Brush tDSTSupportColor, int width, bool buyQ1OR, bool sellQ1OR, int sequentialBars, int sequentialLookBackBars, bool sequentialBar8VersusBar5Qualifier, bool sequentialBar13VersusBar8Qualifier, int sequentialRecycleBars, Brush sequentialFontColor, int sequentialFontSize, Brush sequentialBackgroundColorBar1, Brush sequentialFontColorBar1, int sequentialFontSizeBar1, Brush sequentialBackgroundColorBar13, Brush sequentialFontColorBar13, int sequentialFontSizeBar13, Brush sequentialBackgroundColorBarRecycle, Brush sequentialFontColorBarRecycle, int sequentialFontSizeBarRecycle, bool sequentialCancelReverseSetup, bool sequentialCancelTDST, int comboBars, int comboLookBackBars, int comboRecycleBars, Brush comboFontColor, int comboFontSize, Brush comboBackgroundColorBar1, Brush comboFontColorBar1, int comboFontSizeBar1, Brush comboBackgroundColorBar13, Brush comboFontColorBar13, int comboFontSizeBar13, Brush comboBackgroundColorBarRecycle, Brush comboFontColorBarRecycle, int comboFontSizeBarRecycle)
		{
			return indicator.IGTDSequential2(input, plotSetupCountDownBuy, plotSetupCountDownSell, plotTDST, plotSequentialCountdownBuy, plotSequentialCountdownSell, plotComboCountdownBuy, plotComboCountdownSell, pixelOffset, setupBars, setupLookBackBars, plotSetupCountDownAfterMinBars, setupFontColor, setupFontSize, setupBackgroundColorBar1, setupFontColorBar1, setupFontSizeBar1, setupBackgroundColorBar9, setupFontColorBar9, setupFontSizeBar9, drawPerfectSignalArrow, perfectSignalBuyColor, perfectSignalSellColor, cancelHaHC_LbLC, cancelHaHH_LbLL, cancelCaHC_CbLC, cancelCaHH_CbLL, cancelCaHTH_CbLTL, tDSTMaxNumber, tDSTResistanceColor, tDSTSupportColor, width, buyQ1OR, sellQ1OR, sequentialBars, sequentialLookBackBars, sequentialBar8VersusBar5Qualifier, sequentialBar13VersusBar8Qualifier, sequentialRecycleBars, sequentialFontColor, sequentialFontSize, sequentialBackgroundColorBar1, sequentialFontColorBar1, sequentialFontSizeBar1, sequentialBackgroundColorBar13, sequentialFontColorBar13, sequentialFontSizeBar13, sequentialBackgroundColorBarRecycle, sequentialFontColorBarRecycle, sequentialFontSizeBarRecycle, sequentialCancelReverseSetup, sequentialCancelTDST, comboBars, comboLookBackBars, comboRecycleBars, comboFontColor, comboFontSize, comboBackgroundColorBar1, comboFontColorBar1, comboFontSizeBar1, comboBackgroundColorBar13, comboFontColorBar13, comboFontSizeBar13, comboBackgroundColorBarRecycle, comboFontColorBarRecycle, comboFontSizeBarRecycle);
		}
	}
}

#endregion
