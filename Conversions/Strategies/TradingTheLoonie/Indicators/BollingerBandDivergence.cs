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
	public class BollingerBandDivergence : Indicator
	{
		private double sec1BOL = 0;
		private double sec2BOL = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BollingerBandDivergence";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				BBDays										= 20;
				
				AddPlot(Brushes.Orange, "Divergence");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("CL 01-18", Data.BarsPeriodType.Day, 1);
				//AddDataSeries("CL 01-18", new BarsPeriod { BarsPeriodType = BarsPeriodType.Minute, Value = 1440 }, "Default 24 x 7");
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsPeriod.BarsPeriodType != BarsPeriodType.Day && !(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value == 1440))
			{
				Draw.TextFixed(this, "NinjaScriptInfo", "This indicator must be applied to a daily series", TextPosition.BottomRight);
				return;
			}
			
			if (CurrentBars[0] < BBDays || CurrentBars[1] < BBDays)
				return;
			
			if (BarsInProgress == 0)
				sec1BOL = 1 + ((Closes[0][0] - SMA(Closes[0], BBDays)[0] + 2 * StdDev(Closes[0], BBDays)[0]) / (4 * StdDev(Closes[0], BBDays)[0] + .0001));
			
			if (BarsInProgress == 1)
				sec2BOL = 1 + ((Closes[1][0] - SMA(Closes[1], BBDays)[0] + 2 * StdDev(Closes[1], BBDays)[0]) / (4 * StdDev(Closes[1], BBDays)[0] + .0001));
			
			Divergence[0] = (sec2BOL - sec1BOL) / sec1BOL * 100;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, 200)]
		[Display(Name="BBDays", Order=1, GroupName="Parameters")]
		public int BBDays
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Divergence
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerBandDivergence[] cacheBollingerBandDivergence;
		public BollingerBandDivergence BollingerBandDivergence(int bBDays)
		{
			return BollingerBandDivergence(Input, bBDays);
		}

		public BollingerBandDivergence BollingerBandDivergence(ISeries<double> input, int bBDays)
		{
			if (cacheBollingerBandDivergence != null)
				for (int idx = 0; idx < cacheBollingerBandDivergence.Length; idx++)
					if (cacheBollingerBandDivergence[idx] != null && cacheBollingerBandDivergence[idx].BBDays == bBDays && cacheBollingerBandDivergence[idx].EqualsInput(input))
						return cacheBollingerBandDivergence[idx];
			return CacheIndicator<BollingerBandDivergence>(new BollingerBandDivergence(){ BBDays = bBDays }, input, ref cacheBollingerBandDivergence);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandDivergence BollingerBandDivergence(int bBDays)
		{
			return indicator.BollingerBandDivergence(Input, bBDays);
		}

		public Indicators.BollingerBandDivergence BollingerBandDivergence(ISeries<double> input , int bBDays)
		{
			return indicator.BollingerBandDivergence(input, bBDays);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandDivergence BollingerBandDivergence(int bBDays)
		{
			return indicator.BollingerBandDivergence(Input, bBDays);
		}

		public Indicators.BollingerBandDivergence BollingerBandDivergence(ISeries<double> input , int bBDays)
		{
			return indicator.BollingerBandDivergence(input, bBDays);
		}
	}
}

#endregion
