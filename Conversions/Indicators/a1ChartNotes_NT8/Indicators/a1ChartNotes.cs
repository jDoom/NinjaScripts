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

using System.Text.RegularExpressions;
using System.Collections;				
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;				//Required for 'UserControl' structure
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{	
	public class a1ChartNotes : Indicator
	{
		private List<IndiChartNotes.StringWrapper> note1CollectionDefaults = new List<IndiChartNotes.StringWrapper>();
		private List<IndiChartNotes.StringWrapper> note2CollectionDefaults = new List<IndiChartNotes.StringWrapper>();
		private List<IndiChartNotes.StringWrapper> note3CollectionDefaults = new List<IndiChartNotes.StringWrapper>();
		private List<IndiChartNotes.StringWrapper> note4CollectionDefaults = new List<IndiChartNotes.StringWrapper>();
		private List<IndiChartNotes.StringWrapper> note5CollectionDefaults = new List<IndiChartNotes.StringWrapper>();
		
		private Collection<IndiChartNotes.StringWrapper> note1Text;
		private Collection<IndiChartNotes.StringWrapper> note2Text;
		private Collection<IndiChartNotes.StringWrapper> note3Text;
		private Collection<IndiChartNotes.StringWrapper> note4Text;
		private Collection<IndiChartNotes.StringWrapper> note5Text;
		
		private string str=""; 
		private int i=0, lines=1, lastFromIndex, lastToIndex;
		
		// Use our own timer to refresh CompactMode instead of OnRender() to work around 250ms delay
		private Timer 	timerToCheck 		= new Timer();
		private Timer 	timerToWait 		= new Timer();
		private bool 	timerToWaitStarted 	= false;
		
		public override string DisplayName
		{
			get { return Name; }
		}
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "a1ChartNotes";
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
				
				ExpandedLines								= 100;
				CompactLines								= 2;
				Compact										= false;
				// Note 1
				Note1Text 									= new Collection<IndiChartNotes.StringWrapper>(note1CollectionDefaults);
				Note1Location								= TextPosition.TopLeft;
				Note1Color									= Brushes.RoyalBlue;
				Note1Font									= new SimpleFont("Arial", 10);
				// Note 2
				Note2Text 									= new Collection<IndiChartNotes.StringWrapper>(note2CollectionDefaults);
				Note2Location								= TextPosition.TopRight;
				Note2Color									= Brushes.RoyalBlue;
				Note2Font									= new SimpleFont("Arial", 10);
				// Note 3
				Note3Text 									= new Collection<IndiChartNotes.StringWrapper>(note3CollectionDefaults);
				Note3Location								= TextPosition.BottomLeft;
				Note3Color									= Brushes.RoyalBlue;
				Note3Font									= new SimpleFont("Arial", 10);
				// Note 4
				Note4Text 									= new Collection<IndiChartNotes.StringWrapper>(note4CollectionDefaults);
				Note4Location								= TextPosition.BottomRight;
				Note4Color									= Brushes.RoyalBlue;
				Note4Font									= new SimpleFont("Arial", 10);
				// Note 5
				Note5Text 									= new Collection<IndiChartNotes.StringWrapper>(note5CollectionDefaults);
				Note5Location								= TextPosition.Center;
				Note5Color									= Brushes.RoyalBlue;
				Note5Font									= new SimpleFont("Arial", 10);
			}
			else if (State == State.Configure)
			{
				timerToCheck.Tick += new EventHandler(TimerToCheckEventProcessor);
				timerToCheck.Interval = 10;
				timerToCheck.Start();
				
				timerToWait.Tick += new EventHandler(TimerToWaitEventProcessor);
				timerToWait.Interval = 250;
			}
			else if (State == State.DataLoaded)
			{
				display_notes();			
			}
			else if (State == State.Terminated)
			{
				timerToCheck.Dispose();
				timerToWait.Dispose();
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		
		private void TimerToWaitEventProcessor(Object myObject, EventArgs myEventArgs)
		{
			timerToWait.Stop();
			timerToWaitStarted = false;
		}
		
		private void TimerToCheckEventProcessor(Object myObject, EventArgs myEventArgs)
		{
			if ( Regex.Match(UserControl.ModifierKeys.ToString(), "(?=.*CONTROL)(?=.*SHIFT)",RegexOptions.IgnoreCase).Success && !timerToWaitStarted ) 
			{
				Compact = (Compact)?false:true;		//### Toggle compact mode
				display_notes();
				
				timerToWait.Start();
				timerToWaitStarted = true;
				
				ChartControl.Dispatcher.InvokeAsync(new Action(() => 
				{
					ChartControl.InvalidateVisual(); // Unsupported, but required to make sure the change for Compact mode is seen immediately.
				}));
			}
		}
		
		private void display_notes() 
		{
			int lines=0;
			
			if ( Note1Text.Count > 0 ) 
			{
				lines = Math.Min((Compact? CompactLines : ExpandedLines),Note1Text.Count);
				for ( i=0, str=""; i < lines; i++ ) str += Note1Text[i] +"\n";
				Draw.TextFixed(this, "TopLeft", str, Note1Location, Note1Color, Note1Font, Brushes.Transparent, Brushes.Transparent, 100);
			}
			if ( Note2Text.Count > 0 ) 
			{
				lines = Math.Min((Compact? CompactLines : ExpandedLines),Note2Text.Count);
				for ( i=0, str=""; i < lines; i++ ) str += Note2Text[i] +"\n";
				Draw.TextFixed(this, "TopRight", str, Note2Location, Note2Color, Note2Font, Brushes.Transparent, Brushes.Transparent, 100);
			}
			if ( Note3Text.Count > 0 ) 
			{
				lines = Math.Min((Compact? CompactLines : ExpandedLines),Note3Text.Count);
				for ( i=0, str=""; i < lines; i++ ) str += Note3Text[i] +"\n";
				Draw.TextFixed(this, "BottomLeft", str, Note3Location, Note3Color, Note3Font, Brushes.Transparent, Brushes.Transparent, 100);
			}
			if ( Note4Text.Count > 0 ) 
			{
				lines = Math.Min((Compact? CompactLines : ExpandedLines),Note4Text.Count);
				for ( i=0, str=""; i < lines; i++ ) str += Note4Text[i] +"\n";
				Draw.TextFixed(this, "BottomRight", str, Note4Location, Note4Color, Note4Font, Brushes.Transparent, Brushes.Transparent, 100);
			}
			if ( Note5Text.Count > 0 ) 
			{
				lines = Math.Min((Compact? CompactLines : ExpandedLines),Note5Text.Count);
				for ( i=0, str=""; i < lines; i++ ) str += Note5Text[i] +"\n";
				Draw.TextFixed(this, "Center", str, Note5Location, Note5Color, Note5Font, Brushes.Transparent, Brushes.Transparent, 100);
			}
		}
		
		#region Reflection to copy Collections to new assembly
		public override void CopyTo(NinjaScript ninjaScript)
		{
			base.CopyTo(ninjaScript);
			Type			newInstType				= ninjaScript.GetType();
			
			// Loop through each Collection
			for (int i = 1; i <= 5; i++) 
			{
				PropertyInfo	noteTextPropertyInfo	= newInstType.GetProperty("Note"+i+"Text");
				Collection<IndiChartNotes.StringWrapper> NoteText;
				
				switch (i) {
					case 1:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note1Text);
						break;
					case 2:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note2Text);
						break;
					case 3:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note3Text);
						break;
					case 4:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note4Text);
						break;
					case 5:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note5Text);
						break;
					default:
						NoteText = new Collection<IndiChartNotes.StringWrapper>(Note1Text);
						break;
				}
				
				// Reflect Note from old assembly
				if (noteTextPropertyInfo != null)
				{
					IList newInstNoteText = noteTextPropertyInfo.GetValue(ninjaScript) as IList;
					if (newInstNoteText != null)
					{
						// Since new instance could be past set defaults, clear any existing
						newInstNoteText.Clear();
						foreach (IndiChartNotes.StringWrapper oldStringWrapper in NoteText)
						{
							try
							{
								object newInstance = oldStringWrapper.AssemblyClone(Core.Globals.AssemblyRegistry.GetType(typeof(IndiChartNotes.StringWrapper).FullName));
								if (newInstance == null)
									continue;
								
								newInstNoteText.Add(newInstance);
							}
							catch { }
						}
					}
				}	
			}
		}
		#endregion
		
		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ExpandedLines", Description="Expanded Lines", Order=1, GroupName="Parameters")]
		public int ExpandedLines
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CompactLines", Description="Compact Lines", Order=2, GroupName="Parameters")]
		public int CompactLines
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Compact", Description="Compact Lines Mode", Order=3, GroupName="Parameters")]
		public bool Compact
		{ get; set; }
		
		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Note 1 Text", Order=4, GroupName="Note 1" , Prompt = "1 string|{0} strings|Add string...|Edit string...|Edit strings...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiChartNotes.StringWrapper> Note1Text
        {
			get 
			{
				return note1Text;
			}
			set
			{
				note1Text = new Collection<IndiChartNotes.StringWrapper>(value.ToList());
			}
		}
		
		// Serializer for the StringWrapper's Collection
        [Browsable(false)]
        public Collection<IndiChartNotes.StringWrapper> Note1TextSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiChartNotes.StringWrapper sw in note1CollectionDefaults.ToList())
				{
					IndiChartNotes.StringWrapper temp = Note1Text.FirstOrDefault(p => p.StringValue == sw.StringValue && p.IsDefault == true);
					if(temp != null)
						note1CollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				Note1Text.All(p => p.IsDefault = false);
				
				return Note1Text;
			}
			set
			{
				Note1Text = value;
			}
        }

		[NinjaScriptProperty]
		[Display(Name="Note1Location", Description="Note 1 Location", Order=5, GroupName="Note 1")]
		public TextPosition Note1Location
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Note1Color", Description="Note 1 Color", Order=6, GroupName="Note 1")]
		public Brush Note1Color
		{ get; set; }

		[Browsable(false)]
		public string Note1ColorSerializable
		{
			get { return Serialize.BrushToString(Note1Color); }
			set { Note1Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Note1Font", Description="Note1Font", Order=7, GroupName="Note 1")]
		public SimpleFont Note1Font
		{ get; set; }
		
		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Note 2 Text", Order=8, GroupName="Note 2" , Prompt = "1 string|{0} strings|Add string...|Edit string...|Edit strings...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiChartNotes.StringWrapper> Note2Text
        {
			get 
			{
				return note2Text;
			}
			set
			{
				note2Text = new Collection<IndiChartNotes.StringWrapper>(value.ToList());
			}
		}
		
		// Serializer for the StringWrapper's Collection
        [Browsable(false)]
        public Collection<IndiChartNotes.StringWrapper> Note2TextSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiChartNotes.StringWrapper sw in note2CollectionDefaults.ToList())
				{
					IndiChartNotes.StringWrapper temp = Note2Text.FirstOrDefault(p => p.StringValue == sw.StringValue && p.IsDefault == true);
					if(temp != null)
						note2CollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				Note2Text.All(p => p.IsDefault = false);
				
				return Note2Text;
			}
			set
			{
				Note2Text = value;
			}
        }

		[NinjaScriptProperty]
		[Display(Name="Note2Location", Description="Note 2 Location", Order=9, GroupName="Note 2")]
		public TextPosition Note2Location
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Note2Color", Description="Note 2 Color", Order=10, GroupName="Note 2")]
		public Brush Note2Color
		{ get; set; }

		[Browsable(false)]
		public string Note2ColorSerializable
		{
			get { return Serialize.BrushToString(Note2Color); }
			set { Note2Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Note2Font", Description="Note 2 Font", Order=11, GroupName="Note 2")]
		public SimpleFont Note2Font
		{ get; set; }
		
		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Note 3 Text", Order=12, GroupName="Note 3" , Prompt = "1 string|{0} strings|Add string...|Edit string...|Edit strings...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiChartNotes.StringWrapper> Note3Text
        {
			get 
			{
				return note3Text;
			}
			set
			{
				note3Text = new Collection<IndiChartNotes.StringWrapper>(value.ToList());
			}
		}
		
		// Serializer for the StringWrapper's Collection
        [Browsable(false)]
        public Collection<IndiChartNotes.StringWrapper> Note3TextSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiChartNotes.StringWrapper sw in note3CollectionDefaults.ToList())
				{
					IndiChartNotes.StringWrapper temp = Note3Text.FirstOrDefault(p => p.StringValue == sw.StringValue && p.IsDefault == true);
					if(temp != null)
						note3CollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				Note3Text.All(p => p.IsDefault = false);
				
				return Note3Text;
			}
			set
			{
				Note3Text = value;
			}
        }

		[NinjaScriptProperty]
		[Display(Name="Note3Location", Description="Note 3 Location", Order=13, GroupName="Note 3")]
		public TextPosition Note3Location
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Note3Color", Description="Note 3 Color", Order=14, GroupName="Note 3")]
		public Brush Note3Color
		{ get; set; }

		[Browsable(false)]
		public string Note3ColorSerializable
		{
			get { return Serialize.BrushToString(Note3Color); }
			set { Note3Color = Serialize.StringToBrush(value); }
		}
		
		[NinjaScriptProperty]
		[Display(Name="Note3Font", Description="Note 3 Font", Order=15, GroupName="Note 3")]
		public SimpleFont Note3Font
		{ get; set; }

		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Note 4 Text", Order=16, GroupName="Note 4" , Prompt = "1 string|{0} strings|Add string...|Edit string...|Edit strings...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiChartNotes.StringWrapper> Note4Text
        {
			get 
			{
				return note4Text;
			}
			set
			{
				note4Text = new Collection<IndiChartNotes.StringWrapper>(value.ToList());
			}
		}
		
		// Serializer for the StringWrapper's Collection
        [Browsable(false)]
        public Collection<IndiChartNotes.StringWrapper> Note4TextSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiChartNotes.StringWrapper sw in note4CollectionDefaults.ToList())
				{
					IndiChartNotes.StringWrapper temp = Note4Text.FirstOrDefault(p => p.StringValue == sw.StringValue && p.IsDefault == true);
					if(temp != null)
						note4CollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				Note4Text.All(p => p.IsDefault = false);
				
				return Note4Text;
			}
			set
			{
				Note4Text = value;
			}
        }
		

		[NinjaScriptProperty]
		[Display(Name="Note4Location", Description="Note 4 Location", Order=17, GroupName="Note 4")]
		public TextPosition Note4Location
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Note4Color", Description="Note 4 Color", Order=18, GroupName="Note 4")]
		public Brush Note4Color
		{ get; set; }

		[Browsable(false)]
		public string Note4ColorSerializable
		{
			get { return Serialize.BrushToString(Note4Color); }
			set { Note4Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Note4Font", Description="Note 4 Font", Order=19, GroupName="Note 4")]
		public SimpleFont Note4Font
		{ get; set; }

		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Note 5 Text", Order=20, GroupName="Note 5" , Prompt = "1 string|{0} strings|Add string...|Edit string...|Edit strings...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiChartNotes.StringWrapper> Note5Text
        {
			get 
			{
				return note5Text;
			}
			set
			{
				note5Text = new Collection<IndiChartNotes.StringWrapper>(value.ToList());
			}
		}
		
		// Serializer for the StringWrapper's Collection
        [Browsable(false)]
        public Collection<IndiChartNotes.StringWrapper> Note5TextSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiChartNotes.StringWrapper sw in note5CollectionDefaults.ToList())
				{
					IndiChartNotes.StringWrapper temp = Note5Text.FirstOrDefault(p => p.StringValue == sw.StringValue && p.IsDefault == true);
					if(temp != null)
						note5CollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				Note5Text.All(p => p.IsDefault = false);
				
				return Note5Text;
			}
			set
			{
				Note5Text = value;
			}
        }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Note5Location", Description="Note 5 Location", Order=21, GroupName="Note 5")]
		public TextPosition Note5Location
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="Note5Color", Description="Note 5 Color", Order=22, GroupName="Note 5")]
		public Brush Note5Color
		{ get; set; }

		[Browsable(false)]
		public string Note5ColorSerializable
		{
			get { return Serialize.BrushToString(Note5Color); }
			set { Note5Color = Serialize.StringToBrush(value); }
		}			

		[NinjaScriptProperty]
		[Display(Name="Note5Font", Description="Note 5 Font", Order=23, GroupName="Note 5")]
		public SimpleFont Note5Font
		{ get; set; }
		

		#endregion

	}

}

#region StringWrapper
namespace IndiChartNotes
{
	[CategoryDefaultExpanded(true)]
    public class StringWrapper : NotifyPropertyChangedBase, ICloneable
    {
        // Parameterless constructor is needed for Clone and serialization
        public StringWrapper() : this(string.Empty)
        {
        }

        public StringWrapper(string value)
        {
            StringValue = value;
        }

        // Display attributes, XmlIgnore attributes, Browsable attributes, etc can be all applied to the object's properties as well.
        [Display(Name = "Messages", GroupName = "Messages")]
        public string StringValue
        { get; set; }

        // Cloned instance returned to the Collection editor with user defined value

		public object Clone()
		{
			StringWrapper p = new StringWrapper();
			p.StringValue = StringValue;
			return p;
		}
		
		//Default value handling
		[Browsable(false)]
		public bool IsDefault { get; set; }
		
        // Customize the displays on the left side of the editor window
        public override string ToString()
        { return StringValue; }
		
		// Use Reflection to be able to copy properties to new instance
		public object AssemblyClone(Type t)
		{
			Assembly a 			= t.Assembly;
			object noteText 	= a.CreateInstance(t.FullName);
			
			foreach (PropertyInfo p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				if (p.CanWrite)
				{
					p.SetValue(noteText, this.GetType().GetProperty(p.Name).GetValue(this), null);
				}
			}
			
			return noteText;
		}
    }
	
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private a1ChartNotes[] cachea1ChartNotes;
		public a1ChartNotes a1ChartNotes(int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			return a1ChartNotes(Input, expandedLines, compactLines, compact, note1Location, note1Color, note1Font, note2Location, note2Color, note2Font, note3Location, note3Color, note3Font, note4Location, note4Color, note4Font, note5Location, note5Color, note5Font);
		}

		public a1ChartNotes a1ChartNotes(ISeries<double> input, int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			if (cachea1ChartNotes != null)
				for (int idx = 0; idx < cachea1ChartNotes.Length; idx++)
					if (cachea1ChartNotes[idx] != null && cachea1ChartNotes[idx].ExpandedLines == expandedLines && cachea1ChartNotes[idx].CompactLines == compactLines && cachea1ChartNotes[idx].Compact == compact && cachea1ChartNotes[idx].Note1Location == note1Location && cachea1ChartNotes[idx].Note1Color == note1Color && cachea1ChartNotes[idx].Note1Font == note1Font && cachea1ChartNotes[idx].Note2Location == note2Location && cachea1ChartNotes[idx].Note2Color == note2Color && cachea1ChartNotes[idx].Note2Font == note2Font && cachea1ChartNotes[idx].Note3Location == note3Location && cachea1ChartNotes[idx].Note3Color == note3Color && cachea1ChartNotes[idx].Note3Font == note3Font && cachea1ChartNotes[idx].Note4Location == note4Location && cachea1ChartNotes[idx].Note4Color == note4Color && cachea1ChartNotes[idx].Note4Font == note4Font && cachea1ChartNotes[idx].Note5Location == note5Location && cachea1ChartNotes[idx].Note5Color == note5Color && cachea1ChartNotes[idx].Note5Font == note5Font && cachea1ChartNotes[idx].EqualsInput(input))
						return cachea1ChartNotes[idx];
			return CacheIndicator<a1ChartNotes>(new a1ChartNotes(){ ExpandedLines = expandedLines, CompactLines = compactLines, Compact = compact, Note1Location = note1Location, Note1Color = note1Color, Note1Font = note1Font, Note2Location = note2Location, Note2Color = note2Color, Note2Font = note2Font, Note3Location = note3Location, Note3Color = note3Color, Note3Font = note3Font, Note4Location = note4Location, Note4Color = note4Color, Note4Font = note4Font, Note5Location = note5Location, Note5Color = note5Color, Note5Font = note5Font }, input, ref cachea1ChartNotes);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.a1ChartNotes a1ChartNotes(int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			return indicator.a1ChartNotes(Input, expandedLines, compactLines, compact, note1Location, note1Color, note1Font, note2Location, note2Color, note2Font, note3Location, note3Color, note3Font, note4Location, note4Color, note4Font, note5Location, note5Color, note5Font);
		}

		public Indicators.a1ChartNotes a1ChartNotes(ISeries<double> input , int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			return indicator.a1ChartNotes(input, expandedLines, compactLines, compact, note1Location, note1Color, note1Font, note2Location, note2Color, note2Font, note3Location, note3Color, note3Font, note4Location, note4Color, note4Font, note5Location, note5Color, note5Font);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.a1ChartNotes a1ChartNotes(int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			return indicator.a1ChartNotes(Input, expandedLines, compactLines, compact, note1Location, note1Color, note1Font, note2Location, note2Color, note2Font, note3Location, note3Color, note3Font, note4Location, note4Color, note4Font, note5Location, note5Color, note5Font);
		}

		public Indicators.a1ChartNotes a1ChartNotes(ISeries<double> input , int expandedLines, int compactLines, bool compact, TextPosition note1Location, Brush note1Color, SimpleFont note1Font, TextPosition note2Location, Brush note2Color, SimpleFont note2Font, TextPosition note3Location, Brush note3Color, SimpleFont note3Font, TextPosition note4Location, Brush note4Color, SimpleFont note4Font, TextPosition note5Location, Brush note5Color, SimpleFont note5Font)
		{
			return indicator.a1ChartNotes(input, expandedLines, compactLines, compact, note1Location, note1Color, note1Font, note2Location, note2Color, note2Font, note3Location, note3Color, note3Font, note4Location, note4Color, note4Font, note5Location, note5Color, note5Font);
		}
	}
}

#endregion
