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
	public class AddCustomBarsTypeExampleUniRenko : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AddCustomBarsTypeExampleUniRenko";
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
			}
			else if (State == State.Configure)
			{
				// Custom BarsTypes must be added by using the BarsPeriodType index casted to BarsPeriodType. This can be noted in the BarsType's 
				// source code or can be asked from the BarsType developer.
				// Please see the "Data Series" parameters in the Data Series window to reference the parameters needed for the custom bartype.
				// You may also reference the State.Configure section of the custom BarType to learn more about the parameters it uses.
				// If there are any questions surrounding the parameters that are to be used, please consult the BarsType developer.
				// This indicator uses UniRenko Bars as an example. This BarsType is a 3rd party BarsType unsupported by NinjaTrader.
				AddDataSeries(new BarsPeriod { BarsPeriodType = (BarsPeriodType)501, BaseBarsPeriodValue = 2, Value = 2, Value2 = 6 });
			}
		}

		protected override void OnBarUpdate()
		{
			if(BarsInProgress == 1)
				Print(Close[0]);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AddCustomBarsTypeExampleUniRenko[] cacheAddCustomBarsTypeExampleUniRenko;
		public AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko()
		{
			return AddCustomBarsTypeExampleUniRenko(Input);
		}

		public AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko(ISeries<double> input)
		{
			if (cacheAddCustomBarsTypeExampleUniRenko != null)
				for (int idx = 0; idx < cacheAddCustomBarsTypeExampleUniRenko.Length; idx++)
					if (cacheAddCustomBarsTypeExampleUniRenko[idx] != null &&  cacheAddCustomBarsTypeExampleUniRenko[idx].EqualsInput(input))
						return cacheAddCustomBarsTypeExampleUniRenko[idx];
			return CacheIndicator<AddCustomBarsTypeExampleUniRenko>(new AddCustomBarsTypeExampleUniRenko(), input, ref cacheAddCustomBarsTypeExampleUniRenko);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko()
		{
			return indicator.AddCustomBarsTypeExampleUniRenko(Input);
		}

		public Indicators.AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko(ISeries<double> input )
		{
			return indicator.AddCustomBarsTypeExampleUniRenko(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko()
		{
			return indicator.AddCustomBarsTypeExampleUniRenko(Input);
		}

		public Indicators.AddCustomBarsTypeExampleUniRenko AddCustomBarsTypeExampleUniRenko(ISeries<double> input )
		{
			return indicator.AddCustomBarsTypeExampleUniRenko(input);
		}
	}
}

#endregion
