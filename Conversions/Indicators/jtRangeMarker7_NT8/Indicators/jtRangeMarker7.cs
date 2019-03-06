#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
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
	public class jtRangeMarker7 : Indicator
	{
		private bool isRangeChart;
		private bool firstTime;
		private int digits;
		private const string noRangeMessage = "jtRangeMarker7 only works on range charts!";
		
		private Dictionary<string, DXMediaMap> dxmBrushes;
        private SharpDX.Direct2D1.RenderTarget myRenderTarget = null;
        private Brush ChartFontBrush
        {
            get { return dxmBrushes["ChartFontBrush"].MediaBrush; }
            set { UpdateBrush(value, "ChartFontBrush"); }
        }
		private Brush LockedBrush
        {
            get { return dxmBrushes["LockedBrush"].MediaBrush; }
            set { UpdateBrush(value, "LockedBrush"); }
        }
		private Brush WarningBrush
        {
            get { return dxmBrushes["WarningBrush"].MediaBrush; }
            set { UpdateBrush(value, "WarningBrush"); }
        }
		private Brush TransparentBrush
        {
            get { return dxmBrushes["TransparentBrush"].MediaBrush; }
            set { UpdateBrush(value, "TransparentBrush"); }
        }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Indicate where the range bar may close and a new bar will start.";
				Name										= "jtRangeMarker7";
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
				
				DefaultColor								= Brushes.Black;
				WarningColor								= Brushes.Blue;
				LockedColor									= Brushes.Red;
				TextColor									= Brushes.White;
				ShowPrices									= true;
				firstTime									= true;
				isRangeChart 								= false;
				
				
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "ChartFontBrush", "LockedBrush", "WarningBrush", "TransparentBrush" } )
            	    dxmBrushes.Add(brushName, new DXMediaMap());
				
				ChartFontBrush 		= TextColor;
				LockedBrush			= LockedColor;
				WarningBrush		= WarningColor;
				TransparentBrush 	= Brushes.Transparent;
			}
			else if (State == State.Terminated)
			{
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();
			}
		}

		protected override void OnBarUpdate()
		{
			try 
			{
				if (ChartControl == null || ChartBars == null || ChartBars.Count == 0 )
					return;
				
				if (firstTime)
				{
					if (BarsPeriod.BarsPeriodType == BarsPeriodType.Range || (ChartBars.Bars.BarsType.BuiltFrom == BarsPeriodType.Tick && ChartBars.Bars.BarsPeriod.ToString().IndexOf("Range") >= 0))
					{
						isRangeChart = true;
						firstTime = false;

						// calc digits from TickSize
						digits = 0;
						string s = ((decimal)TickSize).ToString(System.Globalization.CultureInfo.InvariantCulture);
						if (s.Contains(".")){
							string ss = s.Substring(s.IndexOf("."));
							digits = ss.Length-1;
						}
						
						ChartFontBrush 	= TextColor;
						LockedBrush		= LockedColor;
						WarningBrush	= WarningColor;
					}
				}
			} 
			catch (Exception ex)
			{
				Print(ex.ToString());
			}
		}
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			try 
			{			
				if (Bars == null)
					return;

				if (isRangeChart)
				{
					int	actualRange	= (int) Math.Round(Math.Max(Close.GetValueAt(CurrentBar) - Low.GetValueAt(CurrentBar), High.GetValueAt(CurrentBar) - Close.GetValueAt(CurrentBar)) / Bars.Instrument.MasterInstrument.TickSize);
					int	rangeCount	= BarsPeriod.Value - actualRange ;
					
					// determine wiggle room in ticks
					
					int barRange = (int) Math.Round( (High.GetValueAt(CurrentBar) - Low.GetValueAt(CurrentBar)) / Bars.Instrument.MasterInstrument.TickSize);
					
					int margin = (BarsPeriod.Value - barRange);
					
					// calc our rectangle properties
					double highPrice = High.GetValueAt(CurrentBar) + (margin*TickSize);
					double lowPrice =  Low.GetValueAt(CurrentBar)  - (margin*TickSize);
					
					int rangeHighY = (ChartPanel.Y + ChartPanel.H) - ((int) ((( highPrice - ChartPanel.MinValue ) / Math.Max(Math.Abs(ChartPanel.MaxValue - ChartPanel.MinValue), 1E-05)) * ChartPanel.H))-1;
					int rangeLowY =  (ChartPanel.Y + ChartPanel.H) - ((int) ((( lowPrice  - ChartPanel.MinValue ) / Math.Max(Math.Abs(ChartPanel.MaxValue - ChartPanel.MinValue), 1E-05)) * ChartPanel.H))-1;
					int height = rangeLowY - rangeHighY;
					int rangeX = ChartControl.GetXByBarIndex(ChartBars, CurrentBar) -  (int)(ChartControl.Properties.BarDistance/2) ;
					int width =  (int)(ChartControl.Properties.BarDistance);
								
					switch(margin)
					{
						case 0:
							SharpDX.RectangleF PLBoundRect0 = new SharpDX.RectangleF(rangeX, rangeHighY, width+1, height);
							RenderTarget.DrawRectangle(PLBoundRect0, dxmBrushes["LockedBrush"].DxBrush, 2f);
							break;
						case 1:
							SharpDX.RectangleF PLBoundRect1 = new SharpDX.RectangleF(rangeX, rangeHighY, width+1, height);
	                    	RenderTarget.DrawRectangle(PLBoundRect1, dxmBrushes["WarningBrush"].DxBrush, 2f);
							break;
					}
					
					if(ShowPrices)
					{
						int lineH = (int)ChartControl.Properties.LabelFont.TextFormatHeight;
						
						DrawString(highPrice.ToString("F"+digits), ChartControl.Properties.LabelFont, "ChartFontBrush", 
								rangeX,
								rangeHighY - lineH);
						DrawString("R:" + rangeCount.ToString(), ChartControl.Properties.LabelFont, "ChartFontBrush", 
								rangeX + width,
								rangeHighY);
						DrawString(lowPrice.ToString("F"+digits), ChartControl.Properties.LabelFont, "ChartFontBrush", 
								rangeX,
								rangeLowY);
						DrawString("C:" + barRange.ToString(), ChartControl.Properties.LabelFont, "ChartFontBrush", 
								rangeX + width,
								rangeLowY - lineH);
					}
						
				}
				else
				{
					// Create a TextLayout so we can use Metrics for MeasureString()
					SharpDX.DirectWrite.TextFormat textFormat = ChartControl.Properties.LabelFont.ToDirectWriteTextFormat();
					SharpDX.DirectWrite.TextLayout textLayout =
						new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
						noRangeMessage, textFormat, ChartPanel.X + ChartPanel.W,
						textFormat.FontSize, 1, true);
					
					DrawString(noRangeMessage, ChartControl.Properties.LabelFont, "ChartFontBrush", 
						ChartPanel.X + ChartPanel.W - textLayout.Metrics.Width-10, 
						ChartPanel.Y + ChartPanel.H - textLayout.Metrics.Height-10);
				}
			} 
			catch (Exception ex)
			{
				Print(ex.ToString());
			}
		}
		
		public override void OnRenderTargetChanged()
        {
            // Dispose and recreate our DX Brushes
            try
            {
				if (dxmBrushes == null)
					return;
                foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
                {
											
                    if (item.Value.DxBrush != null)
                        item.Value.DxBrush.Dispose();

                    if (RenderTarget != null && item.Value.MediaBrush != null && !RenderTarget.IsDisposed)
                        item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);
                }
            }
            catch (Exception exception)
            {
                Log(exception.ToString(), LogLevel.Error);
            }
        }
		
		#region SharpDX Helper Classes/Methods

        [Browsable(false)]
        public class DXMediaMap
        {
            public SharpDX.Direct2D1.Brush DxBrush;
            public System.Windows.Media.Brush MediaBrush;
        }

        private void SetOpacity(string brushName)
        {
            if (dxmBrushes[brushName].MediaBrush == null)
                return;

            if (dxmBrushes[brushName].MediaBrush.IsFrozen)
                dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

            dxmBrushes[brushName].MediaBrush.Opacity = 100.0 / 100.0;
            dxmBrushes[brushName].MediaBrush.Freeze();
        }
		
		private void UpdateBrush(Brush mediaBrush, string brushName)
        {
            dxmBrushes[brushName].MediaBrush = mediaBrush;
            SetOpacity(brushName);
            if (dxmBrushes[brushName].DxBrush != null)
                dxmBrushes[brushName].DxBrush.Dispose();
            if (RenderTarget != null && !RenderTarget.IsDisposed)
            {
                dxmBrushes[brushName].DxBrush = dxmBrushes[brushName].MediaBrush.ToDxBrush(RenderTarget);
            }
                
        }
		
		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY, string areaBrushName)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY).ToVector2();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX+2, (float)pointY, newW+5, newH+2);
            RenderTarget.FillRectangle(PLBoundRect, dxmBrushes[areaBrushName].DxBrush);
			
			RenderTarget.DrawTextLayout(TextPlotPoint, textLayout, dxmBrushes[brushName].DxBrush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}
		
		private void DrawString(string text, SimpleFont font, string brushName, double pointX, double pointY)
		{
			DrawString(text, font, brushName, pointX, pointY, "TransparentBrush");
		}
    #endregion

		#region Properties
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="DefaultColor", Description="Default Box Color", Order=1, GroupName="Parameters")]
		public Brush DefaultColor
		{ get; set; }

		[Browsable(false)]
		public string DefaultColorSerializable
		{
			get { return Serialize.BrushToString(DefaultColor); }
			set { DefaultColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="WarningColor", Description="Warning Box Color", Order=2, GroupName="Parameters")]
		public Brush WarningColor
		{ get; set; }

		[Browsable(false)]
		public string WarningColorSerializable
		{
			get { return Serialize.BrushToString(WarningColor); }
			set { WarningColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="LockedColor", Description="Locked Box Color", Order=3, GroupName="Parameters")]
		public Brush LockedColor
		{ get; set; }

		[Browsable(false)]
		public string LockedColorSerializable
		{
			get { return Serialize.BrushToString(LockedColor); }
			set { LockedColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TextColor", Description="Text Color", Order=4, GroupName="Parameters")]
		public Brush TextColor
		{ get; set; }

		[Browsable(false)]
		public string TextColorSerializable
		{
			get { return Serialize.BrushToString(TextColor); }
			set { TextColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="ShowPrices", Description="Show price at high and low of box.", Order=5, GroupName="Parameters")]
		public bool ShowPrices
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private jtRangeMarker7[] cachejtRangeMarker7;
		public jtRangeMarker7 jtRangeMarker7(Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			return jtRangeMarker7(Input, defaultColor, warningColor, lockedColor, textColor, showPrices);
		}

		public jtRangeMarker7 jtRangeMarker7(ISeries<double> input, Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			if (cachejtRangeMarker7 != null)
				for (int idx = 0; idx < cachejtRangeMarker7.Length; idx++)
					if (cachejtRangeMarker7[idx] != null && cachejtRangeMarker7[idx].DefaultColor == defaultColor && cachejtRangeMarker7[idx].WarningColor == warningColor && cachejtRangeMarker7[idx].LockedColor == lockedColor && cachejtRangeMarker7[idx].TextColor == textColor && cachejtRangeMarker7[idx].ShowPrices == showPrices && cachejtRangeMarker7[idx].EqualsInput(input))
						return cachejtRangeMarker7[idx];
			return CacheIndicator<jtRangeMarker7>(new jtRangeMarker7(){ DefaultColor = defaultColor, WarningColor = warningColor, LockedColor = lockedColor, TextColor = textColor, ShowPrices = showPrices }, input, ref cachejtRangeMarker7);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.jtRangeMarker7 jtRangeMarker7(Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			return indicator.jtRangeMarker7(Input, defaultColor, warningColor, lockedColor, textColor, showPrices);
		}

		public Indicators.jtRangeMarker7 jtRangeMarker7(ISeries<double> input , Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			return indicator.jtRangeMarker7(input, defaultColor, warningColor, lockedColor, textColor, showPrices);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.jtRangeMarker7 jtRangeMarker7(Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			return indicator.jtRangeMarker7(Input, defaultColor, warningColor, lockedColor, textColor, showPrices);
		}

		public Indicators.jtRangeMarker7 jtRangeMarker7(ISeries<double> input , Brush defaultColor, Brush warningColor, Brush lockedColor, Brush textColor, bool showPrices)
		{
			return indicator.jtRangeMarker7(input, defaultColor, warningColor, lockedColor, textColor, showPrices);
		}
	}
}

#endregion
