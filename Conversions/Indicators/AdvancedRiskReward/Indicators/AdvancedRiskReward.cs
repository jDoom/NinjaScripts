#region Using declarations
using System;
using System.IO;
using System.Reflection;
using System.Globalization;
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

using System.Windows.Automation;
using System.Windows.Automation.Provider;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AdvancedRiskReward : Indicator
	{
		#region MenuItem Variables Clean me up!
		private System.Windows.Media.SolidColorBrush		activeBackgroundDarkGray;
		private System.Windows.Media.SolidColorBrush		backGroundMediumGray;
		private System.Windows.Controls.Grid				chartGrid;
		private NinjaTrader.Gui.Chart.ChartTab				chartTab;
		private NinjaTrader.Gui.Chart.ChartTrader			chartTraderControl;
		private NinjaTrader.Gui.Chart.Chart					chartWindow;
		private System.Windows.Media.SolidColorBrush		controlLightGray;
		private bool										panelActive;
		private int											tabControlStartColumn;
		private int											tabControlStartRow;
		private System.Windows.Controls.TabItem				tabItem;
		private System.Windows.Media.SolidColorBrush		textColor;
		private System.Windows.Controls.Menu				topMenu;

		private System.Windows.Controls.MenuItem			buttonReset, buttonSaveChanges, buttonLongShort, buttonAttachDetach, nullFocusItem;
		private System.Windows.Controls.Primitives.ToggleButton buttonShowHide;
		private bool buttonShowHideChecked = false;
		#endregion
		
		
		private Interactive interactive  = Interactive.Yes;
        private TargetLines targetLines = TargetLines.One;
        private List<double> HLCollection;
		private HorizontalLine 	EnterHLine,TargetHLine,StopHLine,TargetHLine2,TargetHLine3=null;
		private Ray 			EnterRLine,TargetRLine,StopRLine,TargetRLine2,TargetRLine3=null;
		private double 			EnterRLineStartSlot,TargetRLineStartSlot,StopRLineStartSlot,TargetRLine2StartSlot,TargetRLine3StartSlot=0;
		private double 			EnterRLineEndSlot,TargetRLineEndSlot,StopRLineEndSlot,TargetRLine2EndSlot,TargetRLine3EndSlot=0;
		private Text 			StopDot,TargetDot;
		
		private string targets;
		private bool HLineDrawn = false;
		private bool buttonsloaded,RetrievalDone,Short,attached,InteractiveRemoved,_init = false;
		private bool CalcBasedOnUser = true;
		private bool ray;

        private int RaysX;
        private int RayBarsAgo;
        private int userscontracts;

        private	double	myChartMax,myChartMin,CloseFromUpperLine,CloseFromLowerLine,RangeValue,PL,RatioValuestarty,
							endy,Y,EnterLineY,TargetLineY,TargetLineY2,TargetLineY3,StopLineY,PotentialProfit,
							PotentialProfit2,PotentialProfit3,PotentialLoss,RatioValue,RatioValue2,RatioValue3=0;
        #region Brushes   
        private Dictionary<string, DXMediaMap> dxmBrushes;
        private SharpDX.Direct2D1.RenderTarget myRenderTarget = null;
        private SimpleFont textFont;
        private Brush RatioTextColor
        {
            get { return dxmBrushes["RatioTextColor"].MediaBrush; }
            set { UpdateBrush(value, "RatioTextColor"); }
        }
        private Brush Ratio2TextColor
        {
            get { return dxmBrushes["Ratio2TextColor"].MediaBrush; }
            set { UpdateBrush(value, "Ratio2TextColor"); }
        }
		private Brush Ratio3TextColor
        {
            get { return dxmBrushes["Ratio3TextColor"].MediaBrush; }
            set { UpdateBrush(value, "Ratio3TextColor"); }
        }     
        private Brush TargetLinetxtColor
        {
            get { return dxmBrushes["TargetLinetxtColor"].MediaBrush; }
            set { UpdateBrush(value, "TargetLinetxtColor"); }
        }
        private Brush StopLinetxtColor
        {
            get { return dxmBrushes["StopLinetxtColor"].MediaBrush; }
            set { UpdateBrush(value, "StopLinetxtColor"); }
        }
        private Brush EnterToTargetColor
        {
            get { return dxmBrushes["EnterToTargetColor"].MediaBrush; }
            set { UpdateBrush(value, "EnterToTargetColor"); }
        }
        private Brush EnterToStopColor
        {
            get { return dxmBrushes["EnterToStopColor"].MediaBrush; }
            set { UpdateBrush(value, "EnterToStopColor"); }
        }
        private Brush UpSplitTagColor
        {
            get { return dxmBrushes["UpSplitTagColor"].MediaBrush; }
            set { UpdateBrush(value, "UpSplitTagColor"); }
        }
        private Brush LowerSplitTagColor
        {
            get { return dxmBrushes["LowerSplitTagColor"].MediaBrush; }
            set { UpdateBrush(value, "LowerSplitTagColor"); }
        }
        private Brush InteractivePLColor
        {
            get { return dxmBrushes["InteractivePLColor"].MediaBrush; }
            set { UpdateBrush(value, "InteractivePLColor"); }
        }
        private Brush InteractivePriceColor
        {
            get { return dxmBrushes["InteractivePriceColor"].MediaBrush; }
            set { UpdateBrush(value, "InteractivePriceColor"); }
        }
        private Brush TargetBeamColor
        {
            get { return dxmBrushes["TargetBeamColor"].MediaBrush; }
            set { UpdateBrush(value, "TargetBeamColor"); }
        }
        private Brush EnterBeamColor
        {
            get { return dxmBrushes["EnterBeamColor"].MediaBrush; }
            set { UpdateBrush(value, "EnterBeamColor"); }
        }
        private Brush StopBeamColor
        {
            get { return dxmBrushes["StopBeamColor"].MediaBrush; }
            set { UpdateBrush(value, "StopBeamColor"); }
        }
        private Brush Target2BeamColor
        {
            get { return dxmBrushes["Target2BeamColor"].MediaBrush; }
            set { UpdateBrush(value, "Target2BeamColor"); }
        }
        private Brush Target3BeamColor
        {
            get { return dxmBrushes["Target3BeamColor"].MediaBrush; }
            set { UpdateBrush(value, "Target3BeamColor"); }
        }
        private Brush ArrowUpColor
        {
            get { return dxmBrushes["ArrowUpColor"].MediaBrush; }
            set { UpdateBrush(value, "ArrowUpColor"); }
        }
        private Brush ArrowDownColor
        {
            get { return dxmBrushes["ArrowDownColor"].MediaBrush; }
            set { UpdateBrush(value, "ArrowDownColor"); }
        }
        private Brush dxmPriceColor
        {
            get { return dxmBrushes["dxmPriceColor"].MediaBrush; }
            set { UpdateBrush(value, "dxmPriceColor"); }
        }
        #endregion

        private Brush PriceColor;
        private Brush GreenColor;
        private Brush RedColor;
        private Brush BeamColor;
        
        private int EnterYPixles;
		private	int TargeYPixles;
		private	int Targe2YPixles;
		private	int Targe3YPixles;
		private	int StopYPixles;
		
		//Mouse
		private int mouseX,LockX,PLShiftX;
		private int  	mouseY = 0; 

        private bool isIndicatorAdded;
        private double currentClose;
		private bool AlertOnce;

        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Advanced Risk Reward by Bassam Alshariti";
				Name								= "AdvancedRiskReward";
				Calculate							= Calculate.OnEachTick;
				IsOverlay							= true;
				DisplayInDataBox					= false;
				IsSuspendedWhileInactive			= true;
				
				PriceColor					        = Brushes.Blue;
				RedColor						    = Brushes.Red;
				BeamColor							= Brushes.OrangeRed;
				GreenColor						    = Brushes.Green;
				ray									= true;
				ShowOnStartup						= true;
				Contracts							= 1;
				RiskPercent							= 10;
				AccountSize							= 10000;
				buttonShowHideChecked				= true;
				RayBarsAgo							= 0;
				LinesWidth							= 1;
                userscontracts                      = 1;
				AlertOnce							= true;

                textFont 							= new NinjaTrader.Gui.Tools.SimpleFont("Arial", 12);
				
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "RatioTextColor", "Ratio2TextColor", "Ratio3TextColor", "TargetLinetxtColor", "StopLinetxtColor",
                    "EnterToTargetColor", "EnterToStopColor", "UpSplitTagColor", "LowerSplitTagColor", "InteractivePLColor", "InteractivePriceColor",
                    "TargetBeamColor", "EnterBeamColor", "StopBeamColor", "Target2BeamColor", "Target3BeamColor", "ArrowUpColor", "ArrowDownColor", "dxmPriceColor"} )

                dxmBrushes.Add(brushName, new DXMediaMap());
				
				RatioTextColor 			= Brushes.Transparent;
				Ratio2TextColor 		= Brushes.Transparent; 
				Ratio3TextColor 		= Brushes.Transparent;
				TargetLinetxtColor 		= Brushes.Transparent;
				StopLinetxtColor 		= Brushes.Transparent;
				EnterToTargetColor 		= Brushes.Transparent;
				EnterToStopColor 		= Brushes.Transparent;
				UpSplitTagColor 		= Brushes.Transparent;
				LowerSplitTagColor 		= Brushes.Transparent;
				InteractivePLColor 		= Brushes.Transparent;
				InteractivePriceColor 	= Brushes.Transparent;
				TargetBeamColor 		= Brushes.Transparent;
				EnterBeamColor 			= Brushes.Transparent;
				StopBeamColor 			= Brushes.Transparent;
				Target2BeamColor 		= Brushes.Transparent;
				Target3BeamColor 		= Brushes.Transparent;
				ArrowUpColor 			= Brushes.Transparent;
				ArrowDownColor 			= Brushes.Transparent;
				dxmPriceColor 			= Brushes.Transparent;
				
            }
			else if (State == State.DataLoaded)
			{				
                HLCollection = new List<double>();

                activeBackgroundDarkGray			= new System.Windows.Media.SolidColorBrush(Color.FromRgb(30, 30, 30));
				activeBackgroundDarkGray.Freeze();
				backGroundMediumGray				= new System.Windows.Media.SolidColorBrush(Color.FromRgb(45, 45, 47));
				backGroundMediumGray.Freeze();
				controlLightGray					= new System.Windows.Media.SolidColorBrush(Color.FromRgb(64, 63, 69));
				controlLightGray.Freeze();
				textColor							= new System.Windows.Media.SolidColorBrush(Color.FromRgb(204, 204, 204));
				textColor.Freeze();
				
				targets =( targetLines==TargetLines.One?"-One":"-Three");				
			}
			else if (State == State.Historical)
			{	
				// Use an Automation ID to limit multiple instances of this indicator.
				if (ChartControl != null && !isIndicatorAdded)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						chartWindow = Window.GetWindow(ChartControl.Parent) as Chart;
						if (chartWindow == null) return;
						
						chartGrid	= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;
						foreach (DependencyObject item in chartGrid.Children)
						{
							if (AutomationProperties.GetAutomationId(item) == "AdvancedRiskRewardToolbar")
							{
								isIndicatorAdded = true;
							}
						}

						if (!isIndicatorAdded)
						{
							CreateWPFControls();
							AutomationProperties.SetAutomationId(topMenu, "AdvancedRiskRewardToolbar");
							
							// Begin AdvancedRiskReward OnStartUp
							if (!_init)
							{
								this.ClearOutputWindow();
								ChartPanel.MouseUp += new MouseButtonEventHandler(MyMouseUpEvent);
								ChartPanel.MouseDown += new MouseButtonEventHandler(MyMouseDownEvent);
								_init = true;
							}
							
							try
							{
								Short= (/*(GetValue(0,targets)==0 ||*/(GetValue(1,targets)==1/* || GetValue(0,targets)==-1)*/? true:false));// if lines are 0 or second number in the file is 0 or the file does not exist it is long
							}
							catch (Exception exp)
							{
			                    //Log(exp.ToString(), LogLevel.Error); // This is expected to receive an error on Reset.
			                }
							switch (interactive)
							{
								case Interactive.Yes:
									CalcBasedOnUser=true;
									TargetHLine2 = TargetHLine3 = null;
									TargetRLine2 = TargetRLine3 = null;
									break;
								case Interactive.No:
									CalcBasedOnUser=false;
									break;
							}
							
						}
					}));
				}
			}
			else if (State == State.Realtime)
			{
				// Refresh lines on reload
				RefreshLines();
			}
			else if (State == State.Terminated)
			{
				if (ChartControl != null)
				{
					ChartControl.Dispatcher.InvokeAsync((Action)(() =>
					{
						DisposeWPFControls();
					}));
					
					ChartPanel.MouseUp 		-= MyMouseUpEvent;
					ChartPanel.MouseDown	-= MyMouseDownEvent;
				}
				
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();
			}
		}
		
		#region Save,Read values
		double GetValue(int index,string targets) // gets the vlaue of the array at sepcific index (first or second ...) of a specific file 
		{   
			string fileName= Bars.Instrument.MasterInstrument.Name;
			bool fileisthere=false; 	
			string filepath = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"),"my documents").ToString()+@"\NinjaTrader 8\bin\Custom\Ranger\"+Bars.Instrument.MasterInstrument.Name+targets+".txt";
            if (File.Exists(filepath))// if the file exists then read it 
            {
               	if((new FileInfo(filepath).Length != 0))// if the files is not empty 
				{	
					while (System.IO.File.ReadAllLines(filepath)[0].Split(';')[index]!= null)
					{	
						return (double.Parse(System.IO.File.ReadAllLines(filepath)[0].Split(';')[index]));// -2 means Item not found
					}
				}
			}
			return -1 ;// return -1 if does not exists 
		}
			
		void WriteValue(bool Reset,string targets)// pas the file name (instrument name) and length of the final array 
		{
			string fileName= Bars.Instrument.MasterInstrument.Name;
			string folderpath = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"),@"my documents").ToString()+@"\NinjaTrader 8\bin\Custom\Ranger\";
			string filepath = Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"),"my documents").ToString()+@"\NinjaTrader 8\bin\Custom\Ranger\"+fileName+targets+".txt";
			
			bool IsExists = System.IO.Directory.Exists(folderpath); // if folder exists 
			
			if(!IsExists)// if folder exists  = false then create new one 
    		System.IO.Directory.CreateDirectory(folderpath);
			try
            {
			    using (System.IO.StreamWriter outfile = new System.IO.StreamWriter(filepath) ) // write the array to new file
			    {
				    // first number is for lines quantity, second number is for short(1) or long (0)
				    if (!Reset)
				    {
					    outfile.Write("{0};",HLCollection.Count);//first number : lines quantity 
					    outfile.Write("{0};",(Short? 1 : 0));//second number : short or long 
                        for (int col = 0; col < HLCollection.Count; col++)
					    {
						    outfile.Write("{0};",HLCollection[col]);// we write the array from memory to file, replace the index by value position in the file
					    }
                    }
				    else if (Reset)
				    {
                        outfile.Write("{0};",0);
				    }
                    outfile.Close();
			    }
			}
			catch(Exception e)
			{
                Log(e.ToString(), LogLevel.Error);
            }
			
		}
		#endregion
		
		#region Format Price maker
		public override string FormatPriceMarker(double price)
		{
			if(Instrument.MasterInstrument.PointValue > 2000 && Instrument.MasterInstrument.PointValue <300000)
			{	
				return price.ToString("N4");
			}
			else if(Instrument.MasterInstrument.PointValue > 300000)
			{
				return price.ToString("N6");
			}
			else 
			{
				return price.ToString("N2");
			}
		}
		#endregion
		
		#region Creat,Collect,Handle Lines
		private void CreateLines(double LinesY,bool retrievemode)
		{
			bool noLineCreated;
			bool enterLineCreated;
			bool targetEnterCreated;
			bool targStoEntCreated;
			bool targetstopentercreated; 
			bool target2created; 
						
			if(ray)
			{
				noLineCreated = ((EnterRLine == null && TargetRLine == null && StopRLine == null && TargetRLine2 == null && TargetRLine3 == null ));
				enterLineCreated = ((EnterRLine !=null && TargetRLine == null && StopRLine == null && TargetRLine2 == null && TargetRLine3 == null));
				targetEnterCreated = ((EnterRLine !=null && TargetRLine != null && StopRLine == null && TargetRLine2 == null && TargetRLine3 == null));
				targStoEntCreated = (targetEnterCreated && StopRLine !=null) ; 
				targetstopentercreated = (TargetRLine2 == null && TargetRLine3 == null);
				target2created =(TargetRLine2 != null && TargetRLine3 == null);
			}
			else
			{
				noLineCreated = (EnterHLine == null && TargetHLine == null && StopHLine == null && TargetHLine2 == null && TargetHLine3 == null );
				enterLineCreated = (EnterHLine !=null && TargetHLine == null && StopHLine == null && TargetHLine2 == null && TargetHLine3 == null);
				targetEnterCreated = (EnterHLine) !=null && TargetHLine != null && StopHLine == null && TargetHLine2 == null && TargetHLine3 == null;
				targStoEntCreated = (targetEnterCreated && StopHLine !=null) ; 
				
				targetstopentercreated = (TargetHLine2 == null && TargetHLine3 == null);
				target2created =(TargetHLine2 != null && TargetHLine3 == null);
			}
			if( noLineCreated )// creat enter 
			{	
				if(ray)
				{	
					EnterRLine = Draw.Ray(this,"EnterRLine",RayBarsAgo,LinesY,RayBarsAgo-1,LinesY,PriceColor);
					EnterRLine.Stroke = new Stroke(PriceColor,LinesWidth);
					EnterRLine.IsLocked=false;
				}
				else
				{
					EnterHLine = Draw.HorizontalLine(this,"EnterHLine",LinesY,PriceColor);
					EnterHLine.Stroke = new Stroke(PriceColor,LinesWidth);
					EnterHLine.IsLocked=false;
				}
				StopDot = Draw.Text(this,"StopDot",true,"EnterDot",0,LinesY,-10,Brushes.Transparent,new SimpleFont("Arial",1),TextAlignment.Left,Brushes.Transparent,Brushes.Transparent,0); // dots so that the horizontal lines still be visible when changing from daily to mins etc...
			}	
			else if(enterLineCreated)//create target 
			{
				if(ray)
				{
					TargetRLine = Draw.Ray(this,"TargetRLine",RayBarsAgo,LinesY,RayBarsAgo-1,LinesY,PriceColor);
					TargetRLine.Stroke = new Stroke(PriceColor,LinesWidth);
					TargetRLine.IsLocked=false;
				}
				else
				{
					TargetHLine = Draw.HorizontalLine(this,"TargetHLine",LinesY, PriceColor);
					TargetHLine.Stroke = new Stroke(PriceColor,LinesWidth);
					TargetHLine.IsLocked=false;
				}				
			}
			else if(targetEnterCreated) // create stop 
			{	
				if(ray)
				{
					StopRLine = Draw.Ray(this,"StopRLine",RayBarsAgo,LinesY,RayBarsAgo-1,LinesY,PriceColor);
					StopRLine.Stroke = new Stroke(PriceColor,LinesWidth);
					StopRLine.IsLocked=false;
				}
				else
				{
					StopHLine = Draw.HorizontalLine(this,"StopHLine",LinesY, PriceColor);
					StopHLine.Stroke = new Stroke(PriceColor,LinesWidth);
					StopHLine.IsLocked=false;	
				}
				
				
				if(targetLines == TargetLines.One)
				{
					TargetDot = Draw.Text(this,"TargetDot",true,"TargetDot",0,LinesY,+10,Brushes.Transparent,new SimpleFont("Arial",1),TextAlignment.Left,Brushes.Transparent,Brushes.Transparent,0); // dots so that the horizontal lines still be visible when changing from daily to mins etc...
					HLineDrawn = true;
					try
					{
						if(!retrievemode)//save without rewriting the values
						{	
							CollectLines();
							HandelLines();
							WriteValue(false,targets); 
						} 
					}
					catch(Exception e)
					{
		                Log(e.ToString(), LogLevel.Error);
		            }
				}
			}
			else if(targetstopentercreated)
			{
				if(ray)
				{
					TargetRLine2 = Draw.Ray(this,"TargetRLine2",RayBarsAgo,LinesY,RayBarsAgo-1,LinesY,PriceColor);
					TargetRLine2.Stroke = new Stroke(PriceColor,LinesWidth);	
					TargetRLine2.IsLocked=false;
				}
				else
				{
					TargetHLine2 = Draw.HorizontalLine(this,"TargetHLine2",LinesY, PriceColor);
					TargetHLine2.Stroke = new Stroke(PriceColor,LinesWidth);	
					TargetHLine2.IsLocked=false;	
				}
				
			}
			else if(target2created)
			{
				if(ray)
				{
					TargetRLine3 = Draw.Ray(this,"TargetRLine3",RayBarsAgo,LinesY,RayBarsAgo-1,LinesY,PriceColor);
					TargetRLine3.Stroke = new Stroke(PriceColor,LinesWidth);	
					TargetRLine3.IsLocked=false;
				}
				else
				{
					TargetHLine3 = Draw.HorizontalLine(this,"TargetHLine3",LinesY, PriceColor);
					TargetHLine3.Stroke = new Stroke(PriceColor,LinesWidth);
					TargetHLine3.IsLocked=false;
				}
				
				TargetDot = Draw.Text(this,"TargetDot",true,"TargetDot3",0,TargetLineY3,-10,Brushes.Transparent,new SimpleFont("Arial",1),TextAlignment.Left,Brushes.Transparent,Brushes.Transparent,0); // dots so that the horizontal lines still be visible when changing from daily to mins etc...
				HLineDrawn = true;
				
				if(!retrievemode)//save without rewriting the values
				{	
					CollectLines();
					HandelLines();
					WriteValue(false,targets); 
				} 
			}
			
			ForceRefresh();
		}
		
		private void CollectLines()
		{
			HLCollection.Clear();
						
			foreach (DrawingTool draw in DrawObjects)
			{  
				if (draw is NinjaTrader.NinjaScript.DrawingTools.Line && draw.GetType().Name == ((ray? "Ray":"HorizontalLine")) && !draw.IsUserDrawn) // if the line is Horizontal
				{						
					NinjaTrader.NinjaScript.DrawingTools.Line drawnLine = (NinjaTrader.NinjaScript.DrawingTools.Line) draw; // get the Line proporties 
					HLCollection.Add(drawnLine.StartAnchor.Price);
				}								
			}								
			
			HLCollection.Sort();
				
			if(Short)
			{
				if(targetLines == TargetLines.Three)
				{
					StopLineY =  HLCollection[4];
					EnterLineY = HLCollection[3];
					TargetLineY = HLCollection[2];
					TargetLineY2= HLCollection[1];
					TargetLineY3= HLCollection[0];	
				}
				else
				{
					StopLineY =  HLCollection[0];
					EnterLineY = HLCollection[1];
					TargetLineY = HLCollection[2];
				}
			}
			else
			{
				StopLineY =  HLCollection[0];
				EnterLineY = HLCollection[1];
				TargetLineY = HLCollection[2];
				
				if(targetLines == TargetLines.Three)
				{
					TargetLineY2= HLCollection[3];
					TargetLineY3= HLCollection[4];
				}
			}					
		}

		private void HandelLines()
		{
            if (HLineDrawn)
			{		
				CollectLines();
				
				PotentialProfit=(Math.Abs(EnterLineY -TargetLineY)) * Instrument.MasterInstrument.PointValue;  
				PotentialProfit2=(Math.Abs(TargetLineY -TargetLineY2)) * Instrument.MasterInstrument.PointValue;  
				PotentialProfit3=(Math.Abs(TargetLineY2 -TargetLineY3)) * Instrument.MasterInstrument.PointValue;
				PotentialLoss = (Math.Abs(EnterLineY - StopLineY)) * Instrument.MasterInstrument.PointValue;
								
				if(ray)
				{	
					if (EnterRLine != null)
					{
						EnterRLine.Stroke = new Stroke(Brushes.LightSlateGray, DashStyleHelper.Dash, EnterRLine.Stroke.Width);
						EnterRLine.StartAnchor.Price  = EnterRLine.EndAnchor.Price = EnterLineY;
					}
					if (TargetRLine != null)
						TargetRLine.StartAnchor.Price = TargetRLine.EndAnchor.Price = TargetLineY;
					if (StopRLine != null)
						StopRLine.StartAnchor.Price =StopRLine.EndAnchor.Price= StopLineY;				
				}
				else
				{
					if (EnterHLine != null)
					{
						EnterHLine.Stroke = new Stroke(Brushes.LightSlateGray, DashStyleHelper.Dash, EnterHLine.Stroke.Width);
						EnterHLine.StartAnchor.Price = EnterLineY;
					}
					if (TargetHLine != null)
						TargetHLine.StartAnchor.Price = TargetLineY;
					if (StopHLine != null)
						StopHLine.StartAnchor.Price = StopLineY;
				}
				if (TargetDot != null)
					TargetDot.Anchor.Price = (targetLines == TargetLines.One ? TargetLineY:TargetLineY3);// for scale issue when changing from daily to minute chart, horizontal line disappear
				if (StopDot != null)
					StopDot.Anchor.Price = StopLineY;
								
				RatioValue = ( (targetLines ==TargetLines.One) && Short ? (PotentialLoss/PotentialProfit):(PotentialProfit/PotentialLoss));
				
				if ((RatioValue) < 1.5 )
				{
                    RatioTextColor = RedColor;
                }
				else if ((RatioValue) > 1.5 )
				{
                    RatioTextColor = GreenColor;
                }
				else
				{
                    RatioTextColor = Brushes.Gray;
                }
				
				if(targetLines == TargetLines.Three)
				{	
					if(ray)
					{	
						if (TargetRLine2 != null)
							TargetRLine2.StartAnchor.Price = TargetRLine2.EndAnchor.Price = TargetLineY2;
						if (TargetRLine3 != null)
							TargetRLine3.StartAnchor.Price = TargetRLine3.EndAnchor.Price = TargetLineY3;
					}
					else
					{
						if (TargetHLine2 != null)
							TargetHLine2.StartAnchor.Price = TargetLineY2;
						if (TargetHLine3 != null)
							TargetHLine3.StartAnchor.Price = TargetLineY3;
					}				
					
					RatioValue2 = (TargetLineY2 - EnterLineY )/(EnterLineY  - StopLineY);
					RatioValue3 = (TargetLineY3 - EnterLineY )/(EnterLineY  - StopLineY);
										
					if ((RatioValue2) < 1.5 )
					{
                        Ratio2TextColor = RedColor;
                    }
					else if ((RatioValue2) > 1.5 )
					{
                        Ratio2TextColor = GreenColor;
                        Ratio3TextColor = GreenColor;
                    }
					else
					{
                        Ratio2TextColor = Brushes.Gray;
                    }
					if ((RatioValue3) < 1.5 )
					{
                        Ratio3TextColor = RedColor;
                    }
					else if ((RatioValue3) > 1.5 )
					{
                        Ratio3TextColor = GreenColor;
                    }
					else
					{
                        Ratio3TextColor = Brushes.Gray;
                    }
				}
								
				if(Short)
				{									
					if(targetLines == TargetLines.Three)
					{
						
						if(ray)
						{	
							if(TargetRLine != null)
								TargetRLine.Stroke = new Stroke(GreenColor, TargetRLine.Stroke.DashStyleHelper, TargetRLine.Stroke.Width);
							if(StopRLine != null)
								StopRLine.Stroke = new Stroke(RedColor, StopRLine.Stroke.DashStyleHelper, StopRLine.Stroke.Width);
							if(TargetRLine2 != null)
								TargetRLine2.Stroke = new Stroke(GreenColor, TargetRLine2.Stroke.DashStyleHelper, TargetRLine2.Stroke.Width);
							if(TargetRLine3 != null)
								TargetRLine3.Stroke = new Stroke(GreenColor, TargetRLine3.Stroke.DashStyleHelper, TargetRLine3.Stroke.Width);
						}
						else
						{
							if(TargetHLine != null)
								TargetHLine.Stroke = new Stroke(GreenColor, TargetHLine.Stroke.DashStyleHelper, TargetHLine.Stroke.Width);
							if(StopHLine != null)
								StopHLine.Stroke = new Stroke(RedColor, StopHLine.Stroke.DashStyleHelper, StopHLine.Stroke.Width);
							if(TargetHLine2 != null)
								TargetHLine2.Stroke = new Stroke(GreenColor, TargetHLine2.Stroke.DashStyleHelper, TargetHLine2.Stroke.Width);
							if(TargetHLine3 != null)
								TargetHLine3.Stroke = new Stroke(GreenColor, TargetHLine3.Stroke.DashStyleHelper, TargetHLine3.Stroke.Width);
						}
                        EnterToTargetColor  = GreenColor;
                        UpSplitTagColor     = GreenColor;
                        EnterToStopColor    = RedColor;
                        LowerSplitTagColor  = RedColor;
                        StopLinetxtColor    = RedColor;
                        TargetLinetxtColor  = GreenColor;
						if(PotentialLoss < 1)
							Contracts = 1;
						else
                        	Contracts = (int)( ((AccountSize * RiskPercent)/100) /PotentialLoss);
						if(Contracts < 1)
							Contracts = 1;
					}
					else
					{
						
						if(ray)
						{	
							if(TargetRLine != null)
								TargetRLine.Stroke = new Stroke(RedColor, TargetRLine.Stroke.DashStyleHelper, TargetRLine.Stroke.Width);
							if(StopRLine != null)
								StopRLine.Stroke = new Stroke(GreenColor, StopRLine.Stroke.DashStyleHelper, StopRLine.Stroke.Width);
						}
						else
						{
							if (TargetHLine != null && StopHLine != null)
							{
								TargetHLine.Stroke = new Stroke(GreenColor, TargetHLine.Stroke.DashStyleHelper, TargetHLine.Stroke.Width);
								StopHLine.Stroke = new Stroke(RedColor, StopHLine.Stroke.DashStyleHelper, StopHLine.Stroke.Width);
								if(targetLines == TargetLines.Three && TargetHLine2 != null && TargetHLine3 != null)
								{
									TargetHLine2.Stroke = new Stroke(GreenColor, TargetHLine2.Stroke.DashStyleHelper, TargetHLine2.Stroke.Width);
									TargetHLine3.Stroke = new Stroke(GreenColor, TargetHLine3.Stroke.DashStyleHelper, TargetHLine3.Stroke.Width);
								}
							}
						}
                        EnterToTargetColor  = RedColor;
                        UpSplitTagColor     = RedColor;
                        EnterToStopColor    = GreenColor;
                        LowerSplitTagColor  = GreenColor;
                        StopLinetxtColor    = GreenColor;
                        TargetLinetxtColor  = RedColor;
						if(PotentialLoss < 1)
							Contracts = 1;
						else
                        	Contracts = (int)( ((AccountSize * RiskPercent)/100) /PotentialProfit);
						if(Contracts < 1)
							Contracts = 1;
					}
				}
				else
				{
					
					if(ray)
					{
						if (TargetRLine != null && StopRLine != null)
						{
							TargetRLine.Stroke = new Stroke(GreenColor, TargetRLine.Stroke.DashStyleHelper, TargetRLine.Stroke.Width);
							StopRLine.Stroke = new Stroke(RedColor, StopRLine.Stroke.DashStyleHelper, StopRLine.Stroke.Width);
							if(targetLines == TargetLines.Three && TargetRLine2 != null && TargetRLine3 != null)
							{
								TargetRLine2.Stroke = new Stroke(GreenColor, TargetRLine.Stroke.DashStyleHelper, TargetRLine.Stroke.Width);
								TargetRLine3.Stroke = new Stroke(GreenColor, TargetRLine.Stroke.DashStyleHelper, TargetRLine.Stroke.Width);
							}
						}
					}
					else
					{
						if (TargetHLine != null && StopHLine != null)
						{
							TargetHLine.Stroke = new Stroke(GreenColor, TargetHLine.Stroke.DashStyleHelper, TargetHLine.Stroke.Width);
							StopHLine.Stroke = new Stroke(RedColor, StopHLine.Stroke.DashStyleHelper, StopHLine.Stroke.Width);
							if(targetLines == TargetLines.Three && TargetHLine2 != null && TargetHLine3 != null)
							{
								TargetHLine2.Stroke = new Stroke(GreenColor, TargetHLine2.Stroke.DashStyleHelper, TargetHLine2.Stroke.Width);
								TargetHLine3.Stroke = new Stroke(GreenColor, TargetHLine3.Stroke.DashStyleHelper, TargetHLine3.Stroke.Width);
							}
						}
					}
                    EnterToTargetColor  = GreenColor;
                    UpSplitTagColor     = GreenColor;
                    EnterToStopColor    = RedColor;
                    LowerSplitTagColor  = RedColor;
                    StopLinetxtColor    = RedColor;
                    TargetLinetxtColor  = GreenColor;
					if(PotentialLoss < 1)
						Contracts = 1;
					else
                    	Contracts = (int)(((AccountSize * RiskPercent)/100) /PotentialLoss);
					if(Contracts < 1)
						Contracts = 1;
				}			
			}
        }
		
			
		private void HandleInteractiveLines()
		{
			if (interactive == Interactive.Yes )
			{	
				PL = ( Short? EnterLineY-currentClose : currentClose-EnterLineY );
				if (!attached) // if not attached
				{
					if( PL < 0 )
                        InteractivePLColor = RedColor;
					else if ( PL > 0 )
                        InteractivePLColor = GreenColor;
					else
                        InteractivePLColor = Brushes.Gray;
				}
				else // if attached
				{
					PL=0;
					if(ray)
					{	
						if(EnterRLine != null)
						EnterRLine.StartAnchor.Price = EnterRLine.EndAnchor.Price = currentClose;
					}
					else
					{
						if(EnterHLine != null)
						EnterHLine.StartAnchor.Price = currentClose;
					}
					RetrieveSavedLines();
					HandelLines();
				}
                InteractivePriceColor = PriceColor;
            }
		}
		
		private void RefreshLines()
		{
			//if (!ray)
			//	return;
			
			if(TargetRLine2 != null)
			{
				TargetRLine2.StartAnchor.SlotIndex = TargetRLine2StartSlot;
				TargetRLine2.EndAnchor.SlotIndex = TargetRLine2EndSlot;
			}
			
			if(TargetRLine3 != null)
			{
				TargetRLine3.StartAnchor.SlotIndex = TargetRLine3StartSlot;
				TargetRLine3.EndAnchor.SlotIndex = TargetRLine3EndSlot;
			}
			
			if(EnterRLine != null)
			{
				EnterRLine.StartAnchor.SlotIndex = EnterRLineStartSlot;
				EnterRLine.EndAnchor.SlotIndex = EnterRLineEndSlot;
			}
			
			if(TargetRLine != null)
			{
				TargetRLine.StartAnchor.SlotIndex = TargetRLineStartSlot;
				TargetRLine.EndAnchor.SlotIndex = TargetRLineEndSlot;
			}
			
			if(StopRLine != null)
			{
				StopRLine.StartAnchor.SlotIndex = StopRLineStartSlot;
				StopRLine.EndAnchor.SlotIndex = StopRLineEndSlot;
			}
			ForceRefresh();	
		}
		#endregion
		
		#region Retrieve Lines
		private void RetrieveSavedLines()
		{
			if (GetValue(0,targets)!=-1 && GetValue(0,targets)!=0 && !RetrievalDone && !HLineDrawn && (GetValue(0,targets) == (targetLines == TargetLines.One? 3:5)))// if file exist and file is not  empty
			{		
				for (int index = 2; index <GetValue(0,targets)+2; index++) 
					CreateLines(GetValue(index,targets),true);// true to retrieve only without overwriting the file 
				
				RetrievalDone = true;
				HLineDrawn = true;
				HandelLines();
			}
		}
		#endregion
		
		protected override void OnBarUpdate()
        {
			if (isIndicatorAdded)
			{
				if(AlertOnce)
				{
					Log("This indicator does not support multiple instances. Please remove any additional instances of this indicator.", LogLevel.Alert);
					AlertOnce = false;
				}
				return;
			}
			
			if (CurrentBar == Count-2 && buttonShowHideChecked )
			{	
				RetrieveSavedLines();
				if (Close[0] != 0 && interactive == Interactive.Yes )
				{
					HandleInteractiveLines();
				}
			}
			
			// Refresh Line offsets
			if(IsFirstTickOfBar)
			{
				TargetRLine2StartSlot = CurrentBar+5;
				TargetRLine2EndSlot = CurrentBar+6;
				
				TargetRLine3StartSlot = CurrentBar+5;
				TargetRLine3EndSlot = CurrentBar+6;	
					
				EnterRLineStartSlot = CurrentBar+5;
				EnterRLineEndSlot = CurrentBar+6;
				
				TargetRLineStartSlot = CurrentBar+5;
				TargetRLineEndSlot = CurrentBar+6;
				
				StopRLineStartSlot = CurrentBar+5;
				StopRLineEndSlot = CurrentBar+6;
			}
			currentClose = Close[0];
		}
		
	#region Mouse
		public void MyMouseUpEvent(object sender, MouseEventArgs e)
		{
			mouseY = (int)e.GetPosition(ChartPanel).Y;
			mouseX = (int)e.GetPosition(ChartPanel).X;
			TriggerCustomEvent(MyCustomHandler,e);
		}
		public void MyMouseDownEvent(object sender, MouseEventArgs e)
		{
			mouseY = (int)e.GetPosition(ChartPanel).Y;
			mouseX = (int)e.GetPosition(ChartPanel).X;
			if(ray)TriggerCustomEvent(MyCustomHandlerDown,e);
		}
		
			
		private void MyCustomHandler(object state) // mouse up 
		{
			if (!HLineDrawn && (GetValue(0,targets)==0  || GetValue(0,targets)==-1) )
			{	
				CreateLines(GetPriceFromY(mouseY),false);
				HandleInteractiveLines();
			}
            TargetBeamColor     = Brushes.Transparent;
            Target2BeamColor    = Brushes.Transparent;
            Target3BeamColor    = Brushes.Transparent;
            EnterBeamColor      = Brushes.Transparent;
            StopBeamColor       = Brushes.Transparent;
			
			ChartControl.InvalidateVisual();
        }
		private void MyCustomHandlerDown(object state) // mouse Down 
		{
			
			if(Math.Abs(mouseY-TargeYPixles) < 10 && mouseX >= (RaysX-100))
			{
                TargetBeamColor = BeamColor;
            }
			if(Math.Abs(mouseY-Targe2YPixles) < 10 && mouseX >= (RaysX-100))
			{
                Target2BeamColor = BeamColor;
            }
			if(Math.Abs(mouseY-Targe3YPixles) < 10 && mouseX >= (RaysX-100))
			{
                Target3BeamColor = BeamColor;
            }
			if(Math.Abs(mouseY-EnterYPixles) < 10 && mouseX >= (RaysX-100))
			{
                EnterBeamColor = BeamColor;
            }
			if(Math.Abs(mouseY-StopYPixles) < 10 && mouseX >= (RaysX-100))
			{
                StopBeamColor = BeamColor;
            }
			ChartControl.InvalidateVisual();
		}
		
		private double GetPriceFromY(int y)
		{
			return (double)(((((ChartPanel.Y + ChartPanel.H) - y) * Math.Max(Math.Abs(myChartMax - myChartMin), 1E-05)) / ((double)ChartPanel.H)) + myChartMin);
		}
	#endregion
		
	#region OnRender() / OnRenderTargetChanged()				
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{	
			if (isIndicatorAdded)
				return;
			/* This will ensure that our other plots are still being drawn correctly. */
			base.OnRender(chartControl, chartScale);
			/* Here we grab the information we need and place it into our variables. */
			
			if(ChartPanel == null)
				return;
			
			myChartMin = ChartPanel.MinValue;
			myChartMax = ChartPanel.MaxValue;
			
			// Refresh Line movement
			RefreshLines();
								
			if( buttonShowHideChecked )
			{	
					if(!RetrievalDone)
						RetrieveSavedLines();
					
					int BarsOnChart =  (int)((double)(ChartPanel.W)/(double)ChartControl.Properties.BarDistance) ; // possible bars on chart ChartControl.Properties.BarDistance
					RayBarsAgo = -1 * (int)((double)BarsOnChart/15);
					
					HandelLines();
					
					HandleInteractiveLines();
					
					EnterYPixles = chartScale.GetYByValue(EnterLineY);
					TargeYPixles = chartScale.GetYByValue(TargetLineY);
					Targe2YPixles = chartScale.GetYByValue(TargetLineY2);
					Targe3YPixles = chartScale.GetYByValue(TargetLineY3);
					
					StopYPixles = chartScale.GetYByValue(StopLineY);
					int LastBarX = ChartControl.GetXByBarIndex(ChartBars, Count);
					int CloseY=  chartScale.GetYByValue(Close[0]);
					
					int	upSplitTagY = chartScale.GetYByValue(EnterLineY+(TargetLineY-EnterLineY)/2);
					int upSplitTagY2 = chartScale.GetYByValue(TargetLineY2 - (TargetLineY2 - TargetLineY)/2);
					int upSplitTagY3 = chartScale.GetYByValue(TargetLineY3 - (TargetLineY3 - TargetLineY2)/2);
					int	LowerSplitTagY = chartScale.GetYByValue(EnterLineY-(EnterLineY-StopLineY)/2);	
						
					int pixelsShift =(targetLines == TargetLines.One? 250:350); // change the distance from last bar 
					int PLShift = (targetLines == TargetLines.One? (int)(pixelsShift/3.5): (int)(pixelsShift/5.5)); 
					int XCooficient = ChartPanel.X +LastBarX+pixelsShift ; 
					int VerticalLineShift= 40;
					int BetweenLines = 75;// distance between first and second vertical lines
					RaysX = ChartControl.GetXByBarIndex(ChartBars, Count+Math.Abs(RayBarsAgo));// Ray line start on the plot 
					
					// enter line text shift up or down 
					int up= -22;
					int down = 0; 
					int up23= -22;
					int down23= 0; 
					
					
					if(Math.Abs(Targe3YPixles- Targe2YPixles) < 20)
					{
						up23 =0 ;
						down23= -22; 	
					}
					else if (Math.Abs(TargeYPixles- Targe2YPixles) < 20)
					{
						up = 0;
						down= -22; 
					}
						
					#region Ratio text
					string RatioText = "®1-"+(RatioValue).ToString("N1"); 
					
					DrawString(RatioText, textFont, "RatioTextColor", 
						(targetLines == TargetLines.One? XCooficient - (VerticalLineShift - BetweenLines -52):XCooficient +45),
						ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? (targetLines == TargetLines.One?EnterYPixles:TargeYPixles+up):TargeYPixles+down));

					if(targetLines == TargetLines.Three)
					{	
						string Ratio2Text = "®1-"+(RatioValue2).ToString("N1");
						string Ratio3Text = "®1-"+(RatioValue3).ToString("N1");
						
						DrawString(Ratio2Text, textFont, "Ratio2TextColor", 
							XCooficient - BetweenLines + 45,
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? Targe2YPixles+up23:Targe2YPixles+down23));

						DrawString(Ratio3Text, textFont, "Ratio3TextColor", 
							XCooficient - 2*BetweenLines + 45,
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? Targe3YPixles -20:Targe3YPixles));
					}
                	#endregion

					#region EnterLine Text
                    dxmPriceColor = PriceColor;

                    string EnterLineText =(interactive==Interactive.Yes ?"":Contracts.ToString("N0")+" Cont."+" @")+FormatPriceMarker(EnterLineY)+"";
					
					DrawString(EnterLineText, textFont, "dxmPriceColor", 
							XCooficient - (VerticalLineShift - BetweenLines -52),
							ChartPanel.Y + ( Short || targetLines != TargetLines.Three? EnterYPixles -22 :EnterYPixles));
					#endregion 
									
					#region TargetLine text
					string TargetLinetxt =FormatPriceMarker(TargetLineY)+"";
					DrawString(TargetLinetxt, textFont, "TargetLinetxtColor", 
							XCooficient - (VerticalLineShift - BetweenLines+45),
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? TargeYPixles+up :TargeYPixles+down));
					#endregion 
					
					#region Target Beam
					DrawLine("TargetBeamColor",ChartPanel.X, TargeYPixles, ChartPanel.X + ChartPanel.W, TargeYPixles, 1);
					#endregion
					
					#region EnterBeam
					DrawLine("EnterBeamColor",ChartPanel.X, EnterYPixles, ChartPanel.X + ChartPanel.W, EnterYPixles, 1);
					#endregion
					
					#region StopBeam
					DrawLine("StopBeamColor",ChartPanel.X, StopYPixles, ChartPanel.X + ChartPanel.W, StopYPixles, 1);
					#endregion
					
					if(targetLines == TargetLines.Three)
					{
						#region Target2 Beam
						DrawLine("Target2BeamColor",ChartPanel.X, Targe2YPixles, ChartPanel.X + ChartPanel.W, Targe2YPixles, 1);
						#endregion
						
						#region Target3 Beam
						DrawLine("Target3BeamColor",ChartPanel.X, Targe3YPixles, ChartPanel.X + ChartPanel.W, Targe3YPixles, 1);
						#endregion
						
						#region Target2Line text
						string Target2Linetxt =FormatPriceMarker(TargetLineY2)+"";
						
						DrawString(Target2Linetxt, textFont, "TargetLinetxtColor", 
							XCooficient - (VerticalLineShift + 45),
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? Targe2YPixles+up23:Targe2YPixles+down23));
						#endregion
						
						#region Target3Line text
						string Target3Linetxt =FormatPriceMarker(TargetLineY3)+"";
						
						DrawString(Target3Linetxt, textFont, "TargetLinetxtColor", 
							XCooficient - (VerticalLineShift + BetweenLines+45),
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? Targe3YPixles -20:Targe3YPixles));
						#endregion
					}	
					
					#region StopLine text
					string StopLinetxt =FormatPriceMarker(StopLineY)+"";
					
					DrawString(StopLinetxt, textFont, "StopLinetxtColor", 
							XCooficient - (VerticalLineShift - BetweenLines+45),
							ChartPanel.Y + ((!(Short && targetLines == TargetLines.Three))? StopYPixles :StopYPixles - 22));
					#endregion 		
			
					#region Enter To Target & Target2 & Target3 Lines
					
					if (!(Short && targetLines == TargetLines.Three) )
					{
						DrawLine("EnterToTargetColor", XCooficient - VerticalLineShift, EnterYPixles, XCooficient - VerticalLineShift, ChartPanel.Y + upSplitTagY +5, LinesWidth, DashStyleHelper.DashDot);
						
						if((ChartPanel.Y + upSplitTagY -20)-(TargeYPixles-4)>4) // dont draw the line when enter and target are too close 
							DrawLine("EnterToTargetColor", XCooficient - VerticalLineShift, ChartPanel.Y + upSplitTagY -20, XCooficient - VerticalLineShift, ChartPanel.Y + TargeYPixles-4, LinesWidth, DashStyleHelper.DashDot);
						
						if(targetLines == TargetLines.Three)
						{
							DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+BetweenLines), EnterYPixles, XCooficient - (VerticalLineShift+BetweenLines), ChartPanel.Y +upSplitTagY2 +5, LinesWidth, DashStyleHelper.DashDot);
							
							if((ChartPanel.Y + upSplitTagY2 -20)-(Targe2YPixles-4)>4)
								DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+BetweenLines), upSplitTagY2 -15, XCooficient - (VerticalLineShift+BetweenLines), ChartPanel.Y + Targe2YPixles - 4, LinesWidth, DashStyleHelper.DashDot);
							
							DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+2*BetweenLines), EnterYPixles, XCooficient - (VerticalLineShift+2*BetweenLines), ChartPanel.Y + upSplitTagY3 +5, LinesWidth, DashStyleHelper.DashDot);
							
							if((ChartPanel.Y + upSplitTagY3 -20)-(Targe3YPixles-4)>4) // dont draw the line when enter and target are too close 
								DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+2*BetweenLines), upSplitTagY3 -15, XCooficient - (VerticalLineShift+2*BetweenLines), ChartPanel.Y + Targe3YPixles -4, LinesWidth, DashStyleHelper.DashDot);
						}
					}
					else 
					{
						DrawLine("EnterToStopColor", XCooficient - VerticalLineShift, EnterYPixles, XCooficient - VerticalLineShift, ChartPanel.Y + LowerSplitTagY +5, LinesWidth, DashStyleHelper.DashDot);
						if((ChartPanel.Y + LowerSplitTagY -20)-(StopYPixles-4)>4) // dont draw the line when enter and target are too close 
							DrawLine("EnterToStopColor", XCooficient - VerticalLineShift, ChartPanel.Y + LowerSplitTagY -20, XCooficient - VerticalLineShift, ChartPanel.Y +  StopYPixles -4, LinesWidth, DashStyleHelper.DashDot);
						
					}
					#endregion
																					
					#region Enter To Stop Lines & Target2 & Target3 Lines
					
					if (!(Short && targetLines == TargetLines.Three)) // if long and one target 
					{	
						DrawLine("EnterToStopColor", XCooficient - VerticalLineShift, EnterYPixles, XCooficient - VerticalLineShift, ChartPanel.Y + LowerSplitTagY-17, LinesWidth, DashStyleHelper.DashDot);
						if(((StopYPixles -5)-(ChartPanel.Y +LowerSplitTagY+8)) > -5)
							DrawLine("EnterToStopColor", XCooficient - VerticalLineShift, ChartPanel.Y + LowerSplitTagY+8, XCooficient - VerticalLineShift, ChartPanel.Y + StopYPixles -5, LinesWidth, DashStyleHelper.DashDot);
					}
					else // if short and multiple targets 
					{
						DrawLine("EnterToTargetColor", XCooficient - VerticalLineShift, EnterYPixles, XCooficient - VerticalLineShift, ChartPanel.Y + upSplitTagY-17, LinesWidth, DashStyleHelper.DashDot);

						if(((TargeYPixles -5)-(ChartPanel.Y + upSplitTagY+12)) > -5)
							DrawLine("EnterToTargetColor", XCooficient - VerticalLineShift, ChartPanel.Y + upSplitTagY +8, XCooficient - VerticalLineShift, ChartPanel.Y + TargeYPixles-5, LinesWidth, DashStyleHelper.DashDot);
						
						DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+BetweenLines), EnterYPixles, XCooficient - (VerticalLineShift+BetweenLines), ChartPanel.Y + upSplitTagY2-17, LinesWidth, DashStyleHelper.DashDot);
						
						if(((upSplitTagY2 -5 )-(ChartPanel.Y +Targe2YPixles-13)) < -7)
							DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+BetweenLines), upSplitTagY2 +12, XCooficient - (VerticalLineShift+BetweenLines), ChartPanel.Y + Targe2YPixles-4, LinesWidth, DashStyleHelper.DashDot);
						
						DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+2*BetweenLines), EnterYPixles, XCooficient - (VerticalLineShift+2*BetweenLines), ChartPanel.Y + upSplitTagY3-17, LinesWidth, DashStyleHelper.DashDot);
						
						if(((Targe3YPixles  -5)-(ChartPanel.Y +upSplitTagY3 +12)) > -5)
							DrawLine("EnterToTargetColor", XCooficient - (VerticalLineShift+2*BetweenLines), upSplitTagY3 +12, XCooficient - (VerticalLineShift+2*BetweenLines), ChartPanel.Y + Targe3YPixles -4, LinesWidth, DashStyleHelper.DashDot);
					}
						
					#endregion
					
					#region UpSplitTag text & UpSplitTag2 text & UpSplitTag3 text
					string UpSplitTagTxt ="( "+(PotentialProfit*((CalcBasedOnUser)? userscontracts:1)).ToString("N0")+" )";
					
					DrawString(UpSplitTagTxt, textFont, "UpSplitTagColor", 
							XCooficient - (VerticalLineShift+25),
							ChartPanel.Y + upSplitTagY -14);
					// UpSplitTag2 & UpSplitTag3  text
					if( targetLines == TargetLines.Three)
					{
						// UpSplitTag2 text
						string UpSplitTagTxt2 ="( "+((PotentialProfit+PotentialProfit2)*((CalcBasedOnUser)? userscontracts:1)).ToString("N0")+" )";
						
						DrawString(UpSplitTagTxt2, textFont, "UpSplitTagColor", 
							XCooficient - (VerticalLineShift+BetweenLines+25),
							ChartPanel.Y + upSplitTagY2 -14);
						
						// UpSplitTag3 text
						string LowerSplitTagTxt3 ="( "+((PotentialProfit+PotentialProfit2+PotentialProfit3)*((CalcBasedOnUser)? userscontracts:1)).ToString("N0")+" )";;
						
						DrawString(LowerSplitTagTxt3, textFont, "UpSplitTagColor", 
							XCooficient - (VerticalLineShift+2*BetweenLines+25),
							ChartPanel.Y + upSplitTagY3 -14);
					}	
					
					#endregion
					
					#region LowerSplitTag text
					string LowerSplitTagTxt ="( "+(PotentialLoss*((CalcBasedOnUser)? userscontracts:1)).ToString("N0")+" )";;
					
					DrawString(LowerSplitTagTxt, textFont, "LowerSplitTagColor", 
							XCooficient -(VerticalLineShift+25),
							ChartPanel.Y + LowerSplitTagY -14);
					#endregion
					
					{	
						#region Arrowup 
							ArrowUpColor = Brushes.Lime;
							int size = 4;
							int arrowshift =1;
							if(Short && targetLines == TargetLines.Three)
							{
								size = size *-1;
								arrowshift= 0;
							}	
							if(targetLines == TargetLines.Three)
							{	
								//Arrowup Target3
								int x= XCooficient - (VerticalLineShift+2*BetweenLines); 
								int y =  Targe3YPixles + arrowshift; 
								if(Math.Abs((ChartPanel.Y + upSplitTagY3)-(Targe3YPixles))>15) // dont draw the line when enter and target are too close 
								{
                                    FillPolygon(new SharpDX.Vector2[] { new Point(x + 1, y + 1).ToVector2(), new Point(x - (size), y + size).ToVector2(), new Point(x + (size), y + size).ToVector2() }, "ArrowUpColor");
                                    DrawPolygon(new SharpDX.Vector2[] { new Point(x, y).ToVector2(), new Point(x - (size), y + size).ToVector2(), new Point(x + (size), y + size).ToVector2() }, "ArrowUpColor");
                                }
								//Arrowup Target 2 
								int x2= XCooficient - (VerticalLineShift+BetweenLines); 
								int y2 = Targe2YPixles +arrowshift;
								if(Math.Abs((ChartPanel.Y + upSplitTagY2)-(Targe2YPixles))>15)
								{
                                    FillPolygon(new SharpDX.Vector2[] { new Point(x2 + 1, y2 + 1).ToVector2(), new Point(x2 - (size), y2 + size).ToVector2(), new Point(x2 + (size), y2 + size).ToVector2() }, "ArrowUpColor");
                                    DrawPolygon(new SharpDX.Vector2[] { new Point(x2, y2).ToVector2(), new Point(x2 - (size), y2 + size).ToVector2(), new Point(x2 + (size), y2 + size).ToVector2() }, "ArrowUpColor");
                                }
								
							}
							//Arrowup Target
							ArrowUpColor = ((Short && targetLines == TargetLines.One)? RedColor:Brushes.Lime);
							int x3= XCooficient - (VerticalLineShift); 
							int y3 = TargeYPixles +arrowshift;
							if(Math.Abs((ChartPanel.Y + upSplitTagY)-(TargeYPixles))>15 )
							{
                                FillPolygon(new SharpDX.Vector2[] { new Point(x3 + 1, y3 + 1).ToVector2(), new Point(x3 - (size), y3 + size).ToVector2(), new Point(x3 + (size), y3 + size).ToVector2() }, "ArrowUpColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x3, y3).ToVector2(), new Point(x3 - (size), y3 + size).ToVector2(), new Point(x3 + (size), y3 + size).ToVector2() }, "ArrowUpColor");
                            }
							//Arrowup Stop
							ArrowUpColor = ((Short && targetLines == TargetLines.One)? Brushes.Lime:RedColor);
							int x8= XCooficient - (VerticalLineShift); 
							int y8 = EnterYPixles +arrowshift;
							if((Math.Abs((ChartPanel.Y + StopYPixles)-(EnterYPixles))>30) && (Math.Abs((EnterYPixles)-(ChartPanel.Y + upSplitTagY))>20) )
							{
								FillPolygon(new SharpDX.Vector2[] { new Point(x8 + 1, y8 + 1).ToVector2(), new Point(x8 - (size), y8 + size).ToVector2(), new Point(x8 + (size), y8 + size).ToVector2() }, "ArrowUpColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x8, y8).ToVector2(), new Point(x8 - (size), y8 + size).ToVector2(), new Point(x8 + (size), y8 + size).ToVector2() }, "ArrowUpColor");
                            }
							
					#endregion
						
						#region Arrowdown
							//ArrowDown Traget3 
							ArrowDownColor = Brushes.Lime;
							if(targetLines == TargetLines.Three)
							{
								int x4= XCooficient - (VerticalLineShift+2*BetweenLines); 
								int y4 =  EnterYPixles -arrowshift;
                              	
                                FillPolygon(new SharpDX.Vector2[] { new Point(x4+1,y4+1).ToVector2(), new Point(x4+(size),y4-size).ToVector2(), new Point(x4-(size),y4-size).ToVector2() }, "ArrowDownColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x4, y4).ToVector2(), new Point(x4 - (size), y4 - size).ToVector2(), new Point(x4 + (size), y4 - size).ToVector2() }, "ArrowDownColor");

                                //ArrowDown Target2
                                int x5= XCooficient - (VerticalLineShift+BetweenLines);
								int y5 =  EnterYPixles - arrowshift;

                                FillPolygon(new SharpDX.Vector2[] { new Point(x5 + 1, y5 + 1).ToVector2(), new Point(x5 + (size), y5 - size).ToVector2(), new Point(x5 - (size), y5 - size).ToVector2() }, "ArrowDownColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x5, y5).ToVector2(), new Point(x5 - (size), y5 - size).ToVector2(), new Point(x5 + (size), y5 - size).ToVector2() }, "ArrowDownColor");
                            }
							//ArrowDown Target
							ArrowDownColor = ((Short && targetLines == TargetLines.One)? RedColor:Brushes.Lime);
							int x6= XCooficient - (VerticalLineShift);
							int y6 =  EnterYPixles -arrowshift;  
							if(Math.Abs((EnterYPixles)-(ChartPanel.Y + upSplitTagY))>15 && (Math.Abs((EnterYPixles)-(ChartPanel.Y + StopYPixles))>30))
							{
                                FillPolygon(new SharpDX.Vector2[] { new Point(x6 + 1, y6 + 1).ToVector2(), new Point(x6 + (size), y6 - size).ToVector2(), new Point(x6 - (size), y6 - size).ToVector2() }, "ArrowDownColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x6, y6).ToVector2(), new Point(x6 - (size), y6 - size).ToVector2(), new Point(x6 + (size), y6 - size).ToVector2() }, "ArrowDownColor");
							}
							//ArrowDown Stop
							ArrowDownColor = ((Short && targetLines == TargetLines.One)? Brushes.Lime:RedColor);
							int x7= XCooficient - (VerticalLineShift);
							int y7 =  StopYPixles -arrowshift;  
							if(Math.Abs((EnterYPixles)-(ChartPanel.Y + StopYPixles))>30)
							{
                                FillPolygon(new SharpDX.Vector2[] { new Point(x7 + 1, y7 + 1).ToVector2(), new Point(x7 + (size), y7 - size).ToVector2(), new Point(x7 - (size), y7 - size).ToVector2() }, "ArrowDownColor");
                                DrawPolygon(new SharpDX.Vector2[] { new Point(x7, y7).ToVector2(), new Point(x7 - (size), y7 - size).ToVector2(), new Point(x7 + (size), y7 - size).ToVector2() }, "ArrowDownColor");
                            }
                    
                    #endregion
                	}
					if(!attached && HLineDrawn && interactive == Interactive.Yes)
					{
						
						#region Interactive PL text
						string InteractivePLTxt =" "+(PL*Instrument.MasterInstrument.PointValue *((CalcBasedOnUser)? userscontracts:1)).ToString("N0")+" ";
						
						DrawString(InteractivePLTxt, textFont, "InteractivePLColor", 
							ChartPanel.X +LastBarX+PLShift,
							CloseY - 9);
						
						// Create a TextLayout so we can use Metrics for our Rectangle
						SharpDX.DirectWrite.TextFormat textFormat = textFont.ToDirectWriteTextFormat();
						
						SharpDX.DirectWrite.TextLayout textLayout =
							new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
							InteractivePLTxt, textFormat, ChartPanel.X + ChartPanel.W,
							textFormat.FontSize, 1, true);
						#endregion
					
						#region InteractivePrice Line						
						DrawLine("InteractivePLColor", ChartPanel.X +LastBarX, CloseY, ChartPanel.X +LastBarX +PLShift, CloseY, 1.5f);
                        #endregion

                        #region PL Boundaries Rectangle
                        float newW = textLayout.Metrics.Width+3; // * 1.5?
                        float newH = textLayout.Metrics.Height;
                        SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF(ChartPanel.X + LastBarX + PLShift, CloseY - 10, newW, newH);
                        RenderTarget.DrawRectangle(PLBoundRect, dxmBrushes["InteractivePLColor"].DxBrush, 1.5f);
						textLayout.Dispose();
						textFormat.Dispose();

                        #endregion

                        #region InteractivePrice Text

                        string InteractivePriceText =FormatPriceMarker(Close[0])+"";
						
						DrawString(InteractivePriceText, textFont, "InteractivePriceColor", 
							ChartPanel.X +LastBarX  +(int)(((double)PLShift-20)/2)-20,
							ChartPanel.Y + CloseY -20);
						#endregion
								
					}
			}
				
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
	#endregion
        
	#region WPF Controls 

        protected void CreateWPFControls()
		{
			chartWindow	= System.Windows.Window.GetWindow(ChartControl.Parent) as NinjaTrader.Gui.Chart.Chart;
			chartGrid	= chartWindow.MainTabControl.Parent as System.Windows.Controls.Grid;

			// Create Menu for Toolbar
			topMenu = new System.Windows.Controls.Menu()
			{
				Background			= Brushes.Transparent,
				BorderBrush			= controlLightGray,
				Padding				= new System.Windows.Thickness(0),
				Margin				= new System.Windows.Thickness(0,-2,0,2),
				VerticalAlignment	= VerticalAlignment.Center				
			};

			// Button 1 - Reset
			buttonReset = new System.Windows.Controls.MenuItem()
			{
				Background			= Application.Current.FindResource("ButtonBackgroundBrush") as LinearGradientBrush,
				BorderBrush			= Application.Current.FindResource("ButtonBorderBrush") as LinearGradientBrush,
				BorderThickness		= new System.Windows.Thickness(1),
				FontSize			= 12,
				Foreground			= textColor,
				Padding				= new System.Windows.Thickness(1),
				//Margin				= new System.Windows.Thickness(5, 0, 5, 0),
				Margin				= new System.Windows.Thickness(0, 0, 0, 0),
				Header				= "Reset"
			};

			buttonReset.Click += buttonReset_Click;
			topMenu.Items.Add(buttonReset);
			
			// Button 2 - SaveChanges
			buttonSaveChanges = new System.Windows.Controls.MenuItem()
			{
				Background			= Application.Current.FindResource("ButtonBackgroundBrush") as LinearGradientBrush,
				BorderBrush			= Application.Current.FindResource("ButtonBorderBrush") as LinearGradientBrush,
				BorderThickness		= new System.Windows.Thickness(1),
				FontSize			= 12,
				Foreground			= textColor,
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(2, 0, 2, 0),
				Header				= "Save Changes"
			};

			buttonSaveChanges.Click += buttonSaveChanges_Click;
			topMenu.Items.Add(buttonSaveChanges);
			
			// Button 3 - LongShort
			string header = (Short == true)?"Short":"Long";
			Brush ForeBrush = (Short == true)?Brushes.Crimson:Brushes.ForestGreen;
			buttonLongShort = new System.Windows.Controls.MenuItem()
			{
				Background			= Application.Current.FindResource("ButtonBackgroundBrush") as LinearGradientBrush,
				BorderBrush			= Application.Current.FindResource("ButtonBorderBrush") as LinearGradientBrush,
				BorderThickness		= new System.Windows.Thickness(1),
				FontSize			= 12,
				Foreground			= ForeBrush,
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(2, 0, 2, 0),
				Header				= header,
				Width				= 35
			};

			buttonLongShort.Click += buttonLongShort_Click;
			topMenu.Items.Add(buttonLongShort);
			
			// Button 4 - AttachedDetached
			buttonAttachDetach = new System.Windows.Controls.MenuItem()
			{
				Background			= Application.Current.FindResource("ButtonBackgroundBrush") as LinearGradientBrush,
				BorderBrush			= Application.Current.FindResource("ButtonBorderBrush") as LinearGradientBrush,
				BorderThickness		= new System.Windows.Thickness(1),
				FontSize			= 12,
				Foreground			= textColor,
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(2, 0, 2, 0),
				Header				= "Detached",
				Width				= 58,
				HorizontalContentAlignment = HorizontalAlignment.Center
			};

			buttonAttachDetach.Click += buttonAttachDetach_Click;
			topMenu.Items.Add(buttonAttachDetach);
			
			// Button 5 - ShowHide
			buttonShowHide = new System.Windows.Controls.Primitives.ToggleButton()
			{
				Background			= Application.Current.FindResource("ButtonBackgroundBrush") as LinearGradientBrush,
				BorderBrush			= Application.Current.FindResource("ButtonBorderBrush") as LinearGradientBrush,
				BorderThickness		= new System.Windows.Thickness(1),
				FontSize			= 12,
				Foreground			= (ShowOnStartup? Brushes.Crimson:Brushes.ForestGreen),
				Padding				= new System.Windows.Thickness(1),
				Margin				= new System.Windows.Thickness(0, 0, 0, 0),
				Content				= (ShowOnStartup? "Hide":"Show"),
				IsChecked			= (ShowOnStartup? true:false),
				Width				= 35
			};

			buttonShowHide.Click += buttonShowHide_Click;
			topMenu.Items.Add(buttonShowHide);
			
			// null Focus menu item
			nullFocusItem = new System.Windows.Controls.MenuItem()
			{
				Visibility 			= Visibility.Hidden,
				Focusable			= true,
				Header				= ""
			};
			topMenu.Items.Add(nullFocusItem);
			
			if (TabSelected())
				InsertWPFControls();

			chartWindow.MainTabControl.SelectionChanged += TabChangedHandler;
		}

		private void DisposeWPFControls()
		{
			if (buttonReset != null)
				buttonReset.Click -= buttonReset_Click;
			
			if (buttonSaveChanges != null)
				buttonSaveChanges.Click -= buttonSaveChanges_Click;
			
			if (buttonLongShort != null)
				buttonLongShort.Click -= buttonLongShort_Click;
			
			if (buttonAttachDetach != null)
				buttonAttachDetach.Click -= buttonAttachDetach_Click;
			
			if (buttonShowHide != null)
				buttonShowHide.Click -= buttonShowHide_Click;

			if (chartWindow != null)
				chartWindow.MainTabControl.SelectionChanged -= TabChangedHandler;

			RemoveWPFControls();
		}

		protected void InsertWPFControls()
		{
			if (panelActive)
				return;

			if (chartGrid.RowDefinitions.Count == 0)
				chartGrid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });

			tabControlStartColumn	= System.Windows.Controls.Grid.GetColumn(chartWindow.MainTabControl);
			tabControlStartRow		= System.Windows.Controls.Grid.GetRow(chartWindow.MainTabControl);

			chartGrid.RowDefinitions.Insert(tabControlStartRow, new System.Windows.Controls.RowDefinition() { Height = new GridLength(20) });

			// including the chartTabControl move all items right of the chart and below the chart to the right one column and down one row
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) >= tabControlStartRow)
					System.Windows.Controls.Grid.SetRow(chartGrid.Children[i], System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) + 1);
			}

			// set the columns and rows for our new items
			System.Windows.Controls.Grid.SetColumn(topMenu, System.Windows.Controls.Grid.GetColumn(chartWindow.MainTabControl));
			System.Windows.Controls.Grid.SetRow(topMenu, tabControlStartRow);
			
			chartGrid.Children.Add(topMenu);

			// let the script know the panel is active
			panelActive = true;
		}

		protected void RemoveWPFControls()
		{
			if (!panelActive)
				return;
				
			if (topMenu != null)
			{
				chartGrid.RowDefinitions.RemoveAt(System.Windows.Controls.Grid.GetRow(topMenu));
				chartGrid.Children.Remove(topMenu);
			}

			// if the childs column is 1 (so we can move it to 0) and the column is to the right of the column we are removing, shift it left
			for (int i = 0; i < chartGrid.Children.Count; i++)
			{
				if (System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > 0 && System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) > System.Windows.Controls.Grid.GetRow(topMenu))
					System.Windows.Controls.Grid.SetRow(chartGrid.Children[i], System.Windows.Controls.Grid.GetRow(chartGrid.Children[i]) - 1);
			}

			panelActive = false;
		}
		
		#region Button Controls
		protected void buttonReset_Click(object sender, RoutedEventArgs e)
		{
			buttonShowHide.IsChecked = true;
			buttonShowHideChecked =true;
			ShowOnStartup=true;
			WriteValue(true,targets);
			System.Windows.Forms.SendKeys.SendWait("{F5}");
		}
		
		protected void buttonSaveChanges_Click(object sender, RoutedEventArgs e)
		{
			WriteValue(false,targets);
		}
		
		protected void buttonLongShort_Click(object sender, RoutedEventArgs e)
		{
			if (buttonLongShort.Header.ToString() == "Long")
			{	
				Short = true;
				buttonLongShort.Header = "Short";
				buttonLongShort.HorizontalAlignment = HorizontalAlignment.Center;
				buttonLongShort.Foreground = Brushes.Crimson;
			}
			else
			{
				buttonLongShort.Header = "Long";
				Short = false;
				buttonLongShort.Foreground = Brushes.ForestGreen;
			}
			ForceRefresh();
			WriteValue(false,targets);
		}
		
		protected void buttonAttachDetach_Click(object sender, RoutedEventArgs e)
		{
			
			if (buttonAttachDetach.Header.ToString() == "Detached")
			{	
				attached= true;
				buttonAttachDetach.Header = "Attached";
			}
			else
			{
				attached= false;
				buttonAttachDetach.Header = "Detached";
			}
			buttonAttachDetach.HorizontalAlignment = HorizontalAlignment.Center;
			
			ForceRefresh();
		}
		
		protected void buttonShowHide_Click(object sender, RoutedEventArgs e)
		{
			if(buttonShowHide.IsChecked == false)
			{
				buttonShowHide.Content 			= "Show";
				buttonShowHide.Foreground 		= Brushes.Green;
				RemoveDrawObject("TargetHLine");
				RemoveDrawObject("TargetHLine2");
				RemoveDrawObject("TargetHLine3");
				RemoveDrawObject("EnterHLine");
				RemoveDrawObject("StopHLine");
				RemoveDrawObject("TargetRLine");
				RemoveDrawObject("TargetRLine2");
				RemoveDrawObject("TargetRLine3");
				RemoveDrawObject("EnterRLine");
				RemoveDrawObject("StopRLine");
				RemoveDrawObject("TargetDot");
				RemoveDrawObject("StopDot");
				
				buttonShowHideChecked = false;
				RetrievalDone = false;
				HLineDrawn= false; 
				EnterHLine = TargetHLine = TargetHLine2 = TargetHLine3 = StopHLine = null;
				EnterRLine = TargetRLine = TargetRLine2 = TargetRLine3 = StopRLine = null;
			}
			else
			{
				buttonShowHideChecked = true;
				buttonShowHide.Content 			= "Hide";
				buttonShowHide.Foreground 		= Brushes.Crimson;
				RetrieveSavedLines();					
			}
			buttonShowHide.Focusable = false;
			nullFocusItem.Focus();
			ForceRefresh();	
		}		
		#endregion		
	#endregion
	
	#region Tab Controls

		private bool TabSelected()
		{
			bool tabSelected = false;

			// loop through each tab and see if the tab this indicator is added to is the selected item
			foreach (System.Windows.Controls.TabItem tab in chartWindow.MainTabControl.Items)
				if ((tab.Content as ChartTab).ChartControl == ChartControl && tab == chartWindow.MainTabControl.SelectedItem)
					tabSelected = true;

			return tabSelected;
		}

		private void TabChangedHandler(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (e.AddedItems.Count <= 0)
				return;

			tabItem = e.AddedItems[0] as System.Windows.Controls.TabItem;
			if (tabItem == null)
				return;

			chartTab = tabItem.Content as NinjaTrader.Gui.Chart.ChartTab;
			if (chartTab == null)
				return;

			if (TabSelected())
				InsertWPFControls();
			else
				RemoveWPFControls();
		}

    #endregion

    #region SharpDX Helper Classes/Methods

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

        private void DrawPolygon(SharpDX.Vector2[] points, string brushName)
        {
            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
            SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());
            sink.AddLines(points);
            sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
            sink.Close();
			
            RenderTarget.DrawGeometry(geometry, dxmBrushes[brushName].DxBrush);
            geometry.Dispose();
        }

        private void FillPolygon(SharpDX.Vector2[] points, string brushName)
        {
            SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
            SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());
            sink.AddLines(points);
            sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
            sink.Close();
			
            RenderTarget.FillGeometry(geometry, dxmBrushes[brushName].DxBrush);
            geometry.Dispose();
        }
		
		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY).ToVector2();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, dxmBrushes[brushName].DxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}
		
		private void DrawLine(string brushName, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			if (dashStyle == DashStyleHelper.Dash)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash;
			if (dashStyle == DashStyleHelper.DashDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot;
			if (dashStyle == DashStyleHelper.DashDotDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;
			if (dashStyle == DashStyleHelper.Dot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;
			if (dashStyle == DashStyleHelper.Solid)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			SharpDX.Vector2 startPoint = new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint = new System.Windows.Point(x2, y2).ToVector2();
			RenderTarget.DrawLine(startPoint, endPoint, dxmBrushes[brushName].DxBrush, 2, strokeStyle);
		}
		
		private void DrawLine(string brushName, double x1, double y1, double x2, double y2, float width)
		{
			DrawLine(brushName, x1, y1, x2, y2, width, DashStyleHelper.Solid);
		}
    #endregion
    
    #region Properties		
        [NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CurrentPriceColor", Description="Price Tag Color", Order=1, GroupName="Parameters")]
		public Brush CurrentPriceColor
        {
            get { return PriceColor; }
            set { PriceColor = value; }
        }

        [Browsable(false)]
		public string CurrentPriceColorSerializable
		{
			get { return Serialize.BrushToString(PriceColor); }
			set { PriceColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="StopLineColor", Description="Stop Line Color", Order=2, GroupName="Parameters")]
		public Brush StopLineColor
        {
            get { return RedColor; }
            set { RedColor = value; }
        }

        [Browsable(false)]
		public string StopLineColorSerializable
		{
			get { return Serialize.BrushToString(RedColor); }
			set { RedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BeamColor", Description="The Color of the pointer", Order=3, GroupName="Parameters")]
		public Brush beamColor
        {
            get { return BeamColor; }
            set { BeamColor = value; }
        }

        [Browsable(false)]
		public string BeamColorSerializable
		{
			get { return Serialize.BrushToString(BeamColor); }
			set { BeamColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TagetLineColor", Description="Target Line Color", Order=4, GroupName="Parameters")]
		public Brush TagetLineColor
        {
            get { return GreenColor; }
            set { GreenColor = value; }
        }

        [Browsable(false)]
		public string TagetLineColorSerializable
		{
			get { return Serialize.BrushToString(GreenColor); }
			set { GreenColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="ExtendedLines", Description="Use extended or short lines", Order=5, GroupName="Parameters")]
		public bool Ray
		{
			get { return !ray; }
            set { ray = !value; }
		}

		[NinjaScriptProperty]
		[Display(Name="ShowOnStartup", Description="Show the entries on startup?", Order=6, GroupName="Parameters")]
		public bool ShowOnStartup
		{ get; set; }

		[ReadOnly(true)]
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Contracts", Description="If interactive mode is off, define how many contracts to trade (default 1), if interactive mode is on the quantity of number is based on Risk/Reward", Order=7, GroupName="Parameters")]
		public int Contracts
		{ get; set; }

		[ReadOnly(false)]
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="RiskPercent", Description="The amount you want to risk in percent", Order=8, GroupName="Parameters")]
		public double RiskPercent
		{ get; set; }

		[ReadOnly(false)]
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="AccountSize", Description="Account size in US$", Order=9, GroupName="Parameters")]
		public double AccountSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[System.ComponentModel.RefreshProperties(RefreshProperties.All)]
		[Display(Name="Interactive", Description="Interactive price change indication", Order=10, GroupName="Parameters")]
		public Interactive Interactive
        {
            get { return interactive; }
            set {
					interactive = value;
			
					bool ContNewValue = (value == Interactive.Yes? false:true );
					
					PropertyDescriptor ContDescriptor = TypeDescriptor.GetProperties(this.GetType())["Contracts"];
					PropertyDescriptor AccountDescriptor = TypeDescriptor.GetProperties(this.GetType())["AccountSize"];
					PropertyDescriptor RiskDescriptor = TypeDescriptor.GetProperties(this.GetType())["RiskPercent"];
					
					ReadOnlyAttribute ContAttrib = (ReadOnlyAttribute)ContDescriptor.Attributes[typeof(ReadOnlyAttribute)];
					ReadOnlyAttribute AccountAttrib = (ReadOnlyAttribute)AccountDescriptor.Attributes[typeof(ReadOnlyAttribute)];
					ReadOnlyAttribute RiskAttrib = (ReadOnlyAttribute)RiskDescriptor.Attributes[typeof(ReadOnlyAttribute)];
					
					FieldInfo ContisReadOnly = ContAttrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo AccountisReadOnly = AccountAttrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
					FieldInfo RiskisReadOnly = RiskAttrib.GetType().GetField("isReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
				
					ContisReadOnly.SetValue(ContAttrib, ContNewValue);
					AccountisReadOnly.SetValue(AccountAttrib, !ContNewValue);
					RiskisReadOnly.SetValue(RiskAttrib, !ContNewValue);
				}
			
        }
		
		
		[TypeConverter(typeof(FriendlyEnumConverter))] // Converts the enum to string values
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Enums normally automatically get a combo box, but we need to apply this specific editor so default value is automatically selected
		[Display(Name="Target Lines", Description="How many target lines to use?", Order=11, GroupName="Parameters")]
		public TargetLines TargetLines
        {
            get { return targetLines; }
            set { targetLines = value; }
        }

        public class FriendlyEnumConverter : TypeConverter
	    {
	        // Set the values to appear in the combo box
	        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	        {
	            List<string> values = new List<string>() { "One", "Three" };

	            return new StandardValuesCollection(values);
	        }

	        // map the value from "Friendly" string to MyEnum type
	        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	        {
	            string stringVal = value.ToString();
	            switch (stringVal)
	            {
	                case "One":
	                return TargetLines.One;
	                case "Three":
	                return TargetLines.Three;
	            }
	            return TargetLines.One;
	        }

	        // map the MyEnum type to "Friendly" string
	        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	        {
	            TargetLines stringVal = (TargetLines) Enum.Parse(typeof(TargetLines), value.ToString());
	            switch (stringVal)
	            {
	                case TargetLines.One:
	                return "One";
	                case TargetLines.Three:
	                return "Three";
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
		[Range(1, int.MaxValue)]
		[Display(Name="LinesWidth", Description="Size of lines", Order=12, GroupName="Parameters")]
		public int LinesWidth
		{ get; set; }
		
		
    #endregion
		
	}
}
#region Helper Enums
public enum Interactive
{
    Yes,
    No,
}
public enum CalculationsBasedOn
{
    User_Choice,
    Suggested_Contract_Qty,
}
public enum TargetLines
{
    One,
    Three,
}
#endregion


#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AdvancedRiskReward[] cacheAdvancedRiskReward;
		public AdvancedRiskReward AdvancedRiskReward(Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			return AdvancedRiskReward(Input, currentPriceColor, stopLineColor, beamColor, tagetLineColor, ray, showOnStartup, contracts, riskPercent, accountSize, interactive, linesWidth);
		}

		public AdvancedRiskReward AdvancedRiskReward(ISeries<double> input, Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			if (cacheAdvancedRiskReward != null)
				for (int idx = 0; idx < cacheAdvancedRiskReward.Length; idx++)
					if (cacheAdvancedRiskReward[idx] != null && cacheAdvancedRiskReward[idx].CurrentPriceColor == currentPriceColor && cacheAdvancedRiskReward[idx].StopLineColor == stopLineColor && cacheAdvancedRiskReward[idx].beamColor == beamColor && cacheAdvancedRiskReward[idx].TagetLineColor == tagetLineColor && cacheAdvancedRiskReward[idx].Ray == ray && cacheAdvancedRiskReward[idx].ShowOnStartup == showOnStartup && cacheAdvancedRiskReward[idx].Contracts == contracts && cacheAdvancedRiskReward[idx].RiskPercent == riskPercent && cacheAdvancedRiskReward[idx].AccountSize == accountSize && cacheAdvancedRiskReward[idx].Interactive == interactive && cacheAdvancedRiskReward[idx].LinesWidth == linesWidth && cacheAdvancedRiskReward[idx].EqualsInput(input))
						return cacheAdvancedRiskReward[idx];
			return CacheIndicator<AdvancedRiskReward>(new AdvancedRiskReward(){ CurrentPriceColor = currentPriceColor, StopLineColor = stopLineColor, beamColor = beamColor, TagetLineColor = tagetLineColor, Ray = ray, ShowOnStartup = showOnStartup, Contracts = contracts, RiskPercent = riskPercent, AccountSize = accountSize, Interactive = interactive, LinesWidth = linesWidth }, input, ref cacheAdvancedRiskReward);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AdvancedRiskReward AdvancedRiskReward(Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			return indicator.AdvancedRiskReward(Input, currentPriceColor, stopLineColor, beamColor, tagetLineColor, ray, showOnStartup, contracts, riskPercent, accountSize, interactive, linesWidth);
		}

		public Indicators.AdvancedRiskReward AdvancedRiskReward(ISeries<double> input , Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			return indicator.AdvancedRiskReward(input, currentPriceColor, stopLineColor, beamColor, tagetLineColor, ray, showOnStartup, contracts, riskPercent, accountSize, interactive, linesWidth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AdvancedRiskReward AdvancedRiskReward(Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			return indicator.AdvancedRiskReward(Input, currentPriceColor, stopLineColor, beamColor, tagetLineColor, ray, showOnStartup, contracts, riskPercent, accountSize, interactive, linesWidth);
		}

		public Indicators.AdvancedRiskReward AdvancedRiskReward(ISeries<double> input , Brush currentPriceColor, Brush stopLineColor, Brush beamColor, Brush tagetLineColor, bool ray, bool showOnStartup, int contracts, double riskPercent, double accountSize, Interactive interactive, int linesWidth)
		{
			return indicator.AdvancedRiskReward(input, currentPriceColor, stopLineColor, beamColor, tagetLineColor, ray, showOnStartup, contracts, riskPercent, accountSize, interactive, linesWidth);
		}
	}
}

#endregion
