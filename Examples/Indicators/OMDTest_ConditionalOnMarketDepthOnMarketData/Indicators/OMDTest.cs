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
	public class OMDTest : Indicator
	{
		private MarketData marketData;
		private MarketDepth<MarketDepthRow> marketDepth;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "OMDTest";
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
				
				UseOnMarketData								= true;
				UseOnMarketDepth							= true;
			}
			else if (State == State.DataLoaded)
			{
				ClearOutputWindow();
				if (UseOnMarketData)
				{
					marketData = new MarketData(Instrument);
         			marketData.Update += MyOnMarketData;
				}
				
				if (UseOnMarketDepth)
				{
					marketDepth = new MarketDepth<MarketDepthRow>(Instrument);
         			marketDepth.Update += MyOnMarketDepth;
				}
			}
			else if (State == State.Terminated)
			{
				if (marketData != null)
				{
					marketData.Update -= MyOnMarketData;
				}
				
				if (marketDepth != null)
				{
					marketDepth.Update -= MyOnMarketDepth;
				}
			}
		}
		
		private void MyOnMarketDepth(object sender, MarketDepthEventArgs e)
		{
			Print(e.ToString());
		}
		
		private void MyOnMarketData(object sender, MarketDataEventArgs e)
		{
			Print(e.ToString());
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="UseOnMarketData", Order=1, GroupName="Parameters")]
		public bool UseOnMarketData
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseOnMarketDepth", Order=2, GroupName="Parameters")]
		public bool UseOnMarketDepth
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OMDTest[] cacheOMDTest;
		public OMDTest OMDTest(bool useOnMarketData, bool useOnMarketDepth)
		{
			return OMDTest(Input, useOnMarketData, useOnMarketDepth);
		}

		public OMDTest OMDTest(ISeries<double> input, bool useOnMarketData, bool useOnMarketDepth)
		{
			if (cacheOMDTest != null)
				for (int idx = 0; idx < cacheOMDTest.Length; idx++)
					if (cacheOMDTest[idx] != null && cacheOMDTest[idx].UseOnMarketData == useOnMarketData && cacheOMDTest[idx].UseOnMarketDepth == useOnMarketDepth && cacheOMDTest[idx].EqualsInput(input))
						return cacheOMDTest[idx];
			return CacheIndicator<OMDTest>(new OMDTest(){ UseOnMarketData = useOnMarketData, UseOnMarketDepth = useOnMarketDepth }, input, ref cacheOMDTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OMDTest OMDTest(bool useOnMarketData, bool useOnMarketDepth)
		{
			return indicator.OMDTest(Input, useOnMarketData, useOnMarketDepth);
		}

		public Indicators.OMDTest OMDTest(ISeries<double> input , bool useOnMarketData, bool useOnMarketDepth)
		{
			return indicator.OMDTest(input, useOnMarketData, useOnMarketDepth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OMDTest OMDTest(bool useOnMarketData, bool useOnMarketDepth)
		{
			return indicator.OMDTest(Input, useOnMarketData, useOnMarketDepth);
		}

		public Indicators.OMDTest OMDTest(ISeries<double> input , bool useOnMarketData, bool useOnMarketDepth)
		{
			return indicator.OMDTest(input, useOnMarketData, useOnMarketDepth);
		}
	}
}

#endregion
