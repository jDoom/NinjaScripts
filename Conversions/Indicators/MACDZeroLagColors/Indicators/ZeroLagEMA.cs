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
	public class ZeroLagEMA : Indicator
	{
		private EMA ema1, ema2;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Zero-Lagging Exponential Moving Average";
				Name										= "ZeroLagEMA";
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
				Period										= 20;
				AddPlot(Brushes.OrangeRed, "ZLEMA");
			}
			else if (State == State.DataLoaded)
			{
				ema1 = EMA(Input, Period);
				ema2 = EMA(ema1, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			ZLEMA[0] = 2 * ema1[0] - ema2[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ZLEMA
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
		private ZeroLagEMA[] cacheZeroLagEMA;
		public ZeroLagEMA ZeroLagEMA(int period)
		{
			return ZeroLagEMA(Input, period);
		}

		public ZeroLagEMA ZeroLagEMA(ISeries<double> input, int period)
		{
			if (cacheZeroLagEMA != null)
				for (int idx = 0; idx < cacheZeroLagEMA.Length; idx++)
					if (cacheZeroLagEMA[idx] != null && cacheZeroLagEMA[idx].Period == period && cacheZeroLagEMA[idx].EqualsInput(input))
						return cacheZeroLagEMA[idx];
			return CacheIndicator<ZeroLagEMA>(new ZeroLagEMA(){ Period = period }, input, ref cacheZeroLagEMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZeroLagEMA ZeroLagEMA(int period)
		{
			return indicator.ZeroLagEMA(Input, period);
		}

		public Indicators.ZeroLagEMA ZeroLagEMA(ISeries<double> input , int period)
		{
			return indicator.ZeroLagEMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZeroLagEMA ZeroLagEMA(int period)
		{
			return indicator.ZeroLagEMA(Input, period);
		}

		public Indicators.ZeroLagEMA ZeroLagEMA(ISeries<double> input , int period)
		{
			return indicator.ZeroLagEMA(input, period);
		}
	}
}

#endregion
