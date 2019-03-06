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

using System.Collections.Generic;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class EdsLevel2 : Indicator
	{
		#region Variables
		private	List<DOMRow> askRows;
		private	List<DOMRow> bidRows;
		private List<DOMRow> oneDOMRow;
		private Dictionary<double, int> askRowsSorted;
		private Dictionary<double, int> bidRowsSorted;
		
		private SimpleFont textFont;
		private int x;
		private int y;
		private int totalY;
		private double maxMinusMin;
		private int barLength;
		private int barYOffset;
		private double barLengthScale = 1;
		private int barHeight = 1;
		private bool isCramped = false;
		
		private DateTime lastRefresh = DateTime.MinValue;
		private int refreshDelay = 300;

		private int totalAskVolume = 0;
		private int totalBidVolume = 0;
		private int DOMLargestVolume = 1;
		private int i = 0;
		
		private int rowVolume = 0;
		private int totalRowVolume = 0;
		private string rowVolumeString = "";
		private string totalRowVolumeString = "";
		
		#endregion
		
		#region SharpDX variables
		private Dictionary<string, DXMediaMap> dxmBrushes;
        private SharpDX.Direct2D1.RenderTarget myRenderTarget = null;
        private Brush HistogramAskBarBrush
        {
            get { return dxmBrushes["HistogramAskBarBrush"].MediaBrush; }
            set { UpdateBrush(value, "HistogramAskBarBrush", true); }
        }
		private Brush HistogramBidBarBrush
        {
            get { return dxmBrushes["HistogramBidBarBrush"].MediaBrush; }
            set { UpdateBrush(value, "HistogramBidBarBrush", true); }
        }
		private Brush TextBrush
        {
            get { return dxmBrushes["TextBrush"].MediaBrush; }
            set { UpdateBrush(value, "TextBrush", false); }
        }
		private Brush TextBrush2
        {
            get { return dxmBrushes["TextBrush2"].MediaBrush; }
            set { UpdateBrush(value, "TextBrush2", false); }
        }
		private Brush TransparentBrush
        {
            get { return dxmBrushes["TransparentBrush"].MediaBrush; }
            set { UpdateBrush(value, "TransparentBrush", false); }
        }
		#endregion
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "EdsLevel2";
				Calculate									= Calculate.OnBarClose;
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
				HistogramOpacity							= 60;
				HistogramAskBarColor						= Brushes.Plum;
				HistogramBidBarColor						= Brushes.LightGreen;
				TextColor									= Brushes.White;
				TextColor2									= Brushes.LightGray;
				
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "HistogramAskBarBrush", "HistogramBidBarBrush", "TextBrush", "TextBrush2", "TransparentBrush" } )
            	    dxmBrushes.Add(brushName, new DXMediaMap());
			}
			else if (State == State.DataLoaded)
			{
				askRows = new List<DOMRow>();
				bidRows = new List<DOMRow>();
				oneDOMRow = null;
				askRowsSorted = new Dictionary<double, int>();
				bidRowsSorted = new Dictionary<double, int>();
				
				textFont = new SimpleFont("GenericSansSerif", 9);
				
				HistogramAskBarBrush 	= HistogramAskBarColor;
				HistogramBidBarBrush 	= HistogramBidBarColor;
				TextBrush				= TextColor;
				TextBrush2				= TextColor2;
				TransparentBrush		= Brushes.Transparent;
			}
			else if (State == State.Terminated)
			{
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();
			}
		}
		
		#region Market Data Events
		protected override void OnBarUpdate()
		{
		}
		
		protected override void OnMarketDepth(MarketDepthEventArgs e)
		{
			// Checks to see if the Market Data is of the Ask type
			if (e.MarketDataType == MarketDataType.Ask)
				oneDOMRow = askRows;
			
			// Checks to see if the Market Data is of the Bid type
			else if (e.MarketDataType == MarketDataType.Bid)
				oneDOMRow = bidRows;
			
			if (oneDOMRow == null)
				return;

			// Checks to see if the action taken by the Ask data was an insertion into the ladder
			if (e.Operation == Operation.Add)
				oneDOMRow.Insert(e.Position, new DOMRow(e.Price, (int) e.Volume));
			
			/* Checks to see if the action taken by the Ask data was a removal of itself from the ladder
			Note: Due to the multi threaded architecture of the NT core, race conditions could occur
			-> check if e.Position is within valid range */
			else if (e.Operation == Operation.Remove && e.Position < oneDOMRow.Count)
				oneDOMRow.RemoveAt(e.Position);
			
			/* Checks to see if the action taken by the Ask data was to update a data already on the ladder
			Note: Due to the multi threaded architecture of the NT core, race conditions could occur
			-> check if e.Position is within valid range */
			else if (e.Operation == Operation.Update && e.Position < oneDOMRow.Count)
			{
				oneDOMRow[e.Position].Price = e.Price;
				oneDOMRow[e.Position].Volume = (int) e.Volume;
			}
			
			try	{
			if (DateTime.Now > lastRefresh.AddMilliseconds(refreshDelay))
			{
				ForceRefresh();
				lastRefresh = DateTime.Now;
			}
			} catch {}
		}
		#endregion
		
		#region Rendering
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
			// if right-most bar is not a real-time updating bar, then do not display level 2 histogram
			if (ChartControl.LastSlotPainted < Bars.Count - 1)
				return;
			
			totalAskVolume = 0;
			totalBidVolume = 0;
			
			x = ChartControl.CanvasRight; 
			totalY = ChartPanel.Y + ChartPanel.H;
			maxMinusMin = Math.Max(Math.Abs(ChartPanel.MaxValue - ChartPanel.MinValue), 1E-05);

			// find largest volume, copy arrays, and combine depths of same price
			DOMLargestVolume = 1;
			askRowsSorted.Clear();
			bidRowsSorted.Clear();
			for (i = 0; i < askRows.Count; i++)
			{
				if (askRowsSorted.ContainsKey(askRows[i].Price) == false)
					askRowsSorted.Add(askRows[i].Price, askRows[i].Volume);
				else
					askRowsSorted[askRows[i].Price] += askRows[i].Volume;
				if (askRowsSorted[askRows[i].Price] > DOMLargestVolume)
					DOMLargestVolume = askRowsSorted[askRows[i].Price];
			}
			for (i = 0; i < bidRows.Count; i++)
			{
				if (bidRowsSorted.ContainsKey(bidRows[i].Price) == false)
					bidRowsSorted.Add(bidRows[i].Price, bidRows[i].Volume);
				else
					bidRowsSorted[bidRows[i].Price] += bidRows[i].Volume;
				if (bidRowsSorted[bidRows[i].Price] > DOMLargestVolume)
					DOMLargestVolume = bidRowsSorted[bidRows[i].Price];
			}
			barLengthScale = 100.0 / DOMLargestVolume;
			
			// if cramped, draw one histogram bar per two depths
			isCramped = (((totalY - (int) (((ChartPanel.MinValue) / maxMinusMin) * ChartPanel.H)) -
						  (totalY - (int) (((ChartPanel.MinValue + TickSize) / maxMinusMin) * ChartPanel.H))) < 8);

			barHeight = isCramped ? 3 : 7;
			barYOffset = isCramped ? 5 : 3;
			
			foreach (KeyValuePair<double, int> key in askRowsSorted)
			{
				if (key.Value > 0)
				{
					y = totalY - ((int) ((( key.Key - ChartPanel.MinValue) / maxMinusMin) * ChartPanel.H)) - 7;
					totalAskVolume += key.Value;
					barLength = (int) (key.Value * barLengthScale);
					
					SharpDX.RectangleF rect1 = new SharpDX.RectangleF(x - barLength, y + barYOffset, barLength, barHeight);
					RenderTarget.FillRectangle(rect1, dxmBrushes["HistogramAskBarBrush"].DxBrush);
					
					if (isCramped == false)
					{
						DrawString(RowVolumeString(key.Value), textFont, "TextBrush", x, y);
						DrawString(RowVolumeString(totalAskVolume), textFont, "TextBrush2", x - 26, y);
					}
					else
					{
						if (askRows.Count > 0)
							if ((key.Key == askRows[0].Price) || (key.Key == askRows[askRows.Count - 1].Price))
								DrawString(RowVolumeString(totalAskVolume), textFont, "TextBrush", x - 26, y);
					}
				}
			}
			
			foreach (KeyValuePair<double, int> key in bidRowsSorted)
			{
				if (key.Value > 0)
				{
					y = totalY - ((int) ((( key.Key - ChartPanel.MinValue) / maxMinusMin) * ChartPanel.H)) - 7;
					totalBidVolume += key.Value;					
					barLength = (int) (key.Value * barLengthScale);
					
					SharpDX.RectangleF rect2 = new SharpDX.RectangleF(x - barLength, y + barYOffset, barLength, barHeight);
					RenderTarget.FillRectangle(rect2, dxmBrushes["HistogramBidBarBrush"].DxBrush);
					
					if (isCramped == false)
					{
						DrawString(RowVolumeString(key.Value), textFont, "TextBrush", x, y);
						DrawString(RowVolumeString(totalBidVolume), textFont, "TextBrush2", x - 26, y);
					}
					else
					{
						if (bidRows.Count > 0)
						{
							if (key.Key == bidRows[0].Price)
								DrawString(RowVolumeString(totalBidVolume), textFont, "TextBrush", x, y);							
							else if (key.Key == bidRows[bidRows.Count - 1].Price)
								DrawString(RowVolumeString(totalBidVolume), textFont, "TextBrush", x - 26, y);
						}
					}
				}
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
		#endregion
		
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

            dxmBrushes[brushName].MediaBrush.Opacity = HistogramOpacity / 100.0;
            dxmBrushes[brushName].MediaBrush.Freeze();
        }
		
		private void UpdateBrush(Brush mediaBrush, string brushName, bool useOpacity)
        {
            dxmBrushes[brushName].MediaBrush = mediaBrush;
			if(useOpacity)
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
			textFormat.TextAlignment = SharpDX.DirectWrite.TextAlignment.Trailing;
			
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, ChartPanel.X + ChartPanel.W,
				textFormat.FontSize);
			
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX - textLayout.Metrics.LayoutWidth, pointY).ToVector2();
			
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
		
		#region Support Methods
		private string RowVolumeString(int volume)
		{
			if (volume >= 1000000)
				return (volume / 1000000).ToString() + "M";
			else if (volume >= 100000)
				return (volume / 1000).ToString() + "K";
			else
				return volume.ToString();
		}
		
		private class DOMRow
		{
			public double Price;
			public int Volume;

			public DOMRow(double myPrice, int myVolume)
			{
				Price = myPrice;
				Volume = myVolume;
			}
		}
		#endregion

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HistogramOpacity", Description="Histogram opacity percentage.", Order=1, GroupName="Parameters")]
		public int HistogramOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="HistogramAskBarColor", Description="Histogram ask bar color.", Order=2, GroupName="Parameters")]
		public Brush HistogramAskBarColor
		{ get; set; }

		[Browsable(false)]
		public string HistogramAskBarColorSerializable
		{
			get { return Serialize.BrushToString(HistogramAskBarColor); }
			set { HistogramAskBarColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="HistogramBidBarColor", Description="Histogram bid bar color.", Order=3, GroupName="Parameters")]
		public Brush HistogramBidBarColor
		{ get; set; }

		[Browsable(false)]
		public string HistogramBidBarColorSerializable
		{
			get { return Serialize.BrushToString(HistogramBidBarColor); }
			set { HistogramBidBarColor = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TextColor", Description="Text color", Order=4, GroupName="Parameters")]
		public Brush TextColor
		{ get; set; }

		[Browsable(false)]
		public string TextColorSerializable
		{
			get { return Serialize.BrushToString(TextColor); }
			set { TextColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="TextColor2", Description="Text color 2", Order=5, GroupName="Parameters")]
		public Brush TextColor2
		{ get; set; }

		[Browsable(false)]
		public string TextColor2Serializable
		{
			get { return Serialize.BrushToString(TextColor2); }
			set { TextColor2 = Serialize.StringToBrush(value); }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EdsLevel2[] cacheEdsLevel2;
		public EdsLevel2 EdsLevel2(int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			return EdsLevel2(Input, histogramOpacity, histogramAskBarColor, histogramBidBarColor, textColor, textColor2);
		}

		public EdsLevel2 EdsLevel2(ISeries<double> input, int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			if (cacheEdsLevel2 != null)
				for (int idx = 0; idx < cacheEdsLevel2.Length; idx++)
					if (cacheEdsLevel2[idx] != null && cacheEdsLevel2[idx].HistogramOpacity == histogramOpacity && cacheEdsLevel2[idx].HistogramAskBarColor == histogramAskBarColor && cacheEdsLevel2[idx].HistogramBidBarColor == histogramBidBarColor && cacheEdsLevel2[idx].TextColor == textColor && cacheEdsLevel2[idx].TextColor2 == textColor2 && cacheEdsLevel2[idx].EqualsInput(input))
						return cacheEdsLevel2[idx];
			return CacheIndicator<EdsLevel2>(new EdsLevel2(){ HistogramOpacity = histogramOpacity, HistogramAskBarColor = histogramAskBarColor, HistogramBidBarColor = histogramBidBarColor, TextColor = textColor, TextColor2 = textColor2 }, input, ref cacheEdsLevel2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EdsLevel2 EdsLevel2(int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			return indicator.EdsLevel2(Input, histogramOpacity, histogramAskBarColor, histogramBidBarColor, textColor, textColor2);
		}

		public Indicators.EdsLevel2 EdsLevel2(ISeries<double> input , int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			return indicator.EdsLevel2(input, histogramOpacity, histogramAskBarColor, histogramBidBarColor, textColor, textColor2);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EdsLevel2 EdsLevel2(int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			return indicator.EdsLevel2(Input, histogramOpacity, histogramAskBarColor, histogramBidBarColor, textColor, textColor2);
		}

		public Indicators.EdsLevel2 EdsLevel2(ISeries<double> input , int histogramOpacity, Brush histogramAskBarColor, Brush histogramBidBarColor, Brush textColor, Brush textColor2)
		{
			return indicator.EdsLevel2(input, histogramOpacity, histogramAskBarColor, histogramBidBarColor, textColor, textColor2);
		}
	}
}

#endregion
