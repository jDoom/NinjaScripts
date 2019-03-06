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
	public class VolumetricTest : Indicator
	{
		private NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType barsType;
		private double		buys;
		private double 		sells;
		private int 		activeBar = -1;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VolumetricTest";
				Calculate									= Calculate.OnEachTick;
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
				
				AddPlot(Brushes.DarkCyan,			NinjaTrader.Custom.Resource.BuySellPressureBuyPressure);
				AddPlot(Brushes.Crimson,			NinjaTrader.Custom.Resource.BuySellPressureSellPressure);

				AddLine(Brushes.DimGray,	75,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddLine(Brushes.DimGray,	25,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
			}
			else if (State == State.DataLoaded)
			{
				barsType = ChartBars.Bars.BarsSeries.BarsType as NinjaTrader.NinjaScript.BarsTypes.VolumetricBarsType;
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < activeBar || CurrentBar <= BarsRequiredToPlot)
				return;

			// New Bar has been formed
			// - Assign last volume counted to the prior bar
			// - Reset volume count for new bar
			if (CurrentBar != activeBar)
			{
				BuyPressure[1] = (buys / (buys + sells)) * 100;
				SellPressure[1] = (sells / (buys + sells)) * 100;
				buys = 1;
				sells = 1;
				activeBar = CurrentBar;
			}

			BuyPressure[0] = (buys / (buys + sells)) * 100;
			SellPressure[0] = (sells / (buys + sells)) * 100;
			
			// OnMarketData is always called after OnBarUpdate place code here
			buys = barsType.Volumes[CurrentBar].TotalBuyingVolume;
			sells = barsType.Volumes[CurrentBar].TotalSellingVolume;
		}
		
		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> BuyPressure
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> SellPressure
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VolumetricTest[] cacheVolumetricTest;
		public VolumetricTest VolumetricTest()
		{
			return VolumetricTest(Input);
		}

		public VolumetricTest VolumetricTest(ISeries<double> input)
		{
			if (cacheVolumetricTest != null)
				for (int idx = 0; idx < cacheVolumetricTest.Length; idx++)
					if (cacheVolumetricTest[idx] != null &&  cacheVolumetricTest[idx].EqualsInput(input))
						return cacheVolumetricTest[idx];
			return CacheIndicator<VolumetricTest>(new VolumetricTest(), input, ref cacheVolumetricTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumetricTest VolumetricTest()
		{
			return indicator.VolumetricTest(Input);
		}

		public Indicators.VolumetricTest VolumetricTest(ISeries<double> input )
		{
			return indicator.VolumetricTest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumetricTest VolumetricTest()
		{
			return indicator.VolumetricTest(Input);
		}

		public Indicators.VolumetricTest VolumetricTest(ISeries<double> input )
		{
			return indicator.VolumetricTest(input);
		}
	}
}

#endregion
