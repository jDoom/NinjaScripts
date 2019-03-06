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
	public class BollingerBandsPerCent : Indicator
	{
		private double maValue;
		private double stdDevValue;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"This study plots the relative percentage that the price tracked has moved from the upper band to the lower band.";
				Name										= "BollingerBandsPerCent";
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
				Displace									= 0;
				Length										= 20;
				NumDevDn									= 2.0;
				NumDevUp									= 2.0;
				BollingerBandMA								= BBPCTypeMA.SMA;
				
				AddPlot(new Stroke(Brushes.RoyalBlue, 2), PlotStyle.Bar, "PercentB");
				AddLine(Brushes.Gray, 0, "ZeroLine");
				AddLine(Brushes.Gray, 50, "FiftyLine");
				AddLine(Brushes.Gray, 100, "OneHundredLine");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			switch (BollingerBandMA) 
			{
				case BBPCTypeMA.EMA:
					maValue    = EMA(Length)[0];
					break;
				case BBPCTypeMA.HMA:
					maValue    = HMA(Length)[0];
					break;
				case BBPCTypeMA.SMA:
					maValue    = SMA(Length)[0];
					break;
				case BBPCTypeMA.WMA:
					maValue    = WMA(Length)[0];
					break;
				case BBPCTypeMA.VMA:
					maValue    = VMA(Length,Length)[0];
					break;
				case BBPCTypeMA.DEMA:
					maValue    = DEMA(Length)[0];
					break;
				case BBPCTypeMA.TMA:
					maValue    = TMA(Length)[0];
					break;
				case BBPCTypeMA.ZLEMA:
					maValue    = ZLEMA(Length)[0];
					break;
				default:
					break;
			}
			
			stdDevValue = StdDev(Length)[0];
			double upperBand = maValue + NumDevUp * stdDevValue;
			double lowerBand = maValue - NumDevDn * stdDevValue;
            PercentB[0] = ((Input[Displace] - lowerBand) / (upperBand - lowerBand) * 100);
//			Print(PercentB[0] + "-----" + upperBand + "-----" + lowerBand + "-----" + Input[0]);

			if(IsRising(PercentB))
			{
				if(PercentB[0] > PercentB[Math.Min(CurrentBar,1)])  PlotBrushes[0][0] = Brushes.LimeGreen;
			}
			else if(IsFalling(PercentB))
			{
				if(PercentB[0] < PercentB[Math.Min(CurrentBar,1)])  PlotBrushes[0][0] = Brushes.Red;
			}
			else
				PlotBrushes[0][0] = Brushes.Gray;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(-10, int.MaxValue)]
		[Display(Name="Displace", Description="The number of bars to displace the plot.", Order=1, GroupName="Parameters")]
		public int Displace
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Length", Description="BB moving average periods.", Order=2, GroupName="Parameters")]
		public int Length
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="NumDevDn", Description="Std Dev Down", Order=3, GroupName="Parameters")]
		public double NumDevDn
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="NumDevUp", Description="Std Dev Up", Order=4, GroupName="Parameters")]
		public double NumDevUp
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="BollingerBandMA", Description="The type of moving average the BB is derived from", Order=5, GroupName="Parameters")]
		public BBPCTypeMA BollingerBandMA
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PercentB
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

public enum BBPCTypeMA
{
	SMA,
	EMA,
	WMA,
	HMA,
	DEMA,
	TMA,
	VMA,
	ZLEMA
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerBandsPerCent[] cacheBollingerBandsPerCent;
		public BollingerBandsPerCent BollingerBandsPerCent(int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			return BollingerBandsPerCent(Input, displace, length, numDevDn, numDevUp, bollingerBandMA);
		}

		public BollingerBandsPerCent BollingerBandsPerCent(ISeries<double> input, int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			if (cacheBollingerBandsPerCent != null)
				for (int idx = 0; idx < cacheBollingerBandsPerCent.Length; idx++)
					if (cacheBollingerBandsPerCent[idx] != null && cacheBollingerBandsPerCent[idx].Displace == displace && cacheBollingerBandsPerCent[idx].Length == length && cacheBollingerBandsPerCent[idx].NumDevDn == numDevDn && cacheBollingerBandsPerCent[idx].NumDevUp == numDevUp && cacheBollingerBandsPerCent[idx].BollingerBandMA == bollingerBandMA && cacheBollingerBandsPerCent[idx].EqualsInput(input))
						return cacheBollingerBandsPerCent[idx];
			return CacheIndicator<BollingerBandsPerCent>(new BollingerBandsPerCent(){ Displace = displace, Length = length, NumDevDn = numDevDn, NumDevUp = numDevUp, BollingerBandMA = bollingerBandMA }, input, ref cacheBollingerBandsPerCent);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerBandsPerCent BollingerBandsPerCent(int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			return indicator.BollingerBandsPerCent(Input, displace, length, numDevDn, numDevUp, bollingerBandMA);
		}

		public Indicators.BollingerBandsPerCent BollingerBandsPerCent(ISeries<double> input , int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			return indicator.BollingerBandsPerCent(input, displace, length, numDevDn, numDevUp, bollingerBandMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerBandsPerCent BollingerBandsPerCent(int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			return indicator.BollingerBandsPerCent(Input, displace, length, numDevDn, numDevUp, bollingerBandMA);
		}

		public Indicators.BollingerBandsPerCent BollingerBandsPerCent(ISeries<double> input , int displace, int length, double numDevDn, double numDevUp, BBPCTypeMA bollingerBandMA)
		{
			return indicator.BollingerBandsPerCent(input, displace, length, numDevDn, numDevUp, bollingerBandMA);
		}
	}
}

#endregion
