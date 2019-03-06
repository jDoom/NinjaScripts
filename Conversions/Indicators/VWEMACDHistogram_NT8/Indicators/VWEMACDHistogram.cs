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
	public class VWEMACDHistogram : Indicator
	{
		private double longMA		= 0; // These variables are used to store intermediate calculations.
		private double shortMA 		= 0;
		private double signalLine 	= 0;
		
		// This indicator has two intermediate Series<double> in addition to the three doubles for holding values.
		private Series<double> volumeClose;
		private Series<double> vMACD;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volume weighted exponential moving average convergence divergence indicator from the October 2009 issue of Stocks & Commodities.";
				Name										= "VWEMACDHistogram";
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
				FastPeriod					= 12;
				SlowPeriod					= 26;
				SmoothingPeriod					= 9;
				AddPlot(new Stroke(Brushes.White), PlotStyle.Bar, "Histogram");
				AddLine(Brushes.Silver, 0, "ZeroLine");
			}
			else if (State == State.Configure)
			{
				volumeClose = new Series<double>(this);
				vMACD 		= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			// A key idea of this indicator is the current volume multiplied by the current price. See the article for details.
			volumeClose[0] = Volume[0] * Close[0];
			
			// Break up the calculations for vMACD
			longMA  = (SlowPeriod * EMA(volumeClose, SlowPeriod)[0]) / (SlowPeriod * EMA(Volume, SlowPeriod)[0]);
			shortMA = (FastPeriod * EMA(volumeClose, FastPeriod)[0]) / (FastPeriod * EMA(Volume, FastPeriod)[0]);

			// Set the intermediate values.
			vMACD[0] = shortMA - longMA;
			signalLine = EMA(vMACD, SmoothingPeriod)[0];
			
			// Plot the final result.
            Histogram[0] = vMACD[0] - signalLine;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastPeriod", Order=1, GroupName="Parameters")]
		public int FastPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowPeriod", Order=2, GroupName="Parameters")]
		public int SlowPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmoothingPeriod", Order=3, GroupName="Parameters")]
		public int SmoothingPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Histogram
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
		private VWEMACDHistogram[] cacheVWEMACDHistogram;
		public VWEMACDHistogram VWEMACDHistogram(int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			return VWEMACDHistogram(Input, fastPeriod, slowPeriod, smoothingPeriod);
		}

		public VWEMACDHistogram VWEMACDHistogram(ISeries<double> input, int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			if (cacheVWEMACDHistogram != null)
				for (int idx = 0; idx < cacheVWEMACDHistogram.Length; idx++)
					if (cacheVWEMACDHistogram[idx] != null && cacheVWEMACDHistogram[idx].FastPeriod == fastPeriod && cacheVWEMACDHistogram[idx].SlowPeriod == slowPeriod && cacheVWEMACDHistogram[idx].SmoothingPeriod == smoothingPeriod && cacheVWEMACDHistogram[idx].EqualsInput(input))
						return cacheVWEMACDHistogram[idx];
			return CacheIndicator<VWEMACDHistogram>(new VWEMACDHistogram(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod, SmoothingPeriod = smoothingPeriod }, input, ref cacheVWEMACDHistogram);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VWEMACDHistogram VWEMACDHistogram(int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			return indicator.VWEMACDHistogram(Input, fastPeriod, slowPeriod, smoothingPeriod);
		}

		public Indicators.VWEMACDHistogram VWEMACDHistogram(ISeries<double> input , int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			return indicator.VWEMACDHistogram(input, fastPeriod, slowPeriod, smoothingPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VWEMACDHistogram VWEMACDHistogram(int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			return indicator.VWEMACDHistogram(Input, fastPeriod, slowPeriod, smoothingPeriod);
		}

		public Indicators.VWEMACDHistogram VWEMACDHistogram(ISeries<double> input , int fastPeriod, int slowPeriod, int smoothingPeriod)
		{
			return indicator.VWEMACDHistogram(input, fastPeriod, slowPeriod, smoothingPeriod);
		}
	}
}

#endregion
