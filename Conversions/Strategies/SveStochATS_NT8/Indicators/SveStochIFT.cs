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
	public class SveStochIFT : Indicator
	{
		private double 		RBW, x;
		private Series<double> 	RBWSeries;
		private Series<double> 	MA;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SveStochIFT";
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
				StochPeriod					= 30;
				StochSlow					= 5;
				AddPlot(Brushes.Red, "IFTStoch");
				AddPlot(Brushes.Blue, "RBWStoch");
				//BarsRequiredToPlot = 0;
			}
			else if (State == State.DataLoaded)
			{
				MA 						= new Series<double> (this);
				RBWSeries				= new Series<double> (this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (WMA(2).IsValidDataPoint(0))
				MA[0] = (WMA(2)[0]);			RBW  = 5 * MA[0];
			
			if (WMA(MA, 2).IsValidDataPoint(0))
			{
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + 4 * MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + 3 * MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + 2 * MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBW  = RBW + MA[0];
				MA[0] = (WMA(MA, 2)[0]);		RBWSeries[0] = ((RBW + MA[0]) / 20); //Sets final value in series to be used as input for Stochastics
			}

			if (Stochastics(RBWSeries, 1, StochPeriod, StochSlow).IsValidDataPoint(0))
				RBWStoch[0] = (Stochastics(RBWSeries, 1, StochPeriod, StochSlow)[0]);
	
			x = .1 * (RBWStoch[0] - 50);

			IFTStoch[0] = (((Math.Exp(2 * x) - 1) / (Math.Exp(2 * x) + 1) + 1) * 50);
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StochPeriod", Description="Number of bars used in the Stochastics calculation", Order=1, GroupName="Parameters")]
		public int StochPeriod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StochSlow", Description="Number of bars used in stochastic smoothing", Order=2, GroupName="Parameters")]
		public int StochSlow
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> IFTStoch
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RBWStoch
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
		private SveStochIFT[] cacheSveStochIFT;
		public SveStochIFT SveStochIFT(int stochPeriod, int stochSlow)
		{
			return SveStochIFT(Input, stochPeriod, stochSlow);
		}

		public SveStochIFT SveStochIFT(ISeries<double> input, int stochPeriod, int stochSlow)
		{
			if (cacheSveStochIFT != null)
				for (int idx = 0; idx < cacheSveStochIFT.Length; idx++)
					if (cacheSveStochIFT[idx] != null && cacheSveStochIFT[idx].StochPeriod == stochPeriod && cacheSveStochIFT[idx].StochSlow == stochSlow && cacheSveStochIFT[idx].EqualsInput(input))
						return cacheSveStochIFT[idx];
			return CacheIndicator<SveStochIFT>(new SveStochIFT(){ StochPeriod = stochPeriod, StochSlow = stochSlow }, input, ref cacheSveStochIFT);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SveStochIFT SveStochIFT(int stochPeriod, int stochSlow)
		{
			return indicator.SveStochIFT(Input, stochPeriod, stochSlow);
		}

		public Indicators.SveStochIFT SveStochIFT(ISeries<double> input , int stochPeriod, int stochSlow)
		{
			return indicator.SveStochIFT(input, stochPeriod, stochSlow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SveStochIFT SveStochIFT(int stochPeriod, int stochSlow)
		{
			return indicator.SveStochIFT(Input, stochPeriod, stochSlow);
		}

		public Indicators.SveStochIFT SveStochIFT(ISeries<double> input , int stochPeriod, int stochSlow)
		{
			return indicator.SveStochIFT(input, stochPeriod, stochSlow);
		}
	}
}

#endregion
