#region Using declarations
using System;
using System.IO;
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
	public class CCIForecasterV7DE : Indicator
	{
		#region Variables
		//Pattern Series
		private Series<int>	wCCIPatternSeries;
		private Series<int>	wCCIDirectionSeries;
		private Series<int>	wCCICloseAtSeries;
		private Series<int>	wCCIBarEndSeries;
		private Series<bool>	sidewaysMktSeries;

		private int 	LastDate;

		//ZLR variable
		private double sumchopzone3L = 0;
		private double sumchopzone3S = 0;
		private double sumchopzone5L = 0;
		private double sumchopzone5S = 0;
		private double sumchopzone6L = 0;
		private double sumchopzone6S = 0;
		private double sumchopzone7L = 0;
		private double sumchopzone7S = 0;
		private double sumchopzone8L = 0;
		private double sumchopzone8S = 0;
		private bool ZLRL_H = false;
		private bool ZLRL_L = false;
		private bool ZLRL_E = false;
		private bool ZLRS_H = false;
		private bool ZLRS_L = false;
		private bool ZLRS_E = false;
		
		//famir variable
	    private int 	Direction=1; int Trend=1; 
		private bool FamirL_H = false;
		private bool FamirL_L = false;
		private bool FamirL_E = false;
		private bool FamirS_H = false;
		private bool FamirS_L = false;
		private bool FamirS_E = false;
		//Vegas variables
		private double	swingHigh		= 0;
		private double	swingLow		= 0;
		private double	vswingHigh		= 0;
		private double	vswingLow		= 0;
		private	bool VegasL_H = false;
		private	bool VegasL_L = false;
		private	bool VegasL_E = false;
		private	bool VegasS_H = false;
		private	bool VegasS_L = false;
		private	bool VegasS_E = false;
		//GB100 variables
		private	bool GBL_H = false;
		private	bool GBL_L = false;
		private	bool GBL_E = false;
		private	bool GBS_H = false;
		private	bool GBS_L = false;
		private	bool GBS_E = false;
		// Tony variables
		protected int	tonyLong		= 0;
		protected int	tonyShort		= 0;
		private	bool TonyL_H = false;
		private	bool TonyL_L = false;
		private	bool TonyL_E = false;
		private	bool TonyS_H = false;
		private	bool TonyS_L = false;
		private	bool TonyS_E = false;
		//Ghost variables
		protected int	gstLong			= 0;
		protected int	gstShort		= 0;
		private	bool GhostL_H = false;
		private	bool GhostL_L = false;
		private	bool GhostL_E = false;
		private	bool GhostS_H = false;
		private	bool GhostS_L = false;
		private	bool GhostS_E = false;
		private	double gstLeftPk = 0;
		private	double gstMiddlePk = 0;
		private	double gstRightPk = 0;
		private	double gstLeftLow = 0;
		private	double gstRightLow = 0;
		private	int gstLeftPkBars = 0;
		private	int gstMiddlePkBars = 0;
		private	int gstRightPkBars = 0;
		private	int gstLeftLowBars = 0;
		private	int gstRightLowBars = 0;
		private	int lbPeriodup;
		private	int lbPerioddown;
		private	double gstChangePerBar;
		private	bool gstTrendBrkShortH;
		private	bool gstTrendBrkLongH;
		private	bool gstTrendBrkShortL;
		private	bool gstTrendBrkLongL;
		//Slingshot variables

		private	bool SSL_H = false;
		private	bool SSL_L = false;
		private	bool SSL_E = false;
		private	bool SSS_H = false;
		private	bool SSS_L = false;
		private	bool SSS_E = false;
		
		//CCI Forecaster Variables
		private bool	cCIShowForecasterHistory 	= false;
		private int n =  0;
		private double typUp = 0;//Last Bar Typical Price at high close
		private double typDown = 0;//Last Bar Typical Price at low close
		private double meanUp = 0;
		private double meanDown = 0;
		private double nMeanUp = 0;
		private double nMeanDown = 0;
		private double CCIup= 0;
		private double CCIdown=0;
		private double newCCIup= 0;
		private double newCCIdown=0;
		private double nCCIup= 0;
		private double nCCIdown=0;
		private double newTypUp = 0;
		private double newTypDown =0;
		private double oldTypUp = 0;
		private double oldTypDown =0;
		//Turbo Forecaster Variables
		private double Turboup= 0;
		private double Turbodown=0;

		//Display options
		private SimpleFont	textFontLabels		= new SimpleFont("Tahoma", 7);//Version labels
		
		private Brush	CZIdownColor;
		private Brush	CZIupColor;
		private int		cziHighsw;
		private int		cziLowsw;
		private Brush	lSMAForecasterColorL = Brushes.Red;
		
		//Sideways
		private int 	sidewaysbar;//Bar when sideways market began
		private int 	resetbar = 1;//Bar when sideways market ended
		private int 	ChopXings;
		private bool	sideways;
        private double 	EMAAngleFactor;
		
		// SW Variables
		private double rad2deg = 180/Math.PI;
		private double MedianEMA = 0;
		private double MedianLSMA = 0;
		private double LSMAAngleH = 0;
		private double LSMAAngleL = 0;
		private double EMASlopeH = 0;
		private double EMASlopeL = 0;
		private double EMAAngleH = 0;
		private double EMAAngleL = 0;
		private double SWValueH = 0; 
		private double SWValueL = 0; 
		private double SWAbsH = 0; 
		private double SWAbsL = 0; 

		//Bars Ago
		private int BarsAgo_0Xup 		= 0;// 0 line cross up
		private int BarsAgo_0Xdown 		= 0;// 0 line cross down
		private int BarsAgo_5Xup 		= 0;// 5 line cross up
		private int BarsAgo_5Xdown 		= 0;// -5 line cross down
		private int BarsAgo_100Xup 		= 0;// 100 line cross up
		private int BarsAgo_100Xdown 	= 0;// 100 line cross down
		private int BarsAgo_n100Xup 	= 0;// -100 line cross down
		private int BarsAgo_n100Xdown 	= 0;// -100 line cross up
		private int BarsAgoExtremeXup 	= 0;// Extreme cross up
		private int BarsAgoExtremeXdown = 0;// Extreme cross down
		private int BarsAgoSidewaysreset = 0;// Extreme cross down
		private int BarsAgoSidewaysresetup 	= 0;// Extreme cross down
		private int barsAgoSwingH 		= 0;
		private int barsAgoSwingL 		= 0;
		private int trendcountL 		= 0;//How long since chop zone 2 non aqua bars
		private int trendcountS 		= 0;//How long since chop zone 2 non red bars
		private int pcbar 				= 0;//Where is the close within the current bar

		private double multiplier = .05714285714;
		private double emaprev = 0;
		private double EMAup = 0;
		private double EMAdown = 0;
		private double projectedHigh 	= 0;
		private double projectedLow 	= 0;
		private double projectedHigh1 	= 0;
		private double projectedLow1 	= 0;
		private bool   pcbarreset		= true;
		//Momo Change Variables
		private int BarsAgoLongPattern	= 0;
		private int BarsAgoShortPattern	= 0;
		private bool LongMomoChange = false;
		private bool ShortMomoChange = false;
		
		//Data Extraction
		private			StreamWriter 	sw;
		private string 	Pattern 		= "";
		private string	TDirection		= "";
		private int		closePosition 	= 0;
		private bool	signal			= false;
		private int		signalbar		= 0;
		private double 	BEStop			= 0;
		private int		stopTicks		= 16;
		private bool	stoppedout		= false;
		private bool	breakeven		= false;
		private int		bETicks			= 10;
		private bool	return_be		= false;
		private bool	breakeven2		= false;
		private bool	return_be_2		= false;
		private bool	momochange		= false;
		private double	momoexit		=0;
		private bool	cross_100		= false;
		private bool 	hold 			= false;
		private double 	entrypoint;
		private DateTime 	BETime;
		private DateTime 	BETime2;
		
		private DateTime SSTime;//Session Start Time


		//Javier's  variables
		private double[,] peak;
		private int BarsAgoPeakH;
		private int BarsAgoPeakL;
		private bool HH = false;
		private bool LL = false;
		private double peakvalH;
		private double peakvalL;
		private double peakvalTotal;
		private double peakval5pc;
		private double swresetH;
		private double swresetL;


		WoodiesCCI myWoodies;
		
		private string AlertFile;
		
		private int HistoricalBars;
		private int HistoricalProcessingStart;
		private Series<double> NoDrawUpper;
		private Series<double> NoDrawLower;
		#endregion
		
		#region SharpDX brushes for Warning message
		private Dictionary<string, DXMediaMap> dxmBrushes;
        private SharpDX.Direct2D1.RenderTarget myRenderTarget = null;
        private Brush MsgBrush
        {
            get { return dxmBrushes["MsgBrush"].MediaBrush; }
            set { UpdateBrush(value, "MsgBrush"); }
        }
		private Brush MsgAreaBrush
        {
            get { return dxmBrushes["MsgAreaBrush"].MediaBrush; }
            set { UpdateBrush(value, "MsgAreaBrush"); }
        }
		#endregion
		
		public override string DisplayName
		{
			get { return Name; }
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"CCI Patterns Forecaster with Data Extraction 1.0";
				Name										= "CCIForecasterV7DE";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				BarsRequiredToPlot 							= 0;
				//MaximumBarsLookback							= MaximumBarsLookBack.Infinite;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				
				// General Parameters
				SideWinder0									= 60;
				RejectLevel									= 100;
				TrendLength									= 12;
				Periods										= 14;
				UsePointsAmount								= 10;
				UsePoints									= "Points";
				Extremes									= 200;
				FamirHookMin								= 5;
				FamirLimits									= 50;
				VTHFEPenetrationLevel						= 185;
				VMaxSwingPoint								= 185;
				VMaxSwingBars								= 10;
				GstColor									= 0.5;
				MinZLRPts									= 15;
				MinCCIPts									= 0;
				MaxSignalValue								= 120;
				MinZLRValue									= 1;
				ChopCrosses									= 4;
				UseJavierSideways							= false;
				Sidewaysboundary							= 100;
				SSZeroBand									= 33;
				SS100Band									= 33;
				SSCCITurboSeparation						= 75;
				MaxSSTurboValue								= 185;
				
				// Forecasters
				CCIShowForecasterArrows						= false;
				CCIShowForecasterDots						= false;
				CCIShowForecasterHistory					= false;
				CCIForecasterColorH							= Brushes.DarkGreen;
				CCIForecasterColorL							= Brushes.Red;
				LSMAPeriods									= 25;
				LSMALineThick								= 2;
				LSMAForecasterColorH						= Brushes.DarkGreen;
				LSMAForecasterColorL						= Brushes.Red;
				ShowLSMA_Fore								= true;
				CZILineThick								= 2;
				ShowCZI_Fore								= true;
				ShowSWForecaster							= false;
				UseSWForecaster								= false;
				
				// Sound and Display
				AlertInterval								= 60;
				AlertFileName								= "Alert4.wav";
				SoundAlert									= false;
				SoundMomoAlert								= false;
				ShowFamirLimits								= false;
				ShowOneTwenty								= false;
				ShowGhostPeaks								= true;
				ShowGhostTrendLine							= true;
				TrendThickness								= 1;
				ShowMomoChange								= false;
				LineExtension								= 6;
				ShowChopCrosses								= true;
				SidewaysColor								= Brushes.Yellow;
				SidewaysTransparency						= 10;
				FamirLimitsColor							= Brushes.White;
				OneTwentyColor								= Brushes.White;
				GstTrendColor								= Brushes.Yellow;
				VegasTrendColor								= Brushes.PaleGreen;
				TextAnchorL									= 250;
				TextAnchorS									= 195;
				SignalLabels								= new SimpleFont("Tahoma", 10);
				
				// Patterns To Display
				SigBackColor								= Brushes.White;
				SigBackOpacity								= 100;
				Show_ZLR									= true;
				ZLRLineThick								= 4;
				ZLRColorL									= Brushes.DarkGreen;
				ZLRColorS									= Brushes.Red;
				ZLRLineColorL								= Brushes.PaleGreen;
				ZLRLineColorS								= Brushes.Pink;
				Show_Famir									= true;
				FamirColorL									= Brushes.DarkGreen;
				FamirColorS									= Brushes.Red;
				Show_Vegas									= true;
				VegasColorL									= Brushes.DarkGreen;
				VegasColorS									= Brushes.Red;
				Show_GB100									= true;
				GBLineThick									= 2;
				GBColorL									= Brushes.DarkGreen;
				GBColorS									= Brushes.Red;
				Show_Tony									= true;
				TonyColorL									= Brushes.DarkGreen;
				TonyColorS									= Brushes.Red;
				Show_Ghost									= true;
				GhostColorL									= Brushes.DarkGreen;
				GhostColorS									= Brushes.Red;
				Show_SS										= true;
				SSColorL									= Brushes.DarkGreen;
				SSColorS									= Brushes.Red;
				Show_Opp_End								= true;
				
				// Data Extraction
				UseDataExtraction							= false;
				ClosingTime									= "3:55 PM";
				MAE_Ticks									= 16;
				Target										= 0;
				Opp_End_MAE_Ticks							= 10;
				BE_Ticks									= 10;
				BE_Offset_Ticks								= -5;
				BE_Ticks_2									= 13;
				BE_Offset_Ticks_2							= 1;
				CloseorOpen									= "Open"; // Change to Enum?
				StopReverse									= false;
				FilePath									= @NinjaTrader.Core.Globals.UserDataDir.ToString() + "Signals.csv";
				
				// Historical Processing Options
				HistoricalBarsToProcess						= 500;
				ShowHistoricalWarning						= true;
				ShowHistoricalWarningRegion					= true;
				ShowHistoricalStartLine						= true;
				
				// Plots
				AddPlot(new Stroke(Brushes.Chartreuse, 2), PlotStyle.Dot, "PlotCCIHigh");
				AddPlot(new Stroke(Brushes.Crimson, 2), PlotStyle.Dot, "PlotCCILow");
				AddPlot(new Stroke(Brushes.Silver, 2), PlotStyle.TriangleUp, "PlotTurboHigh");
				AddPlot(new Stroke(Brushes.Gray, 2), PlotStyle.TriangleDown, "PlotTurboLow");
			}
			else if (State == State.Configure)
			{
				AlertFile = NinjaTrader.Core.Globals.InstallDir+@"sounds\" + AlertFileName;
			}
			else if (State == State.DataLoaded)
			{
				wCCIPatternSeries	= new Series<int>(this);
				wCCIDirectionSeries	= new Series<int>(this);
				wCCICloseAtSeries	= new Series<int>(this);
				wCCIBarEndSeries	= new Series<int>(this);
				sidewaysMktSeries	= new Series<bool>(this);
				
				peak = new double[6,2];
				
				HistoricalBars = BarsArray[0].Count;
				HistoricalProcessingStart = HistoricalBars - HistoricalBarsToProcess;
				NoDrawUpper = new Series<double>(this, MaximumBarsLookBack.Infinite);
				NoDrawLower = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "MsgBrush", "MsgAreaBrush" } )
            	    dxmBrushes.Add(brushName, new DXMediaMap());
				
				MsgBrush 			= Brushes.White;
				MsgAreaBrush 	= Brushes.Black;
				
			}
			else if (State == State.Historical)
			{
				SetZOrder(int.MaxValue);
			}
			else if (State == State.Terminated)
			{
				// Disposes resources used by the StreamWriter
				if(sw != null)
				{
					sw.Dispose();
					sw = null;
				}
			}
		}

		protected override void OnBarUpdate()
        {
			NoDrawUpper[0] = 500;
			NoDrawLower[0] = -500;
			
			if(CurrentBar < HistoricalProcessingStart)
			{
				if(ShowHistoricalWarningRegion)
					Draw.Region(this, "BlankRegion", CurrentBar, 0, NoDrawUpper, NoDrawLower, Brushes.Red, Brushes.OrangeRed, 10, 0);
				return;
			}
			else if (CurrentBar == HistoricalProcessingStart && ShowHistoricalStartLine)
				Draw.Line(this, "LowerBound", false, 0, -500, 0, 500, Brushes.Red, DashStyleHelper.Dot, 1); 
			
			if (CurrentBar < Math.Max(34,Math.Max(Periods,LSMAPeriods))) return;
//			if (CurrentBar == 34) Draw.Text(this,"CB44", "34", 0, 265, Brushes.Red);
			myWoodies = WoodiesCCI(2, 5, Periods, 34, 25, 6, SideWinder0, 100, 2);
			if(Bars.IsFirstBarOfSession && IsFirstTickOfBar) 
			{
				SSTime = Time[0];
				BETime = SSTime;
				BETime2 = SSTime;
			}
			projectedHigh 	= Low[0]+BarsPeriod.Value*TickSize;
			projectedLow 	= High[0]-BarsPeriod.Value*TickSize;
			
			pcbar = ((GetCurrentBid() <= projectedLow) ? -1 : ((GetCurrentAsk()>=projectedHigh) ? 1 : 0));//Is the bid/ask outside bar range?
			if(pcbar == 0)
				pcbar = ((Close[0]-Low[0])/(High[0]-Low[0])<.1 ? -1 :((Close[0]-Low[0])/(High[0]-Low[0])>.9 ? 1 : 0));//Where is the close within the current bar
				
			if(projectedHigh != projectedHigh1 || projectedLow != projectedLow1)
			{	projectedHigh1 = projectedHigh;
				projectedLow1 = projectedLow;
				OnNewHighLow();
				pcbarreset = true;
			}
			else
			{
				if(pcbar != 0)
				{	OnNewHighLow();
					pcbarreset = true;//reset to true if we get close to a high/low
				}
				else if(pcbarreset)//recalculate patterns if we got close to a high/low and retreated
				{	OnNewHighLow();
					pcbarreset = false;
				}
			}
			
			DrawPatterns();
			PlotCCILow[0] = (CCIdown);
			PlotCCIHigh[0] = (CCIup);
			if (!CCIShowForecasterDots)
			{
			PlotCCILow.Reset(1);
			PlotCCIHigh.Reset(1);
			}
			if(UseDataExtraction) DataExtraction();
		}
		
		protected void OnNewHighLow()
		{
			BarsAgo_0Xup 	= MRO(delegate{return (myWoodies[0] > 0 && (myWoodies[Math.Min(CurrentBar, 1)]==0 ? myWoodies[Math.Min(CurrentBar, 2)]<0 : myWoodies[Math.Min(CurrentBar, 1)]<0));},1,CurrentBar-3) +1;// 0 line cross up
			BarsAgo_0Xdown 	= MRO(delegate{return (myWoodies[0] < 0 && (myWoodies[Math.Min(CurrentBar, 1)]==0 ? myWoodies[Math.Min(CurrentBar, 2)]>0 : myWoodies[Math.Min(CurrentBar, 1)]>0));},1,CurrentBar-3) +1;// 0 line cross down
			BarsAgo_5Xup 	= MRO(delegate{return (myWoodies[0] > 5 && (myWoodies[Math.Min(CurrentBar, 1)]==5 ? myWoodies[Math.Min(CurrentBar, 2)]<5 : myWoodies[Math.Min(CurrentBar, 1)]<5));},1,CurrentBar-3) +1;// 0 line cross up
			BarsAgo_5Xdown 	= MRO(delegate{return (myWoodies[0] < -5 && (myWoodies[Math.Min(CurrentBar, 1)]==-5 ? myWoodies[Math.Min(CurrentBar, 2)]>-5 : myWoodies[Math.Min(CurrentBar, 1)]>-5));},1,CurrentBar-3) +1;// 0 line cross down
			BarsAgo_100Xup 	= MRO(delegate{return (myWoodies[0] > 100 && (myWoodies[Math.Min(CurrentBar, 1)]==100 ? myWoodies[Math.Min(CurrentBar, 2)]<100 : myWoodies[Math.Min(CurrentBar, 1)]<100));},1,CurrentBar-3) +1;// 100 line cross up
			BarsAgo_100Xdown = MRO(delegate{return (myWoodies[0] < 100 && (myWoodies[Math.Min(CurrentBar, 1)]==100 ? myWoodies[Math.Min(CurrentBar, 2)]>100 : myWoodies[Math.Min(CurrentBar, 1)]>100));},1,CurrentBar-3) +1;// 100 line cross down
			BarsAgo_n100Xup	= MRO(delegate{return (myWoodies[0] > -100 && (myWoodies[Math.Min(CurrentBar, 1)]==-100 ? myWoodies[Math.Min(CurrentBar, 2)]<-100 : myWoodies[Math.Min(CurrentBar, 1)]<-100));},1,CurrentBar-3) +1;// -100 line cross down
			BarsAgo_n100Xdown= MRO(delegate{return (myWoodies[0] < -100 && (myWoodies[Math.Min(CurrentBar, 1)]==-100 ? myWoodies[Math.Min(CurrentBar, 2)]>-100 : myWoodies[Math.Min(CurrentBar, 1)]>-100));},1,CurrentBar-3) +1;// -100 line cross up
			BarsAgoExtremeXup = MRO(delegate{return (myWoodies[0]>-Extremes && (myWoodies[Math.Min(CurrentBar, 1)]==-Extremes ? myWoodies[Math.Min(CurrentBar, 2)]<-Extremes : myWoodies[Math.Min(CurrentBar, 1)]<-Extremes));},1,CurrentBar-3) +1;// Extreme cross up
			BarsAgoExtremeXdown = MRO(delegate{return (myWoodies[0]<Extremes && (myWoodies[Math.Min(CurrentBar, 1)]==Extremes ? myWoodies[Math.Min(CurrentBar, 2)]>Extremes : myWoodies[Math.Min(CurrentBar, 1)]>Extremes));},1,CurrentBar-3) +1;// Extreme cross down
			wCCIPatternSeries[0] = (0);
			wCCIDirectionSeries[0] = (0);
			wCCIBarEndSeries[0] = (0);
			
			//if(BarsAgoExtremeXup == 0)
			//	BarsAgoExtremeXup = 1;
			
			#region DETERMINATION OF TREND
				if(myWoodies[0]>0)
					{Direction = 1;}//On which side of 0 is the CCI
				if(myWoodies[0]<0)
					{Direction = -1;}
				if(IsFirstTickOfBar)
				{
					if(((BarsAgo_0Xup >= 7 && myWoodies[0]<0) || (BarsAgo_0Xup >= 6 && myWoodies[0]>0)) && BarsAgo_100Xup <= BarsAgo_0Xup && myWoodies[Math.Min(CurrentBar,1)]>0) Trend = 1;
					if(((BarsAgo_0Xdown >= 7 && myWoodies[0]>0) || (BarsAgo_0Xdown >= 6 && myWoodies[0]<0)) && BarsAgo_n100Xdown <= BarsAgo_0Xdown && myWoodies[Math.Min(CurrentBar,1)]<0) Trend = -1;
				}
			#endregion
			//Initialize Data Series
			wCCICloseAtSeries[0] = (0);		
			if(Close[0] == Low[0]) wCCICloseAtSeries[0] = (-1);
			else if(Close[0] == High[0]) wCCICloseAtSeries[0] = (1);
			
			emaprev = EMA(34)[Math.Min(CurrentBar,1)];
			EMAup = (projectedHigh*multiplier)+(emaprev * (1-multiplier));
			EMAdown = (projectedLow*multiplier)+(emaprev*(1-multiplier));
			LastDate = ToDay(DateTime.Now);

			#region CCI Forecaster
					n =  Periods - 1;
					typUp = (Low[0] + (2*projectedHigh))/3;//Last Bar Typical Price at high close
					typDown = (High[0] + (2*projectedLow))/3;//Last Bar Typical Price at low close
					///Sum of Typical Prices
					for (int i = (Periods - 2); i >= 1; i--)
					{
						typUp += Typical[i];
						typDown += Typical[i];
							
					}
					newTypUp = typUp + (Low[0] + (2*projectedHigh))/3;
					newTypDown =typDown + (High[0] + (2*projectedLow))/3;
					oldTypUp = typUp + Typical[n] ;
					oldTypDown = typDown + Typical[n];
					meanUp =  Math.Abs(Typical[n] - oldTypUp/Periods);
					meanDown = Math.Abs(Typical[n] - oldTypDown/Periods);
					meanUp += Math.Abs((Low[0] + (2*projectedHigh))/3 - oldTypUp/Periods);
					meanDown += Math.Abs((High[0] + (2*projectedLow))/3 - oldTypDown/Periods);
					nMeanUp = Math.Abs(projectedHigh - newTypUp/Periods);
					nMeanDown = Math.Abs(projectedLow - newTypDown/Periods);
					nMeanUp += Math.Abs((Low[0] + (2*projectedHigh))/3 - newTypUp/Periods);
					nMeanDown += Math.Abs((High[0] + (2*projectedLow))/3 - newTypDown/Periods);
					for (int idx = ( Periods - 2); idx >= 1; idx--)
					{
						meanUp += Math.Abs(Typical[idx] - oldTypUp/Periods);
						meanDown += Math.Abs(Typical[idx] - oldTypDown/Periods);
						nMeanUp += Math.Abs(Typical[idx] - newTypUp/Periods);
						nMeanDown += Math.Abs(Typical[idx] - newTypDown/Periods);
					}
				
					CCIup =((Low[0] + (2*projectedHigh))/3 - oldTypUp/Periods) / (meanUp == 0 ? 1 : (0.015 * (meanUp / Periods)));
					CCIdown =((High[0] + (2*projectedLow))/3 - oldTypDown/Periods) / (meanDown == 0 ? 1 : (0.015 * (meanDown / Periods)));

/*					if(CCIShowForecasterArrows == true)
					{
					Draw.ArrowUp(this,"LowCCI",true, 0, CCIdown, CCIForecasterColorL);
					Draw.ArrowDown(this,"HighCCI",true, 0, CCIup, CCIForecasterColorH);
					}
					else if (CCIShowForecasterDots == true)
					{
					Draw.Dot(this,"LowCCI",false,0,CCIdown,CCIForecasterColorL);
					Draw.Dot(this,"HighCCI",false,0,CCIup,CCIForecasterColorH);
					}
*/			// Plot CCI Forecaster
//			Plots[0].Pen.Width = 2;
//			Plots[Math.Min(CurrentBar,1)].Pen.Width = 2;
			PlotCCILow[0] = (CCIdown);
			PlotCCIHigh[0] = (CCIup);
			#endregion
					
			#region Turbo Forecaster
					n =  5;
					typUp = (Low[0] + (2*projectedHigh))/3;//Last Bar Typical Price at high close
					typDown = (High[0] + (2*projectedLow))/3;//Last Bar Typical Price at low close
					///Sum of Typical Prices
					for (int i = (4); i >= 1; i--)
					{
						typUp += Typical[i];
						typDown += Typical[i];
							
					}
					newTypUp = typUp + (Low[0] + (2*projectedHigh))/3;
					newTypDown =typDown + (High[0] + (2*projectedLow))/3;
					oldTypUp = typUp + Typical[n] ;
					oldTypDown = typDown + Typical[n];
					meanUp =  Math.Abs(Typical[n] - oldTypUp/6);
					meanDown = Math.Abs(Typical[n] - oldTypDown/6);
					meanUp += Math.Abs((Low[0] + (2*projectedHigh))/3 - oldTypUp/6);
					meanDown += Math.Abs((High[0] + (2*projectedLow))/3 - oldTypDown/6);
					nMeanUp = Math.Abs(projectedHigh - newTypUp/6);
					nMeanDown = Math.Abs(projectedLow - newTypDown/6);
					nMeanUp += Math.Abs((Low[0] + (2*projectedHigh))/3 - newTypUp/6);
					nMeanDown += Math.Abs((High[0] + (2*projectedLow))/3 - newTypDown/6);
					for (int idx = ( 6 - 2); idx >= 1; idx--)
					{
						meanUp += Math.Abs(Typical[idx] - oldTypUp/6);
						meanDown += Math.Abs(Typical[idx] - oldTypDown/6);
						nMeanUp += Math.Abs(Typical[idx] - newTypUp/6);
						nMeanDown += Math.Abs(Typical[idx] - newTypDown/6);
					}
				
					Turboup =((Low[0] + (2*projectedHigh))/3 - oldTypUp/6) / (meanUp == 0 ? 1 : (0.015 * (meanUp / 6)));
					Turbodown =((High[0] + (2*projectedLow))/3 - oldTypDown/6) / (meanDown == 0 ? 1 : (0.015 * (meanDown / 6)));

			PlotTurboLow[0] = (Turbodown);
			PlotTurboHigh[0] = (Turboup);
			PlotTurboLow.Reset(1);
			PlotTurboHigh.Reset(1);
			#endregion

			if(ToDay(Time[0]) == LastDate) //Don't do this unless necessary
			{
			#region LSMA Forecaster
				if(ShowLSMA_Fore == true)
				{
					if(projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0])
						Draw.Line(this,/*CurrentBar.ToString()+*/"LSMAHighClose",true,1,9,0,9,LSMAForecasterColorH,DashStyleHelper.Solid,LSMALineThick);
					else
						Draw.Line(this,/*CurrentBar.ToString()+*/"LSMAHighClose",true,1,9,0,9,LSMAForecasterColorL,DashStyleHelper.Solid,LSMALineThick);
					
					if(projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0])
						Draw.Line(this,/*CurrentBar.ToString()+*/"LSMALowClose",true,1,-7,0,-7,LSMAForecasterColorH,DashStyleHelper.Solid,LSMALineThick);
					else
						Draw.Line(this,/*CurrentBar.ToString()+*/"LSMALowClose",true,1,-7,0,-7,LSMAForecasterColorL,DashStyleHelper.Solid,LSMALineThick);
				}
			#endregion

			#region CZI Forecaster
			if(ShowCZI_Fore == true)
			{
				double cziHigh = Math.Round((projectedHigh-EMAup)/TickSize,1);
				double cziLow = Math.Round((projectedLow-EMAdown)/TickSize,1);
				if(cziHigh>=4)//.25)
					cziHighsw=4;
					else if(cziHigh>=3)//.15)
					cziHighsw=3;
						else if(cziHigh>=2)//.1)
						cziHighsw=2;
							else if(cziHigh>=1)//.1)
							cziHighsw=1;
								else if(cziHigh<=-4)//.25)
								cziHighsw=-4;
									else if(cziHigh<=-3)//.15)
									cziHighsw=-3;
										else if(cziHigh<=-2)//.1)
										cziHighsw=-2;
											else if(cziHigh<=-1)//.1)
											cziHighsw=-1;
												else cziHighsw=0;
					if(cziLow>=4)//.25)
						cziLowsw=4;
						else if(cziLow>=3)//.15)
						cziLowsw=3;
							else if(cziLow>=2)//.1)
							cziLowsw=2;
								else if(cziLow>=1)//.1)
								cziLowsw=1;
									else if(cziLow<=-4)//.25)
									cziLowsw=-4;
										else if(cziLow<=-3)//.15)
										cziLowsw=-3;
											else if(cziLow<=-2)//.1)
											cziLowsw=-2;
												else if(cziLow<=-1)//.1)
												cziLowsw=-1;
													else cziLowsw=0;
					
					switch (cziHighsw)
					{
						case 4:
							CZIupColor = Brushes.Cyan;
							break;
						case 3:
							CZIupColor = Brushes.DarkGreen;
							break;
						case 2:
							CZIupColor = Brushes.DarkSeaGreen;
							break;
						case 1:
							CZIupColor = Brushes.Lime;
							break;
						case 0:
							CZIupColor = Brushes.Yellow;
							break;
						case -1:
							CZIupColor = Brushes.Gold;
							break;
						case -2:
							CZIupColor = Brushes.Orange;
							break;
						case -3:
							CZIupColor = Brushes.OrangeRed;
							break;
						case -4:
							CZIupColor = Brushes.DarkRed;
							break;
						default:
							break;
					}
					switch (cziLowsw)
					{
						case 4:
							CZIdownColor = Brushes.Cyan;
							break;
						case 3:
							CZIdownColor = Brushes.DarkGreen;
							break;
						case 2:
							CZIdownColor = Brushes.DarkSeaGreen;
							break;
						case 1:
							CZIdownColor = Brushes.Lime;
							break;
						case 0:
							CZIdownColor = Brushes.Yellow;
							break;
						case -1:
							CZIdownColor = Brushes.Gold;
							break;
						case -2:
							CZIdownColor = Brushes.Orange;
							break;
						case -3:
							CZIdownColor = Brushes.OrangeRed;
							break;
						case -4:
							CZIdownColor = Brushes.DarkRed;
							break;
						default:
							break;
					}
	
				}
//	Draw.Text(this,CurrentBar.ToString()+"CZI",(Close[0]==High[0]?cziHighsw.ToString():cziLowsw.ToString())+"\n"+myWoodies.ChopZone,0,50,Brushes.Red);
			#endregion

			#region SW Forecaster
				SWAbsH = 0; 
				SWAbsL = 0; 

			if(ShowSWForecaster || UseSWForecaster)
			{
				MedianEMA = (EMA(34)[Math.Min(CurrentBar,1)]+EMA(34)[Math.Min(CurrentBar,2)])/2;
				MedianLSMA = (LinReg(LSMAPeriods)[Math.Min(CurrentBar,1)]+LinReg(LSMAPeriods)[Math.Min(CurrentBar,2)])/2;
				LSMAAngleH = (Math.Atan((LinRegPredictor(LSMAPeriods).LRHighClose[0]-(LinReg(25)[Math.Min(CurrentBar,1)]+LinReg(25)[Math.Min(CurrentBar,2)])/2)/1.5/TickSize))*rad2deg;
				LSMAAngleL = (Math.Atan((LinRegPredictor(LSMAPeriods).LRLowClose[0]-(LinReg(25)[Math.Min(CurrentBar,1)]+LinReg(25)[Math.Min(CurrentBar,2)])/2)/1.5/TickSize))*rad2deg;
				EMASlopeH = (EMAup-MedianEMA)/1.5/TickSize;
				EMASlopeL = (EMAdown-MedianEMA)/1.5/TickSize;
				EMAAngleH = (Math.Atan((EMAup-(EMA(34)[Math.Min(CurrentBar,1)]+EMA(34)[Math.Min(CurrentBar,2)])/2)/1.5/TickSize))*rad2deg;
				EMAAngleL = (Math.Atan((EMAdown-(EMA(34)[Math.Min(CurrentBar,1)]+EMA(34)[Math.Min(CurrentBar,2)])/2)/1.5/TickSize))*rad2deg;
				SWValueH = LSMAAngleH + EMAAngleH; 
				SWValueL = LSMAAngleL + EMAAngleL; 
				SWAbsH = Math.Abs(SWValueH); 
				SWAbsL = Math.Abs(SWValueL); 

				RemoveDrawObject((CurrentBar-1).ToString()+"FSW");
			}
			#endregion
			
			#region Draw CZI and SW Forecasts
			if(myWoodies[0]<-75)
					{
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIup",true,1,109,0,109,CZIupColor,DashStyleHelper.Solid,CZILineThick);
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIdown",true,1,96,0,96,CZIdownColor,DashStyleHelper.Solid,CZILineThick);
						if(ShowSWForecaster == true)
						{
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWup",true,1,209,0,209,SWAbsH>=100?Brushes.LimeGreen:(SWAbsH>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWdown",true,1,196,0,196,SWAbsL>=100?Brushes.LimeGreen:(SWAbsL>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
						}
					RemoveDrawObject("CZIup2");
					RemoveDrawObject("CZIdown2");
					RemoveDrawObject("SWup2");
					RemoveDrawObject("SWdown2");
					
					}
					else if(myWoodies[0]>75)
					{
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIup2",true,1,-91,0,-91,CZIupColor,DashStyleHelper.Solid,CZILineThick);
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIdown2",true,1,-104,0,-104,CZIdownColor,DashStyleHelper.Solid,CZILineThick);
					if(ShowSWForecaster == true)
					{
						Draw.Line(this,/*CurrentBar.ToString()+*/"SWup2",true,1,-191,0,-191,SWAbsH>=100?Brushes.LimeGreen:(SWAbsH>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
						Draw.Line(this,/*CurrentBar.ToString()+*/"SWdown2",true,1,-204,0,-204,SWAbsL>=100?Brushes.LimeGreen:(SWAbsL>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
					}
					RemoveDrawObject("CZIup");
					RemoveDrawObject("CZIdown");
					RemoveDrawObject("SWup");
					RemoveDrawObject("SWdown");
					}
					else
					{
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIup2",true,1,-91,0,-91,CZIupColor,DashStyleHelper.Solid,CZILineThick);
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIdown2",true,1,-104,0,-104,CZIdownColor,DashStyleHelper.Solid,CZILineThick);
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIup",true,1,109,0,109,CZIupColor,DashStyleHelper.Solid,CZILineThick);
					Draw.Line(this,/*CurrentBar.ToString()+*/"CZIdown",true,1,96,0,96,CZIdownColor,DashStyleHelper.Solid,CZILineThick);
						if(ShowSWForecaster == true)
						{
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWup",true,1,209,0,209,SWAbsH>=100?Brushes.LimeGreen:(SWAbsH>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWdown",true,1,196,0,196,SWAbsL>=100?Brushes.LimeGreen:(SWAbsL>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWup2",true,1,-191,0,-191,SWAbsH>=100?Brushes.LimeGreen:(SWAbsH>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
							Draw.Line(this,/*CurrentBar.ToString()+*/"SWdown2",true,1,-204,0,-204,SWAbsL>=100?Brushes.LimeGreen:(SWAbsL>=30?Brushes.Yellow:Brushes.Red),DashStyleHelper.Solid,CZILineThick);
						}

					}
//	Draw.Line(this,CurrentBar.ToString()+"SW",1,208,0,208,Close[0]==High[0]?(SWAbsH>=100?Brushes.LimeGreen:(SWAbsH>=30?Brushes.Yellow:Brushes.DarkRed)):(SWAbsL>=100?Brushes.LimeGreen:(SWAbsL>=30?Brushes.Yellow:Brushes.DarkRed)));//,DashStyleHelper.Solid,CZILineThick);
//	Draw.Line(this,CurrentBar.ToString()+"CZ",1,108,0,108,Close[0]==High[0]?CZIupColor:CZIdownColor,DashStyleHelper.Solid,CZILineThick);
			#endregion
			}					
			#region Sideways market
			if(IsFirstTickOfBar && !UseJavierSideways)
			{
			BarsAgoSidewaysreset 		= MRO(delegate{return (Math.Abs(myWoodies[Math.Min(CurrentBar, 1)])>=Extremes);},1,CurrentBar-3) +1;// Extreme cross up
			int swBarsAgo_100Xup 		= MRO(delegate{return (myWoodies[Math.Min(CurrentBar, 1)] > Sidewaysboundary && (myWoodies[Math.Min(CurrentBar, 2)]==Sidewaysboundary ? myWoodies[Math.Min(CurrentBar, 3)]<Sidewaysboundary : myWoodies[Math.Min(CurrentBar, 2)]<Sidewaysboundary));},1,CurrentBar-3) +1;// 100 line cross up
			int swBarsAgo_n100Xdown		= MRO(delegate{return (myWoodies[Math.Min(CurrentBar, 1)] < -Sidewaysboundary && (myWoodies[Math.Min(CurrentBar, 2)]==-Sidewaysboundary ? myWoodies[Math.Min(CurrentBar, 3)]>-Sidewaysboundary : myWoodies[Math.Min(CurrentBar, 2)]>-Sidewaysboundary));},1,CurrentBar-3) +1;// 100 line cross down
				if(ChopXings<ChopCrosses)
				{
					ChopXings = CountIf(delegate{return((myWoodies[Math.Min(CurrentBar,1)]>0 && myWoodies[Math.Min(CurrentBar,2)]<0) || (myWoodies[Math.Min(CurrentBar,1)]<0 && myWoodies[Math.Min(CurrentBar,2)]>0));},Math.Min(Math.Max((swBarsAgo_n100Xdown-1),1), Math.Max((swBarsAgo_100Xup-1),1))-1);
//					-(Math.Abs(myWoodies[(Math.Min(Math.Max((BarsAgo_n100Xdown-1),1), Math.Max((BarsAgo_100Xup-1),1)))])+Math.Abs(myWoodies[(Math.Min(Math.Max((BarsAgo_n100Xdown-1),1), Math.Max((BarsAgo_100Xup-1),1)))+1])>Math.Abs(myWoodies[(Math.Min(Math.Max((BarsAgo_n100Xdown-1),1), Math.Max((BarsAgo_100Xup-1),1)))]) ? 1 : 0));
					sidewaysbar = CurrentBar-1;//Set bar # of sideways market start
				}
				
				sideways = ChopXings>=ChopCrosses;// && Math.Abs(myWoodies[Math.Min(CurrentBar,1)])<Extremes;
				if((BarsAgoSidewaysreset<=(CurrentBar-sidewaysbar)&& BarsAgoSidewaysreset!=0))// || (BarsAgoSidewaysresetup<=(CurrentBar-sidewaysbar) && BarsAgoSidewaysresetup!=0))
				{
					sideways = false;
					ChopXings = 0;
					resetbar = CurrentBar-1;//Set bar # of sideways market end
				}
				

				if(sideways)
					sidewaysMktSeries[0] = (true);
				else
					sidewaysMktSeries[0] = (false);
				
				if(ShowChopCrosses && sideways) 
				{
					Draw.Rectangle(this,sidewaysbar.ToString()+"sideways",true,CurrentBar-sidewaysbar,Extremes,0,-Extremes,SidewaysColor,SidewaysColor,SidewaysTransparency);
				}
			}
			if(ChopXings==ChopCrosses) 
				Draw.TextFixed(this,"chop","SIDEWAYS MARKET",TextPosition.TopRight);
			else
				RemoveDrawObject("chop");

			#endregion
			
			#region Javier's sideways reset
			if(IsFirstTickOfBar && UseJavierSideways)
			{
				if(CurrentBar>50 && !sideways)
				{
					HH = false;
					LL = false;
					for(int i=0;i<6;i++)
					{
						BarsAgoPeakH = MRO(delegate {return(myWoodies[0]<myWoodies[Math.Min(CurrentBar,1)] && myWoodies[Math.Min(CurrentBar,1)]>myWoodies[Math.Min(CurrentBar,2)]);},(i+1),CurrentBar)+1;
						BarsAgoPeakL = MRO(delegate {return(myWoodies[0]>myWoodies[Math.Min(CurrentBar,1)] && myWoodies[Math.Min(CurrentBar,1)]<myWoodies[Math.Min(CurrentBar,2)]);},(i+1),CurrentBar)+1;
						peak[i,0] = (BarsAgoPeakH > 0 ? myWoodies[BarsAgoPeakH] : 0);
						peak[i,1] = (BarsAgoPeakL > 0 ? myWoodies[BarsAgoPeakL] : 0);
					}
					for(int k=0;k<5;k++)
					{
						if((peak[k,0]>peak[(k+1),0] ||  peak[k,0] >= Extremes) && !HH)
						{
							HH=true;
							if(peak[k,0] >= Extremes && k > 0)
								peakvalH = peak[(k-1),0];
							else
								peakvalH = peak[k,0];
						}
						if((peak[k,1]<peak[(k+1),1] ||  peak[k,0] <= -Extremes) && !LL)
						{
							LL=true;
							if(peak[k,1] <= -Extremes  & k > 0)
								peakvalL = peak[(k-1),1];
							else
								peakvalL = peak[k,1];
						}
						if(HH && LL)
						break;
					}
					peakval5pc = (peakvalH + Math.Abs(peakvalL))*.05;
					swresetH = peakvalH+peakval5pc;
					swresetL = peakvalL-peakval5pc;
				}
					int BarsAgoSidewaysresetdown = MRO(delegate{return ((myWoodies[Math.Min(CurrentBar,1)]<swresetL));},1,CurrentBar-3) +1;// Extreme cross down
					int BarsAgoSidewaysresetup   = MRO(delegate{return ((myWoodies[Math.Min(CurrentBar,1)]>swresetH ));},1,CurrentBar-3) +1;// Extreme cross up
				
				int swBarsAgo_100Xup 		= MRO(delegate{return (myWoodies[Math.Min(CurrentBar, 1)] > Sidewaysboundary && (myWoodies[Math.Min(CurrentBar, 2)]==Sidewaysboundary ? myWoodies[Math.Min(CurrentBar, 3)]<Sidewaysboundary : myWoodies[Math.Min(CurrentBar, 2)]<Sidewaysboundary));},1,CurrentBar-3) +1;// 100 line cross up
				int swBarsAgo_n100Xdown		= MRO(delegate{return (myWoodies[Math.Min(CurrentBar, 1)] < -Sidewaysboundary && (myWoodies[Math.Min(CurrentBar, 2)]==-Sidewaysboundary ? myWoodies[Math.Min(CurrentBar, 3)]>-Sidewaysboundary : myWoodies[Math.Min(CurrentBar, 2)]>-Sidewaysboundary));},1,CurrentBar-3) +1;// 100 line cross down
				if(ChopXings<ChopCrosses)
				{
					ChopXings = CountIf(delegate{return((myWoodies[Math.Min(CurrentBar,1)]>0 && myWoodies[Math.Min(CurrentBar,2)]<0) || (myWoodies[Math.Min(CurrentBar,1)]<0 && myWoodies[Math.Min(CurrentBar,2)]>0));},Math.Min(Math.Min(Math.Max((swBarsAgo_n100Xdown-1),1), Math.Max((swBarsAgo_100Xup-1),1)),CurrentBar-resetbar)-1);
					sidewaysbar = CurrentBar-1;//Set bar # of sideways market start
				}
				
				if(sideways &&((BarsAgoSidewaysresetdown<=(CurrentBar-sidewaysbar)&& BarsAgoSidewaysresetdown!=0) || (BarsAgoSidewaysresetup<=(CurrentBar-sidewaysbar) && BarsAgoSidewaysresetup!=0)))
				{
					ChopXings = 0;
					resetbar = CurrentBar-1;//Set bar # of sideways market end
				}
				
				sideways = ChopXings==ChopCrosses;

				if(ShowChopCrosses && sideways) 
				{
				Draw.Rectangle(this,sidewaysbar.ToString()+"sideways",true,CurrentBar-sidewaysbar,swresetH,0,swresetL,SidewaysColor,SidewaysColor,SidewaysTransparency);
//					Draw.Rectangle(this,sidewaysbar.ToString()+"sideways",true,CurrentBar-sidewaysbar,SidewaysReset,0,-SidewaysReset,SidewaysColor,SidewaysColor,SidewaysTransparency);
				}
			
			if(ChopXings==ChopCrosses) 
				Draw.TextFixed(this,"chop","SIDEWAYS MARKET",TextPosition.TopRight);
			else
				RemoveDrawObject("chop");
			}
			#endregion
			
			if((ShowChopCrosses == true && sideways == true) || sideways == false)//Don't show patterns during choppy sideways markey
			{
			#region Reset Patterns
				ZLRL_H = false;
				ZLRL_L = false;
				ZLRL_E = false;
				ZLRS_H = false;
				ZLRS_L = false;
				ZLRS_E = false;
				FamirL_H = false;
				FamirL_L = false;
				FamirL_E = false;
				FamirS_H = false;
				FamirS_L = false;
				FamirS_E = false;
				VegasL_H = false;
				VegasL_L = false;
				VegasL_E = false;
				VegasS_H = false;
				VegasS_L = false;
				VegasS_E = false;
				GBL_H	= false;
				GBL_L 	= false;
				GBL_E	= false;
				GBS_H	= false;
				GBS_L 	= false;
				GBS_E	= false;
				TonyL_H = false;
				TonyL_L = false;
				TonyL_E = false;
				TonyS_H = false;
				TonyS_L = false;
				TonyS_E = false;
				GhostL_H = false;
				GhostL_L = false;
				GhostL_E = false;
				GhostS_H = false;
				GhostS_L = false;
				GhostS_E = false;
				SSL_H	= false;
				SSL_L 	= false;
				SSL_E	= false;
				SSS_H	= false;
				SSS_L 	= false;
				SSS_E	= false;
			#endregion
				
			#region ZLR
				bool czL1 = false;
				bool czS1 = false;
				if(ShowCZI_Fore)
				{
				czL1 = (ToDay(Time[0]) == LastDate ? cziHighsw : myWoodies.ChopZone[0])  == 4;
				czS1 = (ToDay(Time[0]) == LastDate ? cziLowsw : myWoodies.ChopZone[0])   == -4;
				}
				else
				{
				czL1 = myWoodies.ChopZone[Math.Min(CurrentBar,0)]  == 4;
				czS1 = myWoodies.ChopZone[Math.Min(CurrentBar,0)]   == -4;
				
				}
				bool czL2 = myWoodies.ChopZone[Math.Min(CurrentBar,1)]  == 4;
				bool czS2 = myWoodies.ChopZone[Math.Min(CurrentBar,1)]   == -4;
				bool czL3 = myWoodies.ChopZone[Math.Min(CurrentBar,2)]  == 4;
				bool czS3 = myWoodies.ChopZone[Math.Min(CurrentBar,2)]   == -4;
				bool czL4 = myWoodies.ChopZone[Math.Min(CurrentBar,3)]  == 4;
				bool czS4 = myWoodies.ChopZone[Math.Min(CurrentBar,3)]   == -4;
				bool czL5 = myWoodies.ChopZone[Math.Min(CurrentBar,4)]  == 4;
				bool czS5 = myWoodies.ChopZone[Math.Min(CurrentBar,4)]   == -4;
				bool czL6 = myWoodies.ChopZone[Math.Min(CurrentBar,5)]  == 4;
				bool czS6 = myWoodies.ChopZone[Math.Min(CurrentBar,5)]   == -4;
				bool czL7 = myWoodies.ChopZone[Math.Min(CurrentBar,6)]  == 4;
				bool czS7 = myWoodies.ChopZone[Math.Min(CurrentBar,6)]   == -4;
				bool czL8 = myWoodies.ChopZone[Math.Min(CurrentBar,7)]  == 4;
				bool czS8 = myWoodies.ChopZone[Math.Min(CurrentBar,7)]   == -4;
				bool czL9 = myWoodies.ChopZone[Math.Min(CurrentBar,8)]  == 4;
				bool czS9 = myWoodies.ChopZone[Math.Min(CurrentBar,8)]   == -4;

				bool cziLong = (czL1 && czL2 && czL3)
								|| (czL5 && czL4 && czL3 && !czL2 && czL1)
								|| ((czL6 && czL5 && czL4 && !czL3 && !czL2 && czL1) || (czL6 && czL5 && czL4 && !czL3 && czL2 && czL1))
								|| ((czL7 && czL6 && czL5 && !czL4 && czL3 && !czL2 && czL1) || (czL7 && czL6 && czL5 && !czL4 && !czL3 && czL2 && czL1))
								|| ((czL8 && czL7 && czL6 && !czL5 && czL4 && !czL3 && czL2 && czL1) || (czL8 && czL7 && czL6 && !czL5 && czL4 && czL3 && !czL2 && czL1))
								|| (czL9 && czL8 && czL7 && !czL6 && czL5 && czL4 && !czL3 && czL2 && czL1);
				bool cziShort = (czS1 && czS2 && czS3)
								|| (czS5 && czS4 && czS3 && !czS2 && czS1)
								|| ((czS6 && czS5 && czS4 && !czS3 && !czS2 && czS1) || (czS6 && czS5 && czS4 && !czS3 && czS2 && czS1))
								|| ((czS7 && czS6 && czS5 && !czS4 && czS3 && !czS2 && czS1) || (czS7 && czS6 && czS5 && !czS4 && !czS3 && czS2 && czS1))
								|| ((czS8 && czS7 && czS6 && !czS5 && czS4 && !czS3 && czS2 && czS1) || (czS8 && czS7 && czS6 && !czS5 && czS4 && czS3 && !czS2 && czS1))
								|| (czS9 && czS8 && czS7 && !czS6 && czS5 && czS4 && !czS3 && czS2 && czS1);
				//******* ZLR LONG ENTRY ******
				
				// Check for up trend, sidewinder must be neutral or trending and CCI can't be greater than 120
				if(UseSWForecaster == false)
				{
					if ((Trend == 1 && Direction == 1 && myWoodies.Sidewinder[0] >= 0 && (cziLong)
					))
					{
					ZLRL_H = (CCIup<=MaxSignalValue 
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinZLRPts 
						&& CCIup >= MinZLRValue); 
					ZLRL_L = (CCIdown<=MaxSignalValue 
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinZLRPts 
						&& CCIdown >= MinZLRValue); 
					}
				}
				else
					if ((Trend == 1 && Direction == 1
						&& (cziLong)//sumchopzone3L>=11 || sumchopzone5L >= 16 || sumchopzone6L >= 18 || sumchopzone7L >= 21 || sumchopzone8L >= 24)
						))
					{
						ZLRL_H = CCIup<=MaxSignalValue 
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinZLRPts 
						&& CCIup >= MinZLRValue 
							&& SWAbsH >= 30;
						ZLRL_L = CCIdown<=MaxSignalValue 
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinZLRPts 
						&& CCIdown >= MinZLRValue 
							&& SWAbsL > 60;
					}	
					if(Show_Opp_End && ZLRL_L && ZLRL_H) ZLRL_E = true;
					
				//******* ZLR SHORT ENTRY *******
				if(!ZLRL_H && !ZLRL_L)// If there is no ZLR Long, check ZLR Shorts
				{
					if(UseSWForecaster == false)
					{
						if ((Trend == -1 && Direction == -1 && myWoodies.Sidewinder[0] >= 0
							&& (cziShort)//sumchopzone3S<=-11 || sumchopzone5S <= -16 || sumchopzone6S <= -18 || sumchopzone7S <= -21 || sumchopzone8S <= -24)
							))
						{
							ZLRS_H = (CCIup>=-MaxSignalValue 
							&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinZLRPts 
							&& CCIup <= -MinZLRValue); 

							ZLRS_L = (CCIdown>=-MaxSignalValue 
							&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinZLRPts 
							&& CCIdown <= -MinZLRValue); 

							}
					}
					else
						if (Trend == -1 && Direction == -1
							&& (cziShort)//sumchopzone3S<=-11 || sumchopzone5S <= -16 || sumchopzone6S <= -18 || sumchopzone7S <= -21 || sumchopzone8S <= -24)
							)
						{
							ZLRS_H = (CCIup>=-MaxSignalValue 
							&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinZLRPts 
							&& CCIup <= -MinZLRValue) 
							&& SWAbsH >= 30;
							
							ZLRS_L = (CCIdown>=-MaxSignalValue 
							&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinZLRPts 
							&& CCIdown <= -MinZLRValue) 
							&& SWAbsL > 60;
						}
					if(Show_Opp_End && ZLRS_L && ZLRS_H) ZLRS_E = true;
				}
				#endregion

			#region Famir
				//	HookL11;
			if(!ZLRL_H && ! ZLRL_L) // If there is no ZLR Long, check Famir Long
			{
				if(	myWoodies[Math.Min(CurrentBar,2)]>=myWoodies[Math.Min(CurrentBar,1)]+FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,2)]>myWoodies[Math.Min(CurrentBar,3)]
					&& myWoodies[Math.Min(CurrentBar,1)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,1)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
					)
				{
					FamirL_H = (CCIup>myWoodies[Math.Min(CurrentBar,1)] 
					&& (CCIup>myWoodies[Math.Min(CurrentBar,2)] || (CCIup>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
					&& CCIup<=MaxSignalValue
					&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
					&& projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0]
					&& Trend==-1
					);
						
					FamirL_L = ( CCIdown>myWoodies[Math.Min(CurrentBar,1)] 
					&& (CCIdown>myWoodies[Math.Min(CurrentBar,2)] || (CCIdown>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
					&& CCIdown<=MaxSignalValue
					&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
					&& projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0]
					&& Trend==-1
					);
				}	 

				//	HookL12;
					
				if(	myWoodies[Math.Min(CurrentBar,1)]>myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,3)]>myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,3)]>=myWoodies[Math.Min(CurrentBar,2)]+FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,3)]>myWoodies[Math.Min(CurrentBar,4)]
					&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]<FamirLimits
					&& !FamirL_H && !FamirL_L
					)
				{

					FamirL_H = ( CCIup>myWoodies[Math.Min(CurrentBar,1)]
						&& (CCIup>myWoodies[Math.Min(CurrentBar,3)] || (CCIup>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0]
						&& Trend==-1
					);
						
					FamirL_L = ( CCIdown>myWoodies[Math.Min(CurrentBar,1)] 
						&& (CCIdown>myWoodies[Math.Min(CurrentBar,3)] || (CCIdown>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0]
						&& Trend==-1
					);
				}

				 //******** HookL21
				
				if(myWoodies[Math.Min(CurrentBar,2)]>myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,3)]>myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,3)]>=myWoodies[Math.Min(CurrentBar,1)]+FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,3)]>myWoodies[Math.Min(CurrentBar,4)]
					&& myWoodies[Math.Min(CurrentBar,1)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,1)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]<FamirLimits
					&& !FamirL_H && !FamirL_L
					)
				{
					FamirL_H = ( CCIup>myWoodies[Math.Min(CurrentBar,1)]
						&& (CCIup>myWoodies[Math.Min(CurrentBar,3)] || (CCIup>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0]
						&& Trend==-1
						);

					FamirL_L = ( CCIdown>myWoodies[Math.Min(CurrentBar,1)] 
						&& (CCIdown>myWoodies[Math.Min(CurrentBar,3)] || (CCIdown>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0]
						&& Trend==-1
					);
				}

				//***********HookL22

				if(myWoodies[Math.Min(CurrentBar,1)]>myWoodies[Math.Min(CurrentBar,2)]
						&& myWoodies[Math.Min(CurrentBar,3)]>myWoodies[Math.Min(CurrentBar,2)]
						&& myWoodies[Math.Min(CurrentBar,4)]>myWoodies[Math.Min(CurrentBar,3)]
						&& myWoodies[Math.Min(CurrentBar,4)]>myWoodies[Math.Min(CurrentBar,1)]
						&& myWoodies[Math.Min(CurrentBar,4)]>=myWoodies[Math.Min(CurrentBar,2)]+FamirHookMin
						&& myWoodies[Math.Min(CurrentBar,4)]>myWoodies[Math.Min(CurrentBar,5)]
						&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
						&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
						&& myWoodies[Math.Min(CurrentBar,4)]>-FamirLimits
						&& myWoodies[Math.Min(CurrentBar,4)]<FamirLimits
					&& !FamirL_H && !FamirL_L
					)
					{
					FamirL_H = ( CCIup>myWoodies[Math.Min(CurrentBar,1)]
						&& (CCIup>myWoodies[Math.Min(CurrentBar,4)] || (CCIup>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0]
						&& Trend==-1
						);
						
					FamirL_L = ( CCIdown>myWoodies[Math.Min(CurrentBar,1)] 
						&& (CCIdown>myWoodies[Math.Min(CurrentBar,3)] || (CCIdown>0 && myWoodies[Math.Min(CurrentBar,1)]<0))
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						&& projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0]
						&& Trend==-1
						);
					}

					if(Show_Opp_End && FamirL_L && FamirL_H) FamirL_E = true;
			}
				//**************************ENTRIES SHORT*********
				// *************HOOKS11
			if(!FamirL_H && !FamirL_L && !ZLRS_H && !ZLRS_L)//If there is no Famir Long or ZLR Short , check Famir Short
			{
				if(myWoodies[Math.Min(CurrentBar,2)]<=myWoodies[Math.Min(CurrentBar,1)]-FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,2)]<myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,2)]<myWoodies[Math.Min(CurrentBar,3)]
					&& myWoodies[Math.Min(CurrentBar,1)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,1)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
					&& !FamirS_H && !FamirS_L
				)
				{
				FamirS_H = ( CCIup<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIup<myWoodies[Math.Min(CurrentBar,2)] || (CCIup<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIup>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
					&& projectedHigh<LinRegPredictor(LSMAPeriods).LRHighClose[0]
					&& Trend==1
					);	
						
				FamirS_L = ( CCIdown<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIdown<myWoodies[Math.Min(CurrentBar,2)] || (CCIdown<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIdown>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
					&& projectedLow<LinRegPredictor(LSMAPeriods).LRLowClose[0]
					&& Trend==1
					);
				}	
			
			   //******* HookS12
			
				if(myWoodies[Math.Min(CurrentBar,1)]<myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,3)]<=myWoodies[Math.Min(CurrentBar,2)]-FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,3)]<myWoodies[Math.Min(CurrentBar,4)]
					&& myWoodies[Math.Min(CurrentBar,3)]<myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]<FamirLimits
					&& !FamirS_H && !FamirS_L
				)
				{
				FamirS_H = ( CCIup<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIup<myWoodies[Math.Min(CurrentBar,3)] || (CCIup<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIup>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
					&& projectedHigh<LinRegPredictor(LSMAPeriods).LRHighClose[0]
					&& Trend==1
					);

				FamirS_L = ( CCIdown<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIdown<myWoodies[Math.Min(CurrentBar,3)] || (CCIdown<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIdown>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
					&& projectedLow<LinRegPredictor(LSMAPeriods).LRLowClose[0]
					&& Trend==1
					);	
				}	
			
			 //******** HookS21

				if(myWoodies[Math.Min(CurrentBar,2)]<myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,3)]<myWoodies[Math.Min(CurrentBar,4)]
					&& myWoodies[Math.Min(CurrentBar,3)]<myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,3)]<=myWoodies[Math.Min(CurrentBar,1)]-FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,1)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,1)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,3)]<FamirLimits
					&& !FamirS_H && !FamirS_L
				)
				{
				FamirS_H = ( CCIup<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIup<myWoodies[Math.Min(CurrentBar,3)] || (CCIup<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIup>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
					&& projectedHigh<LinRegPredictor(LSMAPeriods).LRHighClose[0]
					&& Trend==1
					);	

				FamirS_L = ( CCIdown<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIdown<myWoodies[Math.Min(CurrentBar,3)] || (CCIdown<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIdown>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
					&& projectedLow<LinRegPredictor(LSMAPeriods).LRLowClose[0]
					&& Trend==1
					);	
				}
			
			   //***********HookS22

				if(myWoodies[Math.Min(CurrentBar,1)]<myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,3)]<myWoodies[Math.Min(CurrentBar,2)]
					&& myWoodies[Math.Min(CurrentBar,4)]<myWoodies[Math.Min(CurrentBar,3)]
					&& myWoodies[Math.Min(CurrentBar,4)]<myWoodies[Math.Min(CurrentBar,5)]
					&& myWoodies[Math.Min(CurrentBar,4)]<myWoodies[Math.Min(CurrentBar,1)]
					&& myWoodies[Math.Min(CurrentBar,4)]<=myWoodies[Math.Min(CurrentBar,2)]-FamirHookMin
					&& myWoodies[Math.Min(CurrentBar,2)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,2)]<FamirLimits
					&& myWoodies[Math.Min(CurrentBar,4)]>-FamirLimits
					&& myWoodies[Math.Min(CurrentBar,4)]<FamirLimits
					&& !FamirS_H && !FamirS_L
				)
				{
				FamirS_H = ( CCIup<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIup<myWoodies[Math.Min(CurrentBar,4)] || (CCIup<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIup>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
					&& projectedHigh<LinRegPredictor(LSMAPeriods).LRHighClose[0]
					&& Trend==1
					);

				FamirS_L = ( CCIdown<myWoodies[Math.Min(CurrentBar,1)]
					&& (CCIdown<myWoodies[Math.Min(CurrentBar,4)] || (CCIdown<0 && myWoodies[Math.Min(CurrentBar,1)]>0))
					&& CCIdown>=-MaxSignalValue
					&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
					&& projectedLow<LinRegPredictor(LSMAPeriods).LRLowClose[0]
					&& Trend==1
					);
				}

				if(Show_Opp_End && FamirS_L && FamirS_H) FamirS_E = true;
			}					
			#endregion
				
			#region Vegas
			
			
			if (BarsAgoExtremeXup < 23 && !FamirL_L && !FamirL_H) 
			{
//VT Long
				swingHigh 		= CheckWoodiesMAX(myWoodies, BarsAgoExtremeXup,Math.Min(CurrentBar, 1));
				barsAgoSwingH 	= MRO(delegate{return myWoodies[0] == swingHigh;},1,CurrentBar-3);
				vswingLow 		= CheckWoodiesMIN(myWoodies,barsAgoSwingH,Math.Min(CurrentBar, 1)); 
				if ((swingHigh > -VTHFEPenetrationLevel 
					&& swingHigh < 0)
					&& myWoodies[Math.Min(CurrentBar,2)] < swingHigh
					&& myWoodies[Math.Min(CurrentBar,1)] < swingHigh 
					&& barsAgoSwingH < 11
					&& barsAgoSwingH >= 4
					&& myWoodies[Math.Min(CurrentBar,1)]<0
					&& myWoodies[Math.Min(CurrentBar,2)]<0
					&& vswingLow >= -VMaxSwingPoint
					&& barsAgoSwingH <= VMaxSwingBars
					)
				{
					VegasL_H = (CCIup > swingHigh
						&& projectedHigh>LinRegPredictor(LSMAPeriods).LRHighClose[0]
						&& CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
					
					VegasL_L = (CCIdown > swingHigh
						&& projectedLow>LinRegPredictor(LSMAPeriods).LRLowClose[0]
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
				}
				if(Show_Opp_End && VegasL_L && VegasL_H) VegasL_E = true;
			}
			
			
			
			if (BarsAgoExtremeXdown < 23 && !FamirS_L && !FamirS_H) 
			{
				swingLow 		= CheckWoodiesMIN(myWoodies,BarsAgoExtremeXdown,Math.Min(CurrentBar, 1)); 
				barsAgoSwingL 	= MRO(delegate{return myWoodies[0] == swingLow;},1,CurrentBar-3);				
				vswingHigh 		= CheckWoodiesMAX(myWoodies,barsAgoSwingL,Math.Min(CurrentBar, 1)); 
				//VT Short
				if ((swingLow < VTHFEPenetrationLevel && swingLow > 0 )
					&& myWoodies[Math.Min(CurrentBar,2)] > swingLow 
					&& myWoodies[Math.Min(CurrentBar,1)] > swingLow 
					&& barsAgoSwingL < 11
					&& barsAgoSwingL >= 4
					&& myWoodies[Math.Min(CurrentBar,1)]>0
					&& myWoodies[Math.Min(CurrentBar,2)]>0
					&& vswingHigh <= VMaxSwingPoint
					&& barsAgoSwingL <= VMaxSwingBars
					)
				{
					VegasS_H = (	CCIup < swingLow
						&& CCIup>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
						&& projectedHigh<LinRegPredictor(LSMAPeriods).LRHighClose[0]
						);
					
					VegasS_L = (	CCIdown < swingLow
						&& CCIdown>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
						&& projectedLow<LinRegPredictor(LSMAPeriods).LRLowClose[0]
						);
				}
					if(Show_Opp_End && VegasS_L && VegasS_H) VegasS_E = true;
			}
			#endregion

			#region GB100
			trendcountL	= MRO(delegate{return myWoodies.ChopZone[0] < 4;},3,CurrentBar);//How long since chop zone 2 non aqua bars
			trendcountS	= MRO(delegate{return myWoodies.ChopZone[0] > -4;},3,CurrentBar);//How long since chop zone 2 non red bars
			
			if(!ZLRL_L && !ZLRL_H)
			{
				if(trendcountL >= TrendLength && Trend == 1
					&& BarsAgo_0Xdown < 6
					&& BarsAgo_n100Xup < BarsAgo_0Xdown//Crossed below -100 since it crossed below 0
				)
				{
					GBL_H = (CCIup > -100 && myWoodies[Math.Min(CurrentBar,1)] < -100
						&& (ToDay(Time[0]) == LastDate ? cziHighsw : myWoodies.ChopZone[0]) == 4
						&& CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
	
					GBL_L = (CCIdown > -100 && myWoodies[Math.Min(CurrentBar,1)] < -100
						&& (ToDay(Time[0]) == LastDate ? cziLowsw : myWoodies.ChopZone[0]) == 4
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
				}
					if(Show_Opp_End && GBL_L && GBL_H) GBL_E = true;
			}
			if(!ZLRS_L && !ZLRS_H)
			{
				if(	trendcountS >= TrendLength && Trend == -1
					&& BarsAgo_0Xup < 6
					&& BarsAgo_100Xdown < BarsAgo_0Xup
					)
				{
					GBS_H = ( CCIup < 100 && myWoodies[Math.Min(CurrentBar,1)] > 100
						&& (ToDay(Time[0]) == LastDate ? cziHighsw : myWoodies.ChopZone[0]) == -4
						&& CCIup>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
						);
	
					GBS_L = ( CCIdown < 100 && myWoodies[Math.Min(CurrentBar,1)] > 100
						&& (ToDay(Time[0]) == LastDate ? cziLowsw : myWoodies.ChopZone[0]) == -4
						&& CCIdown>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
						);
					
				}
				if(Show_Opp_End && GBS_L && GBS_H) GBS_E = true;
			}
			#endregion
			
			#region Tony
			if((!ZLRL_L && !ZLRL_H))
			{
				if	(Trend == 1
						&& BarsAgo_0Xdown < 10
						&& BarsAgo_0Xdown > 4
						&& myWoodies[Math.Min(CurrentBar,1)]<0
						&& BarsAgo_n100Xup >= BarsAgo_0Xdown//Crossed below -100 since it crossed below 0
					)
				{
					TonyL_H = (CCIup > 0 && CCIup<=MaxSignalValue
						&& CCIup-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
	
					TonyL_L = (CCIdown > 0
						&& CCIdown<=MaxSignalValue
						&& CCIdown-myWoodies[Math.Min(CurrentBar,1)]>=MinCCIPts 
						);
				}
				if(Show_Opp_End && TonyL_L && TonyL_H) TonyL_E = true;
			}
			
			if((!ZLRS_L && !ZLRS_H))
			{
				if	(Trend==-1
						&& BarsAgo_0Xup < 10
						&& BarsAgo_0Xup > 4
						&& myWoodies[Math.Min(CurrentBar,1)]>0
						&& BarsAgo_100Xdown >= BarsAgo_0Xup
					)
				{
					TonyS_H = (CCIup < 0
						&& CCIup>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIup>=MinCCIPts 
						);
						
					TonyS_L = (CCIdown < 0
						&& CCIdown>=-MaxSignalValue
						&& myWoodies[Math.Min(CurrentBar,1)]-CCIdown>=MinCCIPts 
						);
				}
				if(Show_Opp_End && TonyS_L && TonyS_H) TonyS_E = true;
			}
			#endregion
			
			#region Ghosts
				gstLong = 0;
				gstShort = 0;
				gstLeftPk = 0;
				gstMiddlePk = 0;
				gstRightPk = 0;
				gstLeftLow = 0;
				gstRightLow = 0;
				gstLeftPkBars = 0;
				gstMiddlePkBars = 0;
				gstRightPkBars = 0;
				gstLeftLowBars = 0;
				gstRightLowBars = 0;
				lbPeriodup = BarsAgo_0Xup;
				lbPerioddown = BarsAgo_0Xdown;
	
				ZigZag gstZigPoints = ZigZag(myWoodies,DeviationType.Points,UsePointsAmount,false);
				ZigZag gstZigPercent = ZigZag(myWoodies,DeviationType.Percent,UsePointsAmount,false);
	
					if(myWoodies[Math.Min(CurrentBar,1)]>0 && UsePoints == "Points")//Ghost Short using Points
					{
						//How many bars since peaks
						gstLeftPkBars = gstZigPoints.HighBar(0,3,BarsAgo_0Xup);
						gstMiddlePkBars = gstZigPoints.HighBar(0,2,BarsAgo_0Xup);
						gstRightPkBars = gstZigPoints.HighBar(0,1,BarsAgo_0Xup);
						//How many bars since troughs
						gstLeftLowBars = gstZigPoints.LowBar(0,2,BarsAgo_0Xup);
						gstRightLowBars = gstZigPoints.LowBar(0,1,BarsAgo_0Xup);
						//Get Peak Values
						gstLeftPk = Math.Abs(myWoodies[Math.Max(5,gstLeftPkBars)]);
						gstMiddlePk = Math.Abs(myWoodies[Math.Max(3,gstMiddlePkBars)]);
						gstRightPk = Math.Abs(myWoodies[Math.Max(0,gstRightPkBars)]);
						//Get trough values
						gstLeftLow = Math.Abs(myWoodies[Math.Max(4,gstLeftLowBars)]);
						gstRightLow = Math.Abs(myWoodies[Math.Max(1,gstRightLowBars)]);
					}
					if(myWoodies[Math.Min(CurrentBar,1)]<0 && UsePoints == "Points")//Ghost Long using Points
					{
						//How many bars since peaks
						gstLeftPkBars = gstZigPoints.LowBar(0,3,BarsAgo_0Xdown);
						gstMiddlePkBars = gstZigPoints.LowBar(0,2,BarsAgo_0Xdown);
						gstRightPkBars = gstZigPoints.LowBar(0,1,BarsAgo_0Xdown);
						//How many bars since troughs
						gstLeftLowBars = gstZigPoints.HighBar(0,2,BarsAgo_0Xdown);
						gstRightLowBars = gstZigPoints.HighBar(0,1,BarsAgo_0Xdown);
						//Get Peak Values
						gstLeftPk = Math.Abs(myWoodies[Math.Max(5,gstLeftPkBars)]);//
						gstMiddlePk = Math.Abs(myWoodies[Math.Max(3,gstMiddlePkBars)]);
						gstRightPk = Math.Abs(myWoodies[Math.Max(0,gstRightPkBars)]);
						//Get trough values
						gstLeftLow = Math.Abs(myWoodies[Math.Max(4,gstLeftLowBars)]);
						gstRightLow = Math.Abs(myWoodies[Math.Max(1,gstRightLowBars)]);
					}	
	
					if(myWoodies[Math.Min(CurrentBar,1)]>0 && UsePoints == "Percent")//Ghost Short using %
					{
						//How many bars since peaks
						gstLeftPkBars = gstZigPercent.HighBar(0,3,BarsAgo_0Xup);
						gstMiddlePkBars = gstZigPercent.HighBar(0,2,BarsAgo_0Xup);
						gstRightPkBars = gstZigPercent.HighBar(0,1,BarsAgo_0Xup);
						//How many bars since troughs
						gstLeftLowBars = gstZigPercent.LowBar(0,2,BarsAgo_0Xup);
						gstRightLowBars = gstZigPercent.LowBar(0,1,BarsAgo_0Xup);
						//Get Peak Values
						gstLeftPk = Math.Abs(myWoodies[Math.Max(5,gstLeftPkBars)]);
						gstMiddlePk = Math.Abs(myWoodies[Math.Max(3,gstMiddlePkBars)]);
						gstRightPk = Math.Abs(myWoodies[Math.Max(0,gstRightPkBars)]);
						//Get trough values
						gstLeftLow = Math.Abs(myWoodies[Math.Max(4,gstLeftLowBars)]);
						gstRightLow = Math.Abs(myWoodies[Math.Max(1,gstRightLowBars)]);
					}
					if(myWoodies[Math.Min(CurrentBar,1)]<0 && UsePoints == "Percent")//Ghost Long using %
					{
						//How many bars since peaks
						gstLeftPkBars = gstZigPercent.LowBar(0,3,BarsAgo_0Xdown);
						gstMiddlePkBars = gstZigPercent.LowBar(0,2,BarsAgo_0Xdown);
						gstRightPkBars = gstZigPercent.LowBar(0,1,BarsAgo_0Xdown);
						//How many bars since troughs
						gstLeftLowBars = gstZigPercent.HighBar(0,2,BarsAgo_0Xdown);
						gstRightLowBars = gstZigPercent.HighBar(0,1,BarsAgo_0Xdown);
						//Get Peak Values
						gstLeftPk = Math.Abs(myWoodies[Math.Max(5,gstLeftPkBars)]);
						gstMiddlePk = Math.Abs(myWoodies[Math.Max(3,gstMiddlePkBars)]);
						gstRightPk = Math.Abs(myWoodies[Math.Max(0,gstRightPkBars)]);
						//Get trough values
						gstLeftLow = Math.Abs(myWoodies[Math.Max(4,gstLeftLowBars)]);
						gstRightLow = Math.Abs(myWoodies[Math.Max(1,gstRightLowBars)]);
					}
	
				//Reset Right Peak if on first bar after what will become a new peak.
					if(gstRightLowBars<gstRightPkBars && Math.Abs(CheckWoodiesMAX(myWoodies,gstRightLowBars,0)-gstRightLow) >= UsePointsAmount)
					{
						if(UsePoints == "Points")
						{
							if(Math.Abs(myWoodies[0])<= (Math.Abs(myWoodies[Math.Min(CurrentBar,1)])-UsePointsAmount))
							{
								gstLeftPk = gstMiddlePk;
								gstLeftPkBars = gstMiddlePkBars;
								gstMiddlePk = gstRightPk;
								gstMiddlePkBars = gstRightPkBars;
								gstRightPk = Math.Abs(myWoodies[Math.Min(CurrentBar,1)]);
								gstRightPkBars = 1;
							}
						}
							else if(Math.Abs(myWoodies[0])<= (Math.Abs(myWoodies[Math.Min(CurrentBar,1)])*((100-UsePointsAmount)/100)))
							{
								gstLeftPk = gstMiddlePk;
								gstLeftPkBars = gstMiddlePkBars;
								gstMiddlePk = gstRightPk;
								gstMiddlePkBars = gstRightPkBars;
								gstRightPk = Math.Abs(myWoodies[Math.Min(CurrentBar,1)]);
								gstRightPkBars = 1;
							}
						
					}
					
					gstChangePerBar = (gstRightLow-gstLeftLow)/(gstLeftLowBars-gstRightLowBars);//What is the slope of the trend line per bar
				//	Reset Left and Right Lows to provide proper trend line drawing			
					int newLeftLowBars = 0;
					int newRightLowBars = 0;
					double gap = 0;
					double gap1 = 0;
					if(gstChangePerBar>0)
					{
						for(int i = gstLeftLowBars; i > gstMiddlePkBars; --i)
						{
								if(Math.Abs(myWoodies[i]) < gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar)
								{
									gap1 = gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar - Math.Abs(myWoodies[i]);
									if(gap<gap1)
									{
										newLeftLowBars = i;
										gap = gap1;
										gstLeftLow = Math.Abs(myWoodies[i]);
									}
								}
						}
						if(newLeftLowBars != 0)
						{
							gstLeftLowBars = newLeftLowBars;
							gstChangePerBar = (gstRightLow-gstLeftLow)/(gstLeftLowBars-gstRightLowBars);//What is the slope of the trend line per bar
						}
						gap = 0;
						gap1 = 0;
						for(int i = gstRightLowBars; i > gstRightPkBars; --i)
						{
								if(Math.Abs(myWoodies[i]) < gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar)
								{
									gap1 = gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar - Math.Abs(myWoodies[i]);
									if(gap<gap1)
									{
										newRightLowBars = i;
										gap = gap1;
										gstRightLow = Math.Abs(myWoodies[i]);
									}
								}
						}
						if(newRightLowBars != 0)
						{
							gstRightLowBars = newRightLowBars;
							gstChangePerBar = (gstRightLow-gstLeftLow)/(gstLeftLowBars-gstRightLowBars);//What is the slope of the trend line per bar
						}
					}

					if(gstChangePerBar<0)
					{
						newRightLowBars = 0;
						gap = 0;
						gap1 = 0;
						for(int i = gstMiddlePkBars; i > gstRightLowBars; --i)
						{
								if(Math.Abs(myWoodies[i]) < gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar)
								{
									gap1 = gstLeftLow+(gstLeftLowBars-i)*gstChangePerBar - Math.Abs(myWoodies[i]);
									if(gap<gap1)
									{
										newRightLowBars = i;
										gap = gap1;
										gstRightLow = Math.Abs(myWoodies[i]);
									}
								}
						}
						if(newRightLowBars != 0)
						{
							gstRightLowBars = newRightLowBars;
							gstChangePerBar = (gstRightLow-gstLeftLow)/(gstLeftLowBars-gstRightLowBars);//What is the slope of the trend line per bar
						}
					}
				//				
					gstTrendBrkShortH = CCIup<(gstLeftLow+(gstLeftLowBars*gstChangePerBar))
						&& myWoodies[Math.Min(CurrentBar,1)]>(gstLeftLow+((gstLeftLowBars-1)*gstChangePerBar)) 
						&& myWoodies[Math.Min(CurrentBar,1)]>0//Has there been a trend line break?
						|| (myWoodies[Math.Min(CurrentBar,1)]>(gstLeftLow+(gstLeftLowBars*gstChangePerBar)) 
							&& CCIup < 0 && myWoodies[Math.Min(CurrentBar,1)] > 0 
							&& (gstLeftLow+(gstLeftLowBars*gstChangePerBar))>-50);//Will there be a trend line break?

						gstTrendBrkShortL = CCIdown<(gstLeftLow+(gstLeftLowBars*gstChangePerBar))
						&& myWoodies[Math.Min(CurrentBar,1)]>(gstLeftLow+((gstLeftLowBars-1)*gstChangePerBar)) 
						&& myWoodies[Math.Min(CurrentBar,1)]>0//Has there been a trend line break?
						|| (myWoodies[Math.Min(CurrentBar,1)]>(gstLeftLow+(gstLeftLowBars*gstChangePerBar)) 
							&& CCIdown < 0 && myWoodies[Math.Min(CurrentBar,1)] > 0 
							&& (gstLeftLow+(gstLeftLowBars*gstChangePerBar))>-50);//Will there be a trend line break?
					
					bool gstGap = gstRightPk >= (gstLeftLow+((gstLeftLowBars-gstRightPkBars)*gstChangePerBar)) + GstColor;
				GhostS_H = (
						(
						(gstLeftPk < gstMiddlePk && gstRightPk < gstMiddlePk) 
						|| (gstLeftPk > gstMiddlePk && gstRightPk > gstMiddlePk)
						)
						&& gstTrendBrkShortH 
						&& BarsAgo_0Xup>gstLeftPkBars 
						&& gstLeftPkBars != -1 
						&& gstMiddlePkBars != -1 
						&& gstRightPkBars != -1
						&& gstMiddlePkBars<gstLeftLowBars 
						&& gstRightPkBars<gstRightLowBars	
						&& CCIup>=-MaxSignalValue
						&& gstGap
					);
				GhostS_L = (
					((gstLeftPk < gstMiddlePk && gstRightPk < gstMiddlePk) 
					|| (gstLeftPk > gstMiddlePk && gstRightPk > gstMiddlePk)
					)
					&& gstTrendBrkShortL ==true 
					&& BarsAgo_0Xup>gstLeftPkBars 
					&& gstLeftPkBars != -1 
					&& gstMiddlePkBars != -1 
					&& gstRightPkBars != -1
					&& gstMiddlePkBars<gstLeftLowBars 
					&& gstRightPkBars<gstRightLowBars	
					&& CCIdown>=-MaxSignalValue
					&& gstGap
					);
				
					gstTrendBrkLongH = (Math.Abs(CCIup)<(gstLeftLow+(gstLeftLowBars*gstChangePerBar))
						&& Math.Abs(myWoodies[Math.Min(CurrentBar,1)])>(gstLeftLow+((gstLeftLowBars-1)*gstChangePerBar))
						&& myWoodies[Math.Min(CurrentBar,1)]<0
						)
						|| (Math.Abs(myWoodies[Math.Min(CurrentBar,1)])>(gstLeftLow+(gstLeftLowBars*gstChangePerBar)) 
							&& CCIup > 0 && myWoodies[Math.Min(CurrentBar,1)] < 0 
							&& (gstLeftLow+(gstLeftLowBars*gstChangePerBar))>-50);//Will there be a trend line break?

					gstTrendBrkLongL = (Math.Abs(CCIdown)<(gstLeftLow+(gstLeftLowBars*gstChangePerBar))
						&& Math.Abs(myWoodies[Math.Min(CurrentBar,1)])>(gstLeftLow+((gstLeftLowBars-1)*gstChangePerBar))
						&& myWoodies[Math.Min(CurrentBar,1)]<0
						)
						|| (Math.Abs(myWoodies[Math.Min(CurrentBar,1)])>(gstLeftLow+(gstLeftLowBars*gstChangePerBar)) 
							&& CCIdown > 0 && myWoodies[Math.Min(CurrentBar,1)] < 0 
							&& (gstLeftLow+(gstLeftLowBars*gstChangePerBar))>-50);//Will there be a trend line break?
				
				
				//Ghosts Long
				GhostL_H = (
					((gstLeftPk < gstMiddlePk && gstRightPk < gstMiddlePk) 
					|| (gstLeftPk > gstMiddlePk && gstRightPk > gstMiddlePk)
					)
					&& gstTrendBrkLongH 
					&& BarsAgo_0Xdown>gstLeftPkBars && gstLeftPkBars != -1 
					&& gstMiddlePkBars != -1 && gstRightPkBars != -1
					&& gstMiddlePkBars<gstLeftLowBars 
					&& gstRightPkBars<gstRightLowBars	
					&& CCIup<=MaxSignalValue
					&& gstGap
					);
					
				GhostL_L = (
					((gstLeftPk < gstMiddlePk && gstRightPk < gstMiddlePk)
					|| (gstLeftPk > gstMiddlePk && gstRightPk > gstMiddlePk)
					)
					&& gstTrendBrkLongL 
					&& BarsAgo_0Xdown>gstLeftPkBars && gstLeftPkBars != -1 
					&& gstMiddlePkBars != -1 && gstRightPkBars != -1
					&& gstMiddlePkBars<gstLeftLowBars 
					&& gstRightPkBars<gstRightLowBars 
					&& CCIdown<=MaxSignalValue
					&& gstGap
					);
	
/*					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0",BarsAgo_0Xdown.ToString(),0,-20,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT",gstLeftPk.ToString("###.#"),0,-70,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT1",gstLeftPkBars.ToString(),0,-95,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT2",gstMiddlePk.ToString("###.#"),0,-120,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT3",gstMiddlePkBars.ToString(),0,-145,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT4",gstRightPk.ToString("###.#"),0,-170,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT5",gstRightPkBars.ToString(),0,-195,Brushes.PaleGreen);
				
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0a",BarsAgo_0Xup.ToString(),0,20,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT6",gstLeftLow.ToString("###.#"),0,120,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT7",gstLeftLowBars.ToString(),0,130,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT8",gstRightLow.ToString("###.#"),0,150,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT9",gstRightLowBars.ToString(),0,160,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "Slope",gstChangePerBar.ToString("##.#"),0,185,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "ShBreak",(gstLeftLow+(gstLeftLowBars*gstChangePerBar)).ToString("##.#"),0,210,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "cb",CurrentBar.ToString(),0,225,Brushes.PaleGreen);
*/					
	/*				Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0a",GhostL_H.ToString(),0,40,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0b",GhostL_L.ToString(),0,20,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0f",GhostL_E.ToString(),0,60,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0c",GhostS_H.ToString(),0,-20,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0d",GhostS_L.ToString(),0,-40,Brushes.PaleGreen);
					Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT0e",GhostS_E.ToString(),0,-60,Brushes.PaleGreen);
	*/			
					if(Show_Opp_End && GhostL_L && GhostL_H) GhostL_E = true;
					if(Show_Opp_End && GhostS_L && GhostS_H) GhostS_E = true;
			#endregion

			#region Slingshot
			//Long
				if (myWoodies.ChopZone[0] == 4 && Show_SS
					&& myWoodies[1] < SSZeroBand && myWoodies[1] > -SSZeroBand//CCI inside 0 band
					&& myWoodies.Turbo[1] < -100 + SS100Band && myWoodies.Turbo[1] > -100 - SS100Band//Turbo inside -100 band
					&& myWoodies[1] >= myWoodies.Turbo[1] + SSCCITurboSeparation)//Enough separation between CCI and Turbo
				{
				SSL_H = (CCIup<=MaxSignalValue && Turboup<=MaxSSTurboValue && myWoodies.Turbo[1] <= MaxSSTurboValue
					&& CCIup > myWoodies[1] && myWoodies[1] < myWoodies[2] && myWoodies[2] < myWoodies[3]
					&& Turboup > myWoodies.Turbo[1]// && myWoodies.Turbo[1] < myWoodies.Turbo[2]
					); 
//				SSL_L = (CCIdown<=MaxSignalValue && Turbodown<=MaxSSTurboValue && myWoodies.Turbo[1] <= MaxSSTurboValue
//					&& CCIdown > myWoodies[1] && myWoodies[1] < myWoodies[2] && myWoodies[2] < myWoodies[3] 
//					&& Turbodown > myWoodies.Turbo[1]// && myWoodies.Turbo[1] < myWoodies.Turbo[2]
//					); 
				}
					if(Show_Opp_End && SSL_L && SSL_H) SSL_E = true;
			//Short
				if (myWoodies.ChopZone[0] == -4 && Show_SS
					&& myWoodies[1] > -SSZeroBand && myWoodies[1] < SSZeroBand
					&& myWoodies.Turbo[1] > 100 - SS100Band && myWoodies.Turbo[1] < 100 + SS100Band
					&& myWoodies[1] <= myWoodies.Turbo[1] - SSCCITurboSeparation)//Short
				{
//				SSS_H = (CCIup>=-MaxSignalValue && Turboup>=-MaxSSTurboValue && myWoodies.Turbo[1] >= -MaxSSTurboValue
//					&& CCIup < myWoodies[1] && myWoodies[1] > myWoodies[2] && myWoodies[2] > myWoodies[3] 
//					&& Turboup < myWoodies.Turbo[1]// && myWoodies.Turbo[1] > myWoodies.Turbo[2]
//					); 
				SSS_L = (CCIdown>=-MaxSignalValue && Turbodown>=-MaxSSTurboValue && myWoodies.Turbo[1] >= -MaxSSTurboValue
					&& CCIdown < myWoodies[1] && myWoodies[1] > myWoodies[2] && myWoodies[2] > myWoodies[3] 
					&& Turbodown < myWoodies.Turbo[1]// && myWoodies.Turbo[1] > myWoodies.Turbo[2]
					); 
				}
//					if(Show_Opp_End && SSS_L && SSS_H) SSS_E = true;
#endregion
			
			#region Draw Momentum Change Exits
			if(ShowMomoChange == true)
			{
				BarsAgoLongPattern	= MRO(delegate{return (WCCIPattern[0] > 0 && WCCIDirection[0] >0);},1,CurrentBar-3);
				BarsAgoShortPattern	= MRO(delegate{return (WCCIPattern[0] > 0 && WCCIDirection[0] <0);},1,CurrentBar-3);
				
				BarsAgoLongPattern 	= BarsAgoLongPattern 	< 1 ? 1 : BarsAgoLongPattern;
				BarsAgoShortPattern = BarsAgoShortPattern 	< 1 ? 1 : BarsAgoShortPattern;

				LongMomoChange = (myWoodies[0]<=myWoodies[Math.Min(CurrentBar, 1)] && Open[0] >=Close[0]);
				ShortMomoChange = (myWoodies[0]>=myWoodies[Math.Min(CurrentBar, 1)] && Open[0] <=Close[0]);

				if(LongMomoChange && (BarsAgo_100Xdown==1 ? Open[0] >=Close[0] : BarsAgo_100Xdown > BarsAgoLongPattern) 
					&& MIN(Low,BarsAgoLongPattern)[0] > Close[BarsAgoLongPattern]-(BarsPeriod.Value*TickSize)
					&& BarsAgoShortPattern>BarsAgoLongPattern)
				{	
					Draw.ArrowLine(this,CurrentBar.ToString()+"momol",0,250,0,210,Brushes.Red,DashStyleHelper.Dash,2);
					if(SoundMomoAlert) Alert(CurrentBar.ToString() + "momoalert1", Priority.Medium, "Momo Change long", "Alert2.wav", AlertInterval, Brushes.DarkGreen, Brushes.White);
				}
				else if(ShortMomoChange && (BarsAgo_n100Xup==1 ? Open[0] <=Close[0] : BarsAgo_n100Xup > BarsAgoShortPattern) 
					&& MAX(High,BarsAgoShortPattern)[0] < Close[BarsAgoShortPattern]+(BarsPeriod.Value*TickSize)
					&& BarsAgoShortPattern<BarsAgoLongPattern)
				{	
					Draw.ArrowLine(this,CurrentBar.ToString()+"momos",0,-250,0,-210,Brushes.DarkGreen,DashStyleHelper.Dash,2);
					if(SoundMomoAlert) Alert(CurrentBar.ToString() + "momoalert2", Priority.Medium, "Momo Change short", "Alert2.wav", AlertInterval, Brushes.DarkGreen, Brushes.White);
				}
				else
				{	
					RemoveDrawObject(CurrentBar.ToString() + "momol");
					RemoveDrawObject(CurrentBar.ToString() + "momos");
				}		
//				Draw.Text(this,CurrentBar.ToString() + "momoTEXT0",BarsAgo_100Xdown.ToString(),0,-20,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "momoLTEXT4",BarsAgo_n100Xup.ToString(),0,-20,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "momoLTEXT",BarsAgoLongPattern.ToString(),0,-70,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "momoLTEXT1",BarsAgoShortPattern.ToString(),0,-95,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "momoLTEXT2",MIN(Low,BarsAgoLongPattern+1)[0].ToString("###.##"),0,-120,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT2",MAX(High,BarsAgoShortPattern+1)[0].ToString("###.##"),0,-120,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT3",Close[BarsAgoLongPattern].ToString("###.##"),0,-145,Brushes.PaleGreen);
//				Draw.Text(this,CurrentBar.ToString() + "GHOSTLTEXT3",Close[BarsAgoShortPattern].ToString("###.##"),0,-145,Brushes.PaleGreen);
			

			}
			#endregion
			}//End sideways if statement
		}
		
		protected void DrawPatterns()
		{
			
			if(CurrentBar % 5 == 0)
				Draw.TextFixed(this,"version","Snaphook's CCI Forecaster v7.5",TextPosition.BottomRight);
			else
				RemoveDrawObject("version");

			#region ZLR
		//LONG
			if(((ZLRL_H  && pcbar != -1) || (Show_Opp_End && ZLRL_L && pcbar != 1) || ZLRL_E) && Show_ZLR)
			{
				wCCIPatternSeries[0] = (1);
				wCCIDirectionSeries[0] = (1);
				
				if(ZLRLineThick > 0)
					Draw.Line(this,CurrentBar.ToString() + "ZLRLLine",true, 0,  myWoodies[0],0,0, ZLRLineColorL,DashStyleHelper.Solid,ZLRLineThick);
					else
						RemoveDrawObject(CurrentBar.ToString() + "ZLRLLine");
						
				if(SoundAlert)
				{
					Alert(CurrentBar.ToString(), Priority.Medium, "Zero Line Reject Long", @AlertFile, AlertInterval, Brushes.DarkGreen, Brushes.White);
				}
				if(ZLRL_E)
				{
					wCCIBarEndSeries[0] = (0);
					Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerL",true,(SSL_E ? "Z-S\ne" : "Z\ne"),0,TextAnchorL-SignalLabels.Size,0,ZLRColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
				}
				else
				{
					if(ZLRL_H)
					{
						wCCIBarEndSeries[0] = (1);
						Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerL",true,(SSL_H ? "Z-S\nt" : "Z\nt"),0,TextAnchorL-SignalLabels.Size,0,ZLRColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
					else if(Show_Opp_End)
					{
						wCCIBarEndSeries[0] = (-1);
						Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerL",true,(SSL_L && !SSL_E ? "Z-S\nb" : "Z\nb"),0,TextAnchorL-SignalLabels.Size,0,ZLRColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				}
			}		
			else
			{
				RemoveDrawObject(CurrentBar.ToString() + "ZLRL");
				RemoveDrawObject(CurrentBar.ToString() + "ZLRLLine");
				RemoveDrawObject(CurrentBar.ToString() + "ZLRTriggerL");
			}
		// SHORT
			if(((Show_Opp_End && ZLRS_H  && pcbar != -1) || (ZLRS_L && pcbar != 1) || ZLRS_E) && Show_ZLR)
			{
				wCCIPatternSeries[0] = (1);
				wCCIDirectionSeries[0] = (-1);
				
				if(ZLRLineThick > 0)
					Draw.Line(this,CurrentBar.ToString() + "ZLRSLine",true, 0,  myWoodies[0],0,0, ZLRLineColorS,DashStyleHelper.Solid,ZLRLineThick);
					else
						RemoveDrawObject(CurrentBar.ToString() + "ZLRSLine");
						
				if(SoundAlert)
				{
					Alert(CurrentBar.ToString(), Priority.Medium, "Zero Line Reject Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
				}
				if(ZLRS_E)
				{
					wCCIBarEndSeries[0] = (0);
					Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerS",true,(SSS_E ? "Z-S\ne" : "Z\ne"),0,TextAnchorS-SignalLabels.Size,0,ZLRColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
				}
				else
				{
					if(Show_Opp_End && ZLRS_H)
					{
						wCCIBarEndSeries[0] = (1);
						Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerS",true,(SSS_H ? "Z-S\nt" : "Z\nt"),0,TextAnchorS-SignalLabels.Size,0,ZLRColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
					else
					{
						wCCIBarEndSeries[0] = (-1);
						Draw.Text(this,CurrentBar.ToString()+"ZLRTriggerS",true,(SSS_L && !SSS_E ? "Z-S\nb" : "Z\nb"),0,TextAnchorS-SignalLabels.Size,0,ZLRColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				}	
			}
			else
			{
				RemoveDrawObject(CurrentBar.ToString() + "ZLRS");
				RemoveDrawObject(CurrentBar.ToString() + "ZLRSLine");
				RemoveDrawObject(CurrentBar.ToString() + "ZLRTriggerS");
			}
				
			#endregion
			
			#region Famir
			if(((FamirL_H  && pcbar != -1) || (Show_Opp_End && FamirL_L && pcbar != 1) || FamirL_E) && Show_Famir && WCCIDirection[0]!= 1)
				{
				wCCIPatternSeries[0] = (2);
				wCCIDirectionSeries[0] = (1);
				
				if(SoundAlert)
				{
					Alert(CurrentBar.ToString(), Priority.Medium, "Famir Long", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
				}
				
				if(FamirL_E)
					{
						wCCIBarEndSeries[0] = (0);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerL",true,"F\ne",0,TextAnchorL-SignalLabels.Size,0,FamirColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				else
				{
					if(FamirL_H)
					{
						wCCIBarEndSeries[0] = (1);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerL",true,"F\nt",0,TextAnchorL-SignalLabels.Size,0,FamirColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
					else if(Show_Opp_End)
					{
						wCCIBarEndSeries[0] = (-1);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerL",true,"F\nb",0,TextAnchorL-SignalLabels.Size,0,FamirColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				}
			}		
			else
			{
				RemoveDrawObject(CurrentBar.ToString() + "FamirTriggerL");
			}
			
			if(((Show_Opp_End && FamirS_H  && pcbar != -1) || (FamirS_L && pcbar != 1) || FamirS_E) && Show_Famir && WCCIDirection[0]!= -1)
			{
				wCCIPatternSeries[0] = (2);
				wCCIDirectionSeries[0] = (-1);
				if(SoundAlert)
				{
					Alert(CurrentBar.ToString(), Priority.Medium, "Famir Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
				}
				if(FamirS_E)
					{
						wCCIBarEndSeries[0] = (0);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerS",true,"F\ne",0,TextAnchorS-SignalLabels.Size,0,FamirColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				else
				{
					if(Show_Opp_End && FamirS_H)
					{
						wCCIBarEndSeries[0] = (1);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerS",true,"F\nt",0,TextAnchorS-SignalLabels.Size,0,FamirColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						wCCIBarEndSeries[0] = (-1);
						Draw.Text(this,CurrentBar.ToString()+"FamirTriggerS",true,"F\nb",0,TextAnchorS-SignalLabels.Size,0,FamirColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
					}
				}
			}		
			else
			{
				RemoveDrawObject(CurrentBar.ToString() + "FamirTriggerS");
			}
			#endregion
			
			#region Vegas
			//LONG
			if((((VegasL_H  && pcbar != -1) || (Show_Opp_End && VegasL_L && pcbar != 1) || VegasL_E)) && Show_Vegas && WCCIDirection[0]!= 1)
				{
					wCCIPatternSeries[0] = (3);
					wCCIDirectionSeries[0] = (1);
					Draw.Line(this,CurrentBar.ToString()+"VSH",true,barsAgoSwingH,swingHigh,0,swingHigh,VegasTrendColor,DashStyleHelper.Dash,TrendThickness);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "Vegas Long", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}

					if(VegasL_E)
						{
							wCCIBarEndSeries[0] = (0);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerL",true,"V\ne",0,TextAnchorL-SignalLabels.Size,0,VegasColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(VegasL_H)
						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerL",true,"V\nt",0,TextAnchorL-SignalLabels.Size,0,VegasColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else if(Show_Opp_End)
						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerL",true,"V\nb",0,TextAnchorL-SignalLabels.Size,0,VegasColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}		
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "VegasL");
					RemoveDrawObject(CurrentBar.ToString() + "VegasTriggerL");
					RemoveDrawObject(CurrentBar.ToString() + "VSH");
				}
			//SHORT	
				if((((Show_Opp_End && VegasS_H  && pcbar != -1) || (VegasS_L && pcbar != 1) || VegasS_E)) && Show_Vegas && WCCIDirection[0]!= -1)
				{
					wCCIPatternSeries[0] = (3);
					wCCIDirectionSeries[0] = (-1);
					Draw.Line(this,CurrentBar.ToString()+"VSL",true,barsAgoSwingL,swingLow,0,swingLow,VegasTrendColor,DashStyleHelper.Dash,TrendThickness);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "Vegas Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}

					if(VegasS_E)
						{
							wCCIBarEndSeries[0] = (0);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerS",true,"V\ne",0,TextAnchorS-SignalLabels.Size,0,VegasColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(Show_Opp_End && VegasS_H)
						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerS",true,"V\nt",0,TextAnchorS-SignalLabels.Size,0,VegasColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else
						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"VegasTriggerS",true,"V\nb",0,TextAnchorS-SignalLabels.Size,0,VegasColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}		
				else
					{
//						RemoveDrawObject(CurrentBar.ToString() + "VegasS");
						RemoveDrawObject(CurrentBar.ToString() + "VegasTriggerS");
						RemoveDrawObject(CurrentBar.ToString() + "VSL");
					}
			#endregion
			
			#region GB100
			//LONG
				if((((GBL_H  && pcbar != -1) || (Show_Opp_End && GBL_L && pcbar != 1) || GBL_E)) && Show_GB100 && WCCIDirection[0]!= 1)
				{
					wCCIPatternSeries[0] = (4);
					wCCIDirectionSeries[0] = (1);
					if(GBLineThick > 0)
						Draw.Line(this,CurrentBar.ToString() + "GBLLine",true, 0,  200,0,-200, Brushes.PaleGreen,DashStyleHelper.Dash,GBLineThick);
					else
						RemoveDrawObject(CurrentBar.ToString() + "GBLLine");
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "GB100 Long", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
					if(GBL_E)
						{
						wCCIBarEndSeries[0] = (0);
						Draw.Text(this,CurrentBar.ToString()+"GBTriggerL",true,"B\ne",0,TextAnchorL-SignalLabels.Size,0,GBColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(GBL_H)
						{
						wCCIBarEndSeries[0] = (1);
						Draw.Text(this,CurrentBar.ToString()+"GBTriggerL",true,"B\nt",0,TextAnchorL-SignalLabels.Size,0,GBColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else if(Show_Opp_End)
						{
						wCCIBarEndSeries[0] = (-1);
						Draw.Text(this,CurrentBar.ToString()+"GBTriggerL",true,"B\nb",0,TextAnchorL-SignalLabels.Size,0,GBColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "GBLLine");
					RemoveDrawObject(CurrentBar.ToString() + "GBTriggerL");
				}
			//SHORT
				if((((Show_Opp_End && GBS_H  && pcbar != -1) || (GBS_L && pcbar != 1) || GBS_E)) && Show_GB100 && WCCIDirection[0]!= -1)
				{
					wCCIPatternSeries[0] = (4);
					wCCIDirectionSeries[0] = (-1);
					if(GBLineThick > 0) 
						Draw.Line(this,CurrentBar.ToString() + "GBSLine",true, 0,  200,0,-200, Brushes.Pink,DashStyleHelper.Dash,GBLineThick);
					else
						RemoveDrawObject(CurrentBar.ToString() + "GBSLine");
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "GB100 Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
					if(GBS_E)
						{
							wCCIBarEndSeries[0] = (0);
							Draw.Text(this,CurrentBar.ToString()+"GBTriggerS",true,"B\ne",0,TextAnchorS-SignalLabels.Size,0,GBColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(Show_Opp_End && GBS_H)
						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"GBTriggerS",true,"B\nt",0,TextAnchorS-SignalLabels.Size,0,GBColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else
						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"GBTriggerS",true,"B\nb",0,TextAnchorS-SignalLabels.Size,0,GBColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}		
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "GBSLine");
					RemoveDrawObject(CurrentBar.ToString() + "GBTriggerS");
				}
			#endregion
			
			#region Tony
				if((((TonyL_H  && pcbar != -1) || (Show_Opp_End && TonyL_L && pcbar != 1) || TonyL_E)) && Show_Tony && WCCIDirection[0]!= 1)
				{
					wCCIPatternSeries[0] = (5);
					wCCIDirectionSeries[0] = (1);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "Tony Long", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
					if(TonyL_E)
						{
							wCCIBarEndSeries[0] = (0);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerL",true,"T\ne",0,TextAnchorL-SignalLabels.Size,0,TonyColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(TonyL_H)
						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerL",true,"T\nt",0,TextAnchorL-SignalLabels.Size,0,TonyColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else if(Show_Opp_End)
						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerL",true,"T\nb",0,TextAnchorL-SignalLabels.Size,0,TonyColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "TonyTriggerL");
				}
			
				if((((Show_Opp_End && TonyS_H  && pcbar != -1) || (TonyS_L && pcbar != 1) || TonyS_E)) && Show_Tony && WCCIDirection[0]!= -1)
				{
					wCCIPatternSeries[0] = (5);
					wCCIDirectionSeries[0] = (-1);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "Tony Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
					if(TonyS_E)
						{
							wCCIBarEndSeries[0] = (0);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerS",true,"T\ne",0,TextAnchorS-SignalLabels.Size,0,TonyColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					else
					{
						if(Show_Opp_End && TonyS_H)
						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerS",true,"T\nt",0,TextAnchorS-SignalLabels.Size,0,TonyColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
						else
						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"TonyTriggerS",true,"T\nb",0,TextAnchorS-SignalLabels.Size,0,TonyColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
						}
					}
				}		
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "TonyTriggerS");
				}
			#endregion
			
			#region Ghost
				if((((GhostL_H  && pcbar != -1) || (Show_Opp_End && GhostL_L && pcbar != 1) || GhostL_E)) && Show_Ghost && WCCIDirection[0]!= 1)
					{
						wCCIPatternSeries[0] = (6);
						wCCIDirectionSeries[0] = (1);
						if(ShowGhostPeaks)
						{
							Draw.Text(this,CurrentBar.ToString() + "GHOST1L","p1",gstLeftPkBars,-gstLeftPk-10,GhostColorL);
							Draw.Text(this,CurrentBar.ToString() + "GHOST2L","p2",gstMiddlePkBars,-gstMiddlePk-10,GhostColorL);
							Draw.Text(this,CurrentBar.ToString() + "GHOST3L","p3",gstRightPkBars,-gstRightPk-10,GhostColorL);
						}
						if(SoundAlert)
						{
							Alert(CurrentBar.ToString() + "Ghost", Priority.Medium, "GHOST Long", @AlertFile, AlertInterval, Brushes.DarkGreen, Brushes.White);
						}
						if(GhostL_E)
							{
								wCCIBarEndSeries[0] = (0);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerL",true,"G\ne",0,TextAnchorL-SignalLabels.Size,0,GhostColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendL",true, gstLeftLowBars,-gstLeftLow,0,-(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
						else
						{
							if(GhostL_H)
							{
								wCCIBarEndSeries[0] = (1);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerL",true,"G\nt",0,TextAnchorL-SignalLabels.Size,0,GhostColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendL",true, gstLeftLowBars,-gstLeftLow,0,-(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
							else if(Show_Opp_End)
							{
								wCCIBarEndSeries[0] = (-1);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerL",true,"G\nb",0,TextAnchorL-SignalLabels.Size,0,GhostColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendL",true, gstLeftLowBars,-gstLeftLow,0,-(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
						}
		
					}
					else
					{
						RemoveDrawObject(CurrentBar.ToString() + "GhostTriggerL");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST1L");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST2L");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST3L");
						RemoveDrawObject(CurrentBar.ToString() + "GTrendL");
						
					}
				//Ghosts Short
				if((((Show_Opp_End && GhostS_H  && pcbar != -1) || (GhostS_L && pcbar != 1) || GhostS_E)) && Show_Ghost && WCCIDirection[0]!= -1)
					{
						wCCIPatternSeries[0] = (6);
						wCCIDirectionSeries[0] = (-1);
						if(ShowGhostPeaks)
						{
							Draw.Text(this,CurrentBar.ToString() + "GHOST1S","p1",gstLeftPkBars,gstLeftPk+10,GhostColorS);
							Draw.Text(this,CurrentBar.ToString() + "GHOST2S","p2",gstMiddlePkBars,gstMiddlePk+10,GhostColorS);
							Draw.Text(this,CurrentBar.ToString() + "GHOST3S","p3",gstRightPkBars,gstRightPk+10,GhostColorS);
						}
						if(SoundAlert)
						{
							Alert(CurrentBar.ToString() + "Ghost", Priority.Medium, "GHOST Short", @AlertFile, AlertInterval, GhostColorL, Brushes.White);
						}
						if(GhostS_E)
							{
								wCCIBarEndSeries[0] = (0);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerS",true,"G\ne",0,TextAnchorS-SignalLabels.Size,0,GhostColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendS",true, gstLeftLowBars,gstLeftLow,0,(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
						else
						{
							if(Show_Opp_End && GhostS_H)
							{
								wCCIBarEndSeries[0] = (1);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerS",true,"G\nt",0,TextAnchorS-SignalLabels.Size,0,GhostColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendS",true, gstLeftLowBars,gstLeftLow,0,(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
							else
							{
								wCCIBarEndSeries[0] = (-1);
								Draw.Text(this,CurrentBar.ToString()+"GhostTriggerS",true,"G\nb",0,TextAnchorS-SignalLabels.Size,0,GhostColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
								if(ShowGhostTrendLine) Draw.Line(this,CurrentBar.ToString() + "GTrendS",true, gstLeftLowBars,gstLeftLow,0,(gstLeftLow+(gstLeftLowBars*gstChangePerBar)),GstTrendColor,DashStyleHelper.Dash,TrendThickness);
							}
						}
					}
					else
					{
						RemoveDrawObject(CurrentBar.ToString() + "GhostTriggerS");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST1S");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST2S");
						RemoveDrawObject(CurrentBar.ToString() + "GHOST3S");
						RemoveDrawObject(CurrentBar.ToString() + "GTrendS");
					}
			#endregion
			
			#region Slingshot
				if((((SSL_H  && pcbar != -1))))// || (Show_Opp_End && SSL_L && pcbar != 1) || SSL_E)) && Show_SS)
				{
					wCCIPatternSeries[0] = (7);
					wCCIDirectionSeries[0] = (1);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "SS Long", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
//					if(SSL_E)
//						{
//							wCCIBarEndSeries[0] = (0);
//							Draw.Text(this,CurrentBar.ToString()+"SSTriggerL",true,(ZLRL_E ? "Z-S\ne" : "S\ne"),0,TextAnchorL-SignalLabels.Size,0,SSColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//					else
//					{
//						if(SSL_H)
//						{
							wCCIBarEndSeries[0] = (1);
							Draw.Text(this,CurrentBar.ToString()+"SSTriggerL",true,(ZLRL_H ? "Z-S\nt" : "S\nt"),0,TextAnchorL-SignalLabels.Size,0,SSColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//						else if(Show_Opp_End)
//						{
//							wCCIBarEndSeries[0] = (-1);
//							Draw.Text(this,CurrentBar.ToString()+"SSTriggerL",true,(ZLRL_L && ! ZLRL_E ? "Z-S\nb" : "S\nb"),0,TextAnchorL-SignalLabels.Size,0,SSColorL,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//					}
				}
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "SSTriggerL");
				}
			
				if(/*(((Show_Opp_End && SSS_H  && pcbar != -1) || (*/SSS_L && pcbar != 1)// || SSS_E)) && Show_SS)
				{
					wCCIPatternSeries[0] = (7);
					wCCIDirectionSeries[0] = (-1);
					if(SoundAlert)
					{
						Alert(CurrentBar.ToString(), Priority.Medium, "SS Short", @AlertFile, AlertInterval, Brushes.Red, Brushes.White);
					}
//					if(SSS_E)
//						{
//							wCCIBarEndSeries[0] = (0);
//							Draw.Text(this,CurrentBar.ToString()+"SSTriggerS",true,(ZLRS_E ? "Z-S\ne" : "S\ne"),0,TextAnchorS-SignalLabels.Size,0,SSColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//					else
//					{
//						if(Show_Opp_End && SSS_H)
//						{
//							wCCIBarEndSeries[0] = (1);
//							Draw.Text(this,CurrentBar.ToString()+"SSTriggerS",true,(ZLRS_H && !ZLRL_E ? "Z-S\nt" : "S\nt"),0,TextAnchorS-SignalLabels.Size,0,SSColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//						else
//						{
							wCCIBarEndSeries[0] = (-1);
							Draw.Text(this,CurrentBar.ToString()+"SSTriggerS",true,(ZLRS_L && !ZLRS_E ? "Z-S\nb" : "S\nb"),0,TextAnchorS-SignalLabels.Size,0,SSColorS,SignalLabels,TextAlignment.Center,Brushes.Transparent,SigBackColor,SigBackOpacity);
//						}
//					}
				}		
				else
				{
					RemoveDrawObject(CurrentBar.ToString() + "SSTriggerS");
				}
			#endregion
				
			if(ShowOneTwenty == true)
			{	Draw.Line(this,"120",true,LineExtension,MaxSignalValue,0,MaxSignalValue,OneTwentyColor,DashStyleHelper.Dash,1);
				Draw.Line(this,"-120",true,LineExtension,-MaxSignalValue,0,-MaxSignalValue,OneTwentyColor,DashStyleHelper.Dash,1);	
			}
			if(ShowFamirLimits == true)
			{	Draw.Line(this,"flimits",true,LineExtension,FamirLimits,0,FamirLimits,FamirLimitsColor,DashStyleHelper.Dash,1);
				Draw.Line(this,"-flimits",true,LineExtension,-FamirLimits,0,-FamirLimits,FamirLimitsColor,DashStyleHelper.Dash,1);	
			}
			
			if(CCIShowForecasterArrows == true)
			{
			Draw.ArrowUp(this,"LowCCI",true, 0, CCIdown, CCIForecasterColorL);
			Draw.ArrowDown(this,"HighCCI",true, 0, CCIup, CCIForecasterColorH);
			}
			else if (CCIShowForecasterDots == true)
			{
			Draw.Dot(this,"LowCCI",false,0,CCIdown,CCIForecasterColorL);
			Draw.Dot(this,"HighCCI",false,0,CCIup,CCIForecasterColorH);
			}
			// Plot CCI Forecaster
//			Plots[0].Pen.Width = 2;
//			Plots[Math.Min(CurrentBar,1)].Pen.Width = 2;
//			PlotCCILow[0] = (CCIdown);
//			PlotCCIHigh[0] = (CCIup);
		}
		
		protected void DataExtraction()
		{
			if(State == State.Historical || !UseDataExtraction) return;

			StreamWriter sw;
		#region Trade Processing				
			//Get performance data for the pattern
			if(signal == true)
			{
					int j = CurrentBar-signalbar;
				
					//Did the close and direction agree?
					if((WCCIDirection[j] == 1 && Close[Math.Min(CurrentBar,1)]==High[Math.Min(CurrentBar,1)])
						|| (WCCIDirection[j] == -1 && Close[Math.Min(CurrentBar,1)]==Low[Math.Min(CurrentBar,1)]))
						hold = true;
					else
						hold = false;
					
					//Has a cross of the 100 occurred?
					if(Math.Abs(myWoodies[Math.Min(CurrentBar,2)])>100 && Math.Abs(myWoodies[Math.Min(CurrentBar,1)])<100  && j>1)
						cross_100 = true;
					else if(Math.Abs(myWoodies[Math.Min(CurrentBar,2)])<100 && Math.Abs(myWoodies[Math.Min(CurrentBar,1)])>100)//resets 100 X if cross occurred on a hold candle
						cross_100 = false;
					
////////////////   Check conditions   ///////////////////
				//Stopped out
					if(!breakeven && ((WCCIDirection[j] == 1 && Close[0]<=(entrypoint-(WCCICloseAt[j] != WCCIDirection[j] ? Opp_End_MAE_Ticks*TickSize :  MAE_Ticks*TickSize)))
						|| (WCCIDirection[j] == -1 && Close[0]>= (entrypoint + (WCCICloseAt[j] != WCCIDirection[j] ? Opp_End_MAE_Ticks*TickSize :  MAE_Ticks*TickSize))))
						)
					{
						try
						{
							sw = File.AppendText(@FilePath);
							if(WCCIDirection[j] == 1)
								sw.Write(","+Close[0]+","+MAX(High,j)[0]+",,");
							else
								sw.Write(","+Close[0]+","+MIN(Low,j)[0]+",,");
								
							sw.Flush();
							sw.Close();
						}
						catch (Exception e)
						{
							// Outputs the error to the log
							Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
							throw;
						}
						EndTrade("S");
					}
					
				//Move to BE
					if(!breakeven && ((WCCIDirection[j] == 1 && Close[0]>=(entrypoint+(BE_Ticks*TickSize)))
						|| (WCCIDirection[j] == -1 && Close[0]<=(entrypoint-(BE_Ticks*TickSize))))
						)
					{
						breakeven = true;
						BETime = Time[0];
					}
					
				//Move to BE 2
					if(!breakeven2 && ((WCCIDirection[j] == 1 && Close[0]>=(entrypoint+(BE_Ticks_2*TickSize)))
						|| (WCCIDirection[j] == -1 && Close[0]<=(entrypoint-(BE_Ticks_2*TickSize))))
						)
					{
						breakeven2 = true;
						BETime2 = Time[0];
					}
				//Target hit
						if(Target > 0 && ((WCCIDirection[j] == 1 && Close[0]>=entrypoint+Target*TickSize)
							|| (WCCIDirection[j] == -1 && Close[0]<=entrypoint-Target*TickSize)))
						{	
							try
							{
								sw = File.AppendText(@FilePath);
								if(momoexit == 0)
								{
									if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j))[0]+","+MAX(High,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+",");
									else
										sw.Write(","+MAX(High,(j))[0]+","+MIN(Low,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+",");
								}
								else
								{	if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j))[0]+","+MAX(High,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+","+momoexit);
									else
										sw.Write(","+MAX(High,(j))[0]+","+MIN(Low,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+","+momoexit);
								}
								
								sw.Flush();
								sw.Close();
							}
							catch (Exception e)
							{
								// Outputs the error to the log
								Log("You cannot read and write from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
								throw;
							}
							EndTrade("T");
						}
					
				//Momo change, a CCI hook against position w/candle close against position
					if(((WCCIDirection[j] == 1 && myWoodies[Math.Min(CurrentBar,1)]<myWoodies[Math.Min(CurrentBar,2)] && Close[Math.Min(CurrentBar,1)] == Low[Math.Min(CurrentBar,1)])
						|| (WCCIDirection[j] == -1 && myWoodies[Math.Min(CurrentBar,1)]>myWoodies[Math.Min(CurrentBar,2)] && Close[Math.Min(CurrentBar,1)] == High[Math.Min(CurrentBar,1)]))
						&& !stoppedout && !momochange)
					{
						momochange	= true;
						momoexit = Close[Math.Min(CurrentBar,1)]-WCCIDirection[j]*TickSize;
					}
////////////////////////////////////////////////////////
				
				//Moved to Brekeven
							if(breakeven2)
							{
								BEStop = entrypoint+(WCCIDirection[j]*BE_Offset_Ticks_2*TickSize);//New BE stop price
							}
							else if(breakeven)
								{
									BEStop = entrypoint+(WCCIDirection[j]*BE_Offset_Ticks*TickSize);//New BE stop price
								}
							
					//Returned to BE
						if(breakeven && ((WCCIDirection[j] == 1 && Close[0]<=BEStop)
							|| (WCCIDirection[j] == -1 && Close[0]>=BEStop))
						 )
						{
							try
							{
								sw = File.AppendText(@FilePath);
								if(momoexit == 0)
								{
									if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j))[0]+","+MAX(High,j)[0]+",,");
									else
										sw.Write(","+MAX(High,(j))[0]+","+MIN(Low,j)[0]+",,");
								}
								else
								{	if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j))[0]+","+MAX(High,j)[0]+",,"+momoexit);
									else
										sw.Write(","+MAX(High,(j))[0]+","+MIN(Low,j)[0]+",,"+momoexit);
								}
								sw.Flush();
								sw.Close();
							}
							catch (Exception e)
							{
								// Outputs the error to the log
								Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
								throw;
							}
							if(!breakeven2)
								EndTrade("B1");
							else
								EndTrade("B");
						}
			
					//100 line cross with close against position
						if(cross_100 && !hold)
						{
							try
							{
								sw = File.AppendText(@FilePath);
								if(momoexit == 0)
								{
									if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j-1))[Math.Min(CurrentBar,1)]+","+MAX(High,j)[Math.Min(CurrentBar,1)]+","+(Close[Math.Min(CurrentBar,1)]-WCCIDirection[j]*TickSize)+",");
									else
										sw.Write(","+MAX(High,(j-1))[Math.Min(CurrentBar,1)]+","+MIN(Low,j)[Math.Min(CurrentBar,1)]+","+(Close[Math.Min(CurrentBar,1)]-WCCIDirection[j]*TickSize)+",");
								}
								else
								{	if(WCCIDirection[j] == 1)
										sw.Write(","+MIN(Low,(j-1))[Math.Min(CurrentBar,1)]+","+MAX(High,j)[Math.Min(CurrentBar,1)]+","+(Close[Math.Min(CurrentBar,1)]-WCCIDirection[j]*TickSize)+","+momoexit);
									else
										sw.Write(","+MAX(High,(j-1))[Math.Min(CurrentBar,1)]+","+MIN(Low,j)[Math.Min(CurrentBar,1)]+","+(Close[Math.Min(CurrentBar,1)]-WCCIDirection[j]*TickSize)+","+momoexit);
								}
								
								sw.Flush();
								sw.Close();
							}
							catch (Exception e)
							{
								// Outputs the error to the log
								Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
								throw;
							}
							EndTrade("X");
						}

						//  Closing Time
					if(ToTime(Time[0]) > ToTime(Convert.ToDateTime(ClosingTime)))//Closing Time
					{
						try
						{
							sw = File.AppendText(@FilePath);
							if(momoexit == 0)
							{
								if(WCCIDirection[j] == 1)
									sw.Write(","+MIN(Low,j)[0]+","+MAX(High,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+",");
								else
									sw.Write(","+MAX(High,j)[0]+","+MIN(Low,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+",");
							}
							else
							{	if(WCCIDirection[j] == 1)
									sw.Write(","+MIN(Low,j)[0]+","+MAX(High,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+","+momoexit);
								else
									sw.Write(","+MAX(High,j)[0]+","+MIN(Low,j)[0]+","+(Close[0]-WCCIDirection[j]*TickSize)+","+momoexit);
							}
							
							sw.Flush();
							sw.Close();
						}
						catch (Exception e)
						{
							// Outputs the error to the log
							Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
							throw;
						}
						EndTrade("C");
					}
			}
		#endregion
			
		#region Trade Entry
			if(IsFirstTickOfBar && !Bars.IsFirstBarOfSession)
			{
/*				for(int i = 0; i <10000; i++)
				{if(i==5000) Print(i);}
*/				int k = CurrentBar-signalbar;

			//Stop and Reverse
				if(signalbar != 0 && WCCIPattern[Math.Min(CurrentBar,1)] != 0 && WCCIDirection[Math.Min(CurrentBar,1)] != WCCIDirection[k] && StopReverse && (WCCIDirection[k]==1 ? Close[Math.Min(CurrentBar,1)]<entrypoint : Close[Math.Min(CurrentBar,1)]>entrypoint))//Exit when a new opposing pattern exists with a losing position on the old pattern
				{
					try
					{
						sw = File.AppendText(@FilePath);
						if(momoexit == 0)
						{
							if(WCCIDirection[k] == 1)
								sw.Write(","+MIN(Low,k)[0]+","+MAX(High,k)[0]+",,");
							else
								sw.Write(","+MAX(High,k)[0]+","+MIN(Low,k)[0]+",,");
						}
						else
						{	if(WCCIDirection[k] == 1)
								sw.Write(","+MIN(Low,k)[0]+","+MAX(High,k)[0]+",,"+momoexit);
							else
								sw.Write(","+MAX(High,k)[0]+","+MIN(Low,k)[0]+",,"+momoexit);
						}
						
						sw.Flush();
						sw.Close();
					}
					catch (Exception e)
					{
						// Outputs the error to the log
						Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
						throw;
					}
					EndTrade("R");
				}
				
				
				Pattern = string.Empty;
				//What pattern do we have
				if(WCCIPattern[Math.Min(CurrentBar,1)] == 1)
				{
					Pattern = "ZLR";
				}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 2)
					{
						Pattern = "FAMIR";
					}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 3)
					{
						Pattern = "VEGAS";
					}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 4)
					{
						Pattern = "GB";
					}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 5)
					{
						Pattern = "TT";
					}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 6)
					{
						Pattern = "GHOST";
						
					}
				else
					if(WCCIPattern[Math.Min(CurrentBar,1)] == 7)
					{
						Pattern = "SS";
						
					}
//				Print(Instrument.FullName + ",  " +ToTime(Time[Math.Min(CurrentBar,1)])+" - "+WCCIPattern[Math.Min(CurrentBar,1)]+ " - "+Pattern+",  Dir = "+WCCIDirection[Math.Min(CurrentBar,1)]+",  BarEnd = "+WCCIBarEnd[Math.Min(CurrentBar,1)]+",  cci[Math.Min(CurrentBar,1)]: "+CCI(14)[Math.Min(CurrentBar,2)].ToString("f0")+",  cci: "+CCI(14)[Math.Min(CurrentBar,1)].ToString("f0"));
				//Write the Header Row	
				if(!File.Exists(@FilePath))
				{	
					try
					{
						sw = File.AppendText(@FilePath);
						sw.Write("Date,Time,Direction,Pattern,Signal Entry,T/B Bar,CCI,Delta CCI,MAE,MFE,100 Cross,Momo Change,Exit Mode,Exit Time,Time to BE,Time to BE2");
						sw.Close();
					}
					catch (Exception e)
					{
						// Outputs the error to the log
						Log("That file is already open for reading.", NinjaTrader.Cbi.LogLevel.Error);
						throw;
					}
				}
				
				//If a new pattern is found, print the initial pattern data
				if(Pattern != string.Empty && !signal && ToTime(Time[Math.Min(CurrentBar,1)]) < ToTime(Convert.ToDateTime(ClosingTime))
					)
				{
					signal		= true;
					signalbar	= CurrentBar-1;//A pattern has been found
					
					if (WCCIDirection[Math.Min(CurrentBar,1)] == 1)
						TDirection = "L";
					else if (WCCIDirection[Math.Min(CurrentBar,1)] == -1)
						TDirection = "S";
					
					sw 			= File.AppendText(@FilePath);
					
					if(Low[Math.Min(CurrentBar,1)] < Close[Math.Min(CurrentBar,1)])
					{	closePosition = 1;	}
					else
					{	closePosition = -1;	}
					
					if(CloseorOpen == "Close")
						entrypoint = Close[Math.Min(CurrentBar,1)];
					else
						entrypoint = Open[0];
					int conbars = MRO(delegate{return (TDirection == "L" ? Close[0] == Low[0] : Close[0]==High[0]);},2,CurrentBar) +1;//How many consecutive bars of same color
					
					try
					{
						sw.Write("\n"+Time[Math.Min(CurrentBar,1)].ToString("M-dd-yyy")+","+Time[Math.Min(CurrentBar,1)].ToString("HH:mm")+","+TDirection+","+Pattern+","/*+conbars+","*/+entrypoint+","+closePosition+","+myWoodies[Math.Min(CurrentBar,1)].ToString("f0")+","+Math.Abs((myWoodies[Math.Min(CurrentBar,1)]-myWoodies[Math.Min(CurrentBar,2)])).ToString("f0"));
						sw.Flush();
						sw.Close();
					}
					catch (Exception e)
					{
						// Outputs the error to the log
						Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
						throw;
					}
				}
			}
		#endregion
		}
		
		public void EndTrade(string Xit)
		{
				int m = CurrentBar-signalbar;
				TimeSpan TimeToBE = BETime - Time[m];
				TimeSpan TimeToBE2 = BETime2 - Time[m];
			try
			{
				sw = File.AppendText(@FilePath);
				sw.Write(","+Xit+"," +Time[0].ToString("HH:mm")+","+(BETime == SSTime ? "" : TimeToBE.ToString())+","+(BETime2 == SSTime ? "" : TimeToBE2.ToString()));
				sw.Flush();
				sw.Close();
			}
			catch (Exception e)
			{
				// Outputs the error to the log
				Log("You cannot write and read from the same file at the same time.", NinjaTrader.Cbi.LogLevel.Error);
				throw;
			}
			signal = false;
			TDirection = string.Empty;
			signalbar = 0;
			stoppedout	= false;
			breakeven	= false;
			breakeven2	= false;
			cross_100 = false;
			return_be = false;
			momochange = false;
			momoexit = 0;
			BETime = SSTime;
			BETime2 = SSTime;
		}
		
		protected double CheckWoodiesMAX(WoodiesCCI input, int period, int barsAgo)
		{
			if(period <= 0)
				return input[barsAgo];
			else
				return MAX(input, period)[barsAgo];
		}
		
		protected double CheckWoodiesMIN(WoodiesCCI input, int period, int barsAgo)
		{
			if(period <= 0)
				return input[barsAgo];
			else
				return MIN(input, period)[barsAgo];
		}
		
		#region SharpDX rendering for Warning message
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			if(ChartBars.FromIndex < HistoricalProcessingStart && ShowHistoricalWarning)
				DrawString("Historical Processing is limited by the Historical Bars property.", ChartControl.Properties.LabelFont, "MsgBrush", ChartPanel.X, ChartPanel.Y + ChartControl.Properties.LabelFont.Size*3, "MsgAreaBrush");
		}
		
		public override void OnRenderTargetChanged()
        {
            // Dispose and recreate our DX Brushes
            try
            {
				if (dxmBrushes == null)
					return;
                foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
                {
											
                    if (item.Value.DxBrush != null)
                        item.Value.DxBrush.Dispose();

                    if (RenderTarget != null && item.Value.MediaBrush != null && !RenderTarget.IsDisposed)
                        item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString(), LogLevel.Error);
            }
        }
		
		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY, string areaBrushName)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY).ToVector2();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX-2, (float)pointY-1, newW+5, newH+2);
            RenderTarget.FillRectangle(PLBoundRect, dxmBrushes[areaBrushName].DxBrush);
			
			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, dxmBrushes[brushName].DxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}
		
		[Browsable(false)]
        public class DXMediaMap
        {
            public SharpDX.Direct2D1.Brush DxBrush;
            public System.Windows.Media.Brush MediaBrush;
        }

        private void SetOpacity(string brushName)
        {
            if (dxmBrushes[brushName].MediaBrush == null)
                return;

            if (dxmBrushes[brushName].MediaBrush.IsFrozen)
                dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

            dxmBrushes[brushName].MediaBrush.Opacity = 100.0 / 100.0;
            dxmBrushes[brushName].MediaBrush.Freeze();
        }
		
		private void UpdateBrush(Brush mediaBrush, string brushName)
        {
            dxmBrushes[brushName].MediaBrush = mediaBrush;
            SetOpacity(brushName);
            if (dxmBrushes[brushName].DxBrush != null)
                dxmBrushes[brushName].DxBrush.Dispose();
            if (RenderTarget != null && !RenderTarget.IsDisposed)
            {
                dxmBrushes[brushName].DxBrush = dxmBrushes[brushName].MediaBrush.ToDxBrush(RenderTarget);
            }
                
        }
		#endregion

	#region Properties	
		
		#region General Properties
		[NinjaScriptProperty]
		[Display(Name="Sidewinder variable", Description="Sidewinder Variable.  Check with Woodies to determine the setting for bar types other than Range Bars.  It defaults to 60 for Range Bars.", Order=0, GroupName="Parameters")]
		public int SideWinder0
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="CCI Trend Level", Description="CCI must go below this value before a ZLR or Vegas can be signaled", Order=0, GroupName="Parameters")]
		public double RejectLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Trend Length for GB100", Description="Number of bars for a trend to be in place for GB100 trades to trigger", Order=0, GroupName="Parameters")]
		public int TrendLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="CCI Periods", Description="CCI Periods", Order=0, GroupName="Parameters")]
		public int Periods
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Ghost, Use Points/Percent Amount", Description="Ghost, Use Points/Percent Amount", Order=0, GroupName="Parameters")]
		public double UsePointsAmount
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Ghost, Use Points or Percent", Description="Type of ZigZag deviation (Points or Percent)", Order=0, GroupName="Parameters")]
		public string UsePoints
		{ get; set; }

		[NinjaScriptProperty]
		[Range(150, 350)]
		[Display(Name="Extremes CCI Level", Description="CCI must close above this value before before it is considered to have 'gone to extremes'.  This will also be the level at which a sideways market is reset.", Order=0, GroupName="Parameters")]
		public double Extremes
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 10)]
		[Display(Name="Famir, CCI Points for Hook", Description="Famir minimum CCI hook points (default = 3)", Order=0, GroupName="Parameters")]
		public double FamirHookMin
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Famir, Max CCI Level", Description="Famir CCI boundaries (default = 50)", Order=0, GroupName="Parameters")]
		public double FamirLimits
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Penetration level before swing high/low", Description="Level that the CCI must penetrate through to reach the swing high/low.", Order=0, GroupName="Parameters")]
		public double VTHFEPenetrationLevel
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Max Swing after swing high/low", Description="Max high or low for swing back to trend after swing high/low (default = 100).  NOT SWING HIGH/LOW.", Order=0, GroupName="Parameters")]
		public double VMaxSwingPoint
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 200)]
		[Display(Name="Vegas, Max Swing bars", Description="Max # of bars between swing high/low and trigger (default = 10).", Order=0, GroupName="Parameters")]
		public int VMaxSwingBars
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Range(0, 10)]
		[Display(Name="Ghost trendline/peak gap", Description="The number of CCI points that must be between trend break line and a ghost right peak", Order=0, GroupName="Parameters")]
		public double GstColor
		{ get; set; }			

		[NinjaScriptProperty]
		[Range(0, 75)]
		[Display(Name="ZLR, Minimum Point Change", Description="Minimum CCI point change before a ZLR can be signaled", Order=0, GroupName="Parameters")]
		public double MinZLRPts
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="Minimum CCI Point Change", Description="Minimum CCI point change for a Famir, Vegas, GB100 or Tony to trigger.", Order=0, GroupName="Parameters")]
		public double MinCCIPts
		{ get; set; }

		[NinjaScriptProperty]
		[Range(75, 200)]
		[Display(Name="Maximum Signal Level", Description="Maximum CCI value for a Pattern to be signaled", Order=0, GroupName="Parameters")]
		public double MaxSignalValue
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="ZLR, Minimum Level", Description="Minimum CCI value for a ZLR to be signaled", Order=0, GroupName="Parameters")]
		public double MinZLRValue
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Sideways mkt crosses", Description="Numbers of 0 line crosses needed to declare a sideways market", Order=0, GroupName="Parameters")]
		public int ChopCrosses
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Sideways mkt, Use Javier's alternate?", Description="Use Javier's alternate sideways market indicator?", Order=0, GroupName="Parameters")]
		public bool UseJavierSideways
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Sideways Market Boundary", Description="Boundary limits between which sideways market crosses must remain.", Order=0, GroupName="Parameters")]
		public int Sidewaysboundary
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="SS Zero Band", Description="Zero Line + or - Band", Order=0, GroupName="Parameters")]
		public int SSZeroBand
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, 100)]
		[Display(Name="SS 100 Band", Description="100 Line + or - Band", Order=0, GroupName="Parameters")]
		public int SS100Band
		{ get; set; }

		[NinjaScriptProperty]
		[Range(50, int.MaxValue)]
		[Display(Name="SS CCI/Turbo Separation", Description="The separation between the previous bar CCI and Turbo", Order=0, GroupName="Parameters")]
		public int SSCCITurboSeparation
		{ get; set; }

		[NinjaScriptProperty]
		[Range(75, 200)]
		[Display(Name="SS Maximum Turbo Level", Description="Maximum Turbo value for a Slingshot to be signaled", Order=0, GroupName="Parameters")]
		public double MaxSSTurboValue
		{ get; set; }
		#endregion
		
		#region FORECASTERS
		[Display(Name="CCI Forecaster, Show as Arrows?", Description="Show CCI Forecaster on chart as ARROWS?  This will take precedence over the dots.", Order=0, GroupName="Forecasters")]
		public bool CCIShowForecasterArrows
		{ get; set; }

		[Display(Name="CCI Forecaster, Show as Dots?", Description="Show CCI Forecaster on chart as DOTS?  Turn arrows off to see the dots.", Order=0, GroupName="Forecasters")]
		public bool CCIShowForecasterDots
		{ get; set; }

		[Display(Name="CCI Forecaster, Show History?", Description="Show CCI Forecaster historical dots on chart?", Order=0, GroupName="Forecasters")]
		public bool CCIShowForecasterHistory
		{ get; set; }

		[XmlIgnore]
		[Display(Name="CCI Forecaster, High Color", Description="High CCI Forecaster Color", Order=0, GroupName="Forecasters")]
		public Brush CCIForecasterColorH
		{ get; set; }

		[Browsable(false)]
		public string CCIForecasterColorHSerializable
		{
			get { return Serialize.BrushToString(CCIForecasterColorH); }
			set { CCIForecasterColorH = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="CCI Forecaster, Low Color", Description="Low CCI Forecaster Color", Order=0, GroupName="Forecasters")]
		public Brush CCIForecasterColorL
		{ get; set; }

		[Browsable(false)]
		public string CCIForecasterColorLSerializable
		{
			get { return Serialize.BrushToString(CCIForecasterColorL); }
			set { CCIForecasterColorL = Serialize.StringToBrush(value); }
		}			

		[Range(1, int.MaxValue)]
		[Display(Name="LSMA Forecaster Periods", Description="Numbers of bars used for LSMA calculations", Order=0, GroupName="Forecasters")]
		public int LSMAPeriods
		{ get; set; }

		[Range(1, 6)]
		[Display(Name="LSMA Forecaster Line Thickness", Description="LSMA Forecaster Line Thickness", Order=0, GroupName="Forecasters")]
		public int LSMALineThick
		{ get; set; }

		[XmlIgnore]
		[Display(Name="LSMA Forecaster, High Color", Description="High LSMA Forecaster Color", Order=0, GroupName="Forecasters")]
		public Brush LSMAForecasterColorH
		{ get; set; }

		[Browsable(false)]
		public string LSMAForecasterColorHSerializable
		{
			get { return Serialize.BrushToString(LSMAForecasterColorH); }
			set { LSMAForecasterColorH = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="LSMA Forecaster, Low Color", Description="Low LSMA Forecaster Color", Order=0, GroupName="Forecasters")]
		public Brush LSMAForecasterColorL
		{ get; set; }

		[Browsable(false)]
		public string LSMAForecasterColorLSerializable
		{
			get { return Serialize.BrushToString(LSMAForecasterColorL); }
			set { LSMAForecasterColorL = Serialize.StringToBrush(value); }
		}			

		[Display(Name="LSMA Forecaster, Show Forecaster?", Description="Display the LSMA Forecaster?", Order=0, GroupName="Forecasters")]
		public bool ShowLSMA_Fore
		{ get; set; }

		[Range(1, 6)]
		[Display(Name="Chop Zone Forecaster Line Thickness", Description="CZI Forecaster Line Thickness", Order=0, GroupName="Forecasters")]
		public int CZILineThick
		{ get; set; }

		[Display(Name="Chop Zone Forecaster, Show Forecaster?", Description="Display the CZI Forecaster?", Order=0, GroupName="Forecasters")]
		public bool ShowCZI_Fore
		{ get; set; }

		[Display(Name="Display Sidewinder Forecaster?", Description="Display Sidewinder Forecaster?", Order=0, GroupName="Forecasters")]
		public bool ShowSWForecaster
		{ get; set; }

		[Display(Name="Use Sidewinder Forecaster in ZLRs?", Description="Use Sidewinder Forecaster in ZLRs?", Order=0, GroupName="Forecasters")]
		public bool UseSWForecaster
		{ get; set; }
		#endregion
		
		#region SOUND AND DISPLAY
		[Range(1, int.MaxValue)]
		[Display(Name="Alert Interval (sec.)", Description="How often do you want the alert to sound?", Order=0, GroupName="Sound and Display")]
		public int AlertInterval
		{ get; set; }

		[Display(Name="Alert .wav File", Description="Where is the alert sound file located?  File name of .wav file to play. Provide either the absolute file path to any .wav file or just the name if the file is located in NinjaTrader Installation Folder sounds folder.  Create your own alert sounds, a different one for each contract traded, if you like.", Order=0, GroupName="Sound and Display")]
		public string AlertFileName
		{ get; set; }

		[Display(Name="Alert Active?", Description="Sound an alert when a CCI Pattern is signaled", Order=0, GroupName="Sound and Display")]
		public bool SoundAlert
		{ get; set; }

		[Display(Name="Momo Change Alert Active?", Description="Sound an alert when a Momentum Reversal is signaled", Order=0, GroupName="Sound and Display")]
		public bool SoundMomoAlert
		{ get; set; }

		[Display(Name="Draw Famir Limits", Description="Display the +/- Famir Limit lines?", Order=0, GroupName="Sound and Display")]
		public bool ShowFamirLimits
		{ get; set; }

		[Display(Name="Draw Max Signal Value", Description="Display the +/- 120 lines?", Order=0, GroupName="Sound and Display")]
		public bool ShowOneTwenty
		{ get; set; }

		[Display(Name="Ghost, Show Peaks?", Description="Display the +/- 120 lines?", Order=0, GroupName="Sound and Display")]
		public bool ShowGhostPeaks
		{ get; set; }

		[Display(Name="Ghost, Show Trend Line?", Description="Display the ghost trend line?", Order=0, GroupName="Sound and Display")]
		public bool ShowGhostTrendLine
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Trend Line Thickness", Description="Vegas and Ghost trend line thickness", Order=0, GroupName="Sound and Display")]
		public int TrendThickness
		{ get; set; }

		[Display(Name="Momo Change, Show?", Description="Display the Momentum Change Exit Points?", Order=0, GroupName="Sound and Display")]
		public bool ShowMomoChange
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="Draw Line Length", Description="Numbers of bars back to extend 120 and Famir limits lines", Order=0, GroupName="Sound and Display")]
		public int LineExtension
		{ get; set; }

		[Display(Name="Sideways mkt, show patterns?", Description="Show the patterns during a sideways market?", Order=0, GroupName="Sound and Display")]
		public bool ShowChopCrosses
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Sideways Color", Description="Color for Sideway Alert Rectangle", Order=0, GroupName="Sound and Display")]
		public Brush SidewaysColor
		{ get; set; }

		[Browsable(false)]
		public string SidewaysColorSerializable
		{
			get { return Serialize.BrushToString(SidewaysColor); }
			set { SidewaysColor = Serialize.StringToBrush(value); }
		}			

		[Range(1, 100)]
		[Display(Name="Sideways Color Transparency", Description="Sideways Market Rectangle Trabsparency", Order=0, GroupName="Sound and Display")]
		public int SidewaysTransparency
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Draw Famir Limits Color", Description="Color for Famir Limits", Order=0, GroupName="Sound and Display")]
		public Brush FamirLimitsColor
		{ get; set; }

		[Browsable(false)]
		public string FamirLimitsColorSerializable
		{
			get { return Serialize.BrushToString(FamirLimitsColor); }
			set { FamirLimitsColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Draw Max Signal Value Color", Description="Color for 120 lines", Order=0, GroupName="Sound and Display")]
		public Brush OneTwentyColor
		{ get; set; }

		[Browsable(false)]
		public string OneTwentyColorSerializable
		{
			get { return Serialize.BrushToString(OneTwentyColor); }
			set { OneTwentyColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Ghost Trend Color", Description="Color for ghost trend lines", Order=0, GroupName="Sound and Display")]
		public Brush GstTrendColor
		{ get; set; }

		[Browsable(false)]
		public string GstTrendColorSerializable
		{
			get { return Serialize.BrushToString(GstTrendColor); }
			set { GstTrendColor = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Vegas Trend Color", Description="Color for Vegas trend lines", Order=0, GroupName="Sound and Display")]
		public Brush VegasTrendColor
		{ get; set; }

		[Browsable(false)]
		public string VegasTrendColorSerializable
		{
			get { return Serialize.BrushToString(VegasTrendColor); }
			set { VegasTrendColor = Serialize.StringToBrush(value); }
		}			

		[Range(-400, 400)]
		[Display(Name="Text Anchor, Long", Description="Anchor point for Long trade text.", Order=0, GroupName="Sound and Display")]
		public int TextAnchorL
		{ get; set; }

		[Range(-400, 400)]
		[Display(Name="Text Anchor, Short", Description="Anchor point for Short trade text.", Order=0, GroupName="Sound and Display")]
		public int TextAnchorS
		{ get; set; }

		[Display(Name="Signal Font", Order=0, GroupName="Sound and Display")]
		public SimpleFont SignalLabels
		{ get; set; }
		
		#endregion
		
		#region DATA SERIES

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotCCIHigh
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotCCILow
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotTurboHigh
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PlotTurboLow
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		[Description("Woodies Pattern, ZLR, FAM, VT, GB, TT, GST")]
		public Series<int> WCCIPattern
		{
			get { Update();
			return wCCIPatternSeries; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		[Description("Direction of the trade (1 = Long, -1 = Short")]
		public Series<int> WCCIDirection
		{
			get { Update();
			return wCCIDirectionSeries; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		[Description("Location of close, Top (1) or Bottom (0) of bar")]
		public Series<int> WCCICloseAt
		{
			get { Update();
			return wCCICloseAtSeries; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		[Description("Location of signal trigger, Top (1), Bottom (-1), Either (0) end of bar")]
		public Series<int> WCCIBarEnd
		{
			get { Update();
			return wCCIBarEndSeries; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		[Description("Sideways Market, 1=Yes, 0=No")]
		public Series<bool> SidewaysMkt
		{
			get { Update();
			return sidewaysMktSeries; }
		}
		#endregion
		
		#region PATTERNS TO DISPLAY
		[XmlIgnore]
		[Display(Name="Signal Text Background Color", Description="Signal Text Background Color", Order=0, GroupName="Patterns to Display")]
		public Brush SigBackColor
		{ get; set; }

		[Browsable(false)]
		public string SigBackColorSerializable
		{
			get { return Serialize.BrushToString(SigBackColor); }
			set { SigBackColor = Serialize.StringToBrush(value); }
		}			

		[Range(1, 100)]
		[Display(Name="Signal Background Opacity", Description="Sets the Signal Text Background OPACITY, 0 = transparent, 100 = opaque.", Order=0, GroupName="Patterns to Display")]
		public int SigBackOpacity
		{ get; set; }

		[Display(Name="ZLR", Description="Display ZLR Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_ZLR
		{ get; set; }

		[Range(1, 6)]
		[Display(Name="ZLR Line Thickness", Description="ZLR Line Thickness. Set to 0 if no line.", Order=0, GroupName="Patterns to Display")]
		public int ZLRLineThick
		{ get; set; }

		[XmlIgnore]
		[Display(Name="ZLR, Long Text Color", Description="ZLR Long Text Color", Order=0, GroupName="Patterns to Display")]
		public Brush ZLRColorL
		{ get; set; }

		[Browsable(false)]
		public string ZLRColorLSerializable
		{
			get { return Serialize.BrushToString(ZLRColorL); }
			set { ZLRColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ZLR, Short Text Color", Description="ZLR Short Text Color", Order=0, GroupName="Patterns to Display")]
		public Brush ZLRColorS
		{ get; set; }

		[Browsable(false)]
		public string ZLRColorSSerializable
		{
			get { return Serialize.BrushToString(ZLRColorS); }
			set { ZLRColorS = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ZLR, Long Line Color", Description="ZLR Long Line Color", Order=0, GroupName="Patterns to Display")]
		public Brush ZLRLineColorL
		{ get; set; }

		[Browsable(false)]
		public string ZLRLineColorLSerializable
		{
			get { return Serialize.BrushToString(ZLRLineColorL); }
			set { ZLRLineColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="ZLR, Short Line Color", Description="ZLR Short Line Color", Order=0, GroupName="Patterns to Display")]
		public Brush ZLRLineColorS
		{ get; set; }

		[Browsable(false)]
		public string ZLRLineColorSSerializable
		{
			get { return Serialize.BrushToString(ZLRLineColorS); }
			set { ZLRLineColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Famir", Description="Display Famir Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_Famir
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Famir, Long Color", Description="Famir Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush FamirColorL
		{ get; set; }

		[Browsable(false)]
		public string FamirColorLSerializable
		{
			get { return Serialize.BrushToString(FamirColorL); }
			set { FamirColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Famir, Short Color", Description="Famir Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush FamirColorS
		{ get; set; }

		[Browsable(false)]
		public string FamirColorSSerializable
		{
			get { return Serialize.BrushToString(FamirColorS); }
			set { FamirColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Vegas", Description="Display Vegas Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_Vegas
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Vegas, Long Color", Description="Vegas Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush VegasColorL
		{ get; set; }

		[Browsable(false)]
		public string VegasColorLSerializable
		{
			get { return Serialize.BrushToString(VegasColorL); }
			set { VegasColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Vegas, Short Color", Description="Vegas Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush VegasColorS
		{ get; set; }

		[Browsable(false)]
		public string VegasColorSSerializable
		{
			get { return Serialize.BrushToString(VegasColorS); }
			set { VegasColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="GB 100", Description="Display GB100 Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_GB100
		{ get; set; }

		[Range(0, 6)]
		[Display(Name="GB 100 Line Thickness", Description="GB 100 Line Thickness. Set to 0 if no line.", Order=0, GroupName="Patterns to Display")]
		public int GBLineThick
		{ get; set; }

		[XmlIgnore]
		[Display(Name="GB 100, Long Color", Description="GB 100 Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush GBColorL
		{ get; set; }

		[Browsable(false)]
		public string GBColorLSerializable
		{
			get { return Serialize.BrushToString(GBColorL); }
			set { GBColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="GB 100, Short Color", Description="GB 100 Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush GBColorS
		{ get; set; }

		[Browsable(false)]
		public string GBColorSSerializable
		{
			get { return Serialize.BrushToString(GBColorS); }
			set { GBColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Tony", Description="Display Tony Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_Tony
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Tony, Long Color", Description="Tony Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush TonyColorL
		{ get; set; }

		[Browsable(false)]
		public string TonyColorLSerializable
		{
			get { return Serialize.BrushToString(TonyColorL); }
			set { TonyColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Tony, Short Color", Description="Tony Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush TonyColorS
		{ get; set; }

		[Browsable(false)]
		public string TonyColorSSerializable
		{
			get { return Serialize.BrushToString(TonyColorS); }
			set { TonyColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Ghost", Description="Display Ghost Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_Ghost
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Ghost, Long Color", Description="Ghost Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush GhostColorL
		{ get; set; }

		[Browsable(false)]
		public string GhostColorLSerializable
		{
			get { return Serialize.BrushToString(GhostColorL); }
			set { GhostColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Ghost, Short Color", Description="Ghost Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush GhostColorS
		{ get; set; }

		[Browsable(false)]
		public string GhostColorSSerializable
		{
			get { return Serialize.BrushToString(GhostColorS); }
			set { GhostColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Slingshot", Description="Display Slingshot Patterns?", Order=0, GroupName="Patterns to Display")]
		public bool Show_SS
		{ get; set; }

		[XmlIgnore]
		[Display(Name="Slingshot, Long Color", Description="Slingshot Long Color", Order=0, GroupName="Patterns to Display")]
		public Brush SSColorL
		{ get; set; }

		[Browsable(false)]
		public string SSColorLSerializable
		{
			get { return Serialize.BrushToString(SSColorL); }
			set { SSColorL = Serialize.StringToBrush(value); }
		}			

		[XmlIgnore]
		[Display(Name="Slingshot, Short Color", Description="Slingshot Short Color", Order=0, GroupName="Patterns to Display")]
		public Brush SSColorS
		{ get; set; }

		[Browsable(false)]
		public string SSColorSSerializable
		{
			get { return Serialize.BrushToString(SSColorS); }
			set { SSColorS = Serialize.StringToBrush(value); }
		}			

		[Display(Name="Opposite End Trades", Description="Display Patterns that appear on an opposite bar end close (Long signal at the low and Short signal at the high)?", Order=0, GroupName="Patterns to Display")]
		public bool Show_Opp_End
		{ get; set; }
		#endregion
		
		#region DATA EXTRACTION
		[Display(Name="Use Data Extraction?", Description="Use the data extraction utility?  This utility extracts performance data of all signals based on the following Data Extraction.", Order=0, GroupName="Data Extraction")]
		public bool UseDataExtraction
		{ get; set; }

		[Display(Name="Session End Time", Description="Time for trading to end and close all positions.", Order=0, GroupName="Data Extraction")]
		public string ClosingTime
		{ get; set; }

		[Range(0, 30)]
		[Display(Name="MAE_Ticks", Description="Number of ticks of MAE to trigger a crash stop", Order=0, GroupName="Data Extraction")]
		public int MAE_Ticks
		{ get; set; }

		[Range(0, 30)]
		[Display(Name="Ticks to Target", Description="Number of ticks to FINAL TARGET. Leave at 0 not to use a target.", Order=0, GroupName="Data Extraction")]
		public int Target
		{ get; set; }

		[Range(0, 30)]
		[Display(Name="Opp_End_MAE_Ticks", Description="Number of ticks of MAE to trigger a crash stop on opposite end triggered trades", Order=0, GroupName="Data Extraction")]
		public int Opp_End_MAE_Ticks
		{ get; set; }

		[Range(0, 30)]
		[Display(Name="BE_Ticks", Description="Number of ticks of MFE to trigger a move to break even", Order=0, GroupName="Data Extraction")]
		public int BE_Ticks
		{ get; set; }

		[Range(-10, 100)]
		[Display(Name="BE_Offset_Ticks", Description="Number of ticks above (+) or below (-) the entry price to set the break even stop", Order=0, GroupName="Data Extraction")]
		public int BE_Offset_Ticks
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="BE_Ticks_2", Description="Number of ticks of MFE to trigger 2nd move to break even", Order=0, GroupName="Data Extraction")]
		public int BE_Ticks_2
		{ get; set; }

		[Range(1, int.MaxValue)]
		[Display(Name="BE_Offset_Ticks_2", Description="Number of ticks above (+) or below (-) the entry price to set the break even stop", Order=0, GroupName="Data Extraction")]
		public int BE_Offset_Ticks_2
		{ get; set; }

		[Display(Name="CloseorOpen", Description="Do you want to log the signal bar Close or the next bar Open? (enter Close or Open)", Order=0, GroupName="Data Extraction")]
		public string CloseorOpen
		{ get; set; }

		[Display(Name="StopReverse", Description="Exit a losing trade and enter one in the opposite direction?.", Order=0, GroupName="Data Extraction")]
		public bool StopReverse
		{ get; set; }

		[Display(Name="FilePath", Description="Path to Data Export File.", Order=0, GroupName="Data Extraction")]
		public string FilePath
		{ get; set; }
		#endregion
		
		#region Historical Processing Options
		[Display(Name="Historical Bars To Process", Description="Limit number of historical bars to process", Order=0, GroupName="Historical Processing")]
		public int HistoricalBarsToProcess
		{ get; set; }
		
		[Display(Name="Show Historical Warning", Description="Warning message explaining that historical bars are not being processed", Order=1, GroupName="Historical Processing")]
		public bool ShowHistoricalWarning
		{ get; set; }
		
		[Display(Name="Show Historical Warning Region", Description="Warning region showing where historical processing is ignored", Order=2, GroupName="Historical Processing")]
		public bool ShowHistoricalWarningRegion
		{ get; set; }
		
		[Display(Name="Show Start Line", Description="Line showing where historical processing starts", Order=3, GroupName="Historical Processing")]
		public bool ShowHistoricalStartLine
		{ get; set; }
		#endregion
	#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CCIForecasterV7DE[] cacheCCIForecasterV7DE;
		public CCIForecasterV7DE CCIForecasterV7DE(int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			return CCIForecasterV7DE(Input, sideWinder0, rejectLevel, trendLength, periods, usePointsAmount, usePoints, extremes, famirHookMin, famirLimits, vTHFEPenetrationLevel, vMaxSwingPoint, vMaxSwingBars, gstColor, minZLRPts, minCCIPts, maxSignalValue, minZLRValue, chopCrosses, useJavierSideways, sidewaysboundary, sSZeroBand, sS100Band, sSCCITurboSeparation, maxSSTurboValue);
		}

		public CCIForecasterV7DE CCIForecasterV7DE(ISeries<double> input, int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			if (cacheCCIForecasterV7DE != null)
				for (int idx = 0; idx < cacheCCIForecasterV7DE.Length; idx++)
					if (cacheCCIForecasterV7DE[idx] != null && cacheCCIForecasterV7DE[idx].SideWinder0 == sideWinder0 && cacheCCIForecasterV7DE[idx].RejectLevel == rejectLevel && cacheCCIForecasterV7DE[idx].TrendLength == trendLength && cacheCCIForecasterV7DE[idx].Periods == periods && cacheCCIForecasterV7DE[idx].UsePointsAmount == usePointsAmount && cacheCCIForecasterV7DE[idx].UsePoints == usePoints && cacheCCIForecasterV7DE[idx].Extremes == extremes && cacheCCIForecasterV7DE[idx].FamirHookMin == famirHookMin && cacheCCIForecasterV7DE[idx].FamirLimits == famirLimits && cacheCCIForecasterV7DE[idx].VTHFEPenetrationLevel == vTHFEPenetrationLevel && cacheCCIForecasterV7DE[idx].VMaxSwingPoint == vMaxSwingPoint && cacheCCIForecasterV7DE[idx].VMaxSwingBars == vMaxSwingBars && cacheCCIForecasterV7DE[idx].GstColor == gstColor && cacheCCIForecasterV7DE[idx].MinZLRPts == minZLRPts && cacheCCIForecasterV7DE[idx].MinCCIPts == minCCIPts && cacheCCIForecasterV7DE[idx].MaxSignalValue == maxSignalValue && cacheCCIForecasterV7DE[idx].MinZLRValue == minZLRValue && cacheCCIForecasterV7DE[idx].ChopCrosses == chopCrosses && cacheCCIForecasterV7DE[idx].UseJavierSideways == useJavierSideways && cacheCCIForecasterV7DE[idx].Sidewaysboundary == sidewaysboundary && cacheCCIForecasterV7DE[idx].SSZeroBand == sSZeroBand && cacheCCIForecasterV7DE[idx].SS100Band == sS100Band && cacheCCIForecasterV7DE[idx].SSCCITurboSeparation == sSCCITurboSeparation && cacheCCIForecasterV7DE[idx].MaxSSTurboValue == maxSSTurboValue && cacheCCIForecasterV7DE[idx].EqualsInput(input))
						return cacheCCIForecasterV7DE[idx];
			return CacheIndicator<CCIForecasterV7DE>(new CCIForecasterV7DE(){ SideWinder0 = sideWinder0, RejectLevel = rejectLevel, TrendLength = trendLength, Periods = periods, UsePointsAmount = usePointsAmount, UsePoints = usePoints, Extremes = extremes, FamirHookMin = famirHookMin, FamirLimits = famirLimits, VTHFEPenetrationLevel = vTHFEPenetrationLevel, VMaxSwingPoint = vMaxSwingPoint, VMaxSwingBars = vMaxSwingBars, GstColor = gstColor, MinZLRPts = minZLRPts, MinCCIPts = minCCIPts, MaxSignalValue = maxSignalValue, MinZLRValue = minZLRValue, ChopCrosses = chopCrosses, UseJavierSideways = useJavierSideways, Sidewaysboundary = sidewaysboundary, SSZeroBand = sSZeroBand, SS100Band = sS100Band, SSCCITurboSeparation = sSCCITurboSeparation, MaxSSTurboValue = maxSSTurboValue }, input, ref cacheCCIForecasterV7DE);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCIForecasterV7DE CCIForecasterV7DE(int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			return indicator.CCIForecasterV7DE(Input, sideWinder0, rejectLevel, trendLength, periods, usePointsAmount, usePoints, extremes, famirHookMin, famirLimits, vTHFEPenetrationLevel, vMaxSwingPoint, vMaxSwingBars, gstColor, minZLRPts, minCCIPts, maxSignalValue, minZLRValue, chopCrosses, useJavierSideways, sidewaysboundary, sSZeroBand, sS100Band, sSCCITurboSeparation, maxSSTurboValue);
		}

		public Indicators.CCIForecasterV7DE CCIForecasterV7DE(ISeries<double> input , int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			return indicator.CCIForecasterV7DE(input, sideWinder0, rejectLevel, trendLength, periods, usePointsAmount, usePoints, extremes, famirHookMin, famirLimits, vTHFEPenetrationLevel, vMaxSwingPoint, vMaxSwingBars, gstColor, minZLRPts, minCCIPts, maxSignalValue, minZLRValue, chopCrosses, useJavierSideways, sidewaysboundary, sSZeroBand, sS100Band, sSCCITurboSeparation, maxSSTurboValue);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCIForecasterV7DE CCIForecasterV7DE(int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			return indicator.CCIForecasterV7DE(Input, sideWinder0, rejectLevel, trendLength, periods, usePointsAmount, usePoints, extremes, famirHookMin, famirLimits, vTHFEPenetrationLevel, vMaxSwingPoint, vMaxSwingBars, gstColor, minZLRPts, minCCIPts, maxSignalValue, minZLRValue, chopCrosses, useJavierSideways, sidewaysboundary, sSZeroBand, sS100Band, sSCCITurboSeparation, maxSSTurboValue);
		}

		public Indicators.CCIForecasterV7DE CCIForecasterV7DE(ISeries<double> input , int sideWinder0, double rejectLevel, int trendLength, int periods, double usePointsAmount, string usePoints, double extremes, double famirHookMin, double famirLimits, double vTHFEPenetrationLevel, double vMaxSwingPoint, int vMaxSwingBars, double gstColor, double minZLRPts, double minCCIPts, double maxSignalValue, double minZLRValue, int chopCrosses, bool useJavierSideways, int sidewaysboundary, int sSZeroBand, int sS100Band, int sSCCITurboSeparation, double maxSSTurboValue)
		{
			return indicator.CCIForecasterV7DE(input, sideWinder0, rejectLevel, trendLength, periods, usePointsAmount, usePoints, extremes, famirHookMin, famirLimits, vTHFEPenetrationLevel, vMaxSwingPoint, vMaxSwingBars, gstColor, minZLRPts, minCCIPts, maxSignalValue, minZLRValue, chopCrosses, useJavierSideways, sidewaysboundary, sSZeroBand, sS100Band, sSCCITurboSeparation, maxSSTurboValue);
		}
	}
}

#endregion
