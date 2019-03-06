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
#endregion



//This namespace holds Drawing tools in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.DrawingTools
{
	/// <summary>
	/// Represents an interface that exposes information regarding an Arrow LabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ArrowLabeledLine : LabeledLines
	{
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				ArrowPathGeometry							= new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
				SharpDX.Direct2D1.GeometrySink geometrySink	= ArrowPathGeometry.Open();
				LineType									= ChartLineType.ArrowLine;
				Name										= "Labeled Arrow Line";

				// create our arrow directx geometry.
				// just make a static size we will scale when drawing
				// all relative to top of line
				// nudge up y slightly to cover up top of stroke (instead of using zero),
				// half the stroke will hide any overlap
				float arrowWidth							= 6f;
				SharpDX.Vector2 top = new SharpDX.Vector2(0, Stroke.Width * 0.5f);
				geometrySink.BeginFigure(top, SharpDX.Direct2D1.FigureBegin.Filled);
				geometrySink.AddLine(new SharpDX.Vector2(arrowWidth, -arrowWidth));
				geometrySink.AddLine(new SharpDX.Vector2(-arrowWidth, -arrowWidth));
				geometrySink.AddLine(top);// cap off figure
				geometrySink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
				geometrySink.Close();
			}
		}
	}

	/// <summary>
	/// Represents an interface that exposes information regarding an Extended LabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class ExtendedLabeledLine : LabeledLines
	{
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				LineType		= ChartLineType.ExtendedLine;
				Name			= "Labeled Extended Line";
				TextDisplayMode	= TextMode.PriceScale;
			}
		}
	}

	/// <summary>
	/// Represents an interface that exposes information regarding a Horizontal LabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class HorizontalLabeledLine : LabeledLines
	{
		// override this, we only need operations on a single anchor
		public override IEnumerable<ChartAnchor> Anchors { get { return new[] { StartAnchor }; } }

		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				EndAnchor.IsBrowsable				= false;
				LineType							= ChartLineType.HorizontalLine;
				Name								= "Labeled Horizontal Line";
				StartAnchor.DisplayName				= Custom.Resource.NinjaScriptDrawingToolAnchor;
				StartAnchor.IsXPropertiesVisible	= false;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a Vertical LabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class VerticalLabeledLine : LabeledLines
	{
		// override this, we only need operations on a single anchor
		public override IEnumerable<ChartAnchor> Anchors
		{
			get { return new[] { StartAnchor }; }
		}

		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				EndAnchor.IsBrowsable				= false;
				LineType							= ChartLineType.VerticalLine;
				Name								= "Labeled Vertical Line";
				StartAnchor.DisplayName				= Custom.Resource.NinjaScriptDrawingToolAnchor;
				StartAnchor.IsYPropertyVisible		= false;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a LabeledRay IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class LabeledRay : LabeledLines
	{
		protected override void OnStateChange()
		{
			base.OnStateChange();
			if (State == State.SetDefaults)
			{
				LineType		= ChartLineType.Ray;
				Name			= "Labeled Ray";
				TextDisplayMode	= TextMode.PriceScale;
			}
		}
	}
	
	/// <summary>
	/// Represents an interface that exposes information regarding a LabeledLine IDrawingTool.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Always)]
	public class LabeledLines : Line
	{
		private bool appendPriceTime;
		private bool needsLayoutUpdate;
		private bool offScreenDXBrushNeedsUpdate;
		private bool backgroundDXBrushNeedsUpdate;
		private string lastText;
		private string displayText;
		private Brush offScreenMediaBrush;
		private Brush backgroundMediaBrush;
		private SharpDX.Direct2D1.Brush offScreenDXBrush;
		private SharpDX.Direct2D1.Brush backgroundDXBrush;
		private SharpDX.DirectWrite.TextLayout cachedTextLayout;
		
		public enum TextMode
		{
			EndPointAtPriceScale,
			PriceScale,
			EndPoint
		}
		
		public enum RectSide
		{
			Top,
			Bottom,
			Left,
			Right,
			None
		}
		
		protected override void OnStateChange()
		{
			base.OnStateChange();
			
			if (State == State.SetDefaults)
			{
				Name						= "Labeled Line";
				OutlineStroke				= new Stroke(Brushes.Black, 2f);
				BackgroundBrush				= Brushes.Black;
				OffScreenBrush				= Brushes.Red;
				DisplayText 				= String.Empty;
				AppendPriceTime				= true;
				Font						= null;
				AreaOpacity 				= 75;
				TextDisplayMode				= TextMode.EndPointAtPriceScale;
				HorizontalOffset			= 0.5;
				VerticalOffset				= 3;
				offScreenDXBrushNeedsUpdate = true;
				backgroundDXBrushNeedsUpdate = true;
			}
			else if (State == State.Terminated)
			{
				if (cachedTextLayout != null)
					cachedTextLayout.Dispose();
				cachedTextLayout = null;
			}
		}
		
		public override void OnRenderTargetChanged()
        {
			base.OnRenderTargetChanged();
			
			if (RenderTarget == null)
				return;
			
			if (offScreenDXBrush != null)
				offScreenDXBrush.Dispose();
			offScreenDXBrush = offScreenMediaBrush.ToDxBrush(RenderTarget);
			
			if (backgroundDXBrush != null)
				backgroundDXBrush.Dispose();
			backgroundDXBrush = backgroundMediaBrush.ToDxBrush(RenderTarget);
			backgroundDXBrush.Opacity = (float)AreaOpacity / 100f;
		}
		
		/* Steps:
		*	1. Project start/end points for rays and extended lines
		*	2. Find collitions with ChartPanel for TextBox coordinates
		*	3. Determine price to be appended 
		*	4. Create message
		*	5. Draw TextBox
		*/

		public override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			
			Stroke.RenderTarget 		= RenderTarget;
			OutlineStroke.RenderTarget	= RenderTarget;
						
			bool snap					= true;
			bool startsOnScreen			= true;
			bool priceOffScreen			= false;
			double priceToUse			= 0;
			string pricetime			= String.Empty;
			string TextToDisplay		= DisplayText;
			MasterInstrument masterInst = GetAttachedToChartBars().Bars.Instrument.MasterInstrument;
			
			Point	startPoint			= StartAnchor.GetPoint(chartControl, ChartPanel, chartScale);
			Point	endPoint			= EndAnchor.GetPoint(chartControl, ChartPanel, chartScale);
			
			double 	strokePixAdj		= ((double)(Stroke.Width % 2)).ApproxCompare(0) == 0 ? 0.5d : 0d;
			Vector	pixelAdjustVec		= new Vector(strokePixAdj, strokePixAdj);
			
			Point 	startAdj			= (LineType == ChartLineType.HorizontalLine ? new Point(ChartPanel.X, startPoint.Y) : new Point(startPoint.X, ChartPanel.Y)) + pixelAdjustVec;
			Point 	endAdj				= (LineType == ChartLineType.HorizontalLine ? new Point(ChartPanel.X + ChartPanel.W, startPoint.Y) : new Point(startPoint.X, ChartPanel.Y + ChartPanel.H)) + pixelAdjustVec;
			
			Vector 	distVec 			= Vector.Divide(Point.Subtract(endPoint, startPoint), 100);
			Vector 	scalVec				= (LineType == ChartLineType.ExtendedLine || LineType == ChartLineType.Ray || LineType == ChartLineType.HorizontalLine) ? Vector.Multiply(distVec, 10000) : Vector.Multiply(distVec, 100);
			Point 	extPoint			= Vector.Add(scalVec, startPoint);		
				
			// Project extended line start point if it is off screen
			if (LineType == ChartLineType.ExtendedLine && TextDisplayMode != TextMode.EndPoint)
				startPoint 				= Point.Subtract(startPoint, scalVec);
			
			// Project TextBox coordinate for extended lines and rays to get ChartPanel bounds
			if (LineType == ChartLineType.ExtendedLine || LineType == ChartLineType.Ray)
				extPoint = Vector.Add(scalVec, extPoint);
			
			// Find collisions with ChartPanel bounds for PriceScale bound TextBox coordinates
			if (LineType == ChartLineType.HorizontalLine || LineType == ChartLineType.VerticalLine)
			{
				extPoint = endAdj;
				startPoint = startAdj;
			}
			else if (TextDisplayMode == TextMode.EndPoint)
			{
				extPoint = endPoint;
				snap 	 = false;
			}
			else
			{
				if (extPoint.X <= ChartPanel.X || extPoint.Y < ChartPanel.Y || extPoint.X > ChartPanel.W || extPoint.Y > ChartPanel.H)
				{
					switch (LineIntersectsRect(startPoint, extPoint, new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H)))
					{
						case RectSide.Top:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.Y));
							break;
						case RectSide.Bottom:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.W, ChartPanel.H), new Point(ChartPanel.X, ChartPanel.H));
							break;
						case RectSide.Left:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.X, ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y));
							break;
						case RectSide.Right:
							extPoint = FindIntersection(startPoint, extPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.H));
							break;
						default:
							return;
					}
				}
				
				if (startPoint.X <= ChartPanel.X || startPoint.Y < ChartPanel.Y || startPoint.X > ChartPanel.W || startPoint.Y > ChartPanel.H)
				{
					switch (LineIntersectsRect(extPoint, startPoint, new SharpDX.RectangleF(ChartPanel.X, ChartPanel.Y, ChartPanel.W, ChartPanel.H)))
					{
						case RectSide.Top:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.Y));
							break;
						case RectSide.Bottom:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.W, ChartPanel.H), new Point(ChartPanel.X, ChartPanel.H));
							break;
						case RectSide.Left:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.X, ChartPanel.H), new Point(ChartPanel.X, ChartPanel.Y));
							break;
						case RectSide.Right:
							startPoint = FindIntersection(extPoint, startPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.H));
							break;
						default:
							return;
					}
				}
				
				if (endPoint.X <= ChartPanel.X || endPoint.Y < ChartPanel.Y || endPoint.X > ChartPanel.W || endPoint.Y > ChartPanel.H)
					priceOffScreen = true;
			}
			
			// Scale coordinates by HorizontalOffset/VerticalOffset
			distVec 	= Point.Subtract(extPoint, startPoint);
			scalVec 	= Vector.Multiply(Vector.Divide(distVec, 100), HorizontalOffset);
			extPoint	= Point.Subtract(extPoint, scalVec);
			extPoint.Y 	-= VerticalOffset;

			// Get a Price or a Timestamp to append to the label
			switch (LineType)
			{
				case ChartLineType.VerticalLine:
					pricetime = StartAnchor.Time.ToString();
					break;
				case ChartLineType.HorizontalLine:
					priceToUse = StartAnchor.Price;
					break;
				case ChartLineType.ExtendedLine:
				case ChartLineType.Ray:
					priceToUse = TextDisplayMode == TextMode.PriceScale
							   ? chartScale.GetValueByY(endPoint.X >= startPoint.X
														? (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.H)).Y
						 								: (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X, ChartPanel.H)).Y)
							   : EndAnchor.Price;
					break;
				default:
					priceToUse = priceOffScreen && TextDisplayMode == TextMode.PriceScale
							   ? chartScale.GetValueByY(endPoint.X >= startPoint.X
														? (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.W, ChartPanel.Y), new Point(ChartPanel.W, ChartPanel.H)).Y
						 								: (float)FindIntersection(startPoint, endPoint, new Point(ChartPanel.X, ChartPanel.Y), new Point(ChartPanel.X, ChartPanel.H)).Y)
							   : EndAnchor.Price;
					break;
			}		
			
			// Round the price
			if (LineType != ChartLineType.VerticalLine)
				pricetime = priceToUse <= masterInst.RoundDownToTickSize(priceToUse) + masterInst.TickSize * 0.5
							? pricetime = masterInst.RoundDownToTickSize(priceToUse).ToString("0.00")		  
							: pricetime = masterInst.RoundToTickSize(priceToUse).ToString("0.00");		  
			
			// Check if we need to append price or time
			if (AppendPriceTime && DisplayText.Length > 0)
				TextToDisplay = String.Format("{0} {1}", DisplayText, pricetime);
			else if (AppendPriceTime)
				TextToDisplay = pricetime;
			
			// Use Label Font if one is not specified by template
			if(Font == null)
				Font = new NinjaTrader.Gui.Tools.SimpleFont(chartControl.Properties.LabelFont.Family.ToString(), 16);
			
			// Update DX Brushes
			if (offScreenDXBrushNeedsUpdate)
			{
				offScreenDXBrush.Dispose();
				offScreenDXBrush = offScreenMediaBrush.ToDxBrush(RenderTarget);
				offScreenDXBrushNeedsUpdate = false;
			}
			
			if (backgroundDXBrushNeedsUpdate)
			{
				backgroundDXBrush.Dispose();
				backgroundDXBrush = backgroundMediaBrush.ToDxBrush(RenderTarget);
				backgroundDXBrush.Opacity = (float)AreaOpacity / 100f;
				backgroundDXBrushNeedsUpdate = false;
			}
			
			// Draw TextBoxes
			switch (LineType)
			{
				case ChartLineType.VerticalLine:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, Stroke.BrushDX, backgroundDXBrush, OutlineStroke, 1.5708f);
					break;
				case ChartLineType.HorizontalLine:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, Stroke.BrushDX, backgroundDXBrush, OutlineStroke, 0);
					break;
				default:
					DrawTextBox(snap, TextToDisplay, extPoint.X, extPoint.Y, priceOffScreen && TextDisplayMode == TextMode.EndPointAtPriceScale ? offScreenDXBrush : Stroke.BrushDX, backgroundDXBrush, OutlineStroke, 0);
					break;					
			}
		}
		
		private void DrawTextBox(bool Snap, string displayText, double x, double y, SharpDX.Direct2D1.Brush brush, SharpDX.Direct2D1.Brush bgBrush, Stroke stroke, float rotate)
		{
			const int padding = 4;
			
			// Text has changed, need to update cached TextLayout
			if (displayText != lastText)
				needsLayoutUpdate = true;
			lastText = displayText;
			
			// Update cachedTextLayout
			if (needsLayoutUpdate || cachedTextLayout == null)
			{
				SharpDX.DirectWrite.TextFormat textFormat = Font.ToDirectWriteTextFormat();
				cachedTextLayout = 	new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
									displayText, textFormat, ChartPanel.X + ChartPanel.W,
									textFormat.FontSize);
				textFormat.Dispose();
				needsLayoutUpdate = false;
			}
			
			// Snap TextBox coordinates to ChartPanel when out of bounds
			if (Snap)
			{
				if (rotate == 1.5708f)
					y = Math.Max(ChartPanel.Y + cachedTextLayout.Metrics.Width + 2 * padding, y);
				else
				{
					y = Math.Min(ChartPanel.H - padding, y);
					y = Math.Max(ChartPanel.Y + cachedTextLayout.Metrics.Height + padding, y);
					x = Math.Max(ChartPanel.X + cachedTextLayout.Metrics.Width + 2 * padding, x);
				}
			}
			
			// Apply rotation
			RenderTarget.Transform = SharpDX.Matrix3x2.Rotation(rotate, new SharpDX.Vector2((float)x, (float)y));
			
			// Add padding to TextPlotPoint
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(x - cachedTextLayout.Metrics.Width - padding / 2, y - cachedTextLayout.Metrics.Height).ToVector2();
			
			// Draw the TextBox
			if (displayText.Length > 0)
			{
	            SharpDX.RectangleF 					PLBoundRect		= new SharpDX.RectangleF((float)x - cachedTextLayout.Metrics.Width - padding, (float)y - cachedTextLayout.Metrics.Height - padding / 2, cachedTextLayout.Metrics.Width + padding, cachedTextLayout.Metrics.Height + padding);
				SharpDX.Direct2D1.RoundedRectangle 	PLRoundedRect 	= new SharpDX.Direct2D1.RoundedRectangle() { Rect = PLBoundRect, RadiusX = cachedTextLayout.FontSize/4, RadiusY = cachedTextLayout.FontSize/4 };
				RenderTarget.FillRoundedRectangle(PLRoundedRect, bgBrush);
				RenderTarget.DrawRoundedRectangle(PLRoundedRect, stroke.BrushDX, stroke.Width, stroke.StrokeStyle);
				
				// Draw the TextLayout
				RenderTarget.DrawTextLayout(TextPlotPoint, cachedTextLayout, brush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			}
			
			// Restore rotation
			RenderTarget.Transform = SharpDX.Matrix3x2.Identity;
		}
		
		private Point FindIntersection(Point p1, Point p2, Point p3, Point p4)
		{
			Point intersection = new Point();
			
		    // Get the segments' parameters.
		    double dx12 = p2.X - p1.X;
		    double dy12 = p2.Y - p1.Y;
		    double dx34 = p4.X - p3.X;
		    double dy34 = p4.Y - p3.Y;

		    // Solve for t1 and t2
		    double denominator = (dy12 * dx34 - dx12 * dy34);

		    double t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) 
						/ denominator;
		    
			if (double.IsInfinity(t1))
		        intersection = new Point(double.NaN, double.NaN);

		    // Find the point of intersection.
		    intersection = new Point(Math.Max(p1.X + dx12 * t1, 0), p1.Y + dy12 * t1);
			return intersection;
		}
		
		private RectSide LineIntersectsRect(Point p1, Point p2, SharpDX.RectangleF r)
	    {

	        if (LineIntersectsLine(p1, p2, new Point(r.X, r.Y), new Point(r.X + r.Width, r.Y)) && p1.Y > r.Y)
				return RectSide.Top;
			if (LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y), new Point(r.X + r.Width, r.Y + r.Height)) && p1.X < r.X + r.Width)
				return RectSide.Right;
			if (LineIntersectsLine(p1, p2, new Point(r.X + r.Width, r.Y + r.Height), new Point(r.X, r.Y + r.Height)) && p1.Y < r.Y + r.Height)
				return RectSide.Bottom;
			if (LineIntersectsLine(p1, p2, new Point(r.X, r.Y + r.Height), new Point(r.X, r.Y)))
				return RectSide.Left;

			return RectSide.None;
		}

	    private bool LineIntersectsLine(Point l1p1, Point l1p2, Point l2p1, Point l2p2)
	    {
	        double q = (l1p1.Y - l2p1.Y) * (l2p2.X - l2p1.X) - (l1p1.X - l2p1.X) * (l2p2.Y - l2p1.Y);
	        double d = (l1p2.X - l1p1.X) * (l2p2.Y - l2p1.Y) - (l1p2.Y - l1p1.Y) * (l2p2.X - l2p1.X);

	        if( d == 0 )
	            return false;

	        double r = q / d;

	        q = (l1p1.Y - l2p1.Y) * (l1p2.X - l1p1.X) - (l1p1.X - l2p1.X) * (l1p2.Y - l1p1.Y);
	        double s = q / d;

	        if( r < 0 || r > 1 || s < 0 || s > 1 )
	            return false;

	        return true;
	    }
		
		[Display(Name = "Text Horizontal Offset", Description = "Distance to offset from End Point", GroupName = "General", Order = 5)]
		[Range(0, 100)]
		public double HorizontalOffset
		{ get; set; }
		
		[Display(Name = "Text Vertical Offset", Description = "Distance from line", GroupName = "General", Order = 6)]
		[Range(-100, 100)]
		public double VerticalOffset
		{ get; set; }
		
		[ExcludeFromTemplate]
		[Display(Name = "Text", GroupName = "General", Order = 7)]
		[PropertyEditor("NinjaTrader.Gui.Tools.MultilineEditor")]
		public string DisplayText
		{
			get { return displayText; }
			set
			{
				if (displayText == value)
					return;
				displayText			= value;
				needsLayoutUpdate 	= true;
			}
		}
		
		[Display(Name = "Append Price/Time", GroupName = "General", Order = 8)]
		public bool AppendPriceTime
		{
			get { return appendPriceTime; }
			set
			{
				if (appendPriceTime == value)
					return;
				appendPriceTime			= value;
				needsLayoutUpdate		= true;
			}
		}
		
		[Display(Name = "Text Display Mode", GroupName = "General", Order = 9)]
		public TextMode TextDisplayMode
		{ get; set; }
		
		[Display(Name = "Font", GroupName = "General", Order = 10)]
		public Gui.Tools.SimpleFont Font
		{ get; set; }
		
		[XmlIgnore]
		[Display(GroupName = "General", Name = "Price Offscreen Text Color", Order = 11)]
		public Brush OffScreenBrush 
		{ 
			get { return offScreenMediaBrush; } 
			set
			{
				offScreenMediaBrush = value;
				offScreenDXBrushNeedsUpdate = true;
			}
		}
		
		[Browsable(false)]
		public string OffScreenBrushSerializable
		{
			get { return Serialize.BrushToString(OffScreenBrush); }
			set { OffScreenBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(GroupName = "General", Name = "Text Box Outline", Order = 100)]
		public Stroke OutlineStroke { get; set; }
		
		[XmlIgnore]
		[Display(GroupName = "General", Name = "Text Box Background Color", Order = 101)]
		public Brush BackgroundBrush 
		{ 
			get { return backgroundMediaBrush; } 
			set
			{
				backgroundMediaBrush = value;
				backgroundDXBrushNeedsUpdate = true;
			}
		}
		
		[Browsable(false)]
		public string BackgroundBrushSerializable
		{
			get { return Serialize.BrushToString(BackgroundBrush); }
			set { BackgroundBrush = Serialize.StringToBrush(value); }
		}
		
		[Display(GroupName = "General", Name = "Text Box Background Opacity", Order = 102)]
		public int AreaOpacity { get; set; }

	}
}
