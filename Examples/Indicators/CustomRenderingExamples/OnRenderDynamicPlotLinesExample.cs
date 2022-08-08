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
	public class OnRenderDynamicPlotLinesExample : Indicator
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
				Name										= "OnRenderDynamicPlotLines";
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
			}
		}

		protected override void OnBarUpdate()
		{
			DynamicPlots[0][0] = Open[0];
			DynamicPlots[1][0] = High[0];
			DynamicPlots[2][0] = Low[0];
			DynamicPlots[3][0] = Close[0];
		}
		
		public override void OnCalculateMinMax()
		{
			// make sure to always start fresh values to calculate new min/max values
			double tmpMin = double.MaxValue;
			double tmpMax = double.MinValue;

			// For performance optimization, only loop through what is viewable on the chart
			int lastbar = ChartBars.ToIndex;
			int offset = Calculate == Calculate.OnBarClose ? 1 : 0;
			
			if (CurrentBars[0] <= ChartBars.ToIndex - offset)
				lastbar = CurrentBars[0];
			
			for (int index = ChartBars.FromIndex; index <= lastbar; index++)
			{
				// return min/max of our High/Low plots
				tmpMin = Math.Min(tmpMin, DynamicPlots[3].GetValueAt(index));
				tmpMax = Math.Max(tmpMax, DynamicPlots[2].GetValueAt(index));
			}

			// Finally, set the minimum and maximum Y-Axis values to +/- 50 ticks from the plot values
			MinValue = tmpMin - 10 * TickSize;
			MaxValue = tmpMax + 10 * TickSize;
		}
		
		public override void OnRenderTargetChanged()
		{
			// Dispose and recreate our DX Brushes
			try
			{
				//if (PlotDXBrushes != null && PlotWMBrushes != null)
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
			
			// make sure brushes are updated
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
			
			// Use our cutom drawing method to draw a line using geometry
			DrawLineWithGeometry(chartScale, DynamicPlots[0], PlotDXBrushes[0], Displacement);
			
			DrawLineWithGeometry(chartScale, DynamicPlots[1], PlotDXBrushes[1], Displacement);
			
			DrawLineWithGeometry(chartScale, DynamicPlots[2], PlotDXBrushes[2], Displacement);
			
			DrawLineWithGeometry(chartScale, DynamicPlots[3], PlotDXBrushes[3], Displacement);
			
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

		private class SharpDXFigure
		{
			public SharpDX.Vector2[] Points;
			public SharpDX.Direct2D1.Brush Brush;
			
			public SharpDXFigure(SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush)
			{
				Points = points;
				Brush = brush;
			}
		}
		
		private void DrawFigure(SharpDXFigure figure)
		{
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
		
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(figure.Points[0], new SharpDX.Direct2D1.FigureBegin());
			
			for (int i = 0; i < figure.Points.Length; i++)
				sink.AddLine(figure.Points[i]);
				
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Open);
			sink.Close();
			
			RenderTarget.DrawGeometry(geometry, figure.Brush, 2, strokeStyle);
			geometry.Dispose();
			sink.Dispose();
			strokeStyle.Dispose();
			strokeStyle = null;
		}
		
		private void DrawLineWithGeometry(ChartScale chartScale, Series<double> inputSeries, SharpDX.Direct2D1.Brush brush, int displacement)
		{	
			List<SharpDX.Vector2> 	SeriesPoints 	= new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> 	tmpPoints 		= new List<SharpDX.Vector2>();
			List<SharpDXFigure>		SharpDXFigures	= new List<SharpDXFigure>();
			
			// Convert Series to points
			int start 	= ChartBars.FromIndex-displacement > 0 ? ChartBars.FromIndex-displacement : 0;
			int end 	= ChartBars.ToIndex;
			int offset = Calculate == Calculate.OnBarClose ? 1 : 0;
			if (CurrentBars[0] <= ChartBars.ToIndex - offset)
				end = CurrentBars[0];
			
			for (int barIndex = Math.Max(start - offset, 0); barIndex <= end; barIndex++)
		    {
				if(inputSeries.IsValidDataPointAt(barIndex))
				{
					SeriesPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex+displacement), (float)chartScale.GetYByValue(inputSeries.GetValueAt(barIndex))));
				}
		    }
			
			for (int i = 0; i < SeriesPoints.Count; i++)
			{
				tmpPoints.Add(SeriesPoints[i]);
			}
			
			SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(), brush);
			DrawFigure(figure);
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.OnRenderDynamicPlotLinesExample[] cacheOnRenderDynamicPlotLinesExample;
		public CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample()
		{
			return OnRenderDynamicPlotLinesExample(Input);
		}

		public CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample(ISeries<double> input)
		{
			if (cacheOnRenderDynamicPlotLinesExample != null)
				for (int idx = 0; idx < cacheOnRenderDynamicPlotLinesExample.Length; idx++)
					if (cacheOnRenderDynamicPlotLinesExample[idx] != null &&  cacheOnRenderDynamicPlotLinesExample[idx].EqualsInput(input))
						return cacheOnRenderDynamicPlotLinesExample[idx];
			return CacheIndicator<CustomRenderingExamples.OnRenderDynamicPlotLinesExample>(new CustomRenderingExamples.OnRenderDynamicPlotLinesExample(), input, ref cacheOnRenderDynamicPlotLinesExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample()
		{
			return indicator.OnRenderDynamicPlotLinesExample(Input);
		}

		public Indicators.CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample(ISeries<double> input )
		{
			return indicator.OnRenderDynamicPlotLinesExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample()
		{
			return indicator.OnRenderDynamicPlotLinesExample(Input);
		}

		public Indicators.CustomRenderingExamples.OnRenderDynamicPlotLinesExample OnRenderDynamicPlotLinesExample(ISeries<double> input )
		{
			return indicator.OnRenderDynamicPlotLinesExample(input);
		}
	}
}

#endregion
