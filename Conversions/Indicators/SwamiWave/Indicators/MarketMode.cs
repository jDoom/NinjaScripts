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
	public class MarketMode : Indicator
	{
		private Series<double> BP;
		private Series<double> Peak;
		private Series<double> Valley;		
		private double alpha, beta, gamma, avgPeak, avgValley, mean;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The Empirical Mode Decomposition as described in the April 2012 issue of Stocks & Commodities.";
				Name										= "MarketMode";
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
				Period										= 20;
				Delta										= 0.5;
				Fraction									= 0.1;
				AddPlot(Brushes.RoyalBlue, "UpperBand");
				AddPlot(Brushes.RoyalBlue, "LowerBand");
				AddPlot(Brushes.Red, "MainPlot");
			}
			else if (State == State.DataLoaded)
			{				
				BP = new Series<double>(this);
				Peak = new Series<double>(this);
				Valley = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				beta = Math.Cos(360 / Period);
				gamma = 1 / Math.Cos(720 * Delta / Period);
				alpha = gamma - Math.Sqrt(Math.Pow(gamma, 2) - 1);
			}
			
			if (CurrentBar < 51)
			{
				BP[0] = (0);
				Peak[0] = (0);
				Valley[0] = (0);
				return;
			}

            BP[0] = (0.5 * (1 - alpha) * (((High[0] + Low[0]) / 2) - ((High[2] + Low[2]) / 2)) + beta * (1 + alpha) * BP[1] - alpha * BP[2]);
			
			mean = SMA(BP, 2*Period)[0];
			
			Peak[0] = (Peak[1]);
			Valley[0] = (Valley[1]);
			
			if (BP[1] > BP[0] && BP[1] > BP[2])
				Peak[0] = (BP[1]);
			if (BP[1] < BP[0] && BP[1] < BP[2])
				Valley[0] = (BP[1]);
			
			avgPeak = SMA(Peak, 50)[0];
			avgValley = SMA(Valley, 50)[0];
			
            UpperBand[0] = (avgPeak * Fraction);
            LowerBand[0] = (avgValley * Fraction);
            MainPlot[0] = (mean);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Description="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Delta", Description="Delta", Order=2, GroupName="Parameters")]
		public double Delta
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Fraction", Description="Fraction", Order=3, GroupName="Parameters")]
		public double Fraction
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBand
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBand
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MainPlot
		{
			get { return Values[2]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MarketMode[] cacheMarketMode;
		public MarketMode MarketMode(int period, double delta, double fraction)
		{
			return MarketMode(Input, period, delta, fraction);
		}

		public MarketMode MarketMode(ISeries<double> input, int period, double delta, double fraction)
		{
			if (cacheMarketMode != null)
				for (int idx = 0; idx < cacheMarketMode.Length; idx++)
					if (cacheMarketMode[idx] != null && cacheMarketMode[idx].Period == period && cacheMarketMode[idx].Delta == delta && cacheMarketMode[idx].Fraction == fraction && cacheMarketMode[idx].EqualsInput(input))
						return cacheMarketMode[idx];
			return CacheIndicator<MarketMode>(new MarketMode(){ Period = period, Delta = delta, Fraction = fraction }, input, ref cacheMarketMode);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MarketMode MarketMode(int period, double delta, double fraction)
		{
			return indicator.MarketMode(Input, period, delta, fraction);
		}

		public Indicators.MarketMode MarketMode(ISeries<double> input , int period, double delta, double fraction)
		{
			return indicator.MarketMode(input, period, delta, fraction);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MarketMode MarketMode(int period, double delta, double fraction)
		{
			return indicator.MarketMode(Input, period, delta, fraction);
		}

		public Indicators.MarketMode MarketMode(ISeries<double> input , int period, double delta, double fraction)
		{
			return indicator.MarketMode(input, period, delta, fraction);
		}
	}
}

#endregion
