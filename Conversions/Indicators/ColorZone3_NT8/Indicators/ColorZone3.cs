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
	public class ColorZone3 : Indicator
	{
		private Brush tr = Brushes.Transparent;
		private double wk_zonesize	= .0001;
		
		private double zone1a		= 1;
		private double zone1b		= 1;
		private double zone2a		= 1;
		private double zone2b		= 1;
		private double zone3a		= 1;
		private double zone3b		= 1;
		private double zone4a		= 1;
		private double zone4b		= 1;
		private double zone5a		= 1;
		private double zone5b		= 1;
		private double zone6a		= 1;
		private double zone6b		= 1;
		private double zone7a		= 1;
		private double zone7b		= 1;
		private double zone8a		= 1;
		private double zone8b		= 1;
		private double zone9a		= 1;
		private double zone9b		= 1;
		private double zone10a		= 1;
		private double zone10b		= 1;
		private double zone11a		= 1;
		private double zone11b		= 1;
		private double zone12a		= 1;
		private double zone12b		= 1;
		
		private Series<double> myDataSeries1a;
		private Series<double> myDataSeries1b;
		private Series<double> myDataSeries2a;
		private Series<double> myDataSeries2b;
		private Series<double> myDataSeries3a;
		private Series<double> myDataSeries3b;
		private Series<double> myDataSeries4a;
		private Series<double> myDataSeries4b;
		private Series<double> myDataSeries5a;
		private Series<double> myDataSeries5b;
		private Series<double> myDataSeries6a;
		private Series<double> myDataSeries6b;
		private Series<double> myDataSeries7a;
		private Series<double> myDataSeries7b;
		private Series<double> myDataSeries8a;
		private Series<double> myDataSeries8b;
		private Series<double> myDataSeries9a;
		private Series<double> myDataSeries9b;
		private Series<double> myDataSeries10a;
		private Series<double> myDataSeries10b;
		private Series<double> myDataSeries11a;
		private Series<double> myDataSeries11b;
		private Series<double> myDataSeries12a;
		private Series<double> myDataSeries12b;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"ColorZone3  (Ver 1.5 ... 02/11/11 by photog53 ported to NT8 by NinjaTrader_Jim) Draws rectangles around desired levels...enter Price and get a 3 tick 'zone'.   Allows changes to color, border, & size.";
				Name										= "ColorZone3";
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
				IsAutoScale									= false;
				MaximumBarsLookBack 						= MaximumBarsLookBack.Infinite;
				
				Opacity										= 30;
				Zonesize									= 1;
				
				Zone1										= 1;
				Zone1Clr									= Brushes.LightCoral;
				Zone1ClrBdr									= Brushes.Red;
				Zone1text									= @" ";
				Zone2										= 1;
				Zone2Clr									= Brushes.LightCoral;
				Zone2ClrBdr									= Brushes.Red;
				Zone2text									= @" ";
				Zone3										= 1;
				Zone3Clr									= Brushes.Orange;
				Zone3ClrBdr									= Brushes.DarkOrange;
				Zone3text									= @" ";
				Zone4										= 1;
				Zone4Clr									= Brushes.Yellow;
				Zone4ClrBdr									= Brushes.Gold;
				Zone5										= 1;
				Zone5Clr									= Brushes.LawnGreen;
				Zone5ClrBdr									= Brushes.LimeGreen;
				Zone5text									= @" ";
				Zone6										= 1;
				Zone6Clr									= Brushes.LawnGreen;
				Zone6ClrBdr									= Brushes.LimeGreen;
				Zone6text									= @" ";
				Zone7										= 1;
				Zone7Clr									= Brushes.LawnGreen;
				Zone7ClrBdr									= Brushes.LimeGreen;
				Zone7text									= @" ";
				Zone8										= 1;
				Zone8Clr									= Brushes.LawnGreen;
				Zone8ClrBdr									= Brushes.LimeGreen;
				Zone9										= 1;
				Zone9Clr									= Brushes.LawnGreen;
				Zone9ClrBdr									= Brushes.LimeGreen;
				Zone9text									= @" ";
				Zone10										= 1;
				Zone10Clr									= Brushes.LawnGreen;
				Zone10ClrBdr								= Brushes.LimeGreen;
				Zone10text									= @" ";
				Zone11										= 1;
				Zone11Clr									= Brushes.LawnGreen;
				Zone11ClrBdr								= Brushes.LimeGreen;
				Zone11text									= @" ";
				Zone12										= 1;
				Zone12Clr									= Brushes.LawnGreen;
				Zone12ClrBdr								= Brushes.LimeGreen;
				
				TextColor									= Brushes.White;
				TextFont									= new SimpleFont("Courier", 10);
				TextOffset									= 10;
				TextShift									= 3;
			}
			else if (State == State.Configure)
			{
				myDataSeries1a = new Series<double>(this);
				myDataSeries1b = new Series<double>(this);
				myDataSeries2a = new Series<double>(this);
				myDataSeries2b = new Series<double>(this);
				myDataSeries3a = new Series<double>(this);
				myDataSeries3b = new Series<double>(this);
				myDataSeries4a = new Series<double>(this);
				myDataSeries4b = new Series<double>(this);
				myDataSeries5a = new Series<double>(this);
				myDataSeries5b = new Series<double>(this);
				myDataSeries6a = new Series<double>(this);
				myDataSeries6b = new Series<double>(this);
				myDataSeries7a = new Series<double>(this);
				myDataSeries7b = new Series<double>(this);
				myDataSeries8a = new Series<double>(this);
				myDataSeries8b = new Series<double>(this);
				myDataSeries9a = new Series<double>(this);
				myDataSeries9b = new Series<double>(this);
				myDataSeries10a = new Series<double>(this);
				myDataSeries10b = new Series<double>(this);
				myDataSeries11a = new Series<double>(this);
				myDataSeries11b = new Series<double>(this);
				myDataSeries12a = new Series<double>(this);
				myDataSeries12b = new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{
			wk_zonesize = Zonesize * TickSize;
			
			if (CurrentBar < TextShift)  return;
			
			zone1a = Zone1 - wk_zonesize;
			zone1b = Zone1 + wk_zonesize;
			myDataSeries1a[0] = (zone1a);
			myDataSeries1b[0] = (zone1b);
			Draw.Region(this, "Zone1", CurrentBar, 0, myDataSeries1a, myDataSeries1b, Zone1ClrBdr, Zone1Clr, Opacity );			
			Draw.Text(this, "Zone1text", IsAutoScale, Zone1text, TextShift, zone1a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);

			zone2a = Zone2 - wk_zonesize;
			zone2b = Zone2 + wk_zonesize;
			myDataSeries2a[0] = (zone2a);
			myDataSeries2b[0] = (zone2b);
			Draw.Region(this, "Zone2", CurrentBar, 0, myDataSeries2a, myDataSeries2b, Zone2ClrBdr, Zone2Clr, Opacity);			
			Draw.Text(this, "Zone2text", IsAutoScale, Zone2text, TextShift, zone2a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);

			zone3a = Zone3 - wk_zonesize;
			zone3b = Zone3 + wk_zonesize;
			myDataSeries3a[0] = (zone3a);
			myDataSeries3b[0] = (zone3b);
			Draw.Region(this, "Zone3", CurrentBar, 0, myDataSeries3a, myDataSeries3b, Zone3ClrBdr, Zone3Clr, Opacity);			
			Draw.Text(this, "Zone3text", IsAutoScale, Zone3text, TextShift, zone3a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);

			zone4a = Zone4 - wk_zonesize;
			zone4b = Zone4 + wk_zonesize;
			myDataSeries4a[0] = (zone4a);
			myDataSeries4b[0] = (zone4b);
			Draw.Region(this, "Zone4", CurrentBar, 0, myDataSeries4a, myDataSeries4b, Zone4ClrBdr, Zone4Clr, Opacity);
			Draw.Text(this, "Zone4text", IsAutoScale, Zone4text, TextShift, zone4a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);

			zone5a = Zone5 - wk_zonesize;
			zone5b = Zone5 + wk_zonesize;
			myDataSeries5a[0] = (zone5a);
			myDataSeries5b[0] = (zone5b);
			Draw.Region(this, "Zone5", CurrentBar, 0, myDataSeries5a, myDataSeries5b, Zone5ClrBdr, Zone5Clr, Opacity);
			Draw.Text(this, "Zone5text", IsAutoScale, Zone5text, TextShift, zone5a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);

			zone6a = Zone6 - wk_zonesize;
			zone6b = Zone6 + wk_zonesize;
			myDataSeries6a[0] = (zone6a);
			myDataSeries6b[0] = (zone6b);
			Draw.Region(this, "Zone6", CurrentBar, 0, myDataSeries6a, myDataSeries6b, Zone6ClrBdr, Zone6Clr, Opacity);
			Draw.Text(this, "Zone6text", IsAutoScale, Zone6text, TextShift, zone6a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			
			
			zone7a = Zone7 - wk_zonesize;
			zone7b = Zone7 + wk_zonesize;
			myDataSeries7a[0] = (zone7a);
			myDataSeries7b[0] = (zone7b);
			Draw.Region(this, "Zone7", CurrentBar, 0, myDataSeries7a, myDataSeries7b, Zone7ClrBdr, Zone7Clr, Opacity);
			Draw.Text(this, "Zone7text", IsAutoScale, Zone7text, TextShift, zone7a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			

			zone8a = Zone8 - wk_zonesize;
			zone8b = Zone8 + wk_zonesize;
			myDataSeries8a[0] = (zone8a);
			myDataSeries8b[0] = (zone8b);
			Draw.Region(this, "Zone8", CurrentBar, 0, myDataSeries8a, myDataSeries8b, Zone8ClrBdr, Zone8Clr, Opacity);
			Draw.Text(this, "Zone8text", IsAutoScale, Zone8text, TextShift, zone8a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			

			zone9a = Zone9 - wk_zonesize;
			zone9b = Zone9 + wk_zonesize;
			myDataSeries9a[0] = (zone9a);
			myDataSeries9b[0] = (zone9b);
			Draw.Region(this, "Zone9", CurrentBar, 0, myDataSeries9a, myDataSeries9b, Zone9ClrBdr, Zone9Clr, Opacity);
			Draw.Text(this, "Zone9text", IsAutoScale, Zone9text, TextShift, zone9a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			
			
			zone10a = Zone10 - wk_zonesize;
			zone10b = Zone10 + wk_zonesize;
			myDataSeries10a[0] = (zone10a);
			myDataSeries10b[0] = (zone10b);
			Draw.Region(this, "Zone10", CurrentBar, 0, myDataSeries10a, myDataSeries10b, Zone10ClrBdr, Zone10Clr, Opacity);
			Draw.Text(this, "Zone10text", IsAutoScale, Zone10text, TextShift, zone10a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			

			zone11a = Zone11 - wk_zonesize;
			zone11b = Zone11 + wk_zonesize;
			myDataSeries11a[0] = (zone11a);
			myDataSeries11b[0] = (zone11b);
			Draw.Region(this, "Zone11", CurrentBar, 0, myDataSeries11a, myDataSeries11b, Zone11ClrBdr, Zone11Clr, Opacity);
			Draw.Text(this, "Zone11text", IsAutoScale, Zone11text, TextShift, zone11a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			
			
			zone12a = Zone12 - wk_zonesize;
			zone12b = Zone12 + wk_zonesize;
			myDataSeries12a[0] = (zone12a);
			myDataSeries12b[0] = (zone12b);
			Draw.Region(this,"Zone12", CurrentBar, 0, myDataSeries12a, myDataSeries12b, Zone12ClrBdr, Zone12Clr, Opacity);
			Draw.Text(this,"Zone12text", IsAutoScale, Zone12text, TextShift, zone12a, TextOffset, TextColor, TextFont, TextAlignment.Center, tr,tr, 0);			
        }

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Opacity", Description="Opacity level for zones....10=light   100= dark", Order=1, GroupName="Parameters")]
		public int Opacity
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Zonesize", Description="Zone size....in ticks.  # of Ticks up/down the region goes from the desired price", Order=2, GroupName="Parameters")]
		public int Zonesize
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 1", Description="Zone 1", Order=3, GroupName="Zone 01")]
		public double Zone1
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 1 Color", Description="Color for Zone 1", Order=4, GroupName="Zone 01")]
		public Brush Zone1Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone1ClrSerializable
		{
			get { return Serialize.BrushToString(Zone1Clr); }
			set { Zone1Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 1 Color Border", Description="Zone 1 Color Border", Order=5, GroupName="Zone 01")]
		public Brush Zone1ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone1ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone1ClrBdr); }
			set { Zone1ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 1 Text", Description="Zone 1 Text", Order=6, GroupName="Zone 01")]
		public string Zone1text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 2", Description="Zone 2", Order=7, GroupName="Zone 02")]
		public double Zone2
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 2 Color", Description="Zone 2 Color", Order=8, GroupName="Zone 02")]
		public Brush Zone2Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone2ClrSerializable
		{
			get { return Serialize.BrushToString(Zone2Clr); }
			set { Zone2Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 2 Color Border", Description="Zone 2 Color Border", Order=9, GroupName="Zone 02")]
		public Brush Zone2ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone2ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone2ClrBdr); }
			set { Zone2ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 2 Text", Description="Zone 2 Text", Order=10, GroupName="Zone 02")]
		public string Zone2text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 3", Description="Zone 3", Order=11, GroupName="Zone 03")]
		public double Zone3
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 3 Color", Description="Zone 3 Color", Order=12, GroupName="Zone 03")]
		public Brush Zone3Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone3ClrSerializable
		{
			get { return Serialize.BrushToString(Zone3Clr); }
			set { Zone3Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 3 Color Border", Description="Zone 3 Color Border", Order=13, GroupName="Zone 03")]
		public Brush Zone3ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone3ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone3ClrBdr); }
			set { Zone3ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 3 Text", Description="Zone 3 Text", Order=14, GroupName="Zone 03")]
		public string Zone3text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 4", Description="Zone 4", Order=15, GroupName="Zone 04")]
		public double Zone4
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 4 Color", Description="Zone 4 Color", Order=16, GroupName="Zone 04")]
		public Brush Zone4Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone4ClrSerializable
		{
			get { return Serialize.BrushToString(Zone4Clr); }
			set { Zone4Clr = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Zone 4 Text", Description="Zone 4 Text", Order=18, GroupName="Zone 04")]
		public string Zone4text
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 4 Color Border", Description="Zone 4 Color Border", Order=17, GroupName="Zone 04")]
		public Brush Zone4ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone4ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone4ClrBdr); }
			set { Zone4ClrBdr = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 5", Description="Zone 5", Order=19, GroupName="Zone 05")]
		public double Zone5
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 5 Color", Description="Color for Zone 5", Order=20, GroupName="Zone 05")]
		public Brush Zone5Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone5ClrSerializable
		{
			get { return Serialize.BrushToString(Zone5Clr); }
			set { Zone5Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 5 Color Border", Description="Color for Zone 5 Border", Order=21, GroupName="Zone 05")]
		public Brush Zone5ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone5ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone5ClrBdr); }
			set { Zone5ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 5 Text", Description="Zone 5 Text", Order=22, GroupName="Zone 05")]
		public string Zone5text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 6", Description="Zone 6", Order=23, GroupName="Zone 06")]
		public double Zone6
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 6 Color", Description="Zone 6 Color", Order=24, GroupName="Zone 06")]
		public Brush Zone6Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone6ClrSerializable
		{
			get { return Serialize.BrushToString(Zone6Clr); }
			set { Zone6Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 6 Color Border", Description="Zone 6 Color Border", Order=25, GroupName="Zone 06")]
		public Brush Zone6ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone6ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone6ClrBdr); }
			set { Zone6ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 6 Text", Description="Zone 6 Text", Order=26, GroupName="Zone 06")]
		public string Zone6text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 7", Description="Zone 7", Order=27, GroupName="Zone 07")]
		public double Zone7
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 7 Color", Description="Zone 7 Color", Order=28, GroupName="Zone 07")]
		public Brush Zone7Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone7ClrSerializable
		{
			get { return Serialize.BrushToString(Zone7Clr); }
			set { Zone7Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 7 Color Border", Description="Zone 7 Color Border", Order=29, GroupName="Zone 07")]
		public Brush Zone7ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone7ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone7ClrBdr); }
			set { Zone7ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 7 Text", Description="Zone 7 Text", Order=30, GroupName="Zone 07")]
		public string Zone7text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 8", Description="Zone 8", Order=31, GroupName="Zone 08")]
		public double Zone8
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 8 Color", Description="Zone 8 Color", Order=32, GroupName="Zone 08")]
		public Brush Zone8Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone8ClrSerializable
		{
			get { return Serialize.BrushToString(Zone8Clr); }
			set { Zone8Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 8 Color Border", Description="Zone 8 Color Border", Order=33, GroupName="Zone 08")]
		public Brush Zone8ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone8ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone8ClrBdr); }
			set { Zone8ClrBdr = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Zone 8 Text", Description="Zone 8 Text", Order=34, GroupName="Zone 08")]
		public string Zone8text
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 9", Description="Zone 9", Order=35, GroupName="Zone 09")]
		public double Zone9
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 9 Color", Description="Zone 9 Color", Order=36, GroupName="Zone 09")]
		public Brush Zone9Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone9ClrSerializable
		{
			get { return Serialize.BrushToString(Zone9Clr); }
			set { Zone9Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 9 Color Border", Description="Zone 9 Color Border", Order=37, GroupName="Zone 09")]
		public Brush Zone9ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone9ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone9ClrBdr); }
			set { Zone9ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 9 Text", Description="Zone 9 Text", Order=38, GroupName="Zone 09")]
		public string Zone9text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 10", Description="Zone 10", Order=39, GroupName="Zone 10")]
		public double Zone10
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 10 Color", Description="Zone 10 Color", Order=40, GroupName="Zone 10")]
		public Brush Zone10Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone10ClrSerializable
		{
			get { return Serialize.BrushToString(Zone10Clr); }
			set { Zone10Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 10 Color Border", Description="Zone 10 Color Border", Order=41, GroupName="Zone 10")]
		public Brush Zone10ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone10ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone10ClrBdr); }
			set { Zone10ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 10 Text", Description="Zone 10 Text", Order=42, GroupName="Zone 10")]
		public string Zone10text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 11", Description="Zone 11", Order=43, GroupName="Zone 11")]
		public double Zone11
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 11 Color", Description="Zone 11 Color", Order=44, GroupName="Zone 11")]
		public Brush Zone11Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone11ClrSerializable
		{
			get { return Serialize.BrushToString(Zone11Clr); }
			set { Zone11Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 11 Color Border", Description="Zone 11 Color Border", Order=45, GroupName="Zone 11")]
		public Brush Zone11ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone11ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone11ClrBdr); }
			set { Zone11ClrBdr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Zone 11 Text", Description="Zone 11 Text", Order=46, GroupName="Zone 11")]
		public string Zone11text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(-10, double.MaxValue)]
		[Display(Name="Zone 12", Description="Zone 12", Order=47, GroupName="Zone 12")]
		public double Zone12
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 12 Color", Description="Zone 12 Color", Order=48, GroupName="Zone 12")]
		public Brush Zone12Clr
		{ get; set; }

		[Browsable(false)]
		public string Zone12ClrSerializable
		{
			get { return Serialize.BrushToString(Zone12Clr); }
			set { Zone12Clr = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Zone 12 Color Border", Description="Zone 12 Color Border", Order=49, GroupName="Zone 12")]
		public Brush Zone12ClrBdr
		{ get; set; }

		[Browsable(false)]
		public string Zone12ClrBdrSerializable
		{
			get { return Serialize.BrushToString(Zone12ClrBdr); }
			set { Zone12ClrBdr = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Zone 12 Text", Description="Zone 12 Text", Order=50, GroupName="Zone 12")]
		public string Zone12text
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Text Color", Description="Text Color", Order=51, GroupName="Parameters")]
		public Brush TextColor
		{ get; set; }

		[Browsable(false)]
		public string TextColorSerializable
		{
			get { return Serialize.BrushToString(TextColor); }
			set { TextColor = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Text Font", Description="Text Font", Order=52, GroupName="Parameters")]
		public SimpleFont TextFont
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Text Offset", Description="Text Offset", Order=53, GroupName="Parameters")]
		public int TextOffset
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Text Shift", Description="Left/Right (in bars)", Order=54, GroupName="Parameters")]
		public int TextShift
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ColorZone3[] cacheColorZone3;
		public ColorZone3 ColorZone3(int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			return ColorZone3(Input, opacity, zonesize, zone1, zone1Clr, zone1ClrBdr, zone1text, zone2, zone2Clr, zone2ClrBdr, zone2text, zone3, zone3Clr, zone3ClrBdr, zone3text, zone4, zone4Clr, zone4text, zone4ClrBdr, zone5, zone5Clr, zone5ClrBdr, zone5text, zone6, zone6Clr, zone6ClrBdr, zone6text, zone7, zone7Clr, zone7ClrBdr, zone7text, zone8, zone8Clr, zone8ClrBdr, zone8text, zone9, zone9Clr, zone9ClrBdr, zone9text, zone10, zone10Clr, zone10ClrBdr, zone10text, zone11, zone11Clr, zone11ClrBdr, zone11text, zone12, zone12Clr, zone12ClrBdr, zone12text, textColor, textFont, textOffset, textShift);
		}

		public ColorZone3 ColorZone3(ISeries<double> input, int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			if (cacheColorZone3 != null)
				for (int idx = 0; idx < cacheColorZone3.Length; idx++)
					if (cacheColorZone3[idx] != null && cacheColorZone3[idx].Opacity == opacity && cacheColorZone3[idx].Zonesize == zonesize && cacheColorZone3[idx].Zone1 == zone1 && cacheColorZone3[idx].Zone1Clr == zone1Clr && cacheColorZone3[idx].Zone1ClrBdr == zone1ClrBdr && cacheColorZone3[idx].Zone1text == zone1text && cacheColorZone3[idx].Zone2 == zone2 && cacheColorZone3[idx].Zone2Clr == zone2Clr && cacheColorZone3[idx].Zone2ClrBdr == zone2ClrBdr && cacheColorZone3[idx].Zone2text == zone2text && cacheColorZone3[idx].Zone3 == zone3 && cacheColorZone3[idx].Zone3Clr == zone3Clr && cacheColorZone3[idx].Zone3ClrBdr == zone3ClrBdr && cacheColorZone3[idx].Zone3text == zone3text && cacheColorZone3[idx].Zone4 == zone4 && cacheColorZone3[idx].Zone4Clr == zone4Clr && cacheColorZone3[idx].Zone4text == zone4text && cacheColorZone3[idx].Zone4ClrBdr == zone4ClrBdr && cacheColorZone3[idx].Zone5 == zone5 && cacheColorZone3[idx].Zone5Clr == zone5Clr && cacheColorZone3[idx].Zone5ClrBdr == zone5ClrBdr && cacheColorZone3[idx].Zone5text == zone5text && cacheColorZone3[idx].Zone6 == zone6 && cacheColorZone3[idx].Zone6Clr == zone6Clr && cacheColorZone3[idx].Zone6ClrBdr == zone6ClrBdr && cacheColorZone3[idx].Zone6text == zone6text && cacheColorZone3[idx].Zone7 == zone7 && cacheColorZone3[idx].Zone7Clr == zone7Clr && cacheColorZone3[idx].Zone7ClrBdr == zone7ClrBdr && cacheColorZone3[idx].Zone7text == zone7text && cacheColorZone3[idx].Zone8 == zone8 && cacheColorZone3[idx].Zone8Clr == zone8Clr && cacheColorZone3[idx].Zone8ClrBdr == zone8ClrBdr && cacheColorZone3[idx].Zone8text == zone8text && cacheColorZone3[idx].Zone9 == zone9 && cacheColorZone3[idx].Zone9Clr == zone9Clr && cacheColorZone3[idx].Zone9ClrBdr == zone9ClrBdr && cacheColorZone3[idx].Zone9text == zone9text && cacheColorZone3[idx].Zone10 == zone10 && cacheColorZone3[idx].Zone10Clr == zone10Clr && cacheColorZone3[idx].Zone10ClrBdr == zone10ClrBdr && cacheColorZone3[idx].Zone10text == zone10text && cacheColorZone3[idx].Zone11 == zone11 && cacheColorZone3[idx].Zone11Clr == zone11Clr && cacheColorZone3[idx].Zone11ClrBdr == zone11ClrBdr && cacheColorZone3[idx].Zone11text == zone11text && cacheColorZone3[idx].Zone12 == zone12 && cacheColorZone3[idx].Zone12Clr == zone12Clr && cacheColorZone3[idx].Zone12ClrBdr == zone12ClrBdr && cacheColorZone3[idx].Zone12text == zone12text && cacheColorZone3[idx].TextColor == textColor && cacheColorZone3[idx].TextFont == textFont && cacheColorZone3[idx].TextOffset == textOffset && cacheColorZone3[idx].TextShift == textShift && cacheColorZone3[idx].EqualsInput(input))
						return cacheColorZone3[idx];
			return CacheIndicator<ColorZone3>(new ColorZone3(){ Opacity = opacity, Zonesize = zonesize, Zone1 = zone1, Zone1Clr = zone1Clr, Zone1ClrBdr = zone1ClrBdr, Zone1text = zone1text, Zone2 = zone2, Zone2Clr = zone2Clr, Zone2ClrBdr = zone2ClrBdr, Zone2text = zone2text, Zone3 = zone3, Zone3Clr = zone3Clr, Zone3ClrBdr = zone3ClrBdr, Zone3text = zone3text, Zone4 = zone4, Zone4Clr = zone4Clr, Zone4text = zone4text, Zone4ClrBdr = zone4ClrBdr, Zone5 = zone5, Zone5Clr = zone5Clr, Zone5ClrBdr = zone5ClrBdr, Zone5text = zone5text, Zone6 = zone6, Zone6Clr = zone6Clr, Zone6ClrBdr = zone6ClrBdr, Zone6text = zone6text, Zone7 = zone7, Zone7Clr = zone7Clr, Zone7ClrBdr = zone7ClrBdr, Zone7text = zone7text, Zone8 = zone8, Zone8Clr = zone8Clr, Zone8ClrBdr = zone8ClrBdr, Zone8text = zone8text, Zone9 = zone9, Zone9Clr = zone9Clr, Zone9ClrBdr = zone9ClrBdr, Zone9text = zone9text, Zone10 = zone10, Zone10Clr = zone10Clr, Zone10ClrBdr = zone10ClrBdr, Zone10text = zone10text, Zone11 = zone11, Zone11Clr = zone11Clr, Zone11ClrBdr = zone11ClrBdr, Zone11text = zone11text, Zone12 = zone12, Zone12Clr = zone12Clr, Zone12ClrBdr = zone12ClrBdr, Zone12text = zone12text, TextColor = textColor, TextFont = textFont, TextOffset = textOffset, TextShift = textShift }, input, ref cacheColorZone3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ColorZone3 ColorZone3(int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			return indicator.ColorZone3(Input, opacity, zonesize, zone1, zone1Clr, zone1ClrBdr, zone1text, zone2, zone2Clr, zone2ClrBdr, zone2text, zone3, zone3Clr, zone3ClrBdr, zone3text, zone4, zone4Clr, zone4text, zone4ClrBdr, zone5, zone5Clr, zone5ClrBdr, zone5text, zone6, zone6Clr, zone6ClrBdr, zone6text, zone7, zone7Clr, zone7ClrBdr, zone7text, zone8, zone8Clr, zone8ClrBdr, zone8text, zone9, zone9Clr, zone9ClrBdr, zone9text, zone10, zone10Clr, zone10ClrBdr, zone10text, zone11, zone11Clr, zone11ClrBdr, zone11text, zone12, zone12Clr, zone12ClrBdr, zone12text, textColor, textFont, textOffset, textShift);
		}

		public Indicators.ColorZone3 ColorZone3(ISeries<double> input , int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			return indicator.ColorZone3(input, opacity, zonesize, zone1, zone1Clr, zone1ClrBdr, zone1text, zone2, zone2Clr, zone2ClrBdr, zone2text, zone3, zone3Clr, zone3ClrBdr, zone3text, zone4, zone4Clr, zone4text, zone4ClrBdr, zone5, zone5Clr, zone5ClrBdr, zone5text, zone6, zone6Clr, zone6ClrBdr, zone6text, zone7, zone7Clr, zone7ClrBdr, zone7text, zone8, zone8Clr, zone8ClrBdr, zone8text, zone9, zone9Clr, zone9ClrBdr, zone9text, zone10, zone10Clr, zone10ClrBdr, zone10text, zone11, zone11Clr, zone11ClrBdr, zone11text, zone12, zone12Clr, zone12ClrBdr, zone12text, textColor, textFont, textOffset, textShift);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ColorZone3 ColorZone3(int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			return indicator.ColorZone3(Input, opacity, zonesize, zone1, zone1Clr, zone1ClrBdr, zone1text, zone2, zone2Clr, zone2ClrBdr, zone2text, zone3, zone3Clr, zone3ClrBdr, zone3text, zone4, zone4Clr, zone4text, zone4ClrBdr, zone5, zone5Clr, zone5ClrBdr, zone5text, zone6, zone6Clr, zone6ClrBdr, zone6text, zone7, zone7Clr, zone7ClrBdr, zone7text, zone8, zone8Clr, zone8ClrBdr, zone8text, zone9, zone9Clr, zone9ClrBdr, zone9text, zone10, zone10Clr, zone10ClrBdr, zone10text, zone11, zone11Clr, zone11ClrBdr, zone11text, zone12, zone12Clr, zone12ClrBdr, zone12text, textColor, textFont, textOffset, textShift);
		}

		public Indicators.ColorZone3 ColorZone3(ISeries<double> input , int opacity, int zonesize, double zone1, Brush zone1Clr, Brush zone1ClrBdr, string zone1text, double zone2, Brush zone2Clr, Brush zone2ClrBdr, string zone2text, double zone3, Brush zone3Clr, Brush zone3ClrBdr, string zone3text, double zone4, Brush zone4Clr, string zone4text, Brush zone4ClrBdr, double zone5, Brush zone5Clr, Brush zone5ClrBdr, string zone5text, double zone6, Brush zone6Clr, Brush zone6ClrBdr, string zone6text, double zone7, Brush zone7Clr, Brush zone7ClrBdr, string zone7text, double zone8, Brush zone8Clr, Brush zone8ClrBdr, string zone8text, double zone9, Brush zone9Clr, Brush zone9ClrBdr, string zone9text, double zone10, Brush zone10Clr, Brush zone10ClrBdr, string zone10text, double zone11, Brush zone11Clr, Brush zone11ClrBdr, string zone11text, double zone12, Brush zone12Clr, Brush zone12ClrBdr, string zone12text, Brush textColor, SimpleFont textFont, int textOffset, int textShift)
		{
			return indicator.ColorZone3(input, opacity, zonesize, zone1, zone1Clr, zone1ClrBdr, zone1text, zone2, zone2Clr, zone2ClrBdr, zone2text, zone3, zone3Clr, zone3ClrBdr, zone3text, zone4, zone4Clr, zone4text, zone4ClrBdr, zone5, zone5Clr, zone5ClrBdr, zone5text, zone6, zone6Clr, zone6ClrBdr, zone6text, zone7, zone7Clr, zone7ClrBdr, zone7text, zone8, zone8Clr, zone8ClrBdr, zone8text, zone9, zone9Clr, zone9ClrBdr, zone9text, zone10, zone10Clr, zone10ClrBdr, zone10text, zone11, zone11Clr, zone11ClrBdr, zone11text, zone12, zone12Clr, zone12ClrBdr, zone12text, textColor, textFont, textOffset, textShift);
		}
	}
}

#endregion
