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
	public class IchimokuCloud : Indicator
	{
		private Dictionary<string, DXMediaMap>	dxmBrushes;
		private int tmpMargin;

		protected override void OnStateChange()
		{
			
			if (State == State.SetDefaults)
			{
				Description									= @"IchimokuCloud Kinko Hyo - \Equilibrium Chart at a Glance\ - For a FULL description of this indicator and how to use it, please see:  http://www.forexabode.com/technical-analysis/ichimoku-cloud";
				Name										= "IchimokuCloud";
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
				IsSuspendedWhileInactive					= false;
				
				// Create default brushes
				dxmBrushes = new Dictionary<string, DXMediaMap>();

				foreach (string brushName in new string[] { "CloudAreaColorUp", "CloudAreaColorDown" })
					dxmBrushes.Add(brushName, new DXMediaMap());
				
				DisplayCloudOnly							= true;
				AdjustBarMargins							= false;
				
				PeriodFast									= 9;
				PeriodMedium								= 26;
				PeriodSlow									= 52;
								
				CloudColorOpacity							= 40;
				CloudAreaColorUp							= Brushes.Green;
				CloudAreaColorDown							= Brushes.Red;
				
				CloudDisplacement							= 26;
				
				AddPlot(Brushes.Red, 		"TenkanSen");
				AddPlot(Brushes.Purple, 	"KijunSen");
				AddPlot(Brushes.MediumBlue, "ChikouSpan");
				
				AddPlot(Brushes.Green, 		"SenkouSpanA");
				AddPlot(Brushes.Red, 		"SenkouSpanB");				
			}
			else if (State == State.DataLoaded)
			{				
				if (ChartControl != null)
				{
					// Adjust margins
					tmpMargin = ChartControl.Properties.BarMarginRight;
					if(AdjustBarMargins)
						ChartControl.Properties.BarMarginRight = ChartingExtensions.ConvertToHorizontalPixels(ChartControl.BarWidth * (CloudDisplacement+2) * 2 , ChartControl.PresentationSource);
				}
			}
			else if (State == State.Terminated)
			{
				//Reset Chart Margins
				if(ChartControl !=null && tmpMargin != 0)
					ChartControl.Properties.BarMarginRight = tmpMargin;
			}
		}

		protected override void OnBarUpdate()
		{
			if (DisplayCloudOnly)
			{
				PlotBrushes[0][0] = Brushes.Transparent;
				PlotBrushes[1][0] = Brushes.Transparent;
				PlotBrushes[2][0] = Brushes.Transparent;
				PlotBrushes[3][0] = Brushes.Transparent;
				PlotBrushes[4][0] = Brushes.Transparent;
			}

			if ((CurrentBar < PeriodFast) || (CurrentBar < PeriodMedium) || (CurrentBar < PeriodSlow)) return;

			double fastSum		= MAX(High,PeriodFast)[0] 	+ MIN(Low,PeriodFast)[0];
			double mediumSum	= MAX(High,PeriodMedium)[0] + MIN(Low,PeriodMedium)[0];

        	TenkanSen[0] 				= fastSum / 2.0;
        	KijunSen[0] 				= mediumSum / 2.0;
			ChikouSpan[PeriodMedium] 	= Close[0];

			SenkouSpanA[0] = ( fastSum + mediumSum ) / 4.0 ;
			SenkouSpanB[0] = ( MAX(High,PeriodSlow)[0] + MIN(Low,PeriodSlow)[0] ) / 2.0 ;			
		}
		
		public override void OnRenderTargetChanged()
		{
			// Dispose and recreate our DX Brushes
			try
			{
				foreach (KeyValuePair<string, DXMediaMap> item in dxmBrushes)
				{
					if (item.Value.DxBrush != null)
						item.Value.DxBrush.Dispose();

					if (RenderTarget != null)
						item.Value.DxBrush = item.Value.MediaBrush.ToDxBrush(RenderTarget);					
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
			base.OnRender(chartControl, chartScale);
			
			// Store previous AA mode
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode 	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode 							= SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			// Draw Region between SenkouSpanA and SenkouSpanB
			DrawRegionBetweenSeries(chartScale, SenkouSpanA, SenkouSpanB, "CloudAreaColorUp", "CloudAreaColorDown", CloudDisplacement);
			
			// Reset AA mode.
			RenderTarget.AntialiasMode = oldAntialiasMode;
		}		
		
		#region SharpDX Helper Classes & Methods
		private SharpDX.Vector2 FindIntersection(SharpDX.Vector2 p1, SharpDX.Vector2 p2, SharpDX.Vector2 p3, SharpDX.Vector2 p4)
		{
			SharpDX.Vector2 intersection = new SharpDX.Vector2();

			bool segments_intersect;
		    // Get the segments' parameters.
		    float dx12 = p2.X - p1.X;
		    float dy12 = p2.Y - p1.Y;
		    float dx34 = p4.X - p3.X;
		    float dy34 = p4.Y - p3.Y;

		    // Solve for t1 and t2
		    float denominator = (dy12 * dx34 - dx12 * dy34);

		    float t1 =
		        ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34)
		            / denominator;
		    if (float.IsInfinity(t1))
		        intersection = new SharpDX.Vector2(float.NaN, float.NaN);

		    // Find the point of intersection.
		    intersection = new SharpDX.Vector2(p1.X + dx12 * t1, p1.Y + dy12 * t1);
			return intersection;
		}
		
		private void SetOpacity(string brushName)
		{
			if (dxmBrushes[brushName].MediaBrush == null)
				return;

			if (dxmBrushes[brushName].MediaBrush.IsFrozen)
				dxmBrushes[brushName].MediaBrush = dxmBrushes[brushName].MediaBrush.Clone();

			dxmBrushes[brushName].MediaBrush.Opacity = CloudColorOpacity / 100.0;
			dxmBrushes[brushName].MediaBrush.Freeze();
		}
				
		private class DXMediaMap
		{
			public SharpDX.Direct2D1.Brush		DxBrush;
			public System.Windows.Media.Brush	MediaBrush;
		}
		

		private class SharpDXFigure
		{
			public SharpDX.Vector2[] Points;
			public string Color;
			
			public SharpDXFigure(SharpDX.Vector2[] points, string color)
			{
				Points = points;
				Color = color;
			}
		}
		
		private void DrawFigure(SharpDXFigure figure)
		{
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(figure.Points[0], new SharpDX.Direct2D1.FigureBegin());
			
			for (int i = 0; i < figure.Points.Length; i++)
				sink.AddLine(figure.Points[i]);
			
			sink.AddLine(figure.Points[0]);
				
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();
			
            RenderTarget.FillGeometry(geometry, dxmBrushes[figure.Color].DxBrush);
			geometry.Dispose();
			sink.Dispose();
		}
		
		private void DrawRegionBetweenSeries(ChartScale chartScale, Series<double> firstSeries, Series<double> secondSeries, string upperColor, string lowerColor, int displacement)
		{
			string BrushName = String.Empty;
			
			List<SharpDX.Vector2> 	SeriesAPoints 	= new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> 	SeriesBPoints 	= new List<SharpDX.Vector2>();
			List<SharpDX.Vector2> 	tmpPoints 		= new List<SharpDX.Vector2>();
			List<SharpDXFigure>		SharpDXFigures	= new List<SharpDXFigure>();
			
			// Convert SeriesA and SeriesB to points
			int start 	= ChartBars.FromIndex-displacement > 0 ? ChartBars.FromIndex-displacement : 0;
			int end 	= ChartBars.ToIndex;
			for (int barIndex = start; barIndex <= end; barIndex++)
		    {
				if(firstSeries.IsValidDataPointAt(barIndex))
				{
					SeriesAPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex+displacement), (float)chartScale.GetYByValue(firstSeries.GetValueAt(barIndex))));
					SeriesBPoints.Add(new SharpDX.Vector2((float)ChartControl.GetXByBarIndex(ChartBars, barIndex+displacement), (float)chartScale.GetYByValue(secondSeries.GetValueAt(barIndex))));
				}
		    }
			
			int lastCross = 0;
			bool isTouching = false;
			bool colorNeeded = true;
			
			for (int i = 0; i < SeriesAPoints.Count; i++)
			{	
				if (colorNeeded)
				{
					colorNeeded = false;
					
					// Set initial color or wait until we need to start a shape
					if (SeriesAPoints[i].Y < SeriesBPoints[i].Y)
						BrushName = upperColor;
					else if (SeriesAPoints[i].Y > SeriesBPoints[i].Y)
						BrushName = lowerColor;
					else
					{
						colorNeeded = true;
						lastCross = i;
					}
					
					if(!colorNeeded)
						tmpPoints.Add(SeriesAPoints[i]);
					
					continue;
				}	
				
				// Check if SeriesA and SeriesB meet or have crossed to loop back and close figure
				if(( SeriesAPoints[i].Y == SeriesBPoints[i].Y && isTouching == false)
					|| (SeriesAPoints[i].Y > SeriesBPoints[i].Y && SeriesAPoints[i-1].Y < SeriesBPoints[i-1].Y)
					|| (SeriesBPoints[i].Y > SeriesAPoints[i].Y && SeriesBPoints[i-1].Y < SeriesAPoints[i-1].Y))
				{					
					// reset isTouching
					isTouching = false;
					
					// Set the endpoint
					SharpDX.Vector2 endpoint = (SeriesAPoints[i].Y != SeriesBPoints[i].Y) ? FindIntersection(SeriesAPoints[i-1], SeriesAPoints[i], SeriesBPoints[i-1], SeriesBPoints[i]) : SeriesAPoints[i];
					tmpPoints.Add(endpoint);
					
					// Loop back and add SeriesBPoints
					for(int j = i-1; j >= lastCross; j--)
						tmpPoints.Add(SeriesBPoints[j]);
					
					// Create figure
					SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(),  (SeriesAPoints[i-1].Y < SeriesBPoints[i-1].Y) ? upperColor : lowerColor);
					SharpDXFigures.Add(figure);
					
					// Clear Points
					tmpPoints.Clear();
					
					// Start new figure if we crossed, otherwise we will wait until we need a new figure
					if (SeriesAPoints[i].Y != SeriesBPoints[i].Y)
					{
						tmpPoints.Add(SeriesBPoints[i]);
						tmpPoints.Add(endpoint);
						tmpPoints.Add(SeriesAPoints[i]);
					}
					else
						isTouching = true;
					
					// Set last cross
					lastCross = i;					
				}
				
				// Check if we are at the end of our rendering pass to loop back to loop back and close figure
				else if (i == SeriesAPoints.Count -1)
				{					
					tmpPoints.Add(SeriesAPoints[i]);
					
					// Loop back and add SeriesBPoints
					for(int j = i; j >= lastCross; j--)
						tmpPoints.Add(SeriesBPoints[j]);
					
					// Create figure
					SharpDXFigure figure = new SharpDXFigure(tmpPoints.ToArray(), (SeriesAPoints[i].Y < SeriesBPoints[i].Y) ? upperColor : lowerColor);
					SharpDXFigures.Add(figure);
					
					// Clear Points
					tmpPoints.Clear();
					break;
				}
				
				// Figure does not need to be closed. Add more points or open a new figure if we were touching
				else if (SeriesAPoints[i].Y != SeriesBPoints[i].Y)
				{
					if (isTouching == true)
					{
						tmpPoints.Add(SeriesAPoints[i-1]);
						lastCross = i-1;
					}
					
					tmpPoints.Add(SeriesAPoints[i]);
					
					isTouching = false;
				}			
			}
			
			// Draw figures
			foreach(SharpDXFigure figure in SharpDXFigures)
				DrawFigure(figure);
		}
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Display Cloud Only", Description="If set TRUE, displays only the Senkou Span lines (i.e. the cloud). If set FALSE, displays the Tenkan Sen conversion line, the Kijun Sen base line, and the Chikou Span (lagging span) line in addition to the cloud (i.e. the Senkou Span A and B lines).", Order=1, GroupName="Parameters")]
		public bool DisplayCloudOnly
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Adjust Chart Margins", Description="If set TRUE, chart margins will be forced to include the Ichimokou cloud. Overrides user defined margin", Order=2, GroupName="Parameters")]
		public bool AdjustBarMargins
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodFast", Description="Tenkan Sen (conversion line) period.  The default value 9 represents 1.5 Japanese working weeks [at 6 working days per week]. Often set to 7 in countries with 5 day work weeks.", Order=3, GroupName="Parameters")]
		public int PeriodFast
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodMedium", Description="Kijun Sen (baseline) period.  The default value 26 represents one Japanese working month [at 6 working days per week]. Often set to 22 in countries with 5 day work weeks.", Order=4, GroupName="Parameters")]
		public int PeriodMedium
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PeriodSlow", Description="Chikou Span period. The default value 52 represents two Japanese working months [at 6 working days per week]. Often set to 44 in countries with 5 day work weeks.", Order=5, GroupName="Parameters")]
		public int PeriodSlow
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CloudColorOpacity", Description="Cloud color opacity. Value 0 indicates complete transparency. Value 100 indicates complete opaqueness.", Order=6, GroupName="Parameters")]
		public int CloudColorOpacity
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CloudAreaColorUp", Description="Cloud area color up; i.e. the color of the positive cloud.", Order=7, GroupName="Parameters")]
		public Brush CloudAreaColorUp
		{
			get { return dxmBrushes["CloudAreaColorUp"].MediaBrush; }
			set
			{
				dxmBrushes["CloudAreaColorUp"].MediaBrush = value;
				SetOpacity("CloudAreaColorUp");
			}
		}

		[Browsable(false)]
		public string CloudAreaColorUpSerializable
		{
			get { return Serialize.BrushToString(CloudAreaColorUp); }
			set { CloudAreaColorUp = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="CloudAreaColorDown", Description="Cloud area color down; i.e. the color of the negative cloud.", Order=7, GroupName="Parameters")]
		public Brush CloudAreaColorDown
		{
			get { return dxmBrushes["CloudAreaColorDown"].MediaBrush; }
			set
			{
				dxmBrushes["CloudAreaColorDown"].MediaBrush = value;
				SetOpacity("CloudAreaColorDown");
			}
		}

		[Browsable(false)]
		public string CloudAreaColorDownSerializable
		{
			get { return Serialize.BrushToString(CloudAreaColorDown); }
			set { CloudAreaColorDown = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(0, int.MaxValue)]
		[Display(Name="Cloud Displacement", Description="Ichimoku Cloud Displacement", Order=8, GroupName="Parameters")]
		public int CloudDisplacement
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TenkanSen
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> KijunSen
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ChikouSpan
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
    	[XmlIgnore]
    	public Series<double> SenkouSpanA
    	{
        	get { return Values[3]; }
    	}

    	[Browsable(false)]
    	[XmlIgnore]
    	public Series<double> SenkouSpanB
    	{
        	get { return Values[4]; }
    	}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IchimokuCloud[] cacheIchimokuCloud;
		public IchimokuCloud IchimokuCloud(bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			return IchimokuCloud(Input, displayCloudOnly, adjustBarMargins, periodFast, periodMedium, periodSlow, cloudColorOpacity, cloudAreaColorUp, cloudAreaColorDown, cloudDisplacement);
		}

		public IchimokuCloud IchimokuCloud(ISeries<double> input, bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			if (cacheIchimokuCloud != null)
				for (int idx = 0; idx < cacheIchimokuCloud.Length; idx++)
					if (cacheIchimokuCloud[idx] != null && cacheIchimokuCloud[idx].DisplayCloudOnly == displayCloudOnly && cacheIchimokuCloud[idx].AdjustBarMargins == adjustBarMargins && cacheIchimokuCloud[idx].PeriodFast == periodFast && cacheIchimokuCloud[idx].PeriodMedium == periodMedium && cacheIchimokuCloud[idx].PeriodSlow == periodSlow && cacheIchimokuCloud[idx].CloudColorOpacity == cloudColorOpacity && cacheIchimokuCloud[idx].CloudAreaColorUp == cloudAreaColorUp && cacheIchimokuCloud[idx].CloudAreaColorDown == cloudAreaColorDown && cacheIchimokuCloud[idx].CloudDisplacement == cloudDisplacement && cacheIchimokuCloud[idx].EqualsInput(input))
						return cacheIchimokuCloud[idx];
			return CacheIndicator<IchimokuCloud>(new IchimokuCloud(){ DisplayCloudOnly = displayCloudOnly, AdjustBarMargins = adjustBarMargins, PeriodFast = periodFast, PeriodMedium = periodMedium, PeriodSlow = periodSlow, CloudColorOpacity = cloudColorOpacity, CloudAreaColorUp = cloudAreaColorUp, CloudAreaColorDown = cloudAreaColorDown, CloudDisplacement = cloudDisplacement }, input, ref cacheIchimokuCloud);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IchimokuCloud IchimokuCloud(bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			return indicator.IchimokuCloud(Input, displayCloudOnly, adjustBarMargins, periodFast, periodMedium, periodSlow, cloudColorOpacity, cloudAreaColorUp, cloudAreaColorDown, cloudDisplacement);
		}

		public Indicators.IchimokuCloud IchimokuCloud(ISeries<double> input , bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			return indicator.IchimokuCloud(input, displayCloudOnly, adjustBarMargins, periodFast, periodMedium, periodSlow, cloudColorOpacity, cloudAreaColorUp, cloudAreaColorDown, cloudDisplacement);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IchimokuCloud IchimokuCloud(bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			return indicator.IchimokuCloud(Input, displayCloudOnly, adjustBarMargins, periodFast, periodMedium, periodSlow, cloudColorOpacity, cloudAreaColorUp, cloudAreaColorDown, cloudDisplacement);
		}

		public Indicators.IchimokuCloud IchimokuCloud(ISeries<double> input , bool displayCloudOnly, bool adjustBarMargins, int periodFast, int periodMedium, int periodSlow, int cloudColorOpacity, Brush cloudAreaColorUp, Brush cloudAreaColorDown, int cloudDisplacement)
		{
			return indicator.IchimokuCloud(input, displayCloudOnly, adjustBarMargins, periodFast, periodMedium, periodSlow, cloudColorOpacity, cloudAreaColorUp, cloudAreaColorDown, cloudDisplacement);
		}
	}
}

#endregion
