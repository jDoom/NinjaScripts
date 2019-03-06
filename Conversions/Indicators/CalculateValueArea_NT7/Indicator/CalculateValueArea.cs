#region Using declarations
using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    // <summary>
    // The Value Area is the price range where 70% of yesterdays volume traded
	// Written by Ben L. at sbgtrading@yahoo.com
	//   Theory taken from several sources, but summarized at: http://www.secretsoftraders.com/ValueAreaHelpGuide.htm
	// May God bless you and your trading!
	//
	
	//	Description of the "ProfileType" parameter:

	//		I've given the option of creating the profile in 3 different ways:
	//			1)  VOC - This method loads all the volume of a bar onto the closing price of that bar.
	//						e.g.  A 5-minute bar has a volume of 280 and a range of 1.5 points with a close at 1534.25, then
	//						all 280 hits of volume are loaded onto the 1534.25 closing price.
	//			2)  TPO - This method disregards volume altogether, and gives a single hit to each price in the range of the bar.
	//						e.g.  A 5-minute bar has a range of 1.5 points with a High at 1534 and a Low at 1532.5, then
	//						1 contract (or "hit") is loaded to the seven prices in that range: 1532.50, 1532.75, 1533.0, 1533.25, 1533.50, 1533.75, and 1534
	//			3)  VWTPO - This method distribues the volume of a bar over the price range of the bar.
	//						e.g.  A 5-minute bar has a volume of 280 and a range of 1.5 points with a High at 1534 and a Low at 1532.5, then
	//						40 contracts (=280/7) are loaded to each of the seven prices in that range: 1532.50, 1532.75, 1533.0, 1533.25, 1533.50, 1533.75, and 1534
	//
	// </summary>
    [Description("The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info")]
    public class CalculateValueArea : Indicator
    {
        #region Variables
        // Wizard generated variables
			private bool inclWeekendVol=false;
			private int openHour=8,openMinute=30;
			private double pctOfVolumeInVA = 0.70;
			private string profileType="VWTPO";
			private double sessionLengthInHours=6.75;

		// User defined variables (add any user defined variables below)
			private string path = Cbi.Core.UserDataDir.ToString()+"test.txt";
			private double[,] PriceHitsArray = new double[2,1000];
			private double TheSessionHigh,TheSessionLow;
			private string FormatString;
			private double VAtop=0.0,VAbot=0.0,PriceOfPOC=0.0,HitsTotal=0.0;
			private DateTime StartTime,EndTime;
			private bool InitializeEndTime;
			private int LastBarOfSession;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void Initialize()
        {
            Add(new Plot(Color.Yellow, PlotStyle.Dot, "VAt"));
            Add(new Plot(Color.Pink, PlotStyle.Dot, "VAb"));
            Add(new Plot(Color.Green, PlotStyle.Line, "POC"));
			AutoScale			= false;
            CalculateOnBarClose	= true;
            Overlay				= true;
            PriceTypeSupported	= false;
			InitializeEndTime = true;
        }
		
		protected override void OnStartUp()
		{
			if(TickSize.ToString().Length<=2) FormatString="0";
			if(TickSize.ToString().Length==3) FormatString="0.0";
			if(TickSize.ToString().Length==4) FormatString="0.00";
			if(TickSize.ToString().Length==5) FormatString="0.000";
			if(TickSize.ToString().Length==6) FormatString="0.0000";
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {			
			int i; 
			double AvgPrice=0.0;
			double Variance=0.0;
			double StandardDeviation=0.0;

			if(CurrentBar<10)	return;

			if(InitializeEndTime)
			{	if(Time[0].CompareTo(StartTime)>=0)
				{	InitializeEndTime=false;
					StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
					if(sessionLengthInHours>=24.0) EndTime = StartTime.AddHours(24.0-1.0/60.0);
					else EndTime = StartTime.AddHours(sessionLengthInHours);
				}
			}

			if(!InitializeEndTime && Time[1].CompareTo(EndTime)<=0 && Time[0].CompareTo(EndTime)>0)
			{	DetermineHighLowOfSession(StartTime);
				if(LastBarOfSession < 0) return;

				StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
				if(sessionLengthInHours>=24) EndTime = StartTime.AddHours(24.0-1.0/60.0);
				else EndTime = StartTime.AddHours(sessionLengthInHours);


				int TicksInRange = (int) Math.Round((TheSessionHigh-TheSessionLow)/TickSize,0);
				if (TicksInRange>=1000) Log("Potential data problem in CalculateValueArea at "+Time[0].ToString()+" Session H/L: "+TheSessionHigh.ToString(FormatString)+" / "+TheSessionLow.ToString(FormatString),LogLevel.Warning);
				if (TicksInRange<0) Log("Potential data problem in CalculateValueArea at "+Time[0].ToString()+" Session H/L: "+TheSessionHigh.ToString(FormatString)+" / "+TheSessionLow.ToString(FormatString),LogLevel.Warning);
				for(i=0;i<1000;i++)
				{ 	PriceHitsArray[0,i]=(i*TickSize+TheSessionLow); 
					PriceHitsArray[1,i]=0.0;
				}

				int index=0;
				i=1;
				while (i <= LastBarOfSession) //Accumulate the volume for each previous days bar into PriceVolume array
				{	if(!inclWeekendVol && (Time[i].DayOfWeek==DayOfWeek.Saturday || Time[i].DayOfWeek==DayOfWeek.Sunday))
						i++;
					else
					{
						if(profileType == "VOC") //Volume On Close - puts all the volume for that bar on the close price
						{	index = (int) Math.Round((Close[i]-TheSessionLow)/TickSize,0);
							PriceHitsArray[1,index] = PriceHitsArray[1,index] + Volume[i];
						}
						if (profileType == "TPO") //Time Price Opportunity - disregards volume, only counts number of times prices are touched
						{	double BarH=High[i]; double BarP=Low[i];
							while(BarP<=BarH+TickSize/2.0)
							{	index = (int) Math.Round((BarP-TheSessionLow)/TickSize,0);
								PriceHitsArray[1,index] = PriceHitsArray[1,index] + 1;
								BarP = BarP + TickSize;
							}
						}
						if (profileType == "VWTPO") //Volume Weighted Time Price Opportunity - Disperses the Volume of the bar over the range of the bar so each price touched is weighted with volume
						{	double BarH=High[i]; double BarP=Low[i];
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
				{	if(PriceHitsArray[1,i]>0.0)
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
				while(ViA/TV < pctOfVolumeInVA)
				{	VAbot = VAbot - Adj;
					VAtop = VAtop + Adj;
					ViA=0.0;
					TV=0.00001;
					for(i=0;i<1000;i++)
					{	if(PriceHitsArray[0,i]>VAbot-Adj && PriceHitsArray[0,i]<VAtop+Adj) ViA=PriceHitsArray[1,i]+ViA;
						TV=TV+PriceHitsArray[1,i];
					}
					Adj=TickSize;
				}
//DrawText("PctInValueArea",(ViA/TV).ToString("0.00"),50,TheSessionHigh,Color.Red);

			}

			if(VAtop>0.0)
			{	VAt.Set(VAtop);
				VAb.Set(VAbot);
				POC.Set(PriceOfPOC);
			}
			StartTime = new DateTime(Time[0].Year,Time[0].Month,Time[0].Day,OpenHour,OpenMinute,0,0,DateTimeKind.Utc);
			if(sessionLengthInHours>=24.0) EndTime = StartTime.AddHours(24.0-1.0/60.0);
			else EndTime = StartTime.AddHours(sessionLengthInHours);
		}

        #region Properties
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries VAt
        {
            get { return Values[0]; }
        }
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries VAb
        {
            get { return Values[1]; }
        }
        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
        public DataSeries POC
        {
            get { return Values[2]; }
        }

        [Description("Include Weekend Volume")]
        [Category("Parameters")]
        public bool InclWeekendVol
        {
            get { return inclWeekendVol; }
			set { inclWeekendVol = value;}
        }
        [Description("Type of profile VOC,TPO,VWTPO")]
        [Category("Parameters")]
        public string ProfileType
        {
            get { return profileType; }
			set { profileType = value.ToUpper();
				  if(profileType!="VOC" && profileType!="TPO" && profileType!="VWTPO") profileType="VWTPO";
				}
        }
        [Description("Percent of volume within Value Area")]
        [Category("Parameters")]
        public double PctOfVolumeInVA
        {
            get { return pctOfVolumeInVA; }
			set { pctOfVolumeInVA = Math.Max(0.01,Math.Min(1.0,value));}
        }
        [Description("Market open hour IN 24HR FORMAT")]
        [Category("Parameters")]
        public int OpenHour
        {
            get { return openHour; }
			set { openHour = Math.Max(0,value);}
        }
        [Description("Market open minute")]
        [Category("Parameters")]
        public int OpenMinute
        {
            get { return openMinute; }
			set { openMinute = Math.Max(0,value);}
        }
        [Description("Session length (in hours)")]
        [Category("Parameters")]
        public double SessionLengthInHours
        {
            get { return sessionLengthInHours; }
			set { sessionLengthInHours = value;}
        }
        #endregion
//===================================================================================
		private void DetermineHighLowOfSession(DateTime SessionStartTime)
		{	int i=1; //first bar to check is the bar immediately prior to currentbar
			TheSessionHigh = High[i];
			TheSessionLow = Low[i];
			LastBarOfSession = -1;
			while(i<CurrentBar-1 && Time[i].CompareTo(SessionStartTime)>0)
			{	if(!inclWeekendVol && (Time[i].DayOfWeek==DayOfWeek.Saturday || Time[i].DayOfWeek==DayOfWeek.Sunday))
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
//===================================================================================

	}
}

#region NinjaScript generated code. Neither change nor remove.
// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.Indicator
{
    public partial class Indicator : IndicatorBase
    {
        private CalculateValueArea[] cacheCalculateValueArea = null;

        private static CalculateValueArea checkCalculateValueArea = new CalculateValueArea();

        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        public CalculateValueArea CalculateValueArea(bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            return CalculateValueArea(Input, inclWeekendVol, openHour, openMinute, pctOfVolumeInVA, profileType, sessionLengthInHours);
        }

        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        public CalculateValueArea CalculateValueArea(Data.IDataSeries input, bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            if (cacheCalculateValueArea != null)
                for (int idx = 0; idx < cacheCalculateValueArea.Length; idx++)
                    if (cacheCalculateValueArea[idx].InclWeekendVol == inclWeekendVol && cacheCalculateValueArea[idx].OpenHour == openHour && cacheCalculateValueArea[idx].OpenMinute == openMinute && Math.Abs(cacheCalculateValueArea[idx].PctOfVolumeInVA - pctOfVolumeInVA) <= double.Epsilon && cacheCalculateValueArea[idx].ProfileType == profileType && Math.Abs(cacheCalculateValueArea[idx].SessionLengthInHours - sessionLengthInHours) <= double.Epsilon && cacheCalculateValueArea[idx].EqualsInput(input))
                        return cacheCalculateValueArea[idx];

            lock (checkCalculateValueArea)
            {
                checkCalculateValueArea.InclWeekendVol = inclWeekendVol;
                inclWeekendVol = checkCalculateValueArea.InclWeekendVol;
                checkCalculateValueArea.OpenHour = openHour;
                openHour = checkCalculateValueArea.OpenHour;
                checkCalculateValueArea.OpenMinute = openMinute;
                openMinute = checkCalculateValueArea.OpenMinute;
                checkCalculateValueArea.PctOfVolumeInVA = pctOfVolumeInVA;
                pctOfVolumeInVA = checkCalculateValueArea.PctOfVolumeInVA;
                checkCalculateValueArea.ProfileType = profileType;
                profileType = checkCalculateValueArea.ProfileType;
                checkCalculateValueArea.SessionLengthInHours = sessionLengthInHours;
                sessionLengthInHours = checkCalculateValueArea.SessionLengthInHours;

                if (cacheCalculateValueArea != null)
                    for (int idx = 0; idx < cacheCalculateValueArea.Length; idx++)
                        if (cacheCalculateValueArea[idx].InclWeekendVol == inclWeekendVol && cacheCalculateValueArea[idx].OpenHour == openHour && cacheCalculateValueArea[idx].OpenMinute == openMinute && Math.Abs(cacheCalculateValueArea[idx].PctOfVolumeInVA - pctOfVolumeInVA) <= double.Epsilon && cacheCalculateValueArea[idx].ProfileType == profileType && Math.Abs(cacheCalculateValueArea[idx].SessionLengthInHours - sessionLengthInHours) <= double.Epsilon && cacheCalculateValueArea[idx].EqualsInput(input))
                            return cacheCalculateValueArea[idx];

                CalculateValueArea indicator = new CalculateValueArea();
                indicator.BarsRequired = BarsRequired;
                indicator.CalculateOnBarClose = CalculateOnBarClose;
#if NT7
                indicator.ForceMaximumBarsLookBack256 = ForceMaximumBarsLookBack256;
                indicator.MaximumBarsLookBack = MaximumBarsLookBack;
#endif
                indicator.Input = input;
                indicator.InclWeekendVol = inclWeekendVol;
                indicator.OpenHour = openHour;
                indicator.OpenMinute = openMinute;
                indicator.PctOfVolumeInVA = pctOfVolumeInVA;
                indicator.ProfileType = profileType;
                indicator.SessionLengthInHours = sessionLengthInHours;
                Indicators.Add(indicator);
                indicator.SetUp();

                CalculateValueArea[] tmp = new CalculateValueArea[cacheCalculateValueArea == null ? 1 : cacheCalculateValueArea.Length + 1];
                if (cacheCalculateValueArea != null)
                    cacheCalculateValueArea.CopyTo(tmp, 0);
                tmp[tmp.Length - 1] = indicator;
                cacheCalculateValueArea = tmp;
                return indicator;
            }
        }
    }
}

// This namespace holds all market analyzer column definitions and is required. Do not change it.
namespace NinjaTrader.MarketAnalyzer
{
    public partial class Column : ColumnBase
    {
        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.CalculateValueArea CalculateValueArea(bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            return _indicator.CalculateValueArea(Input, inclWeekendVol, openHour, openMinute, pctOfVolumeInVA, profileType, sessionLengthInHours);
        }

        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        public Indicator.CalculateValueArea CalculateValueArea(Data.IDataSeries input, bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            return _indicator.CalculateValueArea(input, inclWeekendVol, openHour, openMinute, pctOfVolumeInVA, profileType, sessionLengthInHours);
        }
    }
}

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    public partial class Strategy : StrategyBase
    {
        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        [Gui.Design.WizardCondition("Indicator")]
        public Indicator.CalculateValueArea CalculateValueArea(bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            return _indicator.CalculateValueArea(Input, inclWeekendVol, openHour, openMinute, pctOfVolumeInVA, profileType, sessionLengthInHours);
        }

        /// <summary>
        /// The Value Area is the price range where 70% of yesterdays volume traded.  See Larry Levin www.secretsoftraders.com/ for more info
        /// </summary>
        /// <returns></returns>
        public Indicator.CalculateValueArea CalculateValueArea(Data.IDataSeries input, bool inclWeekendVol, int openHour, int openMinute, double pctOfVolumeInVA, string profileType, double sessionLengthInHours)
        {
            if (InInitialize && input == null)
                throw new ArgumentException("You only can access an indicator with the default input/bar series from within the 'Initialize()' method");

            return _indicator.CalculateValueArea(input, inclWeekendVol, openHour, openMinute, pctOfVolumeInVA, profileType, sessionLengthInHours);
        }
    }
}
#endregion
