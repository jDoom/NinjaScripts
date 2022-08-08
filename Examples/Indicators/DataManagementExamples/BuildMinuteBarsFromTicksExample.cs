//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim
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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required.
namespace NinjaTrader.NinjaScript.Indicators.DataManagementExamples
{
	public class BuildMinuteBarsFromTicksExample : Indicator
	{
		private bool		BarStarted;
		private DateTime	startTime, nextBarTime;
		private fakeBar[]	fakeBars;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BuildMinuteBarsFromTicksExample";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Tick, 1);
			}
			else if (State == State.DataLoaded)
			{
				BarStarted	= false;
			}
		}
		
		private class fakeBar
		{
			public double Open, High, Low, Close, Volume;
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 1)
				return;
			
			if (!BarStarted)
			{
				startTime	= Time[0];
				startTime	= startTime.AddMinutes(1);
				startTime	= startTime.AddSeconds(-startTime.Second);
				startTime	= startTime.AddMilliseconds(-startTime.Millisecond);
				BarStarted	= true;
				
				fakeBars	= new fakeBar[14];
			}
			
			if (Time[0] >= startTime)
			{
				if (Time[0] > nextBarTime)
				{
					//Bar Closed
					nextBarTime	= Time[0];
					nextBarTime	= nextBarTime.AddMinutes(1);
					nextBarTime	= nextBarTime.AddSeconds(-nextBarTime.Second);
					nextBarTime	= nextBarTime.AddMilliseconds(-nextBarTime.Millisecond);
					
					if (fakeBars[0] == null)
					{
						// initalize fakeBars
						for (int i = 0; i < fakeBars.Length; i++)
						{
							fakeBars[i] = new fakeBar(){ Open = Close[0], High = Close[0], Low = Close[0], Close = Close[0] };
						}
						//Print("?");
					}
					else
					{
						Print(String.Format("Time: {0} Open: {1} High: {2} Low: {3} Close: {4}", nextBarTime.AddMinutes(-1), fakeBars[0].Open, fakeBars[0].High, fakeBars[0].Low, fakeBars[0].Close));
						// rotate fakeBars
						for (int i = fakeBars.Length-1; i > 0; i--)
						{
							fakeBars[i] = fakeBars[i-1];
						}
						fakeBars[0] = new fakeBar(){ Open = Close[0], High = Close[0], Low = Close[0], Close = Close[0] };
					}
					
				}
				else
				{
					if (fakeBars[0] == null)
						return;
					//Bar not closed
					fakeBars[0].High	= Math.Max(Close[0], fakeBars[0].High);
					fakeBars[0].Low		= Math.Min(Close[0], fakeBars[0].Low);
					fakeBars[0].Close	= Close[0];
				}
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DataManagementExamples.BuildMinuteBarsFromTicksExample[] cacheBuildMinuteBarsFromTicksExample;
		public DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample()
		{
			return BuildMinuteBarsFromTicksExample(Input);
		}

		public DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample(ISeries<double> input)
		{
			if (cacheBuildMinuteBarsFromTicksExample != null)
				for (int idx = 0; idx < cacheBuildMinuteBarsFromTicksExample.Length; idx++)
					if (cacheBuildMinuteBarsFromTicksExample[idx] != null &&  cacheBuildMinuteBarsFromTicksExample[idx].EqualsInput(input))
						return cacheBuildMinuteBarsFromTicksExample[idx];
			return CacheIndicator<DataManagementExamples.BuildMinuteBarsFromTicksExample>(new DataManagementExamples.BuildMinuteBarsFromTicksExample(), input, ref cacheBuildMinuteBarsFromTicksExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample()
		{
			return indicator.BuildMinuteBarsFromTicksExample(Input);
		}

		public Indicators.DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample(ISeries<double> input )
		{
			return indicator.BuildMinuteBarsFromTicksExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample()
		{
			return indicator.BuildMinuteBarsFromTicksExample(Input);
		}

		public Indicators.DataManagementExamples.BuildMinuteBarsFromTicksExample BuildMinuteBarsFromTicksExample(ISeries<double> input )
		{
			return indicator.BuildMinuteBarsFromTicksExample(input);
		}
	}
}

#endregion
