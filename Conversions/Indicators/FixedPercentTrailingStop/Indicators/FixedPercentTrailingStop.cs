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
	public class FixedPercentTrailingStop : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicator as described in Sylvain Vervoort's 'Using Initial Stop Methods' May 2009 S&C article.";
				Name										= "FixedPercentTrailingStop";
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
				BarsRequiredToPlot							= 1;
				Percent										= 12;
				AddPlot(Brushes.Blue, "TrailingStop");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
        {
			if (CurrentBar < 1)
				return;

			// Trailing stop
			double trail;
			double loss = Close[0] * ((double)Percent / 100);
			
			if (Close[0] > Value[1] && Close[1] > Value[1])
				trail = Math.Max(Value[1], Close[0] - loss);
			
			else if (Close[0] < Value[1] && Close[1] < Value[1])
				trail = Math.Min(Value[1], Close[0] + loss);
				
			else if (Close[0] > Value[1])
			{
				trail = Close[0] - loss;
				Draw.ArrowDown(this,CurrentBar.ToString(), false, 1, Value[1], Brushes.Orange);
			}
			
			else
			{
				trail = Close[0] + loss;
				Draw.ArrowUp(this,CurrentBar.ToString(), false, 1, Value[1], Brushes.Orange);
			}
			
            Value[0] = trail;
        }
		
		public override string ToString()
		{
			return Name + "(" + Percent + "%" + ")";
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Percent", Order=1, GroupName="Parameters")]
		public int Percent
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrailingStop
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
		private FixedPercentTrailingStop[] cacheFixedPercentTrailingStop;
		public FixedPercentTrailingStop FixedPercentTrailingStop(int percent)
		{
			return FixedPercentTrailingStop(Input, percent);
		}

		public FixedPercentTrailingStop FixedPercentTrailingStop(ISeries<double> input, int percent)
		{
			if (cacheFixedPercentTrailingStop != null)
				for (int idx = 0; idx < cacheFixedPercentTrailingStop.Length; idx++)
					if (cacheFixedPercentTrailingStop[idx] != null && cacheFixedPercentTrailingStop[idx].Percent == percent && cacheFixedPercentTrailingStop[idx].EqualsInput(input))
						return cacheFixedPercentTrailingStop[idx];
			return CacheIndicator<FixedPercentTrailingStop>(new FixedPercentTrailingStop(){ Percent = percent }, input, ref cacheFixedPercentTrailingStop);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FixedPercentTrailingStop FixedPercentTrailingStop(int percent)
		{
			return indicator.FixedPercentTrailingStop(Input, percent);
		}

		public Indicators.FixedPercentTrailingStop FixedPercentTrailingStop(ISeries<double> input , int percent)
		{
			return indicator.FixedPercentTrailingStop(input, percent);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FixedPercentTrailingStop FixedPercentTrailingStop(int percent)
		{
			return indicator.FixedPercentTrailingStop(Input, percent);
		}

		public Indicators.FixedPercentTrailingStop FixedPercentTrailingStop(ISeries<double> input , int percent)
		{
			return indicator.FixedPercentTrailingStop(input, percent);
		}
	}
}

#endregion
