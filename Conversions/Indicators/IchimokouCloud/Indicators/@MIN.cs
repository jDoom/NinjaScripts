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
	/// The Minimum shows the minimum of the last n bars.
	/// </summary>
	public class MIN : Indicator
	{
		private int		lastBar;
		private double	lastMin;
		private double	runningMin;
		private int		runningBar;
		private int		thisBar;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMIN;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMIN;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMIN);
			}
			else if (State == State.Configure)
			{
				lastBar		= 0;
				lastMin		= 0;
				runningMin	= 0;
				runningBar	= 0;
				thisBar		= 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
			{
				runningMin  = Input[0];
				lastMin     = Input[0];
				runningBar  = 0;
				lastBar		= 0;
				thisBar		= 0;
				Value[0]	= Input[0];
				return;
			}

			if (CurrentBar - runningBar >= Period || CurrentBar < thisBar)
			{
				runningMin = double.MaxValue;
				for (int barsBack = Math.Min(CurrentBar, Period - 1); barsBack > 0; barsBack--)
					if (Input[barsBack] <= runningMin)
					{
						runningMin  = Input[barsBack];
						runningBar  = CurrentBar - barsBack;
					}
			}

			if (thisBar != CurrentBar)
			{
				lastMin = runningMin;
				lastBar = runningBar;
				thisBar = CurrentBar;
			}

			if (Input[0] <= lastMin)
			{
				runningMin = Input[0];
				runningBar = CurrentBar;
			}
			else
			{
				runningMin = lastMin;
				runningBar = lastBar;
			}

			Value[0] = runningMin;
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
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
		private MIN[] cacheMIN;
		public MIN MIN(int period)
		{
			return MIN(Input, period);
		}

		public MIN MIN(ISeries<double> input, int period)
		{
			if (cacheMIN != null)
				for (int idx = 0; idx < cacheMIN.Length; idx++)
					if (cacheMIN[idx] != null && cacheMIN[idx].Period == period && cacheMIN[idx].EqualsInput(input))
						return cacheMIN[idx];
			return CacheIndicator<MIN>(new MIN(){ Period = period }, input, ref cacheMIN);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MIN MIN(int period)
		{
			return indicator.MIN(Input, period);
		}

		public Indicators.MIN MIN(ISeries<double> input , int period)
		{
			return indicator.MIN(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MIN MIN(int period)
		{
			return indicator.MIN(Input, period);
		}

		public Indicators.MIN MIN(ISeries<double> input , int period)
		{
			return indicator.MIN(input, period);
		}
	}
}

#endregion
