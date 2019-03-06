#region Using declarations
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using SharpDX;
using SharpDX.Direct2D1;
using Point = System.Windows.Point;
using System.Collections.Generic;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class SwamiWave : Indicator
	{
		private Series<double>[] iValues;
		private Dictionary<SharpDX.Color, DXColorMap> brush_collection;
		private SharpDX.Color bullishColor;
        private SharpDX.Color bearishColor;
        private SharpDX.Color neutralColor;
        
		delegate byte ComponentSelector(SharpDX.Color color);

        private static ComponentSelector redSelector = color => color.R;
        private static ComponentSelector greenSelector = color => color.G;
        private static ComponentSelector blueSelector = color => color.B;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Stocks and Commodities - April 2012 - Setting Strategies with SwamiCharts";
				Name										= "SwamiWave";
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
				MinLength									= 8;
				MaxLength									= 40;
				BearishColor								= System.Windows.Media.Brushes.Blue;
				BullishColor								= System.Windows.Media.Brushes.Orange;
				NeutralColor								= System.Windows.Media.Brushes.Transparent;
				bearishColor								= Color.Black;
				bullishColor								= Color.Black;
				neutralColor								= Color.Black;
			}
			else if (State == State.DataLoaded)
			{
				iValues = new Series<double>[MaxLength - MinLength + 1];
                for (int i = 0; i < iValues.Length; i++)
                    iValues[i] = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				brush_collection = new Dictionary<SharpDX.Color,DXColorMap>();
			}
			else if (State == State.Terminated)
			{
				if(brush_collection != null)
				{
					foreach (KeyValuePair<SharpDX.Color, DXColorMap> item in brush_collection)
						if (item.Value.DxBrush != null)
							item.Value.DxBrush.Dispose();
				}
			}
			
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < MaxLength) return;
			for (int x = 0; x < iValues.Length; x++)
				iValues[x][0] = BPF(x + MinLength)[0];
		}
		
		public override void OnRenderTargetChanged()
        {
            // Dispose and recreate our DX Brushes
            try
            {
				if (brush_collection == null)
					return;
                foreach (KeyValuePair<SharpDX.Color, DXColorMap> item in brush_collection)
                {						
                    if (item.Value != null)
                        item.Value.DxBrush.Dispose();

                    if (RenderTarget != null && !RenderTarget.IsDisposed)
                        item.Value.DxBrush = new SolidColorBrush(RenderTarget, item.Key);
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString(), LogLevel.Error);
            }
        }

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (ChartControl == null)
				return;

			// Create our basic SharpDX colors from our Media Brushes
			if(RenderTarget != null)
			{
				if(bearishColor == Color.Black)
					bearishColor = (SharpDX.Color)((SharpDX.Direct2D1.SolidColorBrush)BearishColor.ToDxBrush(RenderTarget)).Color;
				if(bullishColor == Color.Black)
                	bullishColor = (SharpDX.Color)((SharpDX.Direct2D1.SolidColorBrush)BullishColor.ToDxBrush(RenderTarget)).Color;
				if(neutralColor == Color.Black)
					if(NeutralColor == System.Windows.Media.Brushes.Transparent)
                		neutralColor = Color.Transparent;	
					else
						neutralColor = (SharpDX.Color)((SharpDX.Direct2D1.SolidColorBrush)NeutralColor.ToDxBrush(RenderTarget)).Color;
			}
			
            try
            {
                for (int idx = chartControl.LastSlotPainted; idx >= chartControl.GetSlotIndexByTime(chartControl.FirstTimePainted); idx--)
                {
                    if (idx - Displacement < 0 || idx - Displacement >= Bars.Count || (idx - Displacement < this.BarsRequiredToPlot))
                        continue;

                    int x = Math.Max(1, chartControl.GetXByBarIndex(ChartBars, idx));
                    int xprev = Math.Max(1, chartControl.GetXByBarIndex(ChartBars, idx-1));
                    for (int cIdx = 0; cIdx < iValues.Length; cIdx++)
                    {   
						double valuePadding = 0.3;
						double curVal 		= iValues[cIdx].GetValueAt(idx);
						double prevVal 		= iValues[cIdx].GetValueAt(Math.Max(0, idx - 1));
						double curAbove 	= cIdx == iValues.Length - 1 ? curVal : iValues[cIdx + 1].GetValueAt(idx);
						double prevAbove 	= cIdx == iValues.Length - 1 ? prevVal : iValues[cIdx + 1].GetValueAt(Math.Max(0, idx - 1));

                        float yt = chartScale.GetYByValue(cIdx + MinLength + valuePadding);
                        float yc = chartScale.GetYByValue(cIdx + MinLength);  //XY
                        float yb = chartScale.GetYByValue(cIdx + MinLength - valuePadding);
                        float tt = chartScale.GetYByValue(cIdx + 1 + MinLength - valuePadding);
                        float dist = (float) (0.25*(x - xprev));
                        float dist2 = Math.Max((float) chartControl.BarWidth, dist);
                        float xl = x - dist2;
                        float xc = x; //XY
                        float xr = x + dist2;
                        float ll = xprev + dist2; //??
                        float radius = (float) Math.Sqrt(Math.Pow(xl - ll, 2) + Math.Pow(yt - tt, 2));

                        Point point_XY = new Point(xc, yc);
                        Point point_CC = new Point(xl, yt);
                        Point point_CR = new Point(xr, yt);
                        Point point_RB = new Point(xr, yb);
                        Point point_CB = new Point(xl, yb);
                        Point point_CL = new Point(ll, yt);
                        Point point_LB = new Point(ll, yb);
                        Point point_LT = new Point(ll, tt);
                        Point point_CT = new Point(xl, tt);
                        Point point_RT = new Point(xr, tt);						
						
						// Assign Interpolated colors
                        SharpDX.Color mainColor = InterpolateBetween(bearishColor, bullishColor, neutralColor, curVal);
                        SharpDX.Color aboveColor = InterpolateBetween(bearishColor, bullishColor, neutralColor, curAbove);
                        SharpDX.Color prevColor = InterpolateBetween(bearishColor, bullishColor, neutralColor, prevVal);
                        SharpDX.Color prevAboveColor = InterpolateBetween(bearishColor, bullishColor, neutralColor, prevAbove);

                        // *********** Main rectangle ***************
                        SharpDX.RectangleF rect_main = new SharpDX.RectangleF((float)point_CC.X, (float)point_CC.Y, (float)(point_CR.X - point_CC.X), (float)(point_RB.Y - point_CR.Y));
						CreateDXBrush((Color)mainColor);
                        RenderTarget.FillRectangle(rect_main, brush_collection[(Color)mainColor].DxBrush);

                        // ************ Above rectangle *************
                        SharpDX.RectangleF rect_above = new SharpDX.RectangleF((float)point_CT.X, (float)point_CT.Y, (float)(point_RT.X - point_CT.X), (float)(point_CR.Y - point_RT.Y));				
						CreateDXBrush((Color)aboveColor);
						RenderTarget.FillRectangle(rect_above, brush_collection[(Color)aboveColor].DxBrush);
                        

                        // ************ Previous rectangle *****************
                        SharpDX.RectangleF rect_prev = new SharpDX.RectangleF((float)point_CL.X, (float)point_CL.Y, (float)(point_CC.X - point_CL.X), (float)(point_LB.Y - point_CL.Y));			 
						CreateDXBrush((Color)prevColor);
						RenderTarget.FillRectangle(rect_prev, brush_collection[(Color)prevColor].DxBrush);

                        // ************ Previous above rectangle **************** 
                        SharpDX.RectangleF rect_prevAbove = new SharpDX.RectangleF((float)point_LT.X, (float)point_LT.Y, (float)(point_CT.X - point_LT.X), (float)(point_CL.Y - point_LT.Y));
						CreateDXBrush((Color)prevAboveColor);
                        RenderTarget.FillRectangle(rect_prevAbove, brush_collection[(Color)prevAboveColor].DxBrush);
                    }
                }
            }
            catch (Exception exc)
            {
            }

            base.OnRender(chartControl, chartScale);
        }

        public override void OnCalculateMinMax()
        {
            MinValue = MinLength - 1;
            MaxValue = MaxLength + 1;
        }
		
		#region Color Inerpolation
        private static SharpDX.Color InterpolateBetween(SharpDX.Color downPoint, SharpDX.Color upPoint, SharpDX.Color midPoint, double lambda)
        {
            lambda = Math.Max(Math.Min(lambda, 1), 0);

            if (midPoint == SharpDX.Color.Transparent)
                return new SharpDX.Color(
                    InterpolateComponent(downPoint, upPoint, lambda, redSelector),
                    InterpolateComponent(downPoint, upPoint, lambda, greenSelector),
                    InterpolateComponent(downPoint, upPoint, lambda, blueSelector));
            if (lambda < 0.5)
                return new SharpDX.Color(
                    InterpolateComponent(downPoint, midPoint, lambda * 2, redSelector),
                    InterpolateComponent(downPoint, midPoint, lambda * 2, greenSelector),
                    InterpolateComponent(downPoint, midPoint, lambda * 2, blueSelector));
            return new SharpDX.Color(
                InterpolateComponent(midPoint, upPoint, lambda * 2 - 1, redSelector),
                InterpolateComponent(midPoint, upPoint, lambda * 2 - 1, greenSelector),
                InterpolateComponent(midPoint, upPoint, lambda * 2 - 1, blueSelector));
        }

        private static byte InterpolateComponent(SharpDX.Color endPoint1, SharpDX.Color endPoint2, double lambda, ComponentSelector selector)
        {
            return (byte)(selector(endPoint1) + (selector(endPoint2) - selector(endPoint1)) * lambda);
        }
		#endregion
		
		#region SharpDX Helper Methods/Classes		
		private void CreateDXBrush(SharpDX.Color color)
		{
			if(RenderTarget == null || RenderTarget.IsDisposed)
				return;
			if(!brush_collection.ContainsKey(color))
			{
				brush_collection.Add(color, new DXColorMap());
				brush_collection[color].DxColor = color;
				brush_collection[color].DxBrush = new SolidColorBrush(RenderTarget, color);
			}
			
		}
		
		[Browsable(false)]
        public class DXColorMap
        {
            public SharpDX.Direct2D1.Brush DxBrush;
            public SharpDX.Color DxColor;
        }
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MinLength", Description="Min Period Length", Order=1, GroupName="Parameters")]
		public int MinLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MaxLength", Description="Max Period Length", Order=2, GroupName="Parameters")]
		public int MaxLength
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BearishColor", Description="Bearish Color", Order=3, GroupName="Parameters")]
		public System.Windows.Media.Brush BearishColor
		{ get; set; }

		[Browsable(false)]
		public string BearishColorSerializable
		{
			get { return Serialize.BrushToString(BearishColor); }
			set { BearishColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="BullishColor", Description="Bullish Color", Order=4, GroupName="Parameters")]
		public System.Windows.Media.Brush BullishColor
		{ get; set; }

		[Browsable(false)]
		public string BullishColorSerializable
		{
			get { return Serialize.BrushToString(BullishColor); }
			set { BullishColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="NeutralColor", Description="Neutral Color", Order=5, GroupName="Parameters")]
		public System.Windows.Media.Brush NeutralColor
		{ get; set; }

		[Browsable(false)]
		public string NeutralColorSerializable
		{
			get { return Serialize.BrushToString(NeutralColor); }
			set { NeutralColor = Serialize.StringToBrush(value); }
		}	
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SwamiWave[] cacheSwamiWave;
		public SwamiWave SwamiWave(int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			return SwamiWave(Input, minLength, maxLength, bearishColor, bullishColor, neutralColor);
		}

		public SwamiWave SwamiWave(ISeries<double> input, int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			if (cacheSwamiWave != null)
				for (int idx = 0; idx < cacheSwamiWave.Length; idx++)
					if (cacheSwamiWave[idx] != null && cacheSwamiWave[idx].MinLength == minLength && cacheSwamiWave[idx].MaxLength == maxLength && cacheSwamiWave[idx].BearishColor == bearishColor && cacheSwamiWave[idx].BullishColor == bullishColor && cacheSwamiWave[idx].NeutralColor == neutralColor && cacheSwamiWave[idx].EqualsInput(input))
						return cacheSwamiWave[idx];
			return CacheIndicator<SwamiWave>(new SwamiWave(){ MinLength = minLength, MaxLength = maxLength, BearishColor = bearishColor, BullishColor = bullishColor, NeutralColor = neutralColor }, input, ref cacheSwamiWave);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SwamiWave SwamiWave(int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			return indicator.SwamiWave(Input, minLength, maxLength, bearishColor, bullishColor, neutralColor);
		}

		public Indicators.SwamiWave SwamiWave(ISeries<double> input , int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			return indicator.SwamiWave(input, minLength, maxLength, bearishColor, bullishColor, neutralColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SwamiWave SwamiWave(int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			return indicator.SwamiWave(Input, minLength, maxLength, bearishColor, bullishColor, neutralColor);
		}

		public Indicators.SwamiWave SwamiWave(ISeries<double> input , int minLength, int maxLength, System.Windows.Media.Brush bearishColor, System.Windows.Media.Brush bullishColor, System.Windows.Media.Brush neutralColor)
		{
			return indicator.SwamiWave(input, minLength, maxLength, bearishColor, bullishColor, neutralColor);
		}
	}
}

#endregion
