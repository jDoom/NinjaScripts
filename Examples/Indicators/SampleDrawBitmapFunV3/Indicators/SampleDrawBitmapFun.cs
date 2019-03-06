using SharpDX;

namespace NinjaTrader.NinjaScript.Indicators
{
	public class SampleDrawBitmapFun : Indicator
	{
		private SharpDX.Direct2D1.Bitmap myBitmap;
		private SharpDX.IO.NativeFileStream fileStream;
		private SharpDX.WIC.BitmapDecoder bitmapDecoder;
		private SharpDX.WIC.FormatConverter converter;
		private SharpDX.WIC.BitmapFrameDecode frame;
		private double lastPosX;
		private double lastPosY;
		private float myfloat;
		private float H = 0;
		private float W = 0;
		private float Ratio = 1;
		private bool HWSet = false;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name = "SampleDrawBitmapFun";
				Calculate = Calculate.OnBarClose;
				IsOverlay = true;
				ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive = true;
			}
			else if (State == State.DataLoaded)
			{				
				if (ChartControl != null)
				{
					ChartPanel.MouseMove += new System.Windows.Input.MouseEventHandler (ChartControl_MouseMove);
				}
			}
			else if (State == State.Terminated)
			{
				if (myBitmap != null) 		myBitmap.Dispose();
				if (fileStream != null) 	fileStream.Dispose();
				if (bitmapDecoder != null) 	bitmapDecoder.Dispose();
				if (converter != null) 		converter.Dispose();
				if (frame != null) 			frame.Dispose();
				
				if (ChartControl != null)
					ChartPanel.MouseMove -= new System.Windows.Input.MouseEventHandler (ChartControl_MouseMove);
				
				if (RenderTarget != null)
					RenderTarget.Transform = Matrix3x2.Identity;
			}
		}

		protected override void OnBarUpdate() { }
		
		void ChartControl_MouseMove (object sender, System.Windows.Input.MouseEventArgs e)
		{
			double curPosX = e.GetPosition(ChartPanel).X;
			double curPosY = e.GetPosition(ChartPanel).Y;
			
			if(curPosX > lastPosX)
				myfloat += 0.1f;
			else if (curPosX < lastPosX)
				myfloat -= 0.1f;
			
			if(curPosY > lastPosY)
			{
				H += 3f;
				W += 3f * Ratio;
			}
			else if (curPosY < lastPosY)
			{
				H -= 3f;
				W -= 3f * Ratio;
			}
			
			lastPosX = curPosX;
			lastPosY = curPosY;
			
			ChartControl.InvalidateVisual();
		}


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
			fileStream = new SharpDX.IO.NativeFileStream(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "SampleDrawBitmap.png"), SharpDX.IO.NativeFileMode.Open, SharpDX.IO.NativeFileAccess.Read);
			// Used to read the image source file.
			bitmapDecoder = new SharpDX.WIC.BitmapDecoder(Core.Globals.WicImagingFactory, fileStream, SharpDX.WIC.DecodeOptions.CacheOnDemand);
			// Get the first frame of the image.
			frame = bitmapDecoder.GetFrame(0);
			// Convert it to a compatible pixel format.			
			converter = new SharpDX.WIC.FormatConverter(Core.Globals.WicImagingFactory);
			converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);
			// Create the new Bitmap1 directly from the FormatConverter.
			myBitmap = SharpDX.Direct2D1.Bitmap.FromWicBitmap(RenderTarget, converter);
			
			if(!HWSet && myBitmap != null)
			{
				H = myBitmap.Size.Height;
				W = myBitmap.Size.Width;
				Ratio = W/H;
				HWSet = true;
			}
		}

		protected override void OnRender(NinjaTrader.Gui.Chart.ChartControl chartControl, NinjaTrader.Gui.Chart.ChartScale chartScale)
		{
			//SetZOrder(int.MaxValue);
			
			//base.OnRender(chartControl, chartScale);
			
			if (RenderTarget == null || Bars == null || Bars.Instrument == null || myBitmap == null)
				return;
			
			if (!IsInHitTest)
			{
				RenderTarget.Transform = Matrix3x2.Rotation(myfloat, new Vector2((float)ChartPanel.W / 2, (float)ChartPanel.H / 2));
				
				RenderTarget.DrawBitmap(myBitmap, new SharpDX.RectangleF((float)ChartPanel.W / 2 - W/2, (float)ChartPanel.H / 2 - H/2, W, H), 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear);
				
				RenderTarget.Transform = Matrix3x2.Identity;
				
				RenderTarget.Transform = Matrix3x2.Rotation(-myfloat, new Vector2((float)ChartPanel.W / 2, (float)ChartPanel.H / 2));
			}
			
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleDrawBitmapFun[] cacheSampleDrawBitmapFun;
		public SampleDrawBitmapFun SampleDrawBitmapFun()
		{
			return SampleDrawBitmapFun(Input);
		}

		public SampleDrawBitmapFun SampleDrawBitmapFun(ISeries<double> input)
		{
			if (cacheSampleDrawBitmapFun != null)
				for (int idx = 0; idx < cacheSampleDrawBitmapFun.Length; idx++)
					if (cacheSampleDrawBitmapFun[idx] != null &&  cacheSampleDrawBitmapFun[idx].EqualsInput(input))
						return cacheSampleDrawBitmapFun[idx];
			return CacheIndicator<SampleDrawBitmapFun>(new SampleDrawBitmapFun(), input, ref cacheSampleDrawBitmapFun);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleDrawBitmapFun SampleDrawBitmapFun()
		{
			return indicator.SampleDrawBitmapFun(Input);
		}

		public Indicators.SampleDrawBitmapFun SampleDrawBitmapFun(ISeries<double> input )
		{
			return indicator.SampleDrawBitmapFun(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleDrawBitmapFun SampleDrawBitmapFun()
		{
			return indicator.SampleDrawBitmapFun(Input);
		}

		public Indicators.SampleDrawBitmapFun SampleDrawBitmapFun(ISeries<double> input )
		{
			return indicator.SampleDrawBitmapFun(input);
		}
	}
}

#endregion
