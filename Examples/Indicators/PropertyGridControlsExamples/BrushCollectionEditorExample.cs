//
// Copyright (C) 2021, NinjaTrader LLC <www.ninjatrader.com>
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component
// Coded by NinjaTrader_Jim
//
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
using System.Windows.Forms;             //Required for 'UserControl' structure
#endregion

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.PropertyGridControlsExamples
{	
	public class BrushCollectionEditorExample : Indicator
	{
		private List<IndiBrushCollection.BrushWrapper> brushCollectionDefaults = new List<IndiBrushCollection.BrushWrapper>();
		
		private Collection<IndiBrushCollection.BrushWrapper> brushCollection;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BrushCollectionEditorExample";
				Calculate									= Calculate.OnBarClose;

				// Note 1
				BrushCollection 							= new Collection<IndiBrushCollection.BrushWrapper>(brushCollectionDefaults);
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
				PropertyInfo	brushCollectionPropertyInfo	= newInstType.GetProperty("BrushCollection");
				Collection<IndiBrushCollection.BrushWrapper> CopyToBrushCollection;
				
				CopyToBrushCollection = new Collection<IndiBrushCollection.BrushWrapper>(BrushCollection);
				
				// Reflect Note from old assembly
				if (brushCollectionPropertyInfo != null)
				{
					IList newInstBrushCollection = brushCollectionPropertyInfo.GetValue(ninjaScript) as IList;
					if (newInstBrushCollection != null)
					{
						// Since new instance could be past set defaults, clear any existing
						newInstBrushCollection.Clear();
						foreach (IndiBrushCollection.BrushWrapper oldBrushWrapper in CopyToBrushCollection)
						{
							try
							{
								object newInstance = oldBrushWrapper.AssemblyClone(Core.Globals.AssemblyRegistry.GetType(typeof(IndiBrushCollection.BrushWrapper).FullName));
								if (newInstance == null)
									continue;
								
								newInstBrushCollection.Add(newInstance);
							}
							catch { }
						}
					}
				}	
			}
		}
		#endregion
			
		[XmlIgnore]
		[Gui.PropertyEditor("NinjaTrader.Gui.Tools.CollectionEditor")]
		[Display(Name="Brush Collection", Order=4, GroupName="Brush Collection" , Prompt = "1 brush|{0} brushes|Add brush...|Edit brush...|Edit brushes...")]
		[SkipOnCopyTo(true)]
		public Collection<IndiBrushCollection.BrushWrapper> BrushCollection
        {
			get 
			{
				return brushCollection;
			}
			set
			{
				brushCollection = new Collection<IndiBrushCollection.BrushWrapper>(value.ToList());
			}
		}
		
		// Serializer for the BrushWrapper's Collection
        [Browsable(false)]
        public Collection<IndiBrushCollection.BrushWrapper> BrushCollectionSerialize
        {
			get
			{
				//Remove actual defaults
				foreach(IndiBrushCollection.BrushWrapper sw in brushCollectionDefaults.ToList())
				{
					IndiBrushCollection.BrushWrapper temp = BrushCollection.FirstOrDefault(p => p.BrushValue == sw.BrushValue && p.IsDefault == true);
					if(temp != null)
						brushCollectionDefaults.Remove(temp);
				}
				
				//Force user added values to not be defaults
				BrushCollection.All(p => p.IsDefault = false);
				
				return BrushCollection;
			}
			set
			{
				BrushCollection = value;
			}
        }
	}

}

#region BrushWrapper
namespace NinjaTrader.NinjaScript.Indicators.PropertyGridControlsExamples.IndiBrushCollection
{
	[CategoryDefaultExpanded(true)]
    public class BrushWrapper : NotifyPropertyChangedBase, ICloneable
    {
        // Parameterless constructor is needed for Clone and serialization
        public BrushWrapper() : this(Brushes.Transparent)
        {
        }

        public BrushWrapper(Brush value)
        {
            BrushValue = value;
        }

          // Display attributes, XmlIgnore attributes, Browsable attributes, etc can be all applied to the object's properties as well.
//        [Display(Name = "Brushes", GroupName = "Brushes")]
//        public Brush BrushValue
//        { get; set; }
		
		[XmlIgnore]
        [Display(Name = "Brushes", GroupName = "Brushes")]
        public Brush BrushValue
        { get; set; }
		
		[Browsable(false)]
		public string BrushValueSerializable
		{
			get { return Serialize.BrushToString(BrushValue); }
			set { BrushValue = Serialize.StringToBrush(value); }
		}

        // Cloned instance returned to the Collection editor with user defined value

		public object Clone()
		{
			BrushWrapper p = new BrushWrapper();
			p.BrushValue = BrushValue;
			return p;
		}
		
		//Default value handling
		[Browsable(false)]
		public bool IsDefault { get; set; }
		
        // Customize the displays on the left side of the editor window
        public override string ToString()
        { return BrushValue.ToString(); }
		
		// Use Reflection to be able to copy properties to new instance
		public object AssemblyClone(Type t)
		{
			Assembly a 				= t.Assembly;
			object brushCollection 	= a.CreateInstance(t.FullName);
			
			foreach (PropertyInfo p in t.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
			{
				if (p.CanWrite)
				{
					p.SetValue(brushCollection, this.GetType().GetProperty(p.Name).GetValue(this), null);
				}
			}
			
			return brushCollection;
		}
    }
	
}
#endregion

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PropertyGridControlsExamples.BrushCollectionEditorExample[] cacheBrushCollectionEditorExample;
		public PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample()
		{
			return BrushCollectionEditorExample(Input);
		}

		public PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample(ISeries<double> input)
		{
			if (cacheBrushCollectionEditorExample != null)
				for (int idx = 0; idx < cacheBrushCollectionEditorExample.Length; idx++)
					if (cacheBrushCollectionEditorExample[idx] != null &&  cacheBrushCollectionEditorExample[idx].EqualsInput(input))
						return cacheBrushCollectionEditorExample[idx];
			return CacheIndicator<PropertyGridControlsExamples.BrushCollectionEditorExample>(new PropertyGridControlsExamples.BrushCollectionEditorExample(), input, ref cacheBrushCollectionEditorExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample()
		{
			return indicator.BrushCollectionEditorExample(Input);
		}

		public Indicators.PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample(ISeries<double> input )
		{
			return indicator.BrushCollectionEditorExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample()
		{
			return indicator.BrushCollectionEditorExample(Input);
		}

		public Indicators.PropertyGridControlsExamples.BrushCollectionEditorExample BrushCollectionEditorExample(ISeries<double> input )
		{
			return indicator.BrushCollectionEditorExample(input);
		}
	}
}

#endregion
