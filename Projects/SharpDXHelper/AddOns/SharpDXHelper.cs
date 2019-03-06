#region Using declarations
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Windows.Media;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
#endregion

//This namespace holds Add ons in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.AddOns
{
	public class DXHelper
	{
		// Dictionary for keeping track of DX brushes made from user defined brushes.
		public Dictionary<string, DXMediaBrush> DXMBrushes;
		
		// Dictionaries for tracking Helper Managed Brushes and usage
		private Dictionary<string, DXMediaBrush> 	HelperManagedBrushes;
		private Dictionary<string, bool> 			HelperManagedBrushesUsed;
		
		// Timer to check how often our Helper Managed Brushes are used.
		private Timer 	timerToCheck;
		
		private Dictionary<string, SharpDX.Direct2D1.Bitmap> 		bitmapDictionary;
		private Dictionary<string, SharpDX.IO.NativeFileStream> 	fileStreamDictionary;
		private Dictionary<string, SharpDX.WIC.BitmapDecoder> 		bitmapDecoderDictionary;
		private Dictionary<string, SharpDX.WIC.FormatConverter> 	converterDictionary;
		private Dictionary<string, SharpDX.WIC.BitmapFrameDecode> 	frameDictionary;
		
		private List<string> bitmapList;
		
		// Constructor
		public DXHelper()
		{
			DXMBrushes = new Dictionary<string, DXMediaBrush>();
		}
		
		// Constructor for Helper Managed Brushes
		public DXHelper(bool allowHelperManagedResources, ChartControl chartControl, int minutesToRefresh)
		{
			DXMBrushes = new Dictionary<string, DXMediaBrush>();
			
			if (allowHelperManagedResources)
			{
				HelperManagedBrushes 		= new Dictionary<string, DXMediaBrush>();
				HelperManagedBrushesUsed 	= new Dictionary<string, bool>();
				
				if (minutesToRefresh > 0)
				{
					chartControl.Dispatcher.InvokeAsync(new Action(() => 
					{
						timerToCheck 			= new Timer();
						
						timerToCheck.Tick 		+= new EventHandler(TimerToCheckEventProcessor);
						timerToCheck.Interval 	= 1000 * 60 * minutesToRefresh;
						timerToCheck.Start();
					}));
				}
				
				bitmapDictionary	 	= new Dictionary<string, SharpDX.Direct2D1.Bitmap>();
				fileStreamDictionary 	= new Dictionary<string, SharpDX.IO.NativeFileStream>();
				bitmapDecoderDictionary = new Dictionary<string, SharpDX.WIC.BitmapDecoder>();
				converterDictionary 	= new Dictionary<string, SharpDX.WIC.FormatConverter>();
				frameDictionary 		= new Dictionary<string, SharpDX.WIC.BitmapFrameDecode>();
				bitmapList 				= new List<string>();
			}
		}
		
		// Destructor
		public void Dispose()
		{
			if (timerToCheck != null)
			{
				timerToCheck.Stop();
  				timerToCheck.Tick -= new EventHandler(TimerToCheckEventProcessor);
				timerToCheck.Dispose();
			}
			
			DisposeBitmapResources();
			bitmapList.Clear();
		}
		
		#region User created brushes
		public void AddBrushes(string brushName)
		{
			DXMBrushes.Add(brushName, new DXMediaBrush());
		}
		public void AddBrushes(string[] brushNames)
		{
			foreach (string brushName in brushNames)
        		DXMBrushes.Add(brushName, new DXMediaBrush());
		}
		
		public void UpdateBrush(SharpDX.Direct2D1.RenderTarget renderTarget, string brushName, Brush brush)
		{
			DXMBrushes[brushName].UpdateBrush(renderTarget, brush);
		}
		
		public void UpdateBrush(SharpDX.Direct2D1.RenderTarget renderTarget, string brushName, Brush brush, double opacity)
		{
			DXMBrushes[brushName].UpdateBrush(renderTarget, brush, opacity);
		}
        #endregion

        #region Helper created brushes

        private string GetBrushString(Brush brush)
        {
            string a = ((Color)brush.GetValue(SolidColorBrush.ColorProperty)).A.ToString();
            string r = ((Color)brush.GetValue(SolidColorBrush.ColorProperty)).R.ToString();
            string g = ((Color)brush.GetValue(SolidColorBrush.ColorProperty)).G.ToString();
            string b = ((Color)brush.GetValue(SolidColorBrush.ColorProperty)).B.ToString();
           
            return a + r + g + b;
        }

        private void HelperCheckAddBrush(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush)
		{
			bool foundBrush = false;

            string brushString = GetBrushString(brush);
			
			// Check if brush already exists
			foreach (string brushToFind in HelperManagedBrushes.Keys)
				if (brushToFind == brushString)
					foundBrush = true;

            if (!foundBrush)
			{
				// Brush was not found, add new brush
        		HelperManagedBrushes.Add(brushString, new DXMediaBrush());
				HelperManagedBrushes[brushString].UpdateBrush(renderTarget, brush, brush.Opacity * 100.0);
				
				// Add brush to activity dictionary and mark as used
				HelperManagedBrushesUsed.Add(brushString, true);
			}
			else
				HelperManagedBrushesUsed[brushString] = true;
		}
		
		private void HelperCheckUnusedBrushes()
		{
			if(HelperManagedBrushes == null || HelperManagedBrushesUsed == null)
				return;
			
			bool brushFound = false;
			bool brushUsed = false;
			string brushString = null;
			foreach (KeyValuePair<string, bool> item in HelperManagedBrushesUsed)
			{
    	    	if (item.Value == true)
				{
					brushFound = true;
					brushUsed = true;
					brushString = item.Key;	
				}
				else 
				{
					brushFound = true;
					brushUsed = false;
					brushString = item.Key;	
				}
			}
			
			if (brushFound && !brushUsed)
			{
				// Dispose and remove unused brushes
				HelperManagedBrushes[brushString].Dispose();
				HelperManagedBrushes.Remove(brushString);
				
				// Reomve unused brush from activity dictionary
				HelperManagedBrushesUsed.Remove(brushString);
			}
			else if (brushFound && brushUsed)
			{
				// Mark Brush as unused for next check
				HelperManagedBrushesUsed[brushString] = false;
			}
		}
		
		private void TimerToCheckEventProcessor(Object myObject, EventArgs myEventArgs)
		{
			HelperCheckUnusedBrushes();
		}
		#endregion
		
		public void RenderTargetChange(SharpDX.Direct2D1.RenderTarget renderTarget)
		{
			if (renderTarget == null || renderTarget.IsDisposed)
				return;
			
			if (DXMBrushes != null)
				foreach (KeyValuePair<string, DXMediaBrush> item in DXMBrushes)
        	    	item.Value.RenderTargetChange(renderTarget);
			
			if (HelperManagedBrushes != null)
				foreach (KeyValuePair<string, DXMediaBrush> item in HelperManagedBrushes)
        	    	item.Value.RenderTargetChange(renderTarget);
				
			DisposeBitmapResources();
			CreateBitmapResources(renderTarget);
		}
		
		#region DrawLine
		#region SharpDX Brushes
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate Point</param>
		/// <param name="endPoint">SharpDX End Coordinate Point</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Draw the line
			renderTarget.DrawLine(startPoint, endPoint, brush, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, double x1, double y1, double x2, double y2, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, strokeStyle);
		}		
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate pair</param>
		/// <param name="endPoint">SharpDX End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, dashStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A SharpDX Brush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Brush brush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, dashStyle);
		}
		#endregion
		
		#region DXMediaBrushes
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate Point</param>
		/// <param name="endPoint">SharpDX End Coordinate Point</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Draw the line
			renderTarget.DrawLine(startPoint, endPoint, brush.DxBrush, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, double x1, double y1, double x2, double y2, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, strokeStyle);
		}		
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate pair</param>
		/// <param name="endPoint">SharpDX End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, dashStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A DXMediaBrush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, DXMediaBrush brush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, dashStyle);
		}
		#endregion
		
		#region Dictionary Brushes
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate Point</param>
		/// <param name="endPoint">SharpDX End Coordinate Point</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Draw the line
			renderTarget.DrawLine(startPoint, endPoint, DXMBrushes[brush].DxBrush, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, double x1, double y1, double x2, double y2, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, strokeStyle);
		}		
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate pair</param>
		/// <param name="endPoint">SharpDX End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, dashStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A string key for a DXMediaBrush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, string brush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, dashStyle);
		}
		#endregion
		
		#region Media Brushes
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate Point</param>
		/// <param name="endPoint">SharpDX End Coordinate Point</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle to use for line</param>
		/// <returns></returns>
		// Condense Line drawing code
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			// Draw the line
			renderTarget.DrawLine(startPoint, endPoint, HelperManagedBrushes[brushString].DxBrush, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, double x1, double y1, double x2, double y2, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="strokeStyle">StrokeStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, strokeStyle);
		}		
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="startPoint">SharpDX Start Coordinate pair</param>
		/// <param name="endPoint">SharpDX End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, SharpDX.Vector2 startPoint, SharpDX.Vector2 endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="startPoint">Windows Point Start Coordinate pair</param>
		/// <param name="endPoint">Windows Point End Coordinate pair</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, System.Windows.Point startPoint, System.Windows.Point endPoint, float width, DashStyleHelper dashStyle)
		{
			// Create Vector2 coordinates
			SharpDX.Vector2 startPointDX 	= startPoint.ToVector2();
			SharpDX.Vector2 endPointDX 		= endPoint.ToVector2();
			
			DrawLine(renderTarget, brush, startPointDX, endPointDX, width, dashStyle);
		}
		
		/// <summary>
		/// Draws a line between two points.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="brush">A Windows Media Brush used for the line</param>
		/// <param name="x1">X Start Coordinate</param>
		/// <param name="y1">Y Start Coordinate</param>
		/// <param name="x2">X End Coordinate</param>
		/// <param name="y2">Y End Coordinate</param>
		/// <param name="width">Line Width</param>
		/// <param name="dashStyle">DashStyle to use for line</param>
		/// <returns></returns>
		public void DrawLine(SharpDX.Direct2D1.RenderTarget renderTarget, Brush brush, double x1, double y1, double x2, double y2, float width, DashStyleHelper dashStyle)
		{			
			// Create Vector2 coordinates
			SharpDX.Vector2 startPoint 	= new System.Windows.Point(x1, y1).ToVector2();
			SharpDX.Vector2 endPoint 	= new System.Windows.Point(x2, y2).ToVector2();
			
			DrawLine(renderTarget, brush, startPoint, endPoint, width, dashStyle);
		}
		#endregion
		#endregion
		
		#region DrawString
		#region SharpDX Brushes
		/// <summary>
		/// Draws text with a background using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param> 
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="brush">SharpDX brush used for the text color</param>
		/// <param name="pointX">X coordinate</param>
		/// <param name="pointY">Y coordinate</param>
		/// <param name="areaBrush">SharpDX brush used for the text background color</param>
		/// <returns></returns>
		public void DrawString(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, string text, SimpleFont font, SharpDX.Direct2D1.Brush brush, double pointX, double pointY, SharpDX.Direct2D1.Brush areaBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout =
			new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
				text, textFormat, chartPanel.X + chartPanel.W,
				textFormat.FontSize);
			SharpDX.Vector2 TextPlotPoint = new System.Windows.Point(pointX, pointY-textLayout.Metrics.Height/2).ToVector2();
			
			float newW = textLayout.Metrics.Width; 
            float newH = textLayout.Metrics.Height;
            SharpDX.RectangleF PLBoundRect = new SharpDX.RectangleF((float)pointX+2, (float)pointY-textLayout.Metrics.Height/2, newW+5, newH+2);
            renderTarget.FillRectangle(PLBoundRect, areaBrush);
			
			renderTarget.DrawTextLayout(TextPlotPoint, textLayout, brush, SharpDX.Direct2D1.DrawTextOptions.NoSnap);
			textLayout.Dispose();
			textFormat.Dispose();
		}
		#endregion
		
		#region DXMediaBrushes
		/// <summary>
		/// Draws text with a background using DXMediaBrushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param> 
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="brush">DXMediaBrush brush used for the text color</param>
		/// <param name="pointX">X coordinate</param>
		/// <param name="pointY">Y coordinate</param>
		/// <param name="areaBrush">DXMediaBrush brush used for the text background color</param>
		/// <returns></returns>
		public void DrawString(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, string text, SimpleFont font, DXMediaBrush brush, double pointX, double pointY, DXMediaBrush areaBrush)
		{
			DrawString(renderTarget, chartPanel, text, font, brush.DxBrush, pointX, pointY, areaBrush.DxBrush);
		}
		#endregion
		
		#region Dictionary Brushes
		/// <summary>
		/// Draws text with a background using Dictionary DXMediaBrushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param> 
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="brushName">Dictionary brush name used for the text color</param>
		/// <param name="pointX">X coordinate</param>
		/// <param name="pointY">Y coordinate</param>
		/// <param name="areaBrush">Dictionary brush name used for the text background color</param>
		/// <returns></returns>
		public void DrawString(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, string text, SimpleFont font, string brushName, double pointX, double pointY, string areaBrushName)
		{
			DrawString(renderTarget, chartPanel, text, font, DXMBrushes[brushName].DxBrush, pointX, pointY, DXMBrushes[areaBrushName].DxBrush);
		}
		#endregion
		
		#region Media Brushes
		/// <summary>
		/// Draws text with a background using Windows Media Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param> 
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="brush">Windows Media Brush used for the text color</param>
		/// <param name="pointX">X coordinate</param>
		/// <param name="pointY">Y coordinate</param>
		/// <param name="areaBrush">Windows Media Brush used for the text background color</param>
		/// <returns></returns>
		public void DrawString(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, string text, SimpleFont font, Brush brush, double pointX, double pointY, Brush areaBrush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			string areaBrushString = GetBrushString(areaBrush);
			
			DrawString(renderTarget, chartPanel, text, font, HelperManagedBrushes[brushString].DxBrush, pointX, pointY, HelperManagedBrushes[areaBrushString].DxBrush);
		}
		#endregion
		#endregion
		
		#region DrawText
		#region SharpDX Brushes
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="stringLength">Length in characters of text to display</param>
		/// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush, options);
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush); 
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="stringLength">Length in characters of text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="stringLength">Length in characters of text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="x">X Coordinate</param>
		/// <param name="y">Y Coordinate</param>
		/// <param name="width">Text Rectangle Width</param>
		/// <param name="height">Text Rectangle Height</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, float x, float y, float width, float height, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="x">X Coordinate</param>
		/// <param name="y">Y Coordinate</param>
		/// <param name="width">Text Rectangle Width</param>
		/// <param name="height">Text Rectangle Height</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="x">X Coordinate</param>
		/// <param name="y">Y Coordinate</param>
		/// <param name="width">Text Rectangle Width</param>
		/// <param name="height">Text Rectangle Height</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="text">Text to display</param>
		/// <param name="font">SimpleFont to use for the drawn text</param>
		/// <param name="x">X Coordinate</param>
		/// <param name="y">Y Coordinate</param>
		/// <param name="width">Text Rectangle Width</param>
		/// <param name="height">Text Rectangle Height</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, stringLength, textFormat, layoutRect, defaultForegroundBrush.DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush.DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush.DxBrush, options);
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, defaultForegroundBrush.DxBrush); 
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, float x, float y, float width, float height, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}
        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, stringLength, textFormat, layoutRect, DXMBrushes[defaultForegroundBrush].DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, DXMBrushes[defaultForegroundBrush].DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush nameh used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, DXMBrushes[defaultForegroundBrush].DxBrush, options);
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, string defaultForegroundBrush)
		{
			renderTarget.DrawText(text, textFormat, layoutRect, DXMBrushes[defaultForegroundBrush].DxBrush); 
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, string defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, float x, float y, float width, float height, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, string defaultForegroundBrush)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawText(text, stringLength, textFormat, layoutRect, HelperManagedBrushes[brushString].DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawText(text, textFormat, layoutRect, HelperManagedBrushes[brushString].DxBrush, options, measuringMode); 
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawText(text, textFormat, layoutRect, HelperManagedBrushes[brushString].DxBrush, options);
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="textFormat">SharpDX TextFormat to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawText(text, textFormat, layoutRect, HelperManagedBrushes[brushString].DxBrush); 
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="layoutRect">SharpDX Rectangle used for the text color</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, SharpDX.RectangleF layoutRect, Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="stringLength">Length in characters of text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, int stringLength, SimpleFont font, float x, float y, float width, float height, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, stringLength, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <param name="measuringMode">SharpDX MeasuringMode used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options, measuringMode); 
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
		}

        /// <summary>
        /// Draws text using Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="text">Text to display</param>
        /// <param name="font">SimpleFont to use for the drawn text</param>
        /// <param name="x">X Coordinate</param>
        /// <param name="y">Y Coordinate</param>
        /// <param name="width">Text Rectangle Width</param>
        /// <param name="height">Text Rectangle Height</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawText(SharpDX.Direct2D1.RenderTarget renderTarget, string text, SimpleFont font, float x, float y, float width, float height, Brush defaultForegroundBrush)
		{
			SharpDX.RectangleF layoutRect = new SharpDX.RectangleF(x, y, width, height);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			DrawText(renderTarget, text, textFormat, layoutRect, defaultForegroundBrush); 
			textFormat.Dispose();
			textFormat = null;
		}
		#endregion
		#endregion
		
		#region DrawTextLayout
		#region SharpDX Brushes
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="textLayout">SharpDX TextLayout to draw</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawTextLayout(origin, textLayout, defaultForegroundBrush, options);
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="textLayout">SharpDX TextLayout to draw</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			renderTarget.DrawTextLayout(origin, textLayout, defaultForegroundBrush);
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="textLayout">SharpDX TextLayout to draw</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="textLayout">SharpDX TextLayout to draw</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="text">Text to draw</param>
		/// <param name="textFormat">SharpDX TextFormat used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="text">Text to draw</param>
		/// <param name="textFormat">SharpDX TextFormat used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="text">Text to draw</param>
		/// <param name="font">SimpleFont used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="text">Text to draw</param>
		/// <param name="font">SimpleFont used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, SharpDX.Direct2D1.Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="text">Text to draw</param>
		/// <param name="textFormat">SharpDX TextFormat used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="text">Text to draw</param>
		/// <param name="textFormat">SharpDX TextFormat used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
		/// <param name="text">Text to draw</param>
		/// <param name="font">SimpleFont used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
		
		/// <summary>
		/// Draws text using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
		/// <param name="x">X coordinate</param>
		/// <param name="y">Y coordinate</param>
		/// <param name="text">Text to draw</param>
		/// <param name="font">SimpleFont used to draw text</param>
		/// <param name="defaultForegroundBrush">SharpDX brush used for the text color</param>
		/// <returns></returns>
		public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, SharpDX.Direct2D1.Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawTextLayout(origin, textLayout, defaultForegroundBrush.DxBrush, options);
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, DXMediaBrush defaultForegroundBrush)
		{
			renderTarget.DrawTextLayout(origin, textLayout, defaultForegroundBrush.DxBrush);
		}

        /// <summary>
        /// Draws text usingDXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, DXMediaBrush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">DXMediaBrush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, DXMediaBrush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			renderTarget.DrawTextLayout(origin, textLayout, DXMBrushes[defaultForegroundBrush].DxBrush, options);
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushess.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, string defaultForegroundBrush)
		{
			renderTarget.DrawTextLayout(origin, textLayout, DXMBrushes[defaultForegroundBrush].DxBrush);
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, string defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, string defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, string defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, string defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, string defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Dictionary brush name used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, string defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawTextLayout(origin, textLayout, HelperManagedBrushes[brushString].DxBrush, options);
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 origin, SharpDX.DirectWrite.TextLayout textLayout, Brush defaultForegroundBrush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, defaultForegroundBrush);
			
			string brushString = GetBrushString(defaultForegroundBrush);
			
			renderTarget.DrawTextLayout(origin, textLayout, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="textLayout">SharpDX TextLayout to draw</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.TextLayout textLayout, Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <param name="options">SharpDX DrawTextOptions used for the drawn text</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, Brush defaultForegroundBrush, SharpDX.Direct2D1.DrawTextOptions options)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush, options);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SharpDX.DirectWrite.TextFormat textFormat, Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="textFormat">SharpDX TextFormat used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SharpDX.DirectWrite.TextFormat textFormat, Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="origin">SharpDX Vector2 coordinates to draw text</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, SharpDX.Vector2 origin, string text, SimpleFont font, Brush defaultForegroundBrush)
		{
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}

        /// <summary>
        /// Draws text using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="chartPanel">The hosting NinjaScript's ChartPanel</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="text">Text to draw</param>
        /// <param name="font">SimpleFont used to draw text</param>
        /// <param name="defaultForegroundBrush">Windows Media Brush used for the text color</param>
        /// <returns></returns>
        public void DrawTextLayout(SharpDX.Direct2D1.RenderTarget renderTarget, ChartPanel chartPanel, float x, float y, string text, SimpleFont font, Brush defaultForegroundBrush)
		{
			SharpDX.Vector2 origin = new SharpDX.Vector2(x, y);
			SharpDX.DirectWrite.TextFormat textFormat = font.ToDirectWriteTextFormat();
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(NinjaTrader.Core.Globals.DirectWriteFactory,
														text, textFormat, chartPanel.X + chartPanel.W,
														textFormat.FontSize);
			
			DrawTextLayout(renderTarget, origin, textLayout, defaultForegroundBrush);
			textFormat.Dispose();
			textFormat = null;
			textLayout.Dispose();
			textLayout = null;
		}
		#endregion
		#endregion
		
		#region DrawEllipse
		#region SharpDX Brushes
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">Ellipse to draw</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawEllipse(ellipse, brush, strokeWidth, strokeStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">Ellipse to draw</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;			
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">Ellipse to draw</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Direct2D1.Brush brush, float strokeWidth)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			// Set the StrokeStyle to solid
			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;	
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">Ellipse to draw</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.DrawEllipse(ellipse, brush);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">Windows Point coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate for the center of the ellipse</param>
		/// <param name="y">Y coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">Windows Point coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate for the center of the ellipse</param>
		/// <param name="y">Y coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">Windows Point coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate for the center of the ellipse</param>
		/// <param name="y">Y coordinate for the center of the ellipse</param>
		/// <param name="radiusX">Sets the horizontal radius</param>
		/// <param name="radiusY">Sets the vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
        #endregion

        #region DXMediaBrushes

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawEllipse(ellipse, brush.DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;			
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">SharpDX brush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, DXMediaBrush brush, float strokeWidth)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			// Set the StrokeStyle to solid
			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;	
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, DXMediaBrush brush)
		{
			renderTarget.DrawEllipse(ellipse, brush.DxBrush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, DXMediaBrush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, DXMediaBrush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, DXMediaBrush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
        #endregion

        #region Dictionary Brushes

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawEllipse(ellipse, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;			
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, string brush, float strokeWidth)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			// Set the StrokeStyle to solid
			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;	
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, string brush)
		{
			renderTarget.DrawEllipse(ellipse, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, string brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, string brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, string brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
		#endregion
		
		#region Media Brushes
		
		/// <summary>
		/// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">Ellipse to draw</param>
		/// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
		/// <param name="strokeWidth">Width of the Ellipses line</param>
		/// <param name="strokeStyle">SharpDX StrokeStyle used to describe the Ellipses line</param>
		/// <returns></returns>
		public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawEllipse(ellipse,  HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;			
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, Brush brush, float strokeWidth)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			// Set the StrokeStyle to solid
			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			
			DrawEllipse(renderTarget, ellipse, brush, strokeWidth, strokeStyle);
			
			// StrokeStyle is device-independant and does not need to be Disposed after each OnRender() or OnRenderTargetChanged() call, but is for good housekeeping and garbage collection
			strokeStyle.Dispose();
			strokeStyle = null;	
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="ellipse">Ellipse to draw</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawEllipse(ellipse, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, Brush brush)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector 2 coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows Point coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws Ellipses (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for the center of the ellipse</param>
        /// <param name="y">Y coordinate for the center of the ellipse</param>
        /// <param name="radiusX">Sets the horizontal radius</param>
        /// <param name="radiusY">Sets the vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <param name="strokeWidth">Width of the Ellipses line</param>
        /// <param name="dashStyle">NinjaTrader DashStyleHelper used to describe the Ellipses line</param>
        /// <returns></returns>
        public void DrawEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			DrawEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush, strokeWidth, dashStyle);
		}
		#endregion
		#endregion
		
		#region FillEllipse
		#region SharpDX Brushes
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">SharpDX Ellipse used to draw text</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.FillEllipse(ellipse, brush);
		}
		
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">SharpDX Vector2 point used to draw the Ellipse</param>
		/// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
		/// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}
		
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="point">Windows point used to draw the Ellipse</param>
		/// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
		/// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}
		
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using SharpDX Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="x">X coordinate used to draw the Ellipse</param>
		/// <param name="y">Y coordinate used to draw the Ellipse</param>
		/// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
		/// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
		/// <param name="brush">SharpDX brush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}
		#endregion
		
		#region DXMediaBrushes
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using DXMediaBrushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">SharpDX Ellipse used to draw text</param>
		/// <param name="brush">DXMediaBrush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, DXMediaBrush brush)
		{
			renderTarget.FillEllipse(ellipse, brush.DxBrush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector2 point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, DXMediaBrush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, DXMediaBrush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate used to draw the Ellipse</param>
        /// <param name="y">Y coordinate used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, DXMediaBrush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}
		#endregion
		
		#region Dictionary Brushes
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using Dictionary DXMediaBrushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">SharpDX Ellipse used to draw text</param>
		/// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, string brush)
		{
			renderTarget.FillEllipse(ellipse, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector2 point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, string brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, string brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate used to draw the Ellipse</param>
        /// <param name="y">Y coordinate used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">String key for a Dictionary managed DXMediaBrush used for the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, string brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}
		#endregion
		
		#region Media Brushes
		/// <summary>
		/// Draws a filled in Ellipse (Circles and ovals) using Windows Media Brushes.
		/// </summary>
		/// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
		/// <param name="ellipse">SharpDX Ellipse used to draw text</param>
		/// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
		/// <returns></returns>
		public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Ellipse ellipse, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.FillEllipse(ellipse, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">SharpDX Vector2 point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 point, float radiusX, float radiusY, Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point, radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="point">Windows point used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, System.Windows.Point point, float radiusX, float radiusY, Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(point.ToVector2(), radiusX, radiusY), brush);
		}

        /// <summary>
        /// Draws a filled in Ellipse (Circles and ovals) using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate used to draw the Ellipse</param>
        /// <param name="y">Y coordinate used to draw the Ellipse</param>
        /// <param name="radiusX">float used to define the Ellipse' horizontal radius</param>
        /// <param name="radiusY">float used to define the Ellipse' vertical radius</param>
        /// <param name="brush">A Windows Media Brush to be used as the Ellipse color</param>
        /// <returns></returns>
        public void FillEllipse(SharpDX.Direct2D1.RenderTarget renderTarget, double x, double y, float radiusX, float radiusY, Brush brush)
		{
			FillEllipse(renderTarget, new SharpDX.Direct2D1.Ellipse(new SharpDX.Vector2(){ X = (float)x, Y = (float)y }, radiusX, radiusY), brush);
		}
        #endregion
        #endregion

        #region DrawRectangle
        #region SharpDX Brushes
        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRectangle(rect, brush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawRectangle(renderTarget, rect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRectangle(rect, brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, SharpDX.Direct2D1.Brush brush, float strokeWidth)
		{
			renderTarget.DrawRectangle(rect, brush, strokeWidth);
		}

        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.DrawRectangle(rect, brush);
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRectangle(rect, brush.DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawRectangle(renderTarget, rect, brush.DxBrush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRectangle(rect, brush.DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, DXMediaBrush brush, float strokeWidth)
		{
			renderTarget.DrawRectangle(rect, brush.DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, DXMediaBrush brush)
		{
			renderTarget.DrawRectangle(rect, brush.DxBrush);
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws a rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using Dictionary DXMBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawRectangle(renderTarget, rect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, string brush, float strokeWidth)
		{
			renderTarget.DrawRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, string brush)
		{
			renderTarget.DrawRectangle(rect, DXMBrushes[brush].DxBrush);
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawRectangle(renderTarget, rect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, Brush brush, float strokeWidth)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRectangle(rect, HelperManagedBrushes[brushString].DxBrush);
		}
        #endregion
        #endregion

        #region FillRectangle
        #region SharpDX Brushes
        /// <summary>
        /// Draws a filled rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.FillRectangle(rect, brush);
		}

        /// <summary>
        /// Draws a filled rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, SharpDX.Direct2D1.Brush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			FillRectangle(renderTarget, rect, brush);
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws a filled rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, DXMediaBrush brush)
		{
			renderTarget.FillRectangle(rect, brush.DxBrush);
		}

        /// <summary>
        /// Draws a filled rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, DXMediaBrush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			FillRectangle(renderTarget, rect, brush);
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws a filled rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, string brush)
		{
			renderTarget.FillRectangle(rect, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws a filled rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, string brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			FillRectangle(renderTarget, rect, brush);
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws a filled rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF used for the Rectangle points</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.FillRectangle(rect, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws a filled rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, Brush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			FillRectangle(renderTarget, rect, brush);
		}
        #endregion
        #endregion

        #region DrawRoundedRectangle
        #region SharpDX Brushes
        /// <summary>
        /// Draws a rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>		
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRoundedRectangle(rect, brush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF to describe Rectangle before rounding</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StokeStyle used for the RoundedRectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRoundedRectangle(rect, brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, SharpDX.Direct2D1.Brush brush, float strokeWidth)
		{
			renderTarget.DrawRoundedRectangle(rect, brush, strokeWidth);
		}

        /// <summary>
        /// Draws a rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.DrawRoundedRectangle(rect, brush);
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws a rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRoundedRectangle(rect, brush.DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush.DxBrush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF to describe Rectangle before rounding</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush.DxBrush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StokeStyle used for the RoundedRectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRoundedRectangle(rect, brush.DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, DXMediaBrush brush, float strokeWidth)
		{
			renderTarget.DrawRoundedRectangle(rect, brush.DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, DXMediaBrush brush)
		{
			renderTarget.DrawRoundedRectangle(rect, brush.DxBrush);
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws a rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRoundedRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF to describe Rectangle before rounding</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StokeStyle used for the RoundedRectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawRoundedRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, string brush, float strokeWidth)
		{
			renderTarget.DrawRoundedRectangle(rect, DXMBrushes[brush].DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, string brush)
		{
			renderTarget.DrawRoundedRectangle(rect, DXMBrushes[brush].DxBrush);
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws a rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>		
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			renderTarget.DrawRoundedRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws a rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RectangleF to describe Rectangle before rounding</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			DrawRoundedRectangle( renderTarget, roundedRect, brush, strokeWidth, dashStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StokeStyle used for the RoundedRectangle line</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRoundedRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws a rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>	
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, Brush brush, float strokeWidth)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRoundedRectangle(rect, HelperManagedBrushes[brushString].DxBrush, strokeWidth);
		}

        /// <summary>
        /// Draws a rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void DrawRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawRoundedRectangle(rect, HelperManagedBrushes[brushString].DxBrush);
		}
        #endregion
        #endregion

        #region FillRoundedRectangle
        #region SharpDX Brushes
        /// <summary>
        /// Draws a filled rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.FillRoundedRectangle(rect, brush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">SharpDX brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, SharpDX.Direct2D1.Brush brush)
		{			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws a filled rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, DXMediaBrush brush)
		{
			renderTarget.FillRoundedRectangle(rect, brush.DxBrush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, DXMediaBrush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">DXMediaBrush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, DXMediaBrush brush)
		{			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws a filled rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, float radiusX, float radiusY, string brush)
		{
			renderTarget.FillRoundedRectangle(rect, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, string brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Dictionary brush name used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, string brush)
		{			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, DXMBrushes[brush].DxBrush);
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws a filled rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.RoundedRectangle rect, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.FillRoundedRectangle(rect, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="x">X coordinate for Rectangle</param>
        /// <param name="y">Y coordinate for Rectangle</param>
        /// <param name="width">Width of Rectangle</param>
        /// <param name="height">Height of Rectangle</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, float width, float height, float radiusX, float radiusY, Brush brush)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}

        /// <summary>
        /// Draws a filled rounded rectangle using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="rect">SharpDX RoundedRectangle to describe shape</param>
        /// <param name="radiusX">Horizontal radius of Rounded Rectangle</param>
        /// <param name="radiusY">Vertical radius of Rounded Rectangle</param>
        /// <param name="brush">Windows Media Brush used for the Rectangle Color</param>
        /// <returns></returns>
        public void FillRoundedRectangle(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.RectangleF rect, float radiusX, float radiusY, Brush brush)
		{			
			SharpDX.Direct2D1.RoundedRectangle roundedRect = new SharpDX.Direct2D1.RoundedRectangle() { RadiusX = radiusX, RadiusY = radiusY, Rect = rect };
			
			FillRoundedRectangle(renderTarget, roundedRect, brush);
		}
        #endregion
        #endregion

        #region DrawGeometry
        #region SharpDX Brushes
        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
		
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, dashStyle);
		}
        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{						
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);			
			
			DrawGeometry(renderTarget, points, brush, strokeWidth, strokeStyle);

			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush, float strokeWidth)
		{
			DrawGeometry(renderTarget, points, brush, strokeWidth, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, points, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush)
		{
			DrawGeometry(renderTarget, points, brush, 1f, DashStyleHelper.Solid);
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawGeometry(geometry, brush.DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <returns></returns>	
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush, float strokeWidth, DashStyleHelper dashStyle)
		{						
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);			
			
			DrawGeometry(renderTarget, points, brush, strokeWidth, strokeStyle);

			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush, float strokeWidth)
		{
			DrawGeometry(renderTarget, points, brush, strokeWidth, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, points, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush)
		{
			DrawGeometry(renderTarget, points, brush, 1f, DashStyleHelper.Solid);
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			renderTarget.DrawGeometry(geometry, DXMBrushes[brush].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);

			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush, float strokeWidth, DashStyleHelper dashStyle)
		{						
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);			
			
			DrawGeometry(renderTarget, points, brush, strokeWidth, strokeStyle);

			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush, float strokeWidth)
		{
			DrawGeometry(renderTarget, points, brush, strokeWidth, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, points, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush)
		{
			DrawGeometry(renderTarget, points, brush, 1f, DashStyleHelper.Solid);
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.DrawGeometry(geometry, HelperManagedBrushes[brushString].DxBrush, strokeWidth, strokeStyle);
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{			
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush)
		{
			DrawGeometry(renderTarget, geometry, brush, 1f, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="strokeStyle">SharpDX StrokeStyle used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush, float strokeWidth, SharpDX.Direct2D1.StrokeStyle strokeStyle)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			DrawGeometry(renderTarget, geometry, brush, strokeWidth, strokeStyle);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush, float strokeWidth, DashStyleHelper dashStyle)
		{						
			// Create StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyleProperties ssProps = new SharpDX.Direct2D1.StrokeStyleProperties();
			
			switch (dashStyle)
			{
				case DashStyleHelper.Dash: 			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dash; 		break;
				case DashStyleHelper.DashDot: 		ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDot; 	break;
				case DashStyleHelper.DashDotDot:	ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.DashDotDot;	break;
				case DashStyleHelper.Dot:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Dot;		break;
				case DashStyleHelper.Solid:			ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
				default: 							ssProps.DashStyle = SharpDX.Direct2D1.DashStyle.Solid;		break;
			}
			
			// Create StrokeStyle from StrokeStyleProperties
			SharpDX.Direct2D1.StrokeStyle strokeStyle = new SharpDX.Direct2D1.StrokeStyle(Core.Globals.D2DFactory, ssProps);			
			
			DrawGeometry(renderTarget, points, brush, strokeWidth, strokeStyle);

			strokeStyle.Dispose();
			strokeStyle = null;
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="strokeWidth">Width of Rectangle line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush, float strokeWidth)
		{
			DrawGeometry(renderTarget, points, brush, strokeWidth, DashStyleHelper.Solid);
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="dashStyle">DashStyleHelper used for the Geometry line</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush, DashStyleHelper dashStyle)
		{
			DrawGeometry(renderTarget, points, brush, 1f, dashStyle);
		}

        /// <summary>
        /// Draws geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush)
		{
			DrawGeometry(renderTarget, points, brush, 1f, DashStyleHelper.Solid);
		}
        #endregion
        #endregion

        #region FillGeometry
        #region SharpDX Brushes
        /// <summary>
        /// Draws filled geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="brushOpacity">SharpDX brush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush, SharpDX.Direct2D1.Brush brushOpacity)
		{
			renderTarget.FillGeometry(geometry, brush, brushOpacity);
		}

        /// <summary>
        /// Draws filled geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, SharpDX.Direct2D1.Brush brush)
		{
			renderTarget.FillGeometry(geometry, brush);
		}
        
        /// <summary>
        /// Draws filled geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <param name="brushOpacity">SharpDX brush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush, SharpDX.Direct2D1.Brush brushOpacity)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush, brushOpacity);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws filled geometry using SharpDX Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">SharpDX brush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, SharpDX.Direct2D1.Brush brush)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}
        #endregion

        #region DXMediaBrushes
        /// <summary>
        /// Draws filled geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="brushOpacity">DXMediaBrush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush, DXMediaBrush brushOpacity)
		{
			renderTarget.FillGeometry(geometry, brush.DxBrush, brushOpacity.DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, DXMediaBrush brush)
		{
			renderTarget.FillGeometry(geometry, brush.DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <param name="brushOpacity">DXMediaBrush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush, DXMediaBrush brushOpacity)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush, brushOpacity);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws filled geometry using DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">DXMediaBrush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, DXMediaBrush brush)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}
        #endregion

        #region Dictionary Brushes
        /// <summary>
        /// Draws filled geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="brushOpacity">Dictionary brush name used for the Geometry Opacity</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush, string brushOpacity)
		{
			renderTarget.FillGeometry(geometry, DXMBrushes[brush].DxBrush, DXMBrushes[brushOpacity].DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, string brush)
		{
			renderTarget.FillGeometry(geometry, DXMBrushes[brush].DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <param name="brushOpacity">Dictionary brush name used for the Geometry Opacity</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush, string brushOpacity)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush, brushOpacity);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws filled geometry using Dictionary DXMediaBrushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Dictionary brush name used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, string brush)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}
        #endregion

        #region Media Brushes
        /// <summary>
        /// Draws filled geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="brushOpacity">Windows Media Brush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush, Brush brushOpacity)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			HelperCheckAddBrush(renderTarget, brushOpacity);
			
			string brushString = GetBrushString(brush);
			string brushOpacityString = GetBrushString(brushOpacity);
			
			renderTarget.FillGeometry(geometry, HelperManagedBrushes[brushString].DxBrush, HelperManagedBrushes[brushOpacityString].DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="geometry">SharpDX Geometry to describe shape</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Direct2D1.Geometry geometry, Brush brush)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, brush);
			
			string brushString = GetBrushString(brush);
			
			renderTarget.FillGeometry(geometry, HelperManagedBrushes[brushString].DxBrush);
		}

        /// <summary>
        /// Draws filled geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <param name="brushOpacity">Windows Media Brush used for the Geometry Opacity</param>
        /// <returns></returns>
        public void DrawGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush, Brush brushOpacity)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush, brushOpacity);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}

        /// <summary>
        /// Draws filled geometry using Windows Media Brushes.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="points">SharpDX Vector2 array of points for our Geometry</param>
        /// <param name="brush">Windows Media Brush used for the Geometry Color</param>
        /// <returns></returns>
        public void FillGeometry(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2[] points, Brush brush)
		{						
			SharpDX.Direct2D1.PathGeometry geometry = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);	
			SharpDX.Direct2D1.GeometrySink sink = geometry.Open();
			
			sink.BeginFigure(points[0], new SharpDX.Direct2D1.FigureBegin());	
			
			for (int i = 1; i < points.GetLength(0); i++)
				sink.AddLine(points[i]);
			
			sink.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink.Close();			
			
			FillGeometry(renderTarget, geometry, brush);
			
			geometry.Dispose();
			geometry = null;
			sink.Dispose();
			sink = null;
		}
		#endregion	
		#endregion
		
		#region DrawGlyphRun
		#region SharpDX Brushes
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 baselineOrigin, SharpDX.DirectWrite.GlyphRun glyphRun, SharpDX.Direct2D1.Brush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawGlyphRun(baselineOrigin, glyphRun, foregroundBrush, measuringMode);
		}
		
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.GlyphRun glyphRun, SharpDX.Direct2D1.Brush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.Vector2 baselineOrigin = new SharpDX.Vector2(x, y);
			DrawGlyphRun(renderTarget, baselineOrigin, glyphRun, foregroundBrush, measuringMode);
		}
		#endregion
		
		#region DXMediaBrushes
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 baselineOrigin, SharpDX.DirectWrite.GlyphRun glyphRun, DXMediaBrush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawGlyphRun(baselineOrigin, glyphRun, foregroundBrush.DxBrush, measuringMode);
		}
		
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.GlyphRun glyphRun, DXMediaBrush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.Vector2 baselineOrigin = new SharpDX.Vector2(x, y);
			DrawGlyphRun(renderTarget, baselineOrigin, glyphRun, foregroundBrush, measuringMode);
		}
		#endregion
		
		#region Dictionary Brushes
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 baselineOrigin, SharpDX.DirectWrite.GlyphRun glyphRun, string foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			renderTarget.DrawGlyphRun(baselineOrigin, glyphRun, DXMBrushes[foregroundBrush].DxBrush, measuringMode);
		}
		
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.GlyphRun glyphRun, string foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.Vector2 baselineOrigin = new SharpDX.Vector2(x, y);
			DrawGlyphRun(renderTarget, baselineOrigin, glyphRun, foregroundBrush, measuringMode);
		}
		#endregion
		
		#region Media Brushes
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, SharpDX.Vector2 baselineOrigin, SharpDX.DirectWrite.GlyphRun glyphRun, Brush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			// Check if we have this brush and create it if not.
			HelperCheckAddBrush(renderTarget, foregroundBrush);
			
			string brushString = GetBrushString(foregroundBrush);
			
			renderTarget.DrawGlyphRun(baselineOrigin, glyphRun, HelperManagedBrushes[brushString].DxBrush, measuringMode);
		}
		
		public void DrawGlyphRun(SharpDX.Direct2D1.RenderTarget renderTarget, float x, float y, SharpDX.DirectWrite.GlyphRun glyphRun, Brush foregroundBrush, SharpDX.Direct2D1.MeasuringMode measuringMode)
		{
			SharpDX.Vector2 baselineOrigin = new SharpDX.Vector2(x, y);
			DrawGlyphRun(renderTarget, baselineOrigin, glyphRun, foregroundBrush, measuringMode);
		}
		#endregion
		#endregion
	
		#region DrawBitmap
		private void CreateBitmapResources(SharpDX.Direct2D1.RenderTarget renderTarget)
		{
			foreach (string filePath in bitmapList)
			{
				if (!fileStreamDictionary.ContainsKey(filePath))
					fileStreamDictionary.Add(filePath, new SharpDX.IO.NativeFileStream(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, filePath), SharpDX.IO.NativeFileMode.Open, SharpDX.IO.NativeFileAccess.Read));
				
				if (!bitmapDecoderDictionary.ContainsKey(filePath))
					bitmapDecoderDictionary.Add(filePath, new SharpDX.WIC.BitmapDecoder(Core.Globals.WicImagingFactory, fileStreamDictionary[filePath], SharpDX.WIC.DecodeOptions.CacheOnDemand));
				
				if (!frameDictionary.ContainsKey(filePath))
					frameDictionary.Add(filePath, bitmapDecoderDictionary[filePath].GetFrame(0));
				
				if (!converterDictionary.ContainsKey(filePath))
				{
					converterDictionary.Add(filePath, new SharpDX.WIC.FormatConverter(Core.Globals.WicImagingFactory));
					converterDictionary[filePath].Initialize(frameDictionary[filePath], SharpDX.WIC.PixelFormat.Format32bppPRGBA);				
				}
				
				if (!bitmapDictionary.ContainsKey(filePath))
					bitmapDictionary.Add(filePath, SharpDX.Direct2D1.Bitmap.FromWicBitmap(renderTarget, converterDictionary[filePath]));
			}
		}
		
		private void DisposeBitmapResources()
		{
			foreach (KeyValuePair<string, SharpDX.Direct2D1.Bitmap> bitmap in bitmapDictionary)
				bitmap.Value.Dispose();
			foreach (KeyValuePair<string, SharpDX.IO.NativeFileStream> fileStream in fileStreamDictionary)
				fileStream.Value.Dispose();
			foreach (KeyValuePair<string, SharpDX.WIC.BitmapDecoder> bitmapDecoder in bitmapDecoderDictionary)
				bitmapDecoder.Value.Dispose();
			foreach (KeyValuePair<string, SharpDX.WIC.FormatConverter> converter in converterDictionary)
				converter.Value.Dispose();
			foreach (KeyValuePair<string, SharpDX.WIC.BitmapFrameDecode> frame in frameDictionary)
				frame.Value.Dispose();
			
			bitmapDictionary.Clear();
			fileStreamDictionary.Clear();
			bitmapDecoderDictionary.Clear();
			converterDictionary.Clear();
			frameDictionary.Clear();
		}
		
		private void AddBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath)
		{
			bitmapList.Add(filePath);
			
			CreateBitmapResources(renderTarget);
		}

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="rect">SharpDX RectangleF describing how the image should be presented</param>
        /// <param name="opacity">Opacity for the image (1-100)</param>
        /// <param name="bitmapInterp">SharpDX BitmapInterpolationMode used for the Iamge</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, SharpDX.RectangleF rect, float opacity, SharpDX.Direct2D1.BitmapInterpolationMode bitmapInterp)
		{
			if(!System.IO.File.Exists(filePath))
				return;
			
			if (!bitmapList.Contains(filePath))
				AddBitmap(renderTarget, filePath);
			
			opacity = Math.Min(100, Math.Max(1, opacity)) / 100;
			
			renderTarget.DrawBitmap(bitmapDictionary[filePath], rect, opacity, bitmapInterp);
		}

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="rect">SharpDX RectangleF describing how the image should be presented</param>
        /// <param name="opacity">Opacity for the image (1-100)</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, SharpDX.RectangleF rect, float opacity)
		{
			DrawBitmap(renderTarget, filePath, rect, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="rect">SharpDX RectangleF describing how the image should be presented</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, SharpDX.RectangleF rect)
		{
			DrawBitmap(renderTarget, filePath, rect, 100, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="x">X coordinate for Image Rectangle</param>
        /// <param name="y">Y coordinate for Image Rectangle</param>
        /// <param name="width">Width of Image Rectangle</param>
        /// <param name="height">Height of Image Rectangle</param>
        /// <param name="opacity">Opacity for the image (1-100)</param>
        /// <param name="bitmapInterp">SharpDX BitmapInterpolationMode used for the Iamge</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, float x, float y, float width, float height, float opacity, SharpDX.Direct2D1.BitmapInterpolationMode bitmapInterp)
		{
			if (!bitmapList.Contains(filePath))
				AddBitmap(renderTarget, filePath);
			
			opacity = Math.Min(100, Math.Max(1, opacity)) / 100;
			
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			renderTarget.DrawBitmap(bitmapDictionary[filePath], rect, opacity, bitmapInterp);
		}
        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="x">X coordinate for Image Rectangle</param>
        /// <param name="y">Y coordinate for Image Rectangle</param>
        /// <param name="width">Width of Image Rectangle</param>
        /// <param name="height">Height of Image Rectangle</param>
        /// <param name="opacity">Opacity for the image (1-100)</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, float x, float y, float width, float height, float opacity)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawBitmap(renderTarget, filePath, rect, opacity, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}

        /// <summary>
        /// Draws an image.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="filePath">Path for the image to be used for the bitmap</param>
        /// <param name="x">X coordinate for Image Rectangle</param>
        /// <param name="y">Y coordinate for Image Rectangle</param>
        /// <param name="width">Width of Image Rectangle</param>
        /// <param name="height">Height of Image Rectangle</param>
        /// <returns></returns>
        public void DrawBitmap(SharpDX.Direct2D1.RenderTarget renderTarget, string filePath, float x, float y, float width, float height)
		{
			SharpDX.RectangleF rect = new SharpDX.RectangleF(x, y, width, height);
			
			DrawBitmap(renderTarget, filePath, rect, 100, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}
		#endregion
	}
	
	// Our DX/Media Brush management class.
    public class DXMediaBrush
    {
		private System.Windows.Media.Brush mediaBrush;
		private SharpDX.Direct2D1.Brush dxBrush;
		private double opacity;

        /// <summary>
        /// Gets Alpha component of the DXMediaBrush
        /// </summary>
        public byte GetAlpha()
        {
            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).A;
        }

        /// <summary>
        /// Gets Red component of the DXMediaBrush
        /// </summary>
        public byte GetRed()
        {
            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).R;
        }

        /// <summary>
        /// Gets Green component of the DXMediaBrush
        /// </summary>
        public byte GetGreen()
        {
            return ((Color)mediaBrush.GetValue(SolidColorBrush.ColorProperty)).G;
        }

        /// <summary>
        /// Gets Blue component of the DXMediaBrush
        /// </summary>
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

        /// <summary>
        /// Default Constructor
        /// </summary>
        public DXMediaBrush()
		{
			dxBrush = null;
			mediaBrush = null;
			opacity = 100.0;
		}

        /// <summary>
        /// Disposes Helper Managed Resources
        /// </summary>
        public void Dispose()
		{
			if(dxBrush != null)
				dxBrush.Dispose();
		}

        /// <summary>
        /// Updates the Media Bursh and SharpDX Brush without changing opacity.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="newOpacity">The new Opacity to use</param>
        public void UpdateOpacity(SharpDX.Direct2D1.RenderTarget renderTarget, double newOpacity)
		{					
			UpdateBrush(renderTarget, mediaBrush, newOpacity);
		}

        /// <summary>
        /// Updates the Media Bursh and SharpDX Brush also changing opacity.
        /// </summary>
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="newMediaBrush">The new Media Brush to use</param>
        /// <param name="newOpacity">The new Opacity to use</param>
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
        /// <param name="renderTarget">The hosting NinjaScript's RenderTarget</param>
        /// <param name="newMediaBrush">The new Media Brush to use</param>
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
}
