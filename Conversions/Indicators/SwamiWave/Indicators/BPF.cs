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
	public class BPF : Indicator
	{
		private const double rtd = Math.PI / 180;
		private Series<double> den;
		private Series<double> nom;
		private Series<double> filter;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BPF";
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
				Period										= 20;
				AddPlot(Brushes.Orange, "Filter");
				AddPlot(Brushes.Orange, "Filter1");
			}
			else if (State == State.DataLoaded)
			{				
				den = new Series<double>(this);
				nom = new Series<double>(this);
				filter = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2)
			{
				Filter[0] = (0);
				return;
			}
			const double delta2 = 0.1;
			double beta2 = Math.Cos(rtd * 360 / Period);
			double gamma2 = 1 / Math.Cos(rtd * 720 * delta2 / Period);
			double alpha2 = gamma2 - Math.Sqrt(gamma2 * gamma2 - 1);

			filter[0] = (0.5 * (1 - alpha2) * (Input[0] - Input[2]) + beta2 * (1 + alpha2) * filter[1] - alpha2 * filter[2]);
			nom[0] = (filter[0] - MIN(filter, Period)[0]);
			den[0] = (MAX(filter, Period)[0] - MIN(filter, Period)[0]);

			if (den[0].CompareTo(0) == 0)
				Filter[0] = (CurrentBar == 0 ? 50 : filter[1]);
			else
				Filter[0] = (Math.Min(1, Math.Max(0, nom[0] / den[0])));
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Filter
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Filter1
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
		private BPF[] cacheBPF;
		public BPF BPF(int period)
		{
			return BPF(Input, period);
		}

		public BPF BPF(ISeries<double> input, int period)
		{
			if (cacheBPF != null)
				for (int idx = 0; idx < cacheBPF.Length; idx++)
					if (cacheBPF[idx] != null && cacheBPF[idx].Period == period && cacheBPF[idx].EqualsInput(input))
						return cacheBPF[idx];
			return CacheIndicator<BPF>(new BPF(){ Period = period }, input, ref cacheBPF);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BPF BPF(int period)
		{
			return indicator.BPF(Input, period);
		}

		public Indicators.BPF BPF(ISeries<double> input , int period)
		{
			return indicator.BPF(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BPF BPF(int period)
		{
			return indicator.BPF(Input, period);
		}

		public Indicators.BPF BPF(ISeries<double> input , int period)
		{
			return indicator.BPF(input, period);
		}
	}
}

#endregion
