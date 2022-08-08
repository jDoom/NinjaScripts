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

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.CustomRenderingExamples
{
	public class MultiPlotsPerBarDynamicExample : Indicator
	{				
		public Series<double>[] DynamicPlots;
		
		private System.Windows.Media.Brush[] 	PlotWMBrushes;
		private SharpDX.Direct2D1.Brush[] 		PlotDXBrushes;
		private bool[]							PlotBrushNeedsUpdate;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volume Spread Analysis";
				Name										= "MultiPlotsPerBarDynamic";
				Calculate									= Calculate.OnBarClose;
				
				UseBarWidth									= false;
				BarDistanceToUse							= 0.9f;
			}
			else if (State == State.DataLoaded)
			{
				int size = 4;
				
				DynamicPlots 			= new Series<double>[size];
				PlotWMBrushes 			= new System.Windows.Media.Brush[size];
				PlotDXBrushes 			= new SharpDX.Direct2D1.Brush[size];
				PlotBrushNeedsUpdate	= new bool[size];
				
				for(int i = 0; i < DynamicPlots.Length; i++)
					DynamicPlots[i] = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				for(int i = 0; i < PlotWMBrushes.Length; i++)
					PlotWMBrushes[i] = Brushes.Red;
				
				for(int i = 0; i < PlotDXBrushes.Length; i++)
					PlotDXBrushes[i] = null;
				
				for(int i = 0; i < PlotBrushNeedsUpdate.Length; i++)
					PlotBrushNeedsUpdate[i] = true;
				
				PlotWMBrushes[0] = Brushes.Red;
				PlotWMBrushes[1] = Brushes.Green;
				PlotWMBrushes[2] = Brushes.Blue;
				PlotWMBrushes[3] = Brushes.Orange;
			}
		}

		protected override void OnBarUpdate()
		{
			DynamicPlots[0][0] = 50;
			DynamicPlots[1][0] = 100;
			DynamicPlots[2][0] = 25;
			DynamicPlots[3][0] = 75;
		}
		
		public override void OnCalculateMinMax()
		{
			// make sure to always start fresh values to calculate new min/max values
			double tmpMin = double.MaxValue;
			double tmpMax = double.MinValue;

			// For performance optimization, only loop through what is viewable on the chart		
			for (int index = Math.Max(ChartBars.FromIndex - 1, 0); index <= ChartBars.ToIndex; index++)
			{
				// return min/max of our High/Low plots
				for (int i = 0; i < DynamicPlots.Length; i++)
				{
					tmpMin = Math.Min(tmpMin, DynamicPlots[i].GetValueAt(index));
					tmpMax = Math.Max(tmpMax, DynamicPlots[i].GetValueAt(index));
				}
			}

			MinValue = tmpMin;
			MaxValue = tmpMax;
		}
		
		public override void OnRenderTargetChanged()
		{
			// Dispose and recreate our DX Brushes
			try
			{
				// OnRenderTargetChange gets called after State.Configure on start up. Since we set up in DataLoaded, need to add error checking
				if (PlotDXBrushes == null)
						return;
				for(int i = 0; i < PlotDXBrushes.Length; i++)
				{									
					if (PlotDXBrushes[i] != null)
						PlotDXBrushes[i].Dispose();
					if (RenderTarget != null)
						PlotDXBrushes[i] = PlotWMBrushes[i].ToDxBrush(RenderTarget);
					PlotBrushNeedsUpdate[i] = false;
				}
			}
			catch (Exception exception)
			{
				Log(exception.ToString(), LogLevel.Error);
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Call base OnRender() method to paint defined Plots.
			//base.OnRender(chartControl, chartScale);
			
			// Store previous AA mode
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode 	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode 							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Make sure brushes are updated. 
			// Since OnRenderTargetChange gets called after State.Configure on start up, and we set up in State.DataLoaded, these brushes are not prepared on our first OnRender pass and need to be created
			for(int i = 0; i < PlotBrushNeedsUpdate.Length; i++)
			{
				if (PlotBrushNeedsUpdate[i] == true)
				{
					if (PlotDXBrushes[i] != null)
						PlotDXBrushes[i].Dispose();
					if (RenderTarget != null)
						PlotDXBrushes[i] = PlotWMBrushes[i].ToDxBrush(RenderTarget);
					PlotBrushNeedsUpdate[i] = false;
				}
			}
			
			for (int barIdx = Math.Max(ChartBars.FromIndex - 1, 0); barIdx <= ChartBars.ToIndex; barIdx++)
			{
				float startx, pwidth;
				
				if (UseBarWidth)
				{
					startx = chartControl.GetXByBarIndex(ChartBars, barIdx) - chartControl.GetBarPaintWidth(ChartBars) / 2;
					pwidth = chartControl.GetBarPaintWidth(ChartBars) / DynamicPlots.Length;
				}
				else
				{
					startx = chartControl.GetXByBarIndex(ChartBars, barIdx) - (chartControl.Properties.BarDistance * BarDistanceToUse) / 2;
					pwidth = (chartControl.Properties.BarDistance * BarDistanceToUse) / DynamicPlots.Length;
				}
				
				for (int pltIdx = 0; pltIdx < DynamicPlots.Length; pltIdx++)
				{
					SharpDX.RectangleF thisRect = new SharpDX.RectangleF()
					{ 
						X = startx + pwidth * pltIdx,
						Y = chartScale.GetYByValue(0),
						Width = pwidth,
						Height = -chartScale.GetYByValue(0) + chartScale.GetYByValue(DynamicPlots[pltIdx].GetValueAt(barIdx))
					};
					RenderTarget.FillRectangle(thisRect, PlotDXBrushes[pltIdx]);
				}
			}
			
			// Reset AA mode.
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}		
		
		#region SharpDX Helper Classes & Methods
		private void SetOpacity(int index, double opacity)
		{
			if (PlotWMBrushes[index] == null)
				return;

			if (PlotWMBrushes[index].IsFrozen)
				PlotWMBrushes[index] = PlotWMBrushes[index].Clone();

			PlotWMBrushes[index].Opacity = opacity / 100.0;
			PlotWMBrushes[index].Freeze();
			
			PlotBrushNeedsUpdate[index] = true;
		}
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="UseBarWidth", Description="Use BarWidth or BarDistance", Order=1, GroupName="Parameters")]
		public bool UseBarWidth
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="BarDistanceToUse", Description="Amount of BarDistance to use with Plot group", Order=2, GroupName="Parameters")]
		public float BarDistanceToUse
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.MultiPlotsPerBarDynamicExample[] cacheMultiPlotsPerBarDynamicExample;
		public CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(bool useBarWidth, float barDistanceToUse)
		{
			return MultiPlotsPerBarDynamicExample(Input, useBarWidth, barDistanceToUse);
		}

		public CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(ISeries<double> input, bool useBarWidth, float barDistanceToUse)
		{
			if (cacheMultiPlotsPerBarDynamicExample != null)
				for (int idx = 0; idx < cacheMultiPlotsPerBarDynamicExample.Length; idx++)
					if (cacheMultiPlotsPerBarDynamicExample[idx] != null && cacheMultiPlotsPerBarDynamicExample[idx].UseBarWidth == useBarWidth && cacheMultiPlotsPerBarDynamicExample[idx].BarDistanceToUse == barDistanceToUse && cacheMultiPlotsPerBarDynamicExample[idx].EqualsInput(input))
						return cacheMultiPlotsPerBarDynamicExample[idx];
			return CacheIndicator<CustomRenderingExamples.MultiPlotsPerBarDynamicExample>(new CustomRenderingExamples.MultiPlotsPerBarDynamicExample(){ UseBarWidth = useBarWidth, BarDistanceToUse = barDistanceToUse }, input, ref cacheMultiPlotsPerBarDynamicExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(bool useBarWidth, float barDistanceToUse)
		{
			return indicator.MultiPlotsPerBarDynamicExample(Input, useBarWidth, barDistanceToUse);
		}

		public Indicators.CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(ISeries<double> input , bool useBarWidth, float barDistanceToUse)
		{
			return indicator.MultiPlotsPerBarDynamicExample(input, useBarWidth, barDistanceToUse);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(bool useBarWidth, float barDistanceToUse)
		{
			return indicator.MultiPlotsPerBarDynamicExample(Input, useBarWidth, barDistanceToUse);
		}

		public Indicators.CustomRenderingExamples.MultiPlotsPerBarDynamicExample MultiPlotsPerBarDynamicExample(ISeries<double> input , bool useBarWidth, float barDistanceToUse)
		{
			return indicator.MultiPlotsPerBarDynamicExample(input, useBarWidth, barDistanceToUse);
		}
	}
}

#endregion
