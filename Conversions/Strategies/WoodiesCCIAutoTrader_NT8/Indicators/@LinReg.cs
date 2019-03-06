//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Linear Regression. The Linear Regression is an indicator that 'predicts' the value of a security's price.
	/// </summary>
	public class LinReg : Indicator
	{
		private double	avg;
		private double	divisor;
		private	double	intercept;
		private double	myPeriod;
		private double	priorSumXY;
		private	double	priorSumY;
		private double	slope;
		private double	sumX2;
		private	double	sumX;
		private double	sumXY;
		private double	sumY;
		private SUM		sum;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionLinReg;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinReg;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameLinReg);
			}
			else if (State == State.Configure)
			{
				avg	= divisor = intercept = myPeriod = priorSumXY
					= priorSumY = slope = sumX = sumX2 = sumY = sumXY = 0;
			}
			else if (State == State.DataLoaded)
			{
				sum = SUM(Inputs[0], Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsArray[0].BarsType.IsRemoveLastBarSupported)
			{
				double sumX = (double)Period * (Period - 1) * 0.5;
				double divisor = sumX * sumX - (double)Period * Period * (Period - 1) * (2 * Period - 1) / 6;
				double sumXY = 0;

				for (int count = 0; count < Period && CurrentBar - count >= 0; count++)
					sumXY += count * Input[count];

				double slope = ((double)Period * sumXY - sumX * SUM(Inputs[0], Period)[0]) / divisor;
				double intercept = (SUM(Inputs[0], Period)[0] - slope * sumX) / Period;

				Value[0] = intercept + slope * (Period - 1);
			}
			else
			{
				if (IsFirstTickOfBar)
				{
					priorSumY = sumY;
					priorSumXY = sumXY;
					myPeriod = Math.Min(CurrentBar + 1, Period);
					sumX = myPeriod * (myPeriod - 1) * 0.5;
					sumX2 = myPeriod * (myPeriod + 1) * 0.5;
					divisor = myPeriod * (myPeriod + 1) * (2 * myPeriod + 1) / 6 - sumX2 * sumX2 / myPeriod;
				}

				double input0 = Input[0];
				sumXY = priorSumXY - (CurrentBar >= Period ? priorSumY : 0) + myPeriod * input0;
				sumY = priorSumY + input0 - (CurrentBar >= Period ? Input[Period] : 0);
				avg = sumY / myPeriod;
				slope = (sumXY - sumX2 * avg) / divisor;
				intercept = (sum[0] - slope * sumX) / myPeriod;
				Value[0] = CurrentBar == 0 ? input0 : (intercept + slope * (myPeriod - 1));
			}
		}

		#region Properties
		[Range(2, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LinReg[] cacheLinReg;
		public LinReg LinReg(int period)
		{
			return LinReg(Input, period);
		}

		public LinReg LinReg(ISeries<double> input, int period)
		{
			if (cacheLinReg != null)
				for (int idx = 0; idx < cacheLinReg.Length; idx++)
					if (cacheLinReg[idx] != null && cacheLinReg[idx].Period == period && cacheLinReg[idx].EqualsInput(input))
						return cacheLinReg[idx];
			return CacheIndicator<LinReg>(new LinReg(){ Period = period }, input, ref cacheLinReg);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinReg LinReg(int period)
		{
			return indicator.LinReg(Input, period);
		}

		public Indicators.LinReg LinReg(ISeries<double> input , int period)
		{
			return indicator.LinReg(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinReg LinReg(int period)
		{
			return indicator.LinReg(Input, period);
		}

		public Indicators.LinReg LinReg(ISeries<double> input , int period)
		{
			return indicator.LinReg(input, period);
		}
	}
}

#endregion
