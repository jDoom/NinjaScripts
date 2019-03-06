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
	public class CalculateValueArea : Indicator
	{
		private string path = NinjaTrader.Core.Globals.UserDataDir.ToString()+"test.txt";
		private double[,] PriceHitsArray = new double[2,1000];
		private double TheSessionHigh,TheSessionLow;
		private string FormatString;
		private double VAtop=0.0,VAbot=0.0,PriceOfPOC=0.0,HitsTotal=0.0;
		private DateTime StartTime,EndTime;
		private bool InitializeEndTime;
		private int LastBarOfSession;
		
		protected override void OnStateChange()
		{			
			if (State == State.SetDefaults)
			{
				Description									= @"The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info";
				Name										= "CalculateValueArea";
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
				InclWeekendVol								= false;
				ProfileType									= @"VWTPO";
				PctOfVolumeInVA								= 0.7;
				OpenHour									= 8;
				OpenMinute									= 30;
				SessionLengthInHours						= 6.75;
				
				AddPlot(new Stroke(Brushes.Yellow, 1), PlotStyle.Dot, "VAt");
				AddPlot(new Stroke(Brushes.Pink, 1), PlotStyle.Dot, "VAb");
				AddPlot(new Stroke(Brushes.Green, 1), PlotStyle.Line, "POC");
			}
			else if (State == State.DataLoaded)
			{
				if(TickSize.ToString().Length<=2) FormatString="0";
				if(TickSize.ToString().Length==3) FormatString="0.0";
				if(TickSize.ToString().Length==4) FormatString="0.00";
				if(TickSize.ToString().Length==5) FormatString="0.000";
				if(TickSize.ToString().Length==6) FormatString="0.0000";
			}
		}

		protected override void OnBarUpdate()
        {			
			int i; 
			double AvgPrice=0.0;
			double Variance=0.0;
			double StandardDeviation=0.0;

			if(CurrentBar<10)	return;

			if(InitializeEndTime)
			{	
				if(Time[0].CompareTo(StartTime)>=0)
				{	
					InitializeEndTime=false;
					StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
					if(SessionLengthInHours>=24.0) EndTime = StartTime.AddHours(24.0-1.0/60.0);
					else EndTime = StartTime.AddHours(SessionLengthInHours);
				}
			}

			if(!InitializeEndTime && Time[1].CompareTo(EndTime)<=0 && Time[0].CompareTo(EndTime)>0)
			{	
				DetermineHighLowOfSession(StartTime);
				if(LastBarOfSession < 0) return;

				StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
				if(SessionLengthInHours>=24) EndTime = StartTime.AddHours(24.0-1.0/60.0);
				else EndTime = StartTime.AddHours(SessionLengthInHours);


				int TicksInRange = (int) Math.Round((TheSessionHigh-TheSessionLow)/TickSize,0);
				if (TicksInRange>=1000) Log("Potential data problem in CalculateValueArea at "+Time[0].ToString()+" Session H/L: "+TheSessionHigh.ToString(FormatString)+" / "+TheSessionLow.ToString(FormatString),LogLevel.Warning);
				if (TicksInRange<0) Log("Potential data problem in CalculateValueArea at "+Time[0].ToString()+" Session H/L: "+TheSessionHigh.ToString(FormatString)+" / "+TheSessionLow.ToString(FormatString),LogLevel.Warning);
				
				try
				{
					for(i=0;i<1000;i++)
					{ 	PriceHitsArray[0,i]=(i*TickSize+TheSessionLow); 
						PriceHitsArray[1,i]=0.0;
					}

					int index=0;
					i=1;
					while (i <= LastBarOfSession) //Accumulate the volume for each previous days bar into PriceVolume array
					{	
						if(!InclWeekendVol && (Time[i].DayOfWeek==DayOfWeek.Saturday || Time[i].DayOfWeek==DayOfWeek.Sunday))
							i++;
						else
						{
							if(ProfileType == "VOC") //Volume On Close - puts all the volume for that bar on the close price
							{	
								index = (int) Math.Round((Close[i]-TheSessionLow)/TickSize,0);
								PriceHitsArray[1,index] = PriceHitsArray[1,index] + Volume[i];
							}
							if (ProfileType == "TPO") //Time Price Opportunity - disregards volume, only counts number of times prices are touched
							{	
								double BarH=High[i]; double BarP=Low[i];
								while(BarP<=BarH+TickSize/2.0)
								{	index = (int) Math.Round((BarP-TheSessionLow)/TickSize,0);
									PriceHitsArray[1,index] = PriceHitsArray[1,index] + 1;
									BarP = BarP + TickSize;
								}
							}
							if (ProfileType == "VWTPO") //Volume Weighted Time Price Opportunity - Disperses the Volume of the bar over the range of the bar so each price touched is weighted with volume
							{	
								double BarH=High[i]; double BarP=Low[i];
								int TicksInBar = (int) Math.Round((BarH-Low[i])/TickSize+1,0);
								while(BarP<=BarH+TickSize/2.0)
								{	index = (int) Math.Round((BarP-TheSessionLow)/TickSize,0);
									PriceHitsArray[1,index] = PriceHitsArray[1,index] + Volume[i]/TicksInBar;
									BarP = BarP + TickSize;
								}
							}
							i++;
						}
					}

			//Calculate the Average price as weighted by the hit counts AND find the price with the highest hits (POC price)
					i=0;
					double THxP=0.0; //Total of Hits multiplied by Price at that volume
					HitsTotal=0.0;
					PriceOfPOC=0.0;
					double MaxHits=0.0;
					while(i<=TicksInRange) //Sum up Volume*Price in THxP...and sum up Volume in VolumeTotal
					{	
						if(PriceHitsArray[1,i]>0.0)
						{	THxP = THxP + PriceHitsArray[1,i] * PriceHitsArray[0,i];
							HitsTotal = HitsTotal + PriceHitsArray[1,i];
							if(PriceHitsArray[1,i] > MaxHits) //used to determine POC level
							{	MaxHits = PriceHitsArray[1,i]; 
								PriceOfPOC = PriceHitsArray[0,i]; 
							}
						}
						i++;
					}
					AvgPrice = THxP/HitsTotal;

					VAtop=AvgPrice;
					VAbot=AvgPrice;

					double ViA=0.0; //This loop calculates the percentage of hits contained within the Value Area
					double TV=0.00001;
					double Adj=0.0;
					while(ViA/TV < PctOfVolumeInVA)
					{	
						VAbot = VAbot - Adj;
						VAtop = VAtop + Adj;
						ViA=0.0;
						TV=0.00001;
						for(i=0;i<1000;i++)
						{	
							if(PriceHitsArray[0,i]>VAbot-Adj && PriceHitsArray[0,i]<VAtop+Adj) ViA=PriceHitsArray[1,i]+ViA;
							TV=TV+PriceHitsArray[1,i];
						}
						Adj=TickSize;
					}
				}
				catch(Exception e)
				{
					Print(e);	
				}
//DrawText("PctInValueArea",(ViA/TV).ToString("0.00"),50,TheSessionHigh,Color.Red);

			}

			if(VAtop>0.0)
			{	
				VAt[0] = (VAtop);
				VAb[0] = (VAbot);
				POC[0] = (PriceOfPOC);
			}
			StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
			if(SessionLengthInHours>=24.0) EndTime = StartTime.AddHours(24.0-1.0/60.0);
			else EndTime = StartTime.AddHours(SessionLengthInHours);
		}
		
		private void DetermineHighLowOfSession(DateTime SessionStartTime)
		{	
			int i=1; //first bar to check is the bar immediately prior to currentbar
			TheSessionHigh = High[i];
			TheSessionLow = Low[i];
			LastBarOfSession = -1;
			while(i<CurrentBar-1 && Time[i].CompareTo(SessionStartTime)>0)
			{	
				if(!InclWeekendVol && (Time[i].DayOfWeek==DayOfWeek.Saturday || Time[i].DayOfWeek==DayOfWeek.Sunday))
					i++;
				else
				{
					if(High[i] > TheSessionHigh) TheSessionHigh =High[i];
					if(Low[i] < TheSessionLow)   TheSessionLow  =Low[i];
					LastBarOfSession = i;
					i++;
				}
			}
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VAt
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VAb
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> POC
		{
			get { return Values[2]; }
		}
		
		[NinjaScriptProperty]
		[Display(Name="InclWeekendVol", Description="Include Weekend Volume", Order=1, GroupName="Parameters")]
		public bool InclWeekendVol
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ProfileType", Description="Type of profile VOC,TPO,VWTPO", Order=2, GroupName="Parameters")]
		public string ProfileType
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0.01, double.MaxValue)]
		[Display(Name="PctOfVolumeInVA", Description="Percent of volume within Value Area", Order=3, GroupName="Parameters")]
		public double PctOfVolumeInVA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="OpenHour", Description="Market open hour IN 24HR FORMAT", Order=4, GroupName="Parameters")]
		public int OpenHour
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="OpenMinute", Description="Market open minute", Order=5, GroupName="Parameters")]
		public int OpenMinute
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="SessionLengthInHours", Description="Session length (in hours)", Order=6, GroupName="Parameters")]
		public double SessionLengthInHours
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CalculateValueArea[] cacheCalculateValueArea;
		public CalculateValueArea CalculateValueArea(bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			return CalculateValueArea(Input, inclWeekendVol, profileType, pctOfVolumeInVA, openHour, openMinute, sessionLengthInHours);
		}

		public CalculateValueArea CalculateValueArea(ISeries<double> input, bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			if (cacheCalculateValueArea != null)
				for (int idx = 0; idx < cacheCalculateValueArea.Length; idx++)
					if (cacheCalculateValueArea[idx] != null && cacheCalculateValueArea[idx].InclWeekendVol == inclWeekendVol && cacheCalculateValueArea[idx].ProfileType == profileType && cacheCalculateValueArea[idx].PctOfVolumeInVA == pctOfVolumeInVA && cacheCalculateValueArea[idx].OpenHour == openHour && cacheCalculateValueArea[idx].OpenMinute == openMinute && cacheCalculateValueArea[idx].SessionLengthInHours == sessionLengthInHours && cacheCalculateValueArea[idx].EqualsInput(input))
						return cacheCalculateValueArea[idx];
			return CacheIndicator<CalculateValueArea>(new CalculateValueArea(){ InclWeekendVol = inclWeekendVol, ProfileType = profileType, PctOfVolumeInVA = pctOfVolumeInVA, OpenHour = openHour, OpenMinute = openMinute, SessionLengthInHours = sessionLengthInHours }, input, ref cacheCalculateValueArea);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CalculateValueArea CalculateValueArea(bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			return indicator.CalculateValueArea(Input, inclWeekendVol, profileType, pctOfVolumeInVA, openHour, openMinute, sessionLengthInHours);
		}

		public Indicators.CalculateValueArea CalculateValueArea(ISeries<double> input , bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			return indicator.CalculateValueArea(input, inclWeekendVol, profileType, pctOfVolumeInVA, openHour, openMinute, sessionLengthInHours);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CalculateValueArea CalculateValueArea(bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			return indicator.CalculateValueArea(Input, inclWeekendVol, profileType, pctOfVolumeInVA, openHour, openMinute, sessionLengthInHours);
		}

		public Indicators.CalculateValueArea CalculateValueArea(ISeries<double> input , bool inclWeekendVol, string profileType, double pctOfVolumeInVA, int openHour, int openMinute, double sessionLengthInHours)
		{
			return indicator.CalculateValueArea(input, inclWeekendVol, profileType, pctOfVolumeInVA, openHour, openMinute, sessionLengthInHours);
		}
	}
}

#endregion
