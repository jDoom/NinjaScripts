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
	public class BollingerModified : Indicator
	{
		private double maValue;
		private double stdDevValue;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Bollinger Bands are plotted at standard deviation levels above and below a moving average. Since standard deviation is a measure of volatility, the bands are self-adjusting: widening during volatile markets and contracting during calmer periods.";
				Name										= "BollingerModified";
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
				
				NumStdDev									= 2;
				Period										= 14;
				BollingerBandMA								= TypeBBMA.SMA;
				AddPlot(Brushes.Orange, "UpperBand");
				AddPlot(Brushes.Orange, "MiddleBand");
				AddPlot(Brushes.Orange, "LowerBand");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			switch (BollingerBandMA) 
			{
				case TypeBBMA.EMA:
					maValue = EMA(Period)[0];
					break;
				case TypeBBMA.HMA:
					maValue = HMA(Period)[0];
					break;
				case TypeBBMA.SMA:
					maValue = SMA(Period)[0];
					break;
				case TypeBBMA.WMA:
					maValue = WMA(Period)[0];
					break;
				case TypeBBMA.VMA:
					maValue = VMA(Period,Period)[0];
					break;
				case TypeBBMA.DEMA:
					maValue = DEMA(Period)[0];
					break;
				case TypeBBMA.TMA:
					maValue = TMA(Period)[0];
					break;
				case TypeBBMA.ZLEMA:
					maValue = ZLEMA(Period)[0];
					break;
			}
		
			stdDevValue = StdDev(Period)[0];
            UpperBand[0] = (maValue + NumStdDev * stdDevValue);
            MiddleBand[0] = (maValue);
            LowerBand[0] = (maValue - NumStdDev * stdDevValue);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="NumStdDev", Description="Number of standard deviations", Order=1, GroupName="Parameters")]
		public double NumStdDev
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Numbers of bars used for calculations", Order=2, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="BollingerBandMA", Description="The type of moving average the BB is derived from", Order=3, GroupName="Parameters")]
		public TypeBBMA BollingerBandMA
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBand
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MiddleBand
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBand
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

public enum TypeBBMA
{
	DEMA,
	EMA,
	HMA,
	SMA,
	TMA,
	VMA,
	WMA,
	ZLEMA
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BollingerModified[] cacheBollingerModified;
		public BollingerModified BollingerModified(double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			return BollingerModified(Input, numStdDev, period, bollingerBandMA);
		}

		public BollingerModified BollingerModified(ISeries<double> input, double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			if (cacheBollingerModified != null)
				for (int idx = 0; idx < cacheBollingerModified.Length; idx++)
					if (cacheBollingerModified[idx] != null && cacheBollingerModified[idx].NumStdDev == numStdDev && cacheBollingerModified[idx].Period == period && cacheBollingerModified[idx].BollingerBandMA == bollingerBandMA && cacheBollingerModified[idx].EqualsInput(input))
						return cacheBollingerModified[idx];
			return CacheIndicator<BollingerModified>(new BollingerModified(){ NumStdDev = numStdDev, Period = period, BollingerBandMA = bollingerBandMA }, input, ref cacheBollingerModified);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BollingerModified BollingerModified(double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			return indicator.BollingerModified(Input, numStdDev, period, bollingerBandMA);
		}

		public Indicators.BollingerModified BollingerModified(ISeries<double> input , double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			return indicator.BollingerModified(input, numStdDev, period, bollingerBandMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BollingerModified BollingerModified(double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			return indicator.BollingerModified(Input, numStdDev, period, bollingerBandMA);
		}

		public Indicators.BollingerModified BollingerModified(ISeries<double> input , double numStdDev, int period, TypeBBMA bollingerBandMA)
		{
			return indicator.BollingerModified(input, numStdDev, period, bollingerBandMA);
		}
	}
}

#endregion
