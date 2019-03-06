//
// Copyright (C) 2018, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The ZigZag indicator shows trend lines filtering out changes below a defined level.
	/// </summary>
	public class ZigZag : Indicator
	{
		private Series<double>		zigZagHighZigZags;
		private Series<double>		zigZagLowZigZags;
		private Series<double>		zigZagHighSeries;
		private Series<double>		zigZagLowSeries;

		private double				currentZigZagHigh;
		private double				currentZigZagLow;
		private int					lastSwingIdx;
		private double				lastSwingPrice;
		private int					trendDir;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionZigZag;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameZigZag;
				DeviationType				= DeviationType.Points;
				DeviationValue				= 0.5;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= false;
				IsSuspendedWhileInactive	= true;
				IsOverlay					= true;
				PaintPriceMarkers			= false;
				UseHighLow					= false;

				AddPlot(Brushes.DodgerBlue, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameZigZag);

				DisplayInDataBox			= false;
				PaintPriceMarkers			= false;
			}
			else if (State == State.Configure)
			{
				currentZigZagHigh	= 0;
				currentZigZagLow	= 0;
				lastSwingIdx		= -1;
				lastSwingPrice		= 0.0;
				trendDir			= 0; // 1 = trend up, -1 = trend down, init = 0
			}
			else if (State == State.DataLoaded)
			{
				zigZagHighZigZags	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				zigZagLowZigZags	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				zigZagHighSeries	= new Series<double>(this, MaximumBarsLookBack.Infinite);
				zigZagLowSeries		= new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		// Returns the number of bars ago a zig zag low occurred. Returns a value of -1 if a zig zag low is not found within the look back period.
		public int LowBar(int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZagLowBarInstanceGreaterEqual, GetType().Name, instance));
			if (barsAgo < 0)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZigLowBarBarsAgoGreaterEqual, GetType().Name, barsAgo));
			if (barsAgo >= Count)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZagLowBarBarsAgoOutOfRange, GetType().Name, (Count - 1), barsAgo));

			Update();
			for (int idx = CurrentBar - barsAgo - 1; idx >= CurrentBar - barsAgo - 1 - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= zigZagLowZigZags.Count)
					continue;

				if (zigZagLowZigZags.GetValueAt(idx).Equals(0.0))
					continue;

				if (instance == 1) // 1-based, < to be save
					return CurrentBar - idx;

				instance--;
			}

			return -1;
		}

		// Returns the number of bars ago a zig zag high occurred. Returns a value of -1 if a zig zag high is not found within the look back period.
		public int HighBar(int barsAgo, int instance, int lookBackPeriod)
		{
			if (instance < 1)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZagHighBarInstanceGreaterEqual, GetType().Name, instance));
			if (barsAgo < 0)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZigHighBarBarsAgoGreaterEqual, GetType().Name, barsAgo));
			if (barsAgo >= Count)
				throw new Exception(string.Format(NinjaTrader.Custom.Resource.ZigZagHighBarBarsAgoOutOfRange, GetType().Name, (Count - 1), barsAgo));

			Update();
			for (int idx = CurrentBar - barsAgo - 1; idx >= CurrentBar - barsAgo - 1 - lookBackPeriod; idx--)
			{
				if (idx < 0)
					return -1;
				if (idx >= zigZagHighZigZags.Count)
					continue;

				if (zigZagHighZigZags.GetValueAt(idx).Equals(0.0))
					continue;

				if (instance <= 1) // 1-based, < to be save
					return CurrentBar - idx;

				instance--;
			}

			return -1;
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 2) // Need at least 3 bars to calculate Low/High
			{
				zigZagHighSeries[0]		= 0;
				zigZagHighZigZags[0]	= 0;
				zigZagLowSeries[0] 		= 0;
				zigZagLowZigZags[0]		= 0;
				return;
			}

			// Initialization
			if (lastSwingPrice == 0.0)
				lastSwingPrice = Input[0];

			ISeries<double> highSeries	= High;
			ISeries<double> lowSeries	= Low;

			if (!UseHighLow)
			{
				highSeries	= Input;
				lowSeries	= Input;
			}

			// Calculation always for 1-bar ago !
			bool isSwingHigh			= highSeries[1].ApproxCompare(highSeries[0]) >= 0
											&& highSeries[1].ApproxCompare(highSeries[2]) >= 0;
			bool isSwingLow				= lowSeries[1].ApproxCompare(lowSeries[0]) <= 0
											&& lowSeries[1].ApproxCompare(lowSeries[2]) <= 0;
			bool isOverHighDeviation	= (DeviationType == DeviationType.Percent && IsPriceGreater(highSeries[1], (lastSwingPrice * (1.0 + DeviationValue / 100.0))))
											|| (DeviationType == DeviationType.Points && IsPriceGreater(highSeries[1], lastSwingPrice + DeviationValue));
			bool isOverLowDeviation		= (DeviationType == DeviationType.Percent && IsPriceGreater(lastSwingPrice * (1.0 - DeviationValue / 100.0), lowSeries[1]))
											|| (DeviationType == DeviationType.Points && IsPriceGreater(lastSwingPrice - DeviationValue, lowSeries[1]));

			double	saveValue	= 0.0;
			bool	addHigh		= false;
			bool	addLow		= false;
			bool	updateHigh	= false;
			bool	updateLow	= false;

			zigZagHighZigZags[0]	= 0;
			zigZagLowZigZags[0]		= 0;

			if (!isSwingHigh && !isSwingLow)
			{
				zigZagHighSeries[0] = currentZigZagHigh;
				zigZagLowSeries[0]	= currentZigZagLow;
				return;
			}

			if (trendDir <= 0 && isSwingHigh && isOverHighDeviation)
			{
				saveValue	= highSeries[1];
				addHigh		= true;
				trendDir	= 1;
			}
			else if (trendDir >= 0 && isSwingLow && isOverLowDeviation)
			{
				saveValue	= lowSeries[1];
				addLow		= true;
				trendDir	= -1;
			}
			else if (trendDir == 1 && isSwingHigh && IsPriceGreater(highSeries[1], lastSwingPrice))
			{
				saveValue	= highSeries[1];
				updateHigh	= true;
			}
			else if (trendDir == -1 && isSwingLow && IsPriceGreater(lastSwingPrice, lowSeries[1]))
			{
				saveValue	= lowSeries[1];
				updateLow	= true;
			}

			if (addHigh || addLow || updateHigh || updateLow)
			{
				if (updateHigh && lastSwingIdx >= 0)
				{
					zigZagHighZigZags[CurrentBar - lastSwingIdx] = 0;
					Value.Reset(CurrentBar - lastSwingIdx);
				}
				else if (updateLow && lastSwingIdx >= 0)
				{
					zigZagLowZigZags[CurrentBar - lastSwingIdx] = 0;
					Value.Reset(CurrentBar - lastSwingIdx);
				}

				if (addHigh || updateHigh)
				{
					zigZagHighZigZags[1]	= saveValue;
					zigZagHighZigZags[0]	= 0;

					currentZigZagHigh 		= saveValue;
					zigZagHighSeries[1]		= currentZigZagHigh;
					Value[1]				= currentZigZagHigh;
				}
				else if (addLow || updateLow)
				{
					zigZagLowZigZags[1]	= saveValue;
					zigZagLowZigZags[0]	= 0;

					currentZigZagLow 	= saveValue;
					zigZagLowSeries[1]	= currentZigZagLow;
					Value[1]			= currentZigZagLow;
				}

				lastSwingIdx	= CurrentBar - 1;
				lastSwingPrice	= saveValue;
			}

			zigZagHighSeries[0]	= currentZigZagHigh;
			zigZagLowSeries[0]	= currentZigZagLow;
		}

		#region Properties
		/// <summary>
		/// Gets the ZigZag high points.
		/// </summary>
		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DeviationType", GroupName = "NinjaScriptParameters", Order = 0)]
		public DeviationType DeviationType
		{ get; set; }

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DeviationValue", GroupName = "NinjaScriptParameters", Order = 1)]
		public double DeviationValue
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "UseHighLow", GroupName = "NinjaScriptParameters", Order = 2)]
		public bool UseHighLow
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZigZagHigh
		{
			get
			{
				Update();
				return zigZagHighSeries;
			}
		}

		/// <summary>
		/// Gets the ZigZag low points.
		/// </summary>
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> ZigZagLow
		{
			get
			{
				Update();
				return zigZagLowSeries;
			}
		}
		#endregion

		#region Miscellaneous
		private bool IsPriceGreater(double a, double b)
		{
			return a.ApproxCompare(b) > 0;
		}

		public override void OnCalculateMinMax()
		{
			MinValue = double.MaxValue;
			MaxValue = double.MinValue;
			if (BarsArray[0] == null || ChartBars == null)
				return;

			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				for (int idx = ChartBars.FromIndex - Displacement; idx <= ChartBars.ToIndex + Displacement; idx++)
				{
					if (idx < 0 || idx > Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0))
						continue;
					if (zigZagHighZigZags.IsValidDataPointAt(idx) && zigZagHighZigZags.GetValueAt(idx) != 0)
						MaxValue = Math.Max(MaxValue, zigZagHighZigZags.GetValueAt(idx));
					if (zigZagLowZigZags.IsValidDataPointAt(idx) && zigZagLowZigZags.GetValueAt(idx) != 0)
						MinValue = Math.Min(MinValue, zigZagLowZigZags.GetValueAt(idx));
				}
			}
		}

		protected override Point[] OnGetSelectionPoints(ChartControl chartControl, ChartScale chartScale)
		{
			if (!IsSelected || Count == 0 || Plots[0].Brush.IsTransparent())
				return new System.Windows.Point[0];

			List<System.Windows.Point> points = new List<System.Windows.Point>();

			int lastIndex	= Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? ChartBars.ToIndex - 1 : ChartBars.ToIndex - 2;

			for (int i = Math.Max(0, ChartBars.FromIndex - Displacement); i <= Math.Max(lastIndex, Math.Min(Bars.Count - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 2 : 1), lastIndex - Displacement)); i++)
			{
				int x = (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti && i + Displacement >= ChartBars.Count
					? chartControl.GetXByTime(ChartBars.GetTimeByBarIdx(chartControl, i + Displacement))
					: chartControl.GetXByBarIndex(ChartBars, i + Displacement));

				if (Value.IsValidDataPointAt(i))
					points.Add(new System.Windows.Point(x, chartScale.GetYByValue(Value.GetValueAt(i))));
			}
			return points.ToArray();
		}

		protected override void OnRender(Gui.Chart.ChartControl chartControl, Gui.Chart.ChartScale chartScale)
		{
			if (Bars == null || chartControl == null)
				return;

			IsValidDataPointAt(Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0)); // Make sure indicator is calculated until last (existing) bar
			int preDiff = 1;
			for (int i = ChartBars.FromIndex - 1; i >= 0; i--)
			{
				if (i - Displacement < 0 || i - Displacement > Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0))
					break;

				bool isHigh	= zigZagHighZigZags.IsValidDataPointAt(i - Displacement) && zigZagHighZigZags.GetValueAt(i - Displacement) > 0;
				bool isLow	= zigZagLowZigZags.IsValidDataPointAt(i - Displacement) && zigZagLowZigZags.GetValueAt(i - Displacement) > 0;

				if (isHigh || isLow)
					break;

				preDiff++;
			}

			preDiff -= (Displacement < 0 ? Displacement : 0 - Displacement);

			int postDiff = 0;
			for (int i = ChartBars.ToIndex; i <= zigZagHighZigZags.Count; i++)
			{
				if (i - Displacement < 0 || i - Displacement > Bars.Count - 1 - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 1 : 0))
					break;

				bool isHigh	= zigZagHighZigZags.IsValidDataPointAt(i - Displacement) && zigZagHighZigZags.GetValueAt(i - Displacement) > 0;
				bool isLow	= zigZagLowZigZags.IsValidDataPointAt(i - Displacement) && zigZagLowZigZags.GetValueAt(i - Displacement) > 0;

				if (isHigh || isLow)
					break;

				postDiff++;
			}

			postDiff += (Displacement < 0 ? 0 - Displacement : Displacement);

			int		lastIdx		= -1;
			double	lastValue	= -1;
			SharpDX.Direct2D1.PathGeometry	g		= null;
			SharpDX.Direct2D1.GeometrySink	sink	= null;

			for (int idx = ChartBars.FromIndex - preDiff; idx <= ChartBars.ToIndex + postDiff; idx++)
			{
				if (idx < 0 || idx > Bars.Count - (Calculate == NinjaTrader.NinjaScript.Calculate.OnBarClose ? 2 : 1) || idx < Math.Max(BarsRequiredToPlot - Displacement, Displacement))
					continue;

				bool isHigh	= zigZagHighZigZags.IsValidDataPointAt(idx) && zigZagHighZigZags.GetValueAt(idx) > 0;
				bool isLow	= zigZagLowZigZags.IsValidDataPointAt(idx) && zigZagLowZigZags.GetValueAt(idx) > 0;

				if (!isHigh && !isLow)
					continue;

				double value = isHigh ? zigZagHighZigZags.GetValueAt(idx) : zigZagLowZigZags.GetValueAt(idx);
				if (lastValue >= 0)
				{
					float x1	= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti && idx + Displacement >= ChartBars.Count
						? chartControl.GetXByTime(ChartBars.GetTimeByBarIdx(chartControl, idx + Displacement))
						: chartControl.GetXByBarIndex(ChartBars, idx + Displacement));
					float y1	= chartScale.GetYByValue(value);

					if (sink == null)
					{
						float x0	= (chartControl.BarSpacingType == BarSpacingType.TimeBased || chartControl.BarSpacingType == BarSpacingType.EquidistantMulti && lastIdx + Displacement >= ChartBars.Count
						? chartControl.GetXByTime(ChartBars.GetTimeByBarIdx(chartControl, lastIdx + Displacement))
						: chartControl.GetXByBarIndex(ChartBars, lastIdx + Displacement));
						float y0	= chartScale.GetYByValue(lastValue);
						g			= new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
						sink		= g.Open();
						sink.BeginFigure(new SharpDX.Vector2(x0, y0), SharpDX.Direct2D1.FigureBegin.Hollow);
					}
					sink.AddLine(new SharpDX.Vector2(x1, y1));
				}

				// Save as previous point
				lastIdx		= idx;
				lastValue	= value;
			}

			if (sink != null)
			{
				sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Open);
				sink.Close();
			}

			if (g != null)
			{
				var oldAntiAliasMode = RenderTarget.AntialiasMode;
				RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
				RenderTarget.DrawGeometry(g, Plots[0].BrushDX, Plots[0].Width, Plots[0].StrokeStyle);
				RenderTarget.AntialiasMode = oldAntiAliasMode;
				g.Dispose();
				RemoveDrawObject("NinjaScriptInfo");
			}
			else
				Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.ZigZagDeviationValueError, TextPosition.BottomRight);
		}
		#endregion
	}
}

public enum DeviationType
{
	Percent,
	Points
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ZigZag[] cacheZigZag;
		public ZigZag ZigZag(DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			return ZigZag(Input, deviationType, deviationValue, useHighLow);
		}

		public ZigZag ZigZag(ISeries<double> input, DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			if (cacheZigZag != null)
				for (int idx = 0; idx < cacheZigZag.Length; idx++)
					if (cacheZigZag[idx] != null && cacheZigZag[idx].DeviationType == deviationType && cacheZigZag[idx].DeviationValue == deviationValue && cacheZigZag[idx].UseHighLow == useHighLow && cacheZigZag[idx].EqualsInput(input))
						return cacheZigZag[idx];
			return CacheIndicator<ZigZag>(new ZigZag(){ DeviationType = deviationType, DeviationValue = deviationValue, UseHighLow = useHighLow }, input, ref cacheZigZag);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ZigZag ZigZag(DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			return indicator.ZigZag(Input, deviationType, deviationValue, useHighLow);
		}

		public Indicators.ZigZag ZigZag(ISeries<double> input , DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			return indicator.ZigZag(input, deviationType, deviationValue, useHighLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ZigZag ZigZag(DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			return indicator.ZigZag(Input, deviationType, deviationValue, useHighLow);
		}

		public Indicators.ZigZag ZigZag(ISeries<double> input , DeviationType deviationType, double deviationValue, bool useHighLow)
		{
			return indicator.ZigZag(input, deviationType, deviationValue, useHighLow);
		}
	}
}

#endregion
