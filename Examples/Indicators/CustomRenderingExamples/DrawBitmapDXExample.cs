//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jesse, NinjaTrader_Jim
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
	public class DrawBitmapDXExample : Indicator
	{
		private SharpDX.WIC.BitmapDecoder		bitmapDecoder;
		private SharpDX.WIC.FormatConverter		converter;
		private SharpDX.IO.NativeFileStream		fileStream;
		private SharpDX.WIC.BitmapFrameDecode	frame;
		private SharpDX.Direct2D1.Bitmap		myBitmap;
		private bool							needToUpdate;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name						= "DrawBitmapDXExample";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				FilePath					= Path.Combine(Core.Globals.UserDataDir, @"templates\CustomIndicatorStorage\DrawBitmapDXExample", "NinjaStar.png");
			}
			else if (State == State.DataLoaded)
			{
				if (!File.Exists(FilePath))
					Draw.TextFixed(this, "message", "File does not exist:\r\n" + FilePath, TextPosition.BottomLeft);
			}
			else if (State == State.Terminated)
			{
				if (myBitmap != null) 		myBitmap.Dispose();
				if (fileStream != null) 	fileStream.Dispose();
				if (bitmapDecoder != null) 	bitmapDecoder.Dispose();
				if (converter != null) 		converter.Dispose();
				if (frame != null) 			frame.Dispose();
			}
		}

		protected override void OnBarUpdate() { }

		public override void OnRenderTargetChanged()
		{
			base.OnRenderTargetChanged();

			if (RenderTarget == null || RenderTarget.IsDisposed) return;

			UpdateImage(FilePath);
		}

		protected override void OnRender(NinjaTrader.Gui.Chart.ChartControl chartControl, NinjaTrader.Gui.Chart.ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);

			if (RenderTarget == null || Bars == null || Bars.Instrument == null || myBitmap == null) return;

			if (needToUpdate)
			{
				UpdateImage(FilePath);
				needToUpdate	= false;
			}

			if (myBitmap != null)
				RenderTarget.DrawBitmap(myBitmap, new SharpDX.RectangleF(((float)ChartPanel.W / 2) - (myBitmap.Size.Width / 2), ((float)ChartPanel.H / 2) - (myBitmap.Size.Height / 2), myBitmap.Size.Width, myBitmap.Size.Height), 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}


		private void UpdateImage(string fileName)
		{
			// Dispose all Render dependant resources on RenderTarget change.
			if (myBitmap != null) 		myBitmap.Dispose();
			if (fileStream != null) 	fileStream.Dispose();
			if (bitmapDecoder != null) 	bitmapDecoder.Dispose();
			if (converter != null) 		converter.Dispose();
			if (frame != null) 			frame.Dispose();
			
			if (RenderTarget == null) return;

			if (!File.Exists(FilePath)) return;

			try
			{
				// Neccessary for creating WIC objects.
				fileStream		= new SharpDX.IO.NativeFileStream(FilePath, SharpDX.IO.NativeFileMode.Open, SharpDX.IO.NativeFileAccess.Read);
				// Used to read the image source file.
				bitmapDecoder	= new SharpDX.WIC.BitmapDecoder(Core.Globals.WicImagingFactory, fileStream, SharpDX.WIC.DecodeOptions.CacheOnDemand);
				// Get the first frame of the image.
				frame			= bitmapDecoder.GetFrame(0);
				// Convert it to a compatible pixel format.			
				converter		= new SharpDX.WIC.FormatConverter(Core.Globals.WicImagingFactory);
				converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);
				// Create the new Bitmap1 directly from the FormatConverter.
				myBitmap		= SharpDX.Direct2D1.Bitmap.FromWicBitmap(RenderTarget, converter);
			}
			catch (Exception exception)
			{
				Log(exception.ToString(), LogLevel.Error);
				Print(string.Format("Could not load bitmap:\r\n{0}\r\n\r\n{1}", FilePath, exception));
			}			
		}

		// TODO: why does a filter with multiple extensions not work?  "Image Files *.jpg, *.jpeg, *.jpe, *.jfif, *.png, *.tiff) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png, *.tiff"
		[NinjaScriptProperty]
		[Display(Name = "Image path", Order = 0, GroupName = "NinjaScriptParameters")]
		[PropertyEditor("NinjaTrader.Gui.Tools.FilePathPicker", Filter = "Image Files (*.*) | *.*", Title = "Select Files")]
		public string FilePath
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CustomRenderingExamples.DrawBitmapDXExample[] cacheDrawBitmapDXExample;
		public CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(string filePath)
		{
			return DrawBitmapDXExample(Input, filePath);
		}

		public CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(ISeries<double> input, string filePath)
		{
			if (cacheDrawBitmapDXExample != null)
				for (int idx = 0; idx < cacheDrawBitmapDXExample.Length; idx++)
					if (cacheDrawBitmapDXExample[idx] != null && cacheDrawBitmapDXExample[idx].FilePath == filePath && cacheDrawBitmapDXExample[idx].EqualsInput(input))
						return cacheDrawBitmapDXExample[idx];
			return CacheIndicator<CustomRenderingExamples.DrawBitmapDXExample>(new CustomRenderingExamples.DrawBitmapDXExample(){ FilePath = filePath }, input, ref cacheDrawBitmapDXExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(string filePath)
		{
			return indicator.DrawBitmapDXExample(Input, filePath);
		}

		public Indicators.CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(ISeries<double> input , string filePath)
		{
			return indicator.DrawBitmapDXExample(input, filePath);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(string filePath)
		{
			return indicator.DrawBitmapDXExample(Input, filePath);
		}

		public Indicators.CustomRenderingExamples.DrawBitmapDXExample DrawBitmapDXExample(ISeries<double> input , string filePath)
		{
			return indicator.DrawBitmapDXExample(input, filePath);
		}
	}
}

#endregion
