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
	public class dDrawABC : Indicator
	{
		#region Variables
		
		private bool _fDragging = false;

		private double _min, _max;

		//make these global and set on plot call, so we don't have to pass around.
		private int rightScrnBnum;	//right most bar# on screen.
		private int leftScrnBnum;	//left most bar# in display
		
		private int _iBarStart,_iBarEnd;
		private double _priceStart, _priceEnd;

		private SimpleFont 			textFont;
		private SimpleFont 			textFontLabel;
		private int 			textOffset		= 15;
		private int 			textOffset2		= 30;
		private int 			textOffset3		= 45;
		private Brush			swingColor		= Brushes.Black;

		private int swingDir	= 1;
		private int rightMaxBarNum = 0;

		private NinjaTrader.NinjaScript.DrawingTools.Line lineAB = null;
		private NinjaTrader.NinjaScript.DrawingTools.Line lineBC = null;
		
		private int _ABbarStart,_ABbarEnd;
		private double _ABpriceStart, _ABpriceEnd;
		private int _BCbarStart,_BCbarEnd;
		private double _BCpriceStart, _BCpriceEnd;
		private bool _fDrawAB = false;
		private bool _fDrawBC = false;

		private double[] FibPct;
		private double[] FibVal;
		
		private ChartScale myChartScale;
		
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "dDrawABC";
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
				
				AdjustableFib								= 1.786;
				FibABRets									= 3;
				FibBCRets									= 1;
				FibABExtends								= 5;
				FibBCExtends								= 4;
				FibMMReverse								= 5;
				FibMMRets									= 4;
				FibMMExtends								= 5;
				FibABTextToRight							= false;
				FibBCTextToRight							= true;
				FibMMTextToRight							= false;
				
				
				APDate										= DateTime.Parse("1:46 PM");
				APVal										= 1;
				BPDate										= DateTime.Parse("1:47 PM");
				BPVal										= 1;
				CPDate										= DateTime.Parse("1:47 PM");
				CPVal										= 1;
				
				
				SwingColorUp								= Brushes.Green;
				SwingColorDn								= Brushes.Red;
				FibABColor									= Brushes.Gray;
				FibBCColor									= Brushes.RoyalBlue;
				
				FibPct = new double[14] ;
				FibVal = new double[14] ;
				
				FibPct[0] = AdjustableFib;	//!0 = show this level
			
				FibPct[1] = 2.0 ;
				FibPct[2] =  1.618 ;
				FibPct[3] = 1.272 ;	//continuation extentions

				FibPct[4] = 1.0 ;	// 100% of move
				//these are backwards for proper display, cause of later math.
				FibPct[5] = 0.764 ;	//not quite accurate, but close enough
				FibPct[6] = 0.618 ;
				FibPct[7] =  0.5 ;
				FibPct[8] = 0.382 ;
				FibPct[9] = 0.236 ;
				FibPct[10] = 0.0 ;	//reference point, low for up, hi for down.
				
				FibPct[11] = -0.272 ;	//retractions
				FibPct[12] = -0.618 ;
				FibPct[13] = -1.0 ;
				
				textFont 		= new SimpleFont("Courier", 9);
				textFontLabel	= new SimpleFont("Courier", 12);
			}
			else if (State == State.DataLoaded)
			{				
				if (ChartControl != null)
				{
					ChartPanel.MouseDown += new System.Windows.Input.MouseButtonEventHandler (ChartControl_MouseDown);
					ChartPanel.MouseMove += new System.Windows.Input.MouseEventHandler (ChartControl_MouseMove);
				}
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartPanel.MouseDown -= new System.Windows.Input.MouseButtonEventHandler (ChartControl_MouseDown);
					ChartPanel.MouseMove -= new System.Windows.Input.MouseEventHandler (ChartControl_MouseMove);
				}
				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
			if(lineAB == null)
				RestoreDrawObjects();
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			myChartScale = chartScale;
			
			_min = ChartPanel.MinValue;
			_max = ChartPanel.MaxValue;

			rightScrnBnum = Math.Min(ChartBars.ToIndex, CurrentBar); //limit to actual bars vs. white space on right.
			leftScrnBnum = Math.Min(ChartBars.FromIndex, CurrentBar); //not bigger than CurrentBar (ie. only 1 bar on screen).
			
			if(lineAB != null)
			{
			_ABbarStart = lineAB.StartAnchor.BarsAgo;
			_ABbarEnd = lineAB.EndAnchor.BarsAgo;
			_ABpriceStart = APVal;
			_ABpriceEnd = BPVal;
			}
			if(lineBC != null)
			{
			_BCbarStart = _ABbarEnd;
			_BCbarEnd = lineBC.EndAnchor.BarsAgo;
			_BCpriceStart = BPVal;
			_BCpriceEnd = CPVal;
			}
		}
		
		int BarFromX (int x)
		{
			int thisBar = CurrentBar - (int)ChartBars.GetBarIdxByX(ChartControl,x);
			//Limit to existing right bar #
			if 		(thisBar < rightMaxBarNum) thisBar = 0;
			return  thisBar;
		}
		
		void ChartControl_MouseDown (object sender, System.Windows.Input.MouseEventArgs e)
		{
			if(myChartScale == null)
				return;
			if(!System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt)
				&& !System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt))
				return;
			
			if(_fDragging == false) {
				//start a new one
				RemoveDrawObjects();	//delete all object from this application
				_iBarStart = BarFromX ((int)e.GetPosition(ChartPanel).X);
				_priceStart = Low.GetValueAt(CurrentBar - BarFromX ((int)e.GetPosition(ChartPanel).X));
				
				_fDrawAB = true;
				_fDrawBC = false;
				_fDragging = true;
				
				_ABbarStart = _iBarStart;
				
				//place 'A' text
				double midBarAdd = (High.GetValueAt(CurrentBar - _iBarStart) - Low.GetValueAt(CurrentBar - _iBarStart)) *0.5;
				swingDir = _priceStart <= myChartScale.GetValueByY((int)e.GetPosition(ChartPanel).Y) + midBarAdd ? -1 : 1; //Low.GetValueAt(CurrentBar - _iBarStart) + midBarAdd ? -1 : 1;
				swingColor = swingDir > 0 ? SwingColorUp : SwingColorDn;
				
				_ABpriceStart = _priceStart;
				
				TriggerCustomEvent(o =>
				{
					Draw.Text(this, "dDwALabel", IsAutoScale, "A",
						_iBarStart, _priceStart, textOffset2 * swingDir *-1, swingColor, textFontLabel,
						TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					Draw.Text(this, "dDwAPct", IsAutoScale, _priceStart.ToString(),
						_iBarStart, _priceStart, textOffset * swingDir *-1, swingColor, textFont,
						TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				}, null);
				
			}
			else if(_fDrawAB == true) {
				//finish AB, start BC
				//save points for first time restore
				APDate = lineAB.StartAnchor.Time;
				APVal = lineAB.StartAnchor.Price;
				BPDate = lineAB.EndAnchor.Time;
				BPVal = lineAB.EndAnchor.Price;
				
				_fDrawAB = false;
				_fDrawBC = true;
				
				_ABbarEnd = _iBarEnd;	//current position from mousemove
				_ABpriceEnd = _priceEnd;
				_BCbarStart = _ABbarEnd;	//current position
				_BCpriceStart = _ABpriceEnd;
				//reset line start to draw BC
				_iBarStart = _BCbarStart;
				_priceStart = _BCpriceStart;
				
				//Place 'B' text on bar H or low
				double moveVal = RoundInst(Math.Abs(_ABpriceStart -_ABpriceEnd));
				
				Draw.Text(this, "dDwBLabel", IsAutoScale, "B",
					_iBarStart, _priceStart, textOffset2 * swingDir, swingColor, textFontLabel,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				Draw.Text(this, "dDwBPrice", IsAutoScale, moveVal.ToString() +"c - " + _priceStart.ToString(),
					_iBarStart, _priceStart, textOffset * swingDir, swingColor, textFont,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			}

			else if(_fDrawBC){
				CPDate = lineBC.EndAnchor.Time;
				CPVal = lineBC.EndAnchor.Price;
				//finish BC draw
				_fDrawBC = false;
				_fDragging = false;
				_BCbarEnd = _iBarEnd;	//current position from mousemove
				_BCpriceEnd = _priceEnd;
				
				DoDrawFibs(1, FibABRets, FibABExtends, FibABColor);
				DoDrawFibs(2, FibBCRets, FibBCExtends, FibBCColor);
				DoDrawFibs(3, FibMMRets, FibMMExtends, swingColor);

				Draw.Text(this, "dDwCLabel", IsAutoScale, "C",
					_iBarEnd, _priceEnd, textOffset3 * swingDir *-1, swingColor, textFontLabel,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				//move value and %
				double moveVal = RoundInst(Math.Abs(_BCpriceStart -_BCpriceEnd));
				double movePct = (moveVal / Math.Abs(_ABpriceStart -_ABpriceEnd)) * 100;
				
				Draw.Text(this, "dDwCPct", IsAutoScale,  movePct.ToString("f1") + "%",
					_iBarEnd, _priceEnd, textOffset2 * swingDir *-1, swingColor, textFont,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				Draw.Text(this, "dDwCPrice", IsAutoScale, moveVal.ToString() +"c - " + _priceEnd.ToString(),
					_iBarEnd, _priceEnd, textOffset * swingDir *-1, swingColor, textFont,
					TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
				
			}
			else {
				_fDragging = false; //error, turn off sequence
			}
		
			
			ChartControl.InvalidateVisual();
		}

		void ChartControl_MouseMove (object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!_fDragging)
				return;

			_iBarEnd = BarFromX ((int)e.GetPosition(ChartPanel).X);
			
			if(!High.IsValidDataPoint(_iBarEnd) || !Low.IsValidDataPoint(_iBarEnd))
				return;
			
			if(_fDrawAB) {
				_priceEnd = swingDir > 0 ? High[_iBarEnd] : Low[_iBarEnd];
				lineAB = UDrawLine("dDwABLine");
			}
			else {
				_priceEnd = swingDir < 0 ? High[_iBarEnd] : Low[_iBarEnd];
				lineBC = UDrawLine("dDwBCLine");
			}
			
			ChartControl.InvalidateVisual();
		}
		
		//round to nearist tick size
		private double RoundInst(double tdbl)
		{
			
			if(Instrument != null) {	//check incase not called from OnBarUpdate().?
				return Instrument.MasterInstrument.RoundToTickSize(tdbl);
			}
			
			return tdbl;
		}
		
		private NinjaTrader.NinjaScript.DrawingTools.Line UDrawLine(string labelStr) 
		{
			NinjaTrader.NinjaScript.DrawingTools.Line Lrtn = null;
			TriggerCustomEvent (
				delegate (object state) {
					try
					{
					Lrtn = Draw.Line(this, 
						labelStr,false,
						_iBarStart, _priceStart,
						_iBarEnd, _priceEnd,
						swingColor, DashStyleHelper.Solid, 2);
					
					}
					catch { }
				},
				null
			);
			return Lrtn;
		}


		private void DoDrawFibs(int fType, int rLevel, int eLevel, Brush tColor)
		{
			double tSwgStartVal, tSwgEndVal;
			int tSwgStartBar, tSwgEndBar, lineEndBar;
			string fBaseStr = "dDwFib";
			int swingBarsHalf;
			int textpositionbar;
			TextAlignment textAlign;
			
			//setup per fType...
			if(fType == 3) {
				if(rLevel == 0 && eLevel == 0 && FibMMReverse == 0) return;	//nothing to do here.
				fBaseStr = fBaseStr + "MM";
				tSwgEndVal = _ABpriceEnd;
				tSwgStartVal = _ABpriceStart;
				tSwgEndBar = _BCbarEnd;
				tSwgStartBar = _ABbarStart;
				swingBarsHalf = (tSwgStartBar - tSwgEndBar) - 3; //so we start @ C point
				lineEndBar = _BCbarEnd - (int) Math.Abs((tSwgStartBar - tSwgEndBar) *1);
				textpositionbar = FibMMTextToRight ? lineEndBar : tSwgStartBar - swingBarsHalf;
				textAlign = FibMMTextToRight ? TextAlignment.Right : TextAlignment.Left;
			}
			else if(fType == 2) {
				if(rLevel == 0 && eLevel == 0) return;	//nothing to do here.
				fBaseStr = fBaseStr + "BC";
				tSwgEndVal = _BCpriceEnd;
				tSwgStartVal = _BCpriceStart;
				tSwgEndBar = _BCbarEnd;
				tSwgStartBar = _BCbarStart;
				swingBarsHalf = (int)((tSwgStartBar - tSwgEndBar) *0.5);
				lineEndBar = _BCbarEnd - (int) Math.Abs((tSwgStartBar - tSwgEndBar) *0.5);
				textpositionbar = FibBCTextToRight ? lineEndBar : tSwgStartBar - swingBarsHalf;
				textAlign = FibBCTextToRight ? TextAlignment.Right : TextAlignment.Left;
			}
			else { //if(fType == 1) {
				if(rLevel == 0 && eLevel == 0) return;	//nothing to do here.
				fBaseStr = fBaseStr + "AB";
				tSwgEndVal = _ABpriceEnd;
				tSwgStartVal = _ABpriceStart;
				tSwgEndBar = _ABbarEnd;
				tSwgStartBar = _ABbarStart;
				swingBarsHalf = (int)((tSwgStartBar - tSwgEndBar) *0.5);
				lineEndBar = _BCbarEnd;
				textpositionbar = FibABTextToRight ? lineEndBar : tSwgStartBar - swingBarsHalf;
				textAlign = FibABTextToRight ? TextAlignment.Right : TextAlignment.Left;
			}
			int swDirUp = (tSwgEndVal > tSwgStartVal ? 1 : -1);	//not correct for outside bar.

			//calc begin and end of lines
			double swgRange = Math.Abs(tSwgStartVal - tSwgEndVal);

			string fibString = null;
			for(int y = 0; y <= 13; y++) {
				//skip those not needed
				if(y == 0 && FibPct[0] == 0) continue;	//skip adjustable one
				
				if(rLevel < 5) {
					if(rLevel == 0) if(y > 3 && y < 11) continue;	//skip all retracts
					if(rLevel == 4 && y == 7) continue;	//skip 50%
					if(rLevel <= 3) if(y == 5 || y == 9) continue;	//skip .76 & .23
					if(rLevel == 2 && y == 7) continue;	//only show .38 & .81
					if(rLevel == 1) if(y == 6 || y == 8) continue;	//only show .5
				}
				if(eLevel < 5) { //do both sides
					if(eLevel == 0) if(y < 4 || y > 10) continue;	//skip all extends
					if(eLevel == 4) if(y == 3 || y == 11) continue;	//skip 1.27 both sides
					if(eLevel == 3) if(y == 3 || y == 11 || y == 12 || y == 2) continue;	//show  200 both sides
					if(eLevel == 2) if(y == 3 || y == 11 || y == 13 || y == 1) continue;	//show  161 both sides
					if(eLevel == 1) if(y == 2 || y == 12 || y == 13 || y == 1) continue;	//show  127 both sides
				}				
				if(fType == 3) {
					if(FibMMReverse < 5) { //skip somethings
						if(FibMMReverse == 0) if(y < 4 ) continue;	//skip all extends
						if(FibMMReverse == 4) if(y == 3) continue;	//skip -.27
						if(FibMMReverse == 3) if(y == 3 || y == 2) continue;	//show -100
						if(FibMMReverse == 2) if(y == 3 || y == 1) continue;	//show  -61
						if(FibMMReverse == 1) if(y == 2 || y == 1) continue;	//show  -27
					}				
						
					int x = 14 - y;	//reverse direction for this
					if(x == 14) x = 0;	//no 14, adjustable is at zero.
					FibVal[y] = _BCpriceEnd + ((swgRange * FibPct[x]) * swDirUp);
					fibString = ( (FibPct[x] * 100)).ToString() + "%m";
				}
				else {
					if(y == 0) {	//adjustable %
						double fadjust =  (swgRange * FibPct[y]);
						if(FibPct[y] < 0) fadjust = swgRange+(fadjust);
						else if(FibPct[y] < 1) fadjust = swgRange-(fadjust);
						FibVal[y] = tSwgStartVal + (fadjust * swDirUp);

						if(FibPct[y] < 0) fibString = ((FibPct[y] * -100)).ToString() + "%r";
						else if(FibPct[y] < 1) fibString = ((FibPct[y] * 100)).ToString() + "%r";
						else fibString = ((FibPct[y] * 100)).ToString() + "%e";//adjust extention labels
					}
					else {
					FibVal[y] = tSwgStartVal + ((swgRange * FibPct[y]) * swDirUp);
					if(y <= 3) fibString = ((FibPct[y] * 100)).ToString() + "%e";//adjust extention labels
					else fibString = (100 - (FibPct[y] * 100)).ToString() + "%r";
					}
				}
				
				Draw.Line(this, fBaseStr + fibString, false, tSwgStartBar - swingBarsHalf, 
					FibVal[y], lineEndBar, FibVal[y], tColor, DashStyleHelper.Dash, 1);
				Draw.Text(this, fBaseStr +"text" + fibString, false,  fibString,
					textpositionbar, FibVal[y], 2 , tColor, textFont,
					textAlign, Brushes.Transparent, Brushes.Transparent, 5);
			}
		}
			
		// Draw objects after any reload of data (if possible)
		// currently wont restore if one of points is -1 (unfinished bar, when calc per bar close)
		private void RestoreDrawObjects()
		{
			
			//check all dates for bars to validate they exist.
			//if(APDate == DateTime.MinValue) return;	//works, but could still be older than oldest bar.
			//make sure All our saved dates are available on chart (null save = 01/01/0001).
			DateTime oldestDate = Time[CurrentBar];
			if(oldestDate > APDate || oldestDate > BPDate || oldestDate > CPDate) return;
			if(Time[0] < APDate || Time[0] < BPDate || Time[0] < CPDate) return;
			
			swingDir = APVal >= BPVal ? -1 : 1;
			swingColor = swingDir > 0 ? SwingColorUp : SwingColorDn;
			
			//Draw AB line and text, using date info
			lineAB = Draw.Line(this, "dDwABLine",false, APDate, APVal  ,BPDate, BPVal, swingColor, DashStyleHelper.Solid, 2);
			lineBC = Draw.Line(this, "dDwBCLine",false, BPDate, BPVal  ,CPDate, CPVal, swingColor, DashStyleHelper.Solid, 2);
			
			//ChartControl.InvalidateVisual();
			ForceRefresh();

			//Place 'A' text
			Draw.Text(this, "dDwALabel", IsAutoScale, "A",
				APDate, APVal, textOffset2 * swingDir *-1, swingColor, textFontLabel,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			Draw.Text(this, "dDwAPct", IsAutoScale, APVal.ToString(),
				APDate, APVal, textOffset * swingDir *-1, swingColor, textFont,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			
			//Place 'B' text
			double moveVal = RoundInst(Math.Abs(APVal -BPVal));
			
			Draw.Text(this, "dDwBLabel", IsAutoScale, "B",
				BPDate, BPVal, textOffset2 * swingDir, swingColor, textFontLabel,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			Draw.Text(this, "dDwBPrice", IsAutoScale, moveVal.ToString() +"c - " + BPVal.ToString(),
				BPDate, BPVal, textOffset * swingDir, swingColor, textFont,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);

			//Draw 'C' text
			moveVal = RoundInst(Math.Abs(BPVal -CPVal));
			double movePct = (moveVal / Math.Abs(APVal -BPVal)) * 100;

			Draw.Text(this, "dDwCLabel", IsAutoScale, "C",
				CPDate, CPVal, textOffset3 * swingDir *-1, swingColor, textFontLabel,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			//move point value and %
			Draw.Text(this, "dDwCPct", IsAutoScale,  movePct.ToString("f1") + "%",
				CPDate, CPVal, textOffset2 * swingDir *-1, swingColor, textFont,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			Draw.Text(this, "dDwCPrice", IsAutoScale, moveVal.ToString() +"c - " + CPVal.ToString(),
				CPDate, CPVal, textOffset * swingDir *-1, swingColor, textFont,
				TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
			
			//setup and call fib draws	
			_ABbarStart = CurrentBar - ChartBars.GetBarIdxByTime(ChartControl, APDate);
			_ABbarEnd =  CurrentBar - ChartBars.GetBarIdxByTime(ChartControl, BPDate);
			_ABpriceStart = APVal;
			_ABpriceEnd = BPVal;
			_BCbarStart = _ABbarEnd;
			_BCbarEnd = CurrentBar - ChartBars.GetBarIdxByTime(ChartControl, CPDate);
			_BCpriceStart = BPVal;
			_BCpriceEnd = CPVal;
			
			DoDrawFibs(1, FibABRets, FibABExtends, FibABColor);
			DoDrawFibs(2, FibBCRets, FibBCExtends, FibBCColor);
			DoDrawFibs(3, FibMMRets, FibMMExtends, swingColor);			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="AdjustableFib", Description="Adjustable Fib Level (+= Extend, -= Retract, -0.786r, 1.764e, 0=no). Effects All ABC points.", Order=0, GroupName="DisplayFibs")]
		public double AdjustableFib
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibABRets", Description="Show AB Retract Levels (0=No, 1= 50%, 2= 38+61, 3=1+2, 4=2+78, 5=All.", Order=0, GroupName="DisplayFibs")]
		public int FibABRets
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibBCRets", Description="Show BC Retract Levels (0=No, 1= 50%, 2= 38+61, 3=1+2, 4=2+78, 5=All.", Order=0, GroupName="DisplayFibs")]
		public int FibBCRets
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibABExtends", Description="Show Extended Levels (0=No, 1= 127, 2= 161, 3=200, 4=161+200, 5=All 3.", Order=0, GroupName="DisplayFibs")]
		public int FibABExtends
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibBCExtends", Description="Show Extended Levels (0=No, 1= 127, 2= 161, 3=200, 4=161+200, 5=All 3.", Order=0, GroupName="DisplayFibs")]
		public int FibBCExtends
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibMMReverse", Description="Show MM Wrong Direction Levels (0=No, 1= -27, 2= -61, 3=-100, 4=61+100, 5=All 3.", Order=0, GroupName="DisplayFibs")]
		public int FibMMReverse
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibMMRets", Description="Show MM Retract Levels (0=No, 1= 50%, 2= 38+61, 3=1+2, 4=2+78, 5=All.", Order=0, GroupName="DisplayFibs")]
		public int FibMMRets
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="FibMMExtends", Description="Show MM Extended Levels (0=No, 1= 127, 2= 161, 3=200, 4=161+200, 5=All", Order=0, GroupName="DisplayFibs")]
		public int FibMMExtends
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FibABTextToRight", Description="Show AB Text Labels Right Justified", Order=0, GroupName="DisplayFibs")]
		public bool FibABTextToRight
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FibBCTextToRight", Description="Show BC Text Labels Right Justified", Order=0, GroupName="DisplayFibs")]
		public bool FibBCTextToRight
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="FibMMTextToRight", Description="Show MM Text Labels Right Justified", Order=0, GroupName="DisplayFibs")]
		public bool FibMMTextToRight
		{ get; set; }
		
		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="APDate", Order=0, GroupName="saveme")]
		public DateTime APDate
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="APVal", Order=0, GroupName="saveme")]
		public double APVal
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="BPDate", Order=0, GroupName="saveme")]
		public DateTime BPDate
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="BPVal", Order=0, GroupName="saveme")]
		public double BPVal
		{ get; set; }

		[NinjaScriptProperty]
		[PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
		[Display(Name="CPDate", Order=0, GroupName="saveme")]
		public DateTime CPDate
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="CPVal", Order=0, GroupName="saveme")]
		public double CPVal
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SwingColorUp", Description="Swing Color Up (also MM Fibs)", Order=0, GroupName="Display Settings")]
		public Brush SwingColorUp
		{ get; set; }

		[Browsable(false)]
		public string SwingColorUpSerializable
		{
			get { return Serialize.BrushToString(SwingColorUp); }
			set { SwingColorUp = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="SwingColorDn", Description="Swing Color Down (also MM Fibs)", Order=0, GroupName="Display Settings")]
		public Brush SwingColorDn
		{ get; set; }

		[Browsable(false)]
		public string SwingColorDnSerializable
		{
			get { return Serialize.BrushToString(SwingColorDn); }
			set { SwingColorDn = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="FibABColor", Description="AB Fib Color", Order=0, GroupName="Display Settings")]
		public Brush FibABColor
		{ get; set; }

		[Browsable(false)]
		public string FibABColorSerializable
		{
			get { return Serialize.BrushToString(FibABColor); }
			set { FibABColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="FibBCColor", Description="BC Fib Color", Order=0, GroupName="Display Settings")]
		public Brush FibBCColor
		{ get; set; }

		[Browsable(false)]
		public string FibBCColorSerializable
		{
			get { return Serialize.BrushToString(FibBCColor); }
			set { FibBCColor = Serialize.StringToBrush(value); }
		}	
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private dDrawABC[] cachedDrawABC;
		public dDrawABC dDrawABC(double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			return dDrawABC(Input, adjustableFib, fibABRets, fibBCRets, fibABExtends, fibBCExtends, fibMMReverse, fibMMRets, fibMMExtends, fibABTextToRight, fibBCTextToRight, fibMMTextToRight, aPDate, aPVal, bPDate, bPVal, cPDate, cPVal, swingColorUp, swingColorDn, fibABColor, fibBCColor);
		}

		public dDrawABC dDrawABC(ISeries<double> input, double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			if (cachedDrawABC != null)
				for (int idx = 0; idx < cachedDrawABC.Length; idx++)
					if (cachedDrawABC[idx] != null && cachedDrawABC[idx].AdjustableFib == adjustableFib && cachedDrawABC[idx].FibABRets == fibABRets && cachedDrawABC[idx].FibBCRets == fibBCRets && cachedDrawABC[idx].FibABExtends == fibABExtends && cachedDrawABC[idx].FibBCExtends == fibBCExtends && cachedDrawABC[idx].FibMMReverse == fibMMReverse && cachedDrawABC[idx].FibMMRets == fibMMRets && cachedDrawABC[idx].FibMMExtends == fibMMExtends && cachedDrawABC[idx].FibABTextToRight == fibABTextToRight && cachedDrawABC[idx].FibBCTextToRight == fibBCTextToRight && cachedDrawABC[idx].FibMMTextToRight == fibMMTextToRight && cachedDrawABC[idx].APDate == aPDate && cachedDrawABC[idx].APVal == aPVal && cachedDrawABC[idx].BPDate == bPDate && cachedDrawABC[idx].BPVal == bPVal && cachedDrawABC[idx].CPDate == cPDate && cachedDrawABC[idx].CPVal == cPVal && cachedDrawABC[idx].SwingColorUp == swingColorUp && cachedDrawABC[idx].SwingColorDn == swingColorDn && cachedDrawABC[idx].FibABColor == fibABColor && cachedDrawABC[idx].FibBCColor == fibBCColor && cachedDrawABC[idx].EqualsInput(input))
						return cachedDrawABC[idx];
			return CacheIndicator<dDrawABC>(new dDrawABC(){ AdjustableFib = adjustableFib, FibABRets = fibABRets, FibBCRets = fibBCRets, FibABExtends = fibABExtends, FibBCExtends = fibBCExtends, FibMMReverse = fibMMReverse, FibMMRets = fibMMRets, FibMMExtends = fibMMExtends, FibABTextToRight = fibABTextToRight, FibBCTextToRight = fibBCTextToRight, FibMMTextToRight = fibMMTextToRight, APDate = aPDate, APVal = aPVal, BPDate = bPDate, BPVal = bPVal, CPDate = cPDate, CPVal = cPVal, SwingColorUp = swingColorUp, SwingColorDn = swingColorDn, FibABColor = fibABColor, FibBCColor = fibBCColor }, input, ref cachedDrawABC);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.dDrawABC dDrawABC(double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			return indicator.dDrawABC(Input, adjustableFib, fibABRets, fibBCRets, fibABExtends, fibBCExtends, fibMMReverse, fibMMRets, fibMMExtends, fibABTextToRight, fibBCTextToRight, fibMMTextToRight, aPDate, aPVal, bPDate, bPVal, cPDate, cPVal, swingColorUp, swingColorDn, fibABColor, fibBCColor);
		}

		public Indicators.dDrawABC dDrawABC(ISeries<double> input , double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			return indicator.dDrawABC(input, adjustableFib, fibABRets, fibBCRets, fibABExtends, fibBCExtends, fibMMReverse, fibMMRets, fibMMExtends, fibABTextToRight, fibBCTextToRight, fibMMTextToRight, aPDate, aPVal, bPDate, bPVal, cPDate, cPVal, swingColorUp, swingColorDn, fibABColor, fibBCColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.dDrawABC dDrawABC(double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			return indicator.dDrawABC(Input, adjustableFib, fibABRets, fibBCRets, fibABExtends, fibBCExtends, fibMMReverse, fibMMRets, fibMMExtends, fibABTextToRight, fibBCTextToRight, fibMMTextToRight, aPDate, aPVal, bPDate, bPVal, cPDate, cPVal, swingColorUp, swingColorDn, fibABColor, fibBCColor);
		}

		public Indicators.dDrawABC dDrawABC(ISeries<double> input , double adjustableFib, int fibABRets, int fibBCRets, int fibABExtends, int fibBCExtends, int fibMMReverse, int fibMMRets, int fibMMExtends, bool fibABTextToRight, bool fibBCTextToRight, bool fibMMTextToRight, DateTime aPDate, double aPVal, DateTime bPDate, double bPVal, DateTime cPDate, double cPVal, Brush swingColorUp, Brush swingColorDn, Brush fibABColor, Brush fibBCColor)
		{
			return indicator.dDrawABC(input, adjustableFib, fibABRets, fibBCRets, fibABExtends, fibBCExtends, fibMMReverse, fibMMRets, fibMMExtends, fibABTextToRight, fibBCTextToRight, fibMMTextToRight, aPDate, aPVal, bPDate, bPVal, cPDate, cPVal, swingColorUp, swingColorDn, fibABColor, fibBCColor);
		}
	}
}

#endregion
