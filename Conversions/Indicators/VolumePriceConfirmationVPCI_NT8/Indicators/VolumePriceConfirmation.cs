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
	public class VolumePriceConfirmation : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volume Price Confirmation Indicator from the July 2007 issue of S+C";
				Name										= "VPCI";
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
				LongTerm									= 50;
				ShortTerm									= 10;
				AddPlot(Brushes.RoyalBlue, "VPCI");
				AddLine(Brushes.DarkGray, 0, "Zero");
			}
		}

		protected override void OnBarUpdate()
		{
			double vpc 	= VWMA(LongTerm)[0] - SMA(LongTerm)[0];
			double vpr 	= VWMA(ShortTerm)[0] / SMA(ShortTerm)[0];
			double vm 	= SMA(Volume, ShortTerm)[0] / SMA(Volume, LongTerm)[0];
			
            VPCI[0] 	= vpc * vpr * vm;
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LongTerm", Order=1, GroupName="Parameters")]
		public int LongTerm
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ShortTerm", Order=2, GroupName="Parameters")]
		public int ShortTerm
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> VPCI
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
		private VolumePriceConfirmation[] cacheVolumePriceConfirmation;
		public VolumePriceConfirmation VolumePriceConfirmation(int longTerm, int shortTerm)
		{
			return VolumePriceConfirmation(Input, longTerm, shortTerm);
		}

		public VolumePriceConfirmation VolumePriceConfirmation(ISeries<double> input, int longTerm, int shortTerm)
		{
			if (cacheVolumePriceConfirmation != null)
				for (int idx = 0; idx < cacheVolumePriceConfirmation.Length; idx++)
					if (cacheVolumePriceConfirmation[idx] != null && cacheVolumePriceConfirmation[idx].LongTerm == longTerm && cacheVolumePriceConfirmation[idx].ShortTerm == shortTerm && cacheVolumePriceConfirmation[idx].EqualsInput(input))
						return cacheVolumePriceConfirmation[idx];
			return CacheIndicator<VolumePriceConfirmation>(new VolumePriceConfirmation(){ LongTerm = longTerm, ShortTerm = shortTerm }, input, ref cacheVolumePriceConfirmation);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumePriceConfirmation VolumePriceConfirmation(int longTerm, int shortTerm)
		{
			return indicator.VolumePriceConfirmation(Input, longTerm, shortTerm);
		}

		public Indicators.VolumePriceConfirmation VolumePriceConfirmation(ISeries<double> input , int longTerm, int shortTerm)
		{
			return indicator.VolumePriceConfirmation(input, longTerm, shortTerm);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumePriceConfirmation VolumePriceConfirmation(int longTerm, int shortTerm)
		{
			return indicator.VolumePriceConfirmation(Input, longTerm, shortTerm);
		}

		public Indicators.VolumePriceConfirmation VolumePriceConfirmation(ISeries<double> input , int longTerm, int shortTerm)
		{
			return indicator.VolumePriceConfirmation(input, longTerm, shortTerm);
		}
	}
}

#endregion
