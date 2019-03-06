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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class AtmStrategySelectorExample : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "AtmStrategySelectorExample";
				Calculate									= Calculate.OnBarClose;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 30;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				
				AtmStrategy = String.Empty;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
		}
		
		[TypeConverter(typeof(FriendlyAtmConverter))] // Converts the bool to string values
        [PropertyEditor("NinjaTrader.Gui.Tools.StringStandardValuesEditorKey")] // Create the combo box on the property grid
        [Display(Name = "Atm Strategy", Order = 1, GroupName = "AtmStrategy")]
        public string AtmStrategy
        { get; set; }		
		
		#region AtmStrategySelector converter
		// Since this is only being applied to a specific property rather than the whole class,
		// we don't need to inherit from IndicatorBaseConverter and can just use a generic TypeConverter
		public class FriendlyAtmConverter : TypeConverter
		{
		    // Set the values to appear in the combo box
		    public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		    {
		       List<string> values = new List<string>();
				string[] files = System.IO.Directory.GetFiles(System.IO.Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "templates", "AtmStrategy"), "*.xml");	
				
				foreach(string atm in files)
				{
					values.Add(System.IO.Path.GetFileNameWithoutExtension(atm));
					NinjaTrader.Code.Output.Process(System.IO.Path.GetFileNameWithoutExtension(atm), PrintTo.OutputTab1);
				}

		        return new StandardValuesCollection(values);
		    }

		    public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
		    {
		        return value.ToString();
		    }

		    public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
		    {
		        return value;
		    }

		    // required interface members needed to compile
		    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		    { return true; }

		    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		    { return true; }

		    public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		    { return true; }

		    public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		    { return true; }
		}
		#endregion
	}
}
