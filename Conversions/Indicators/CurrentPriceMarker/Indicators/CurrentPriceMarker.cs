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
	public class CurrentPriceMarker : Indicator
	{
		private DXMediaBrush PriceLineBrush, PriceTextBrush, PriceAreaBrush;
		
		public override string DisplayName
		{
			get { return Name; }
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Places a marker on the chart displaying where the current price is.";
				Name										= "CurrentPriceMarker";
				Calculate									= Calculate.OnPriceChange;
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
				
				Font				= new SimpleFont("Arial", 10);
				PriceLineColor		= Brushes.RoyalBlue;
				PriceTextColor		= Brushes.White;
				PriceAreaColor		= Brushes.Black;
				PriceLineLength		= 15;
				PriceLineWidth		= 2;
				PriceLineStyle		= DashStyleHelper.Dash;
				PriceLineOpacity 	= 50;
			}
			else if (State == State.Configure)
			{	
				// Create our DXMediaBrushes
				PriceLineBrush = new DXMediaBrush();
				PriceTextBrush = new DXMediaBrush();
				PriceAreaBrush = new DXMediaBrush();
				
				// Set our DXMediaBrush properties
				PriceLineBrush.UpdateBrush(RenderTarget, PriceLineColor, PriceLineOpacity);
				PriceTextBrush.UpdateBrush(RenderTarget, PriceTextColor, PriceLineOpacity);
				PriceAreaBrush.UpdateBrush(RenderTarget, PriceAreaColor, PriceLineOpacity);				
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			if (ChartBars == null)
				return;
			
			// Set AntiAlias mode for smoothing
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Draw Price Line
			DrawLine(PriceLineBrush,
					chartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex - PriceLineLength),
					chartScale.GetYByValue(Close.GetValueAt(CurrentBar)),
					ChartPanel.W,
					chartScale.GetYByValue(Close.GetValueAt(CurrentBar)),
					PriceLineWidth,
					PriceLineStyle);
			
			// Draw label
			DrawString(Close.GetValueAt(CurrentBar).ToString() + " = Last Price --  ",
					Font,
					PriceTextBrush,
					chartControl.GetXByBarIndex(ChartBars,ChartBars.ToIndex - PriceLineLength),
					chartScale.GetYByValue(Close.GetValueAt(CurrentBar)),
					PriceAreaBrush);
			
			//Reset AntiAlias mode
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}
		
		public override void OnRenderTargetChanged()
        {
            // Dispose and recreate our DX Brushes or other Device Dependant resources on RenderTarget changes
			// Any DXMediaBrush that we create on our own will need to be handled here.
			if (PriceLineBrush != null)
				PriceLineBrush.RenderTargetChange(RenderTarget);
			
			if (PriceTextBrush != null)
				PriceTextBrush.RenderTargetChange(RenderTarget);
			
			if (PriceTextBrush != null)
				PriceAreaBrush.RenderTargetChange(RenderTarget);
        }
		
		#region SharpDX Helper Classes/Methods

		// Our DX/Media Brush management class.
        [Browsable(false)]
	    public class DXMediaBrush
	    {
			private System.Windows.Media.Brush mediaBrush;
			private SharpDX.Direct2D1.Brush dxBrush;
			private double opacity;

	        public byte GetAlpha()
	        {
	            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).A;
	        }

	        public byte GetRed()
	        {
	            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).R;
	        }

	        public byte GetGreen()
	        {
	            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).G;
	        }

	        public byte GetBlue()
	        {
	            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).B;
	        }

	        private void SetOpacity(double newOpacity)
	        {
	            if (mediaBrush == null)
	                return;
				
				// Force opcatity to be in bounds
				opacity = Math.Min(100.0, Math.Max(0, newOpacity));

				// Clone any Frozen brush so it can be modified
	            if (mediaBrush.IsFrozen)
	                mediaBrush = mediaBrush.Clone();

				// Set Opacity and freeze brush.
	            mediaBrush.Opacity = opacity / 100.0;
	            mediaBrush.Freeze();
	        }
			
			public DXMediaBrush()
			{
				dxBrush = null;
				mediaBrush = null;
				opacity = 100.0;
			}
			
			public void Dispose()
			{
				if(dxBrush != null)
					dxBrush.Dispose();
			}
			
			/// <summary>
			/// Updates the Media Bursh and SharpDX Brush without changing opacity.
			/// </summary>
			/// <param name="owner">The hosting NinjaScript's RenderTarget</param>
			/// <param name="owner">The new Opacity to use</param>
			public void UpdateOpacity(SharpDX.Direct2D1.RenderTarget renderTarget, double newOpacity)
			{					
				UpdateBrush(renderTarget, mediaBrush, newOpacity);
			}
			
			/// <summary>
			/// Updates the Media Bursh and SharpDX Brush also changing opacity.
			/// </summary>
			/// <param name="owner">The hosting NinjaScript's RenderTarget</param>
			/// <param name="owner">The new Media Brush to use</param>
			/// <param name="owner">The new Opacity to use</param>
			public void UpdateBrush(SharpDX.Direct2D1.RenderTarget renderTarget, Brush newMediaBrush, double newOpacity)
	        {					
				// Set Media Brush to brush passed
	            mediaBrush = newMediaBrush;
				
				// Call SetOpacity() to clone, set opacity and freeze brush.
	            SetOpacity(newOpacity);
				
				// Dispose DX Brushes and other Device Dependant resources
	            if (dxBrush != null)
	                dxBrush.Dispose();
				
				// Recreate DX Brushes and other Device Dependant Resources here, making sure RenderTarget is not null or IsDisposed
	            if (renderTarget != null && !renderTarget.IsDisposed)
				{
					dxBrush = mediaBrush.ToDxBrush(renderTarget); 
				}
	        }
			
			/// <summary>
			/// Updates the Media Bursh and SharpDX Brush without changing opacity.
			/// </summary>
			/// <param name="owner">The hosting NinjaScript's RenderTarget</param>
			/// <param name="owner">The new Media Brush to use</param>
			public void UpdateBrush(SharpDX.Direct2D1.RenderTarget renderTarget, Brush newMediaBrush)
	        {				
				UpdateBrush(renderTarget, newMediaBrush, opacity);
	        }
			
			/// <summary>
			/// Updates device dependent resources for when RenderTarget changes.
			/// </summary>
			/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
			public void RenderTargetChange(SharpDX.Direct2D1.RenderTarget renderTarget)
			{
				if (renderTarget == null || renderTarget.IsDisposed)
					return;
				
				if (dxBrush != null)
					dxBrush.Dispose();

				if (mediaBrush != null)
					dxBrush = mediaBrush.ToDxBrush(renderTarget);
			}
			
			/// <summary>
			/// Returns SharpDX Brush.
			/// </summary>
			public SharpDX.Direct2D1.Brush DxBrush
			{
				get { return dxBrush; }
			}
			
			/// <summary>
			/// Returns Windows Media Brush.
			/// </summary>
			/// 
	        public System.Windows.Media.Brush MediaBrush
			{
				get { return mediaBrush; }
			}
			
			/// <summary>
			/// Returns Brush Opactiy.
			/// </summary>
			public double Opacity
			{
				get { return opacity; }
			}
	    }
		
		private void DrawString(string text, SimpleFont font, DXMediaBrush brush, double pointX, double pointY, DXMediaBrush areaBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
			
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX - newW, pointY-textLayout.Metrics.Height/2 - 1).ToVector2();
			
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX - newW - 4, (float)pointY-textLayout.Metrics.Height/2 - 1, newW+6, newH+2);
			
			SharpDX.Direct2D1.RoundedRectangle PLRoundedRect = new SharpDX.Direct2D1.RoundedRectangle();
			
			PLRoundedRect.RadiusX = newW/4;
			PLRoundedRect.RadiusY = newH/4;
			PLRoundedRect.Rect = PLBoundRect;
            
			RenderTarget.FillRoundedRectangle(PLRoundedRect, areaBrush.DxBrush);
			
			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, brush.DxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			
			textLayout.Dispose();
			textLayout = null;
			textFormat.Dispose();
			textFormat = null;
		}
		
		private void DrawLine(DXMediaBrush brush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			if (dashStyle == DashStyleHelper.Dash)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash;
			if (dashStyle == DashStyleHelper.DashDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot;
			if (dashStyle == DashStyleHelper.DashDotDot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;
			if (dashStyle == DashStyleHelper.Dot)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;
			if (dashStyle == DashStyleHelper.Solid)
				ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			SharpDX.Vector2 startPoint = new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint = new System.Windows.Point(x2, y2).ToVector2();
			
			RenderTarget.DrawLine(startPoint, endPoint, brush.DxBrush, width, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}
    	#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Price Line Font", Description="Font of price line text", Order=1, GroupName="Parameters")]
		public SimpleFont Font
		{ get; set; }
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Price Line Color", Description="Color of price line", Order=1, GroupName="Parameters")]
		public Brush PriceLineColor
		{ get; set; }

		[Browsable(false)]
		public string PriceLineColorSerializable
		{
			get { return Serialize.BrushToString(PriceLineColor); }
			set { PriceLineColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Price Line Text Color", Description="Color of price line text", Order=2, GroupName="Parameters")]
		public Brush PriceTextColor
		{ get; set; }

		[Browsable(false)]
		public string PriceTextColorSerializable
		{
			get { return Serialize.BrushToString(PriceTextColor); }
			set { PriceTextColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Price Line Text Area Color", Description="Color of price line text area", Order=3, GroupName="Parameters")]
		public Brush PriceAreaColor
		{ get; set; }

		[Browsable(false)]
		public string PriceAreaColorSerializable
		{
			get { return Serialize.BrushToString(PriceAreaColor); }
			set { PriceAreaColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Price Line length", Description="Length of price line.", Order=4, GroupName="Parameters")]
		public int PriceLineLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Price Line width", Description="Width of price line.", Order=5, GroupName="Parameters")]
		public int PriceLineWidth
		{ get; set; }	

		[NinjaScriptProperty]
		[Display(Name="Price Line style", Description="Style of price line.", Order=6, GroupName="Parameters")]
		public DashStyleHelper PriceLineStyle
        { get; set; }
		
		[NinjaScriptProperty]
		[Range(1, 100)]
		[Display(Name="Price Line opacity", Description="Opacity of price line.", Order=7, GroupName="Parameters")]
		public double PriceLineOpacity
        { get; set; }
		#endregion
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CurrentPriceMarker[] cacheCurrentPriceMarker;
		public CurrentPriceMarker CurrentPriceMarker(SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			return CurrentPriceMarker(Input, font, priceLineColor, priceTextColor, priceAreaColor, priceLineLength, priceLineWidth, priceLineStyle, priceLineOpacity);
		}

		public CurrentPriceMarker CurrentPriceMarker(ISeries<double> input, SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			if (cacheCurrentPriceMarker != null)
				for (int idx = 0; idx < cacheCurrentPriceMarker.Length; idx++)
					if (cacheCurrentPriceMarker[idx] != null && cacheCurrentPriceMarker[idx].Font == font && cacheCurrentPriceMarker[idx].PriceLineColor == priceLineColor && cacheCurrentPriceMarker[idx].PriceTextColor == priceTextColor && cacheCurrentPriceMarker[idx].PriceAreaColor == priceAreaColor && cacheCurrentPriceMarker[idx].PriceLineLength == priceLineLength && cacheCurrentPriceMarker[idx].PriceLineWidth == priceLineWidth && cacheCurrentPriceMarker[idx].PriceLineStyle == priceLineStyle && cacheCurrentPriceMarker[idx].PriceLineOpacity == priceLineOpacity && cacheCurrentPriceMarker[idx].EqualsInput(input))
						return cacheCurrentPriceMarker[idx];
			return CacheIndicator<CurrentPriceMarker>(new CurrentPriceMarker(){ Font = font, PriceLineColor = priceLineColor, PriceTextColor = priceTextColor, PriceAreaColor = priceAreaColor, PriceLineLength = priceLineLength, PriceLineWidth = priceLineWidth, PriceLineStyle = priceLineStyle, PriceLineOpacity = priceLineOpacity }, input, ref cacheCurrentPriceMarker);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CurrentPriceMarker CurrentPriceMarker(SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			return indicator.CurrentPriceMarker(Input, font, priceLineColor, priceTextColor, priceAreaColor, priceLineLength, priceLineWidth, priceLineStyle, priceLineOpacity);
		}

		public Indicators.CurrentPriceMarker CurrentPriceMarker(ISeries<double> input , SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			return indicator.CurrentPriceMarker(input, font, priceLineColor, priceTextColor, priceAreaColor, priceLineLength, priceLineWidth, priceLineStyle, priceLineOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CurrentPriceMarker CurrentPriceMarker(SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			return indicator.CurrentPriceMarker(Input, font, priceLineColor, priceTextColor, priceAreaColor, priceLineLength, priceLineWidth, priceLineStyle, priceLineOpacity);
		}

		public Indicators.CurrentPriceMarker CurrentPriceMarker(ISeries<double> input , SimpleFont font, Brush priceLineColor, Brush priceTextColor, Brush priceAreaColor, int priceLineLength, int priceLineWidth, DashStyleHelper priceLineStyle, double priceLineOpacity)
		{
			return indicator.CurrentPriceMarker(input, font, priceLineColor, priceTextColor, priceAreaColor, priceLineLength, priceLineWidth, priceLineStyle, priceLineOpacity);
		}
	}
}

#endregion
