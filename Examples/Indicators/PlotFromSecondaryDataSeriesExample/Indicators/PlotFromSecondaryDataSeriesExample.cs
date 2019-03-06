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
	public class PlotFromSecondaryDataSeriesExample : Indicator
	{
		private SMA SMA1;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "PlotFromSecondaryDataSeriesExample";
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
				AddPlot(Brushes.RoyalBlue, "MyPlot");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(null, Data.BarsPeriodType.Minute, 5, Data.MarketDataType.Last);
			}
			else if (State == State.DataLoaded)
			{				
				SMA1				= SMA(Closes[1], 14);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 1 || CurrentBars[1] < 1)
				return;

			if(BarsInProgress == 0)
				Value[0] = SMA1[0];		
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PlotFromSecondaryDataSeriesExample[] cachePlotFromSecondaryDataSeriesExample;
		public PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample()
		{
			return PlotFromSecondaryDataSeriesExample(Input);
		}

		public PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample(ISeries<double> input)
		{
			if (cachePlotFromSecondaryDataSeriesExample != null)
				for (int idx = 0; idx < cachePlotFromSecondaryDataSeriesExample.Length; idx++)
					if (cachePlotFromSecondaryDataSeriesExample[idx] != null &&  cachePlotFromSecondaryDataSeriesExample[idx].EqualsInput(input))
						return cachePlotFromSecondaryDataSeriesExample[idx];
			return CacheIndicator<PlotFromSecondaryDataSeriesExample>(new PlotFromSecondaryDataSeriesExample(), input, ref cachePlotFromSecondaryDataSeriesExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample()
		{
			return indicator.PlotFromSecondaryDataSeriesExample(Input);
		}

		public Indicators.PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample(ISeries<double> input )
		{
			return indicator.PlotFromSecondaryDataSeriesExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample()
		{
			return indicator.PlotFromSecondaryDataSeriesExample(Input);
		}

		public Indicators.PlotFromSecondaryDataSeriesExample PlotFromSecondaryDataSeriesExample(ISeries<double> input )
		{
			return indicator.PlotFromSecondaryDataSeriesExample(input);
		}
	}
}

#endregion
