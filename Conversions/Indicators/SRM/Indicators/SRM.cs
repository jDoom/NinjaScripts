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
	public class SRM : Indicator
	{
		private TextFixed warningText;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Stocks and Commodies - 2012 August - Applying The Sector Rotation Model";
				Name										= "SRM";
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
				
				LBP											= 75;
				AddPlot(new Stroke(Brushes.Purple, 2), PlotStyle.Bar, "Oscillator");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("XLY", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				AddDataSeries("XLF", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				AddDataSeries("XLE", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				AddDataSeries("XLU", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				AddDataSeries("XLP", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsPeriod.BarsPeriodType != BarsPeriodType.Day && BarsInProgress == 0 && warningText == null)
			{
				warningText = Draw.TextFixed(this, "warning", "SRM works best on daily equities or equity indices", TextPosition.TopLeft);
				return;
			}
			
			if (CurrentBars[0] < LBP || CurrentBars[1] < LBP
					|| CurrentBars[2] < LBP || CurrentBars[3] < LBP
					|| CurrentBars[4] < LBP || CurrentBars[5] < LBP)
				return;
			
			double bull01 	= (Closes[1][0] - Closes[1][LBP]) / Closes[1][LBP];
			double bull02 	= (Closes[2][0] - Closes[2][LBP]) / Closes[2][LBP];
			double bear01 	= (Closes[3][0] - Closes[3][LBP]) / Closes[3][LBP];
			double bear02 	= (Closes[4][0] - Closes[4][LBP]) / Closes[4][LBP];
			double bear03 	= (Closes[5][0] - Closes[5][LBP]) / Closes[5][LBP];
			double bear		= (bear01 + bear02 + bear03) / 3;
			double bull		= (bull01 + bull02) / 2;
			
			Oscillator[0] = bull - bear;
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Oscillator
		{
			get { return Values[0]; }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="LBP", Description="ROC LookBack Period", Order=1, GroupName="Parameters")]
		public int LBP
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SRM[] cacheSRM;
		public SRM SRM(int lBP)
		{
			return SRM(Input, lBP);
		}

		public SRM SRM(ISeries<double> input, int lBP)
		{
			if (cacheSRM != null)
				for (int idx = 0; idx < cacheSRM.Length; idx++)
					if (cacheSRM[idx] != null && cacheSRM[idx].LBP == lBP && cacheSRM[idx].EqualsInput(input))
						return cacheSRM[idx];
			return CacheIndicator<SRM>(new SRM(){ LBP = lBP }, input, ref cacheSRM);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SRM SRM(int lBP)
		{
			return indicator.SRM(Input, lBP);
		}

		public Indicators.SRM SRM(ISeries<double> input , int lBP)
		{
			return indicator.SRM(input, lBP);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SRM SRM(int lBP)
		{
			return indicator.SRM(Input, lBP);
		}

		public Indicators.SRM SRM(ISeries<double> input , int lBP)
		{
			return indicator.SRM(input, lBP);
		}
	}
}

#endregion
