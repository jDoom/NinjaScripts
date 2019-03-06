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
	public class LinRegPredictor : Indicator
	{
		private Series<double> lRHighCloseSeries;
		private Series<double> lRLowCloseSeries;
		private Series<double> yHigh;
		private Series<double> yLow;
		private Series<double> y;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"The Linear Regression Predictor is an indicator that 'predicts' the value of a security's price.";
				Name										= "LinRegPredictor";
				Calculate									= Calculate.OnEachTick;
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
				Period										= 1;
				
				AddPlot(Brushes.Orange, "WoodiesLRPredictor");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				lRHighCloseSeries = new Series<double>(this);
				lRLowCloseSeries = new Series<double>(this);
				yHigh = new Series<double>(this);
				yLow = new Series<double>(this);
				y = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			double	sumX	= (double) Period * (Period - 1) * 0.5;
			double	divisor = sumX * sumX - (double) Period * Period * (Period - 1) * (2 * Period - 1) / 6;
			double	sumXY	= 0;

			for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
				sumXY += count * Input[count];
			
			y[0] = (Input[0]);
			yHigh[0] = (Low[0] + BarsPeriod.Value*TickSize);
			yLow[0] = (High[0] - BarsPeriod.Value*TickSize);
			double	slope		= ((double) Period * sumXY - sumX * SUM(y, Period)[0]) / divisor;
			double	intercept	= (SUM(y, Period)[0] - slope * sumX) / Period;
			double	slopeHigh		= ((double) Period * sumXY - (sumX * SUM(y, Period-1)[Math.Min(CurrentBar,1)] + sumX * yHigh[0])) / divisor;
			double	interceptHigh	= ((SUM(y, Period-1)[Math.Min(CurrentBar,1)] + yHigh[0]) - slopeHigh * sumX) / Period;
			double	slopeLow		= ((double) Period * sumXY - (sumX * SUM(y, Period-1)[Math.Min(CurrentBar,1)]+sumX*yLow[0])) / divisor;
			double	interceptLow	= ((SUM(y, Period-1)[Math.Min(CurrentBar,1)] + yLow[0]) - slopeLow * sumX) / Period;
			
			//Value.Set(intercept + slope * (Period - 1));
			lRHighCloseSeries[0] = (interceptHigh + slopeHigh * (Period-1));
			lRLowCloseSeries[0] = (interceptLow + slopeLow * (Period-1));
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(2, int.MaxValue)]
		[Display(Name="Period", Description="Numbers of bars used for calculations", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore()]
		[Description("LSMA at a High Bar Close")]
		public Series<double> LRHighClose
		{
			get { Update();
			return lRHighCloseSeries; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		[Description("Woodies Pattern, ZLR, FAM, VT, GB, TT, GST")]
		public Series<double> LRLowClose
		{
			get { Update();
			return lRLowCloseSeries; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinRegPredictor[] cacheLinRegPredictor;
		public LinRegPredictor LinRegPredictor(int period)
		{
			return LinRegPredictor(Input, period);
		}

		public LinRegPredictor LinRegPredictor(ISeries<double> input, int period)
		{
			if (cacheLinRegPredictor != null)
				for (int idx = 0; idx < cacheLinRegPredictor.Length; idx++)
					if (cacheLinRegPredictor[idx] != null && cacheLinRegPredictor[idx].Period == period && cacheLinRegPredictor[idx].EqualsInput(input))
						return cacheLinRegPredictor[idx];
			return CacheIndicator<LinRegPredictor>(new LinRegPredictor(){ Period = period }, input, ref cacheLinRegPredictor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegPredictor LinRegPredictor(int period)
		{
			return indicator.LinRegPredictor(Input, period);
		}

		public Indicators.LinRegPredictor LinRegPredictor(ISeries<double> input , int period)
		{
			return indicator.LinRegPredictor(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegPredictor LinRegPredictor(int period)
		{
			return indicator.LinRegPredictor(Input, period);
		}

		public Indicators.LinRegPredictor LinRegPredictor(ISeries<double> input , int period)
		{
			return indicator.LinRegPredictor(input, period);
		}
	}
}

#endregion
