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
	public class MACDZeroLagColors : Indicator
	{
		private Series<double> fastEma;
		private Series<double> diffArr;
		private Series<double> macdAvg2;
		private Series<double> signal;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Conversion of Big Mike's Zero lag MACD indicator from Ninjatrader 6.5 and NinjaTrader 7";
				Name										= "MACDZeroLagColors";
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
				
				Fast										= 12;
				Slow										= 26;
				Smooth										= 9;
				Threshold									= 0;
				Acceleration								= 1;
				
				AddPlot(new Stroke(Brushes.LightSteelBlue, 						3), PlotStyle.Line, "Macd");
				AddPlot(new Stroke(Brushes.RoyalBlue, 							6), PlotStyle.Hash, "MacdUp");
				AddPlot(new Stroke(Brushes.DarkRed, 							6), PlotStyle.Hash, "MacdDn");
				AddPlot(new Stroke(Brushes.Yellow, 								6), PlotStyle.Hash, "MacdNeutral");
				AddPlot(new Stroke(Brushes.DarkViolet, DashStyleHelper.Dash, 	1), PlotStyle.Line, "Avg");
				AddPlot(new Stroke(Brushes.DimGray, 							2), PlotStyle.Bar, 	"Diff");
				AddPlot(new Stroke(Brushes.Teal, 								2), PlotStyle.Line, "ADX");
				
			}
			else if (State == State.DataLoaded)
			{				
				fastEma  = new Series<double>(this);
				diffArr  = new Series<double>(this);
				macdAvg2 = new Series<double>(this);
				signal   = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				fastEma[0] 	= Input[0];
				macdAvg2[0] = Input[0];
				Value[0] 	= 0;
				Avg[0] 		= 0;
				Diff[0] 	= 0;
			}
			else
			{
				fastEma[0] 	= (ZeroLagEMA(Input, (int)(Fast / Acceleration))[0]) - (ZeroLagEMA(Input, (int)(Slow / Acceleration))[0]);
				double macd = fastEma[0] ;
				
				macdAvg2[0] 	= ZeroLagEMA(fastEma,Smooth)[0];
				double macdAvg 	= macdAvg2[0];
				
				Value[0] 	= macd;
				Avg[0] 		= macdAvg;
				ADX[0] 		= ADXVMA(fastEma, (int)(Fast / Acceleration))[0];
				
				Diff[0] 	= macd - macdAvg;
	
				if ((Value[0] > ADX[0]) && (Value[0] > Threshold))
				{ 
					MacdUp[0] = 0; 
					MacdDn.Reset();
					MacdNeutral.Reset();
					signal[0] = 1; 
				}
				else
				{
					if ((Value[0] < ADX[0]) && (Value[0] < -Threshold))
					{ 
						MacdDn[0] = 0;
						MacdUp.Reset();
						MacdNeutral.Reset();
						signal[0] = -1; 
					}
					else
					{ 
						MacdNeutral[0] = 0; 
						MacdDn.Reset();
						MacdUp.Reset();
						signal[0] = 0; 
					}
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Fast", Description="Number of bars for fast EMA", Order=1, GroupName="Parameters")]
		public int Fast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Slow", Description="Number of bars for slow EMA", Order=2, GroupName="Parameters")]
		public int Slow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Smooth", Description="Number of bars for smoothing", Order=3, GroupName="Parameters")]
		public int Smooth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Threshold", Description="Threshold for zero line", Order=4, GroupName="Parameters")]
		public int Threshold
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="Acceleration", Description="Acceleration of fast and slow. Typical values might be 0.5, 1, 1.5, or 2.0", Order=5, GroupName="Parameters")]
		public double Acceleration
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Macd
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MacdUp
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MacdDn
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MacdNeutral
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Avg
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Diff
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADX
		{
			get { return Values[6]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Signal
		{
			get { return signal; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MACDZeroLagColors[] cacheMACDZeroLagColors;
		public MACDZeroLagColors MACDZeroLagColors(int fast, int slow, int smooth, int threshold, double acceleration)
		{
			return MACDZeroLagColors(Input, fast, slow, smooth, threshold, acceleration);
		}

		public MACDZeroLagColors MACDZeroLagColors(ISeries<double> input, int fast, int slow, int smooth, int threshold, double acceleration)
		{
			if (cacheMACDZeroLagColors != null)
				for (int idx = 0; idx < cacheMACDZeroLagColors.Length; idx++)
					if (cacheMACDZeroLagColors[idx] != null && cacheMACDZeroLagColors[idx].Fast == fast && cacheMACDZeroLagColors[idx].Slow == slow && cacheMACDZeroLagColors[idx].Smooth == smooth && cacheMACDZeroLagColors[idx].Threshold == threshold && cacheMACDZeroLagColors[idx].Acceleration == acceleration && cacheMACDZeroLagColors[idx].EqualsInput(input))
						return cacheMACDZeroLagColors[idx];
			return CacheIndicator<MACDZeroLagColors>(new MACDZeroLagColors(){ Fast = fast, Slow = slow, Smooth = smooth, Threshold = threshold, Acceleration = acceleration }, input, ref cacheMACDZeroLagColors);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MACDZeroLagColors MACDZeroLagColors(int fast, int slow, int smooth, int threshold, double acceleration)
		{
			return indicator.MACDZeroLagColors(Input, fast, slow, smooth, threshold, acceleration);
		}

		public Indicators.MACDZeroLagColors MACDZeroLagColors(ISeries<double> input , int fast, int slow, int smooth, int threshold, double acceleration)
		{
			return indicator.MACDZeroLagColors(input, fast, slow, smooth, threshold, acceleration);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MACDZeroLagColors MACDZeroLagColors(int fast, int slow, int smooth, int threshold, double acceleration)
		{
			return indicator.MACDZeroLagColors(Input, fast, slow, smooth, threshold, acceleration);
		}

		public Indicators.MACDZeroLagColors MACDZeroLagColors(ISeries<double> input , int fast, int slow, int smooth, int threshold, double acceleration)
		{
			return indicator.MACDZeroLagColors(input, fast, slow, smooth, threshold, acceleration);
		}
	}
}

#endregion
