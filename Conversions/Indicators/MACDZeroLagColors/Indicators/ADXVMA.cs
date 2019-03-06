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
	public class ADXVMA : Indicator
	{
		private Series<double> Out;
		
		private double PDM0, PDM1;
		private double MDM0, MDM1;
		private double PDI0, PDI1;
		private double MDI0, MDI1;
		
		private double WeightDM;
		private double WeightDI;
		private double WeightDX;
		private double ChandeEMA;
		private double HHV = double.MinValue;
		private double LLV = double.MaxValue;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ADXVMA";
				Name										= "ADXVMA";
				Calculate									= Calculate.OnEachTick;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				ADXPeriod									= 6;
				AddPlot(Brushes.Lime, "ADXVMAPlot");
			}
			else if (State == State.DataLoaded)
			{
				WeightDX = WeightDM = WeightDI = ChandeEMA 	= ADXPeriod;
				Out = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			if( CurrentBar < 2 )
			{
				ADXVMAPlot[0] = 0;
				PDM0 = PDM1 = 0;
				MDM0 = MDM0 = 0;
				PDI0 = PDI1 = 0;
				MDI0 = MDI1 = 0;
				Out[0] =  0;
				return;
			}
			try
			{
				PDM0 =  0;
				MDM0 =  0;
				if(Input[0] > Input[1])
					PDM0 = Input[0] - Input[1]; //This array is not displayed.
				else
					MDM0 = Input[1] - Input[0]; //This array is not displayed.
				
				PDM0 = ((WeightDM - 1) * PDM1 + PDM0) / WeightDM; //ema.
				MDM0 = ((WeightDM - 1) * MDM1 + MDM0) / WeightDM; //ema.
				PDM1 = PDM0;
				MDM1 = MDM0;
				
				double TR = PDM0 + MDM0;
				
				//Avoid division by zero. Minimum step size is one unnormalized price pip.
				if (TR > 0)
				{
					PDI0 = PDM0 / TR;
					MDI0 = MDM0 / TR;
				}
				else
				{
					PDI0 = 0;
					MDI0 = 0;
				}
				
				PDI0 = ((WeightDI - 1) * PDI1 + PDI0) / WeightDI; //ema.
				MDI0 = ((WeightDI - 1) * MDI1 + MDI0) / WeightDI; //ema.
				PDI1 = PDI0;
				MDI1 = MDI0;

				double DI_Diff = PDI0 - MDI0;  
				if (DI_Diff < 0)
					DI_Diff = -DI_Diff; //Only positive momentum signals are used.
				double DI_Sum = PDI0 + MDI0;
				double DI_Factor = 0; //Zero case, DI_Diff will also be zero when DI_Sum is zero.
				if (DI_Sum > 0)
					Out[0] = DI_Diff / DI_Sum; //Factional, near zero when PDM==MDM (horizonal), near 1 for laddering.
				else
					Out[0] = 0;
	
				  Out[0] = ((WeightDX - 1) * Out[1] + Out[0]) / WeightDX;
				
				if (Out[0] > Out[1])
				{
					HHV = Out[0];
					LLV = Out[1];
				}
				else
				{
					HHV = Out[1];
					LLV = Out[0];
				}
	
				for(int j = 1; j < Math.Min(ADXPeriod, CurrentBar); j++)
				{
					if(Out[j+1] > HHV) HHV = Out[j+1];
					if(Out[j+1] < LLV) LLV = Out[j+1];
				}
				
				
				double diff = HHV - LLV; //Veriable reference scale, adapts to recent activity level, unnormalized.
				double VI = 0; //Zero case. This fixes the output at its historical level. 
				if (diff > 0)
					VI = (Out[0] - LLV) / diff; //Normalized, 0-1 scale.
				
				//   if (VI_0.VIsq_1.VIsqroot_2 == 1) VI *= VI;
				//   if (VI_0.VIsq_1.VIsqroot_2 == 2) VI = MathSqrt(VI);
				//   if (VI > VImax) VI = VImax; //Used by Bemac with VImax=0.4, still used in vma1 and affects 5min trend definition.
										//All the ema weight settings, including Chande, affect 5 min trend definition.
				//   if (VI <= zeroVIbelow) VI = 0;                    
				
				ADXVMAPlot[0] = ((ChandeEMA-VI) * ADXVMAPlot[1] + VI * Input[0]) / ChandeEMA; //Chande VMA formula with ema built in.
			}
			catch( Exception ex )
			{
				Print( ex.ToString() );
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ADXPeriod", Description="ADX Period", Order=1, GroupName="Parameters")]
		public int ADXPeriod
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ADXVMAPlot
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
		private ADXVMA[] cacheADXVMA;
		public ADXVMA ADXVMA(int aDXPeriod)
		{
			return ADXVMA(Input, aDXPeriod);
		}

		public ADXVMA ADXVMA(ISeries<double> input, int aDXPeriod)
		{
			if (cacheADXVMA != null)
				for (int idx = 0; idx < cacheADXVMA.Length; idx++)
					if (cacheADXVMA[idx] != null && cacheADXVMA[idx].ADXPeriod == aDXPeriod && cacheADXVMA[idx].EqualsInput(input))
						return cacheADXVMA[idx];
			return CacheIndicator<ADXVMA>(new ADXVMA(){ ADXPeriod = aDXPeriod }, input, ref cacheADXVMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ADXVMA ADXVMA(int aDXPeriod)
		{
			return indicator.ADXVMA(Input, aDXPeriod);
		}

		public Indicators.ADXVMA ADXVMA(ISeries<double> input , int aDXPeriod)
		{
			return indicator.ADXVMA(input, aDXPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ADXVMA ADXVMA(int aDXPeriod)
		{
			return indicator.ADXVMA(Input, aDXPeriod);
		}

		public Indicators.ADXVMA ADXVMA(ISeries<double> input , int aDXPeriod)
		{
			return indicator.ADXVMA(input, aDXPeriod);
		}
	}
}

#endregion
