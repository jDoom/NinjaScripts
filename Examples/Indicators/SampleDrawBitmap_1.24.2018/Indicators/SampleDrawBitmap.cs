namespace NinjaTrader.NinjaScript.Indicators
{
	public class SampleDrawBitmap : Indicator
	{
		private SharpDX.Direct2D1.Bitmap myBitmap;
		private SharpDX.IO.NativeFileStream fileStream;
		private SharpDX.WIC.BitmapDecoder bitmapDecoder;
		private SharpDX.WIC.FormatConverter converter;
		private SharpDX.WIC.BitmapFrameDecode frame;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "SampleDrawBitmap";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;

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
			
			// Dispose all Render dependant resources on RenderTarget change.
			if (myBitmap != null) 		myBitmap.Dispose();
			if (fileStream != null) 	fileStream.Dispose();
			if (bitmapDecoder != null) 	bitmapDecoder.Dispose();
			if (converter != null) 		converter.Dispose();
			if (frame != null) 			frame.Dispose();
			
			// Neccessary for creating WIC objects.
			fileStream = new SharpDX.IO.NativeFileStream(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "woodenBox.png"), SharpDX.IO.NativeFileMode.Open, SharpDX.IO.NativeFileAccess.Read);
			// Used to read the image source file.
			bitmapDecoder = new SharpDX.WIC.BitmapDecoder(Core.Globals.WicImagingFactory, fileStream, SharpDX.WIC.DecodeOptions.CacheOnDemand);
			// Get the first frame of the image.
			frame = bitmapDecoder.GetFrame(0);
			// Convert it to a compatible pixel format.			
			converter = new SharpDX.WIC.FormatConverter(Core.Globals.WicImagingFactory);
			converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);
			// Create the new Bitmap1 directly from the FormatConverter.
			myBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(RenderTarget, converter);
						
		}

		protected override void OnRender(NinjaTrader.Gui.Chart.ChartControl chartControl, NinjaTrader.Gui.Chart.ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if (RenderTarget == null || Bars == null || Bars.Instrument == null || myBitmap == null)
				return;
			RenderTarget.DrawBitmap(myBitmap, new SharpDX.RectangleF((float)ChartPanel.W / 2, (float)ChartPanel.H / 2, 150, 150), 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
		}
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleDrawBitmap[] cacheSampleDrawBitmap;
		public SampleDrawBitmap SampleDrawBitmap()
		{
			return SampleDrawBitmap(Input);
		}

		public SampleDrawBitmap SampleDrawBitmap(ISeries<double> input)
		{
			if (cacheSampleDrawBitmap != null)
				for (int idx = 0; idx < cacheSampleDrawBitmap.Length; idx++)
					if (cacheSampleDrawBitmap[idx] != null &&  cacheSampleDrawBitmap[idx].EqualsInput(input))
						return cacheSampleDrawBitmap[idx];
			return CacheIndicator<SampleDrawBitmap>(new SampleDrawBitmap(), input, ref cacheSampleDrawBitmap);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleDrawBitmap SampleDrawBitmap()
		{
			return indicator.SampleDrawBitmap(Input);
		}

		public Indicators.SampleDrawBitmap SampleDrawBitmap(ISeries<double> input )
		{
			return indicator.SampleDrawBitmap(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleDrawBitmap SampleDrawBitmap()
		{
			return indicator.SampleDrawBitmap(Input);
		}

		public Indicators.SampleDrawBitmap SampleDrawBitmap(ISeries<double> input )
		{
			return indicator.SampleDrawBitmap(input);
		}
	}
}

#endregion
