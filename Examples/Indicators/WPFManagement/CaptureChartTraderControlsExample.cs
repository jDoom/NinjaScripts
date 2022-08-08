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

using System.Collections;				
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Reflection;
#endregion

namespace NinjaTrader.NinjaScript.Indicators.WPFManagement
{
	public class CaptureChartTraderControlsExample : Indicator
	{
		private Account 															account;
		private NinjaTrader.NinjaScript.AtmStrategy									atmStrategy;
		private string 																instrument;		
		private NinjaTrader.Gui.Tools.AccountSelector 								myAccountSelector;
		private NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector 		myAtmStrategySelector;
		private System.Windows.Controls.ComboBox 									myInstrumentSelector;
		private NinjaTrader.Gui.Tools.QuantityUpDown								myQudSelector;
		private NinjaTrader.Gui.Tools.TifSelector									myTifSelector;
		private int																	quantity;
		private TimeInForce															timeInForce;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"This indicator describes how to find ChartTrader Controls and subscribe to their SelectionChanged events.";
				Name								= "CaptureChartTraderControlsExample";
				IsOverlay							= true;
				IsChartOnly 						= true;
			}
			else if (State == State.DataLoaded)
			{			
				if (ChartControl != null)
					ChartControl.Dispatcher.InvokeAsync(new Action(() => { FindAssignControls(); }));
			}
			else if(State == State.Terminated)
			{        		
				if (account != null)
					account.OrderUpdate -= OnOrderUpdate;
				
				if (myAccountSelector != null)
					myAccountSelector.SelectionChanged -= OnChartTraderAccountSelectionChange;
					
				if (myInstrumentSelector != null)
					myInstrumentSelector.SelectionChanged -= OnChartTraderInstrumentSelectionChange;
				
				if (myAtmStrategySelector != null)
					myAtmStrategySelector.SelectionChanged -= OnChartTraderAtmSelectionChange;
				
				if (myTifSelector != null)
					myTifSelector.SelectionChanged -= OnChartTraderTifSelectionChange;
				
				if (myQudSelector != null)
					myQudSelector.ValueChanged -= OnChartTraderQudSelectionChange;
			}
		}

		private void FindAssignControls()
		{
			myAccountSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlAccountSelector") as NinjaTrader.Gui.Tools.AccountSelector;

			if (myAccountSelector.SelectedAccount != null)
			{
				account				= myAccountSelector.SelectedAccount;
				account.OrderUpdate	+= OnOrderUpdate;

				Print(account.Name);
			}

			myAccountSelector.SelectionChanged	+= OnChartTraderAccountSelectionChange;

			myInstrumentSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlInstrumentSelector") as System.Windows.Controls.ComboBox;

			if (myInstrumentSelector.SelectedValue != null)
			{
				instrument	= myInstrumentSelector.SelectedValue.ToString();

				Print(instrument);
			}

			myInstrumentSelector.SelectionChanged	+= OnChartTraderInstrumentSelectionChange;

			myAtmStrategySelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlATMStrategySelector") as NinjaTrader.Gui.NinjaScript.AtmStrategy.AtmStrategySelector;

			if (myAtmStrategySelector != null)
			{
				atmStrategy	= myAtmStrategySelector.SelectedAtmStrategy;

				Print(atmStrategy.DisplayName);
			}

			myAtmStrategySelector.SelectionChanged	+= OnChartTraderAtmSelectionChange;

			myTifSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlTIFSelector") as NinjaTrader.Gui.Tools.TifSelector;

			if (myTifSelector.SelectedTif != null)
			{
				timeInForce	= myTifSelector.SelectedTif;

				Print(timeInForce.ToString());
			}

			myTifSelector.SelectionChanged	+= OnChartTraderTifSelectionChange;

			myQudSelector = Window.GetWindow(ChartControl.Parent).FindFirst("ChartTraderControlQuantitySelector") as NinjaTrader.Gui.Tools.QuantityUpDown;

			if (myQudSelector.Value != null)
			{
				quantity	= myQudSelector.Value;

				Print(quantity);
			}

			myQudSelector.ValueChanged	+= OnChartTraderQudSelectionChange;
		}
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
		{
			// In this case, we are looking at the Instrument from ChartTrader, not from the indicator.
			if (e.Order.Instrument.FullName == instrument)
				Print(e.ToString());
		}
		
		private void OnChartTraderAccountSelectionChange(object sender, EventArgs e)
		{
			if (myAccountSelector.SelectedAccount != null)
			{	
				if (account != null)
					account.OrderUpdate -= OnOrderUpdate;
				
				account = myAccountSelector.SelectedAccount;
				account.OrderUpdate   += OnOrderUpdate;
				
				Print(account.Name);
			}
		}
		
		private void OnChartTraderInstrumentSelectionChange(object sender, EventArgs e)
		{
			if (myInstrumentSelector.SelectedValue != null)
			{	
				instrument = myInstrumentSelector.SelectedValue.ToString();
				
				Print(instrument);
			}
		}
		
		private void OnChartTraderAtmSelectionChange(object sender, EventArgs e)
		{
			if (myAtmStrategySelector.SelectedAtmStrategy != null)
			{					
				atmStrategy = myAtmStrategySelector.SelectedAtmStrategy;
				// Note: it can be sometimes easier to just access ChartControl.OwnerChart.ChartTrader.AtmStrategy through a dispatcher
				
				Print(atmStrategy.DisplayName);
			}
		}
		
		private void OnChartTraderTifSelectionChange(object sender, EventArgs e)
		{
			if (myTifSelector.SelectedTif != null)
			{					
				timeInForce = myTifSelector.SelectedTif;
				
				Print(timeInForce.ToString());
			}
		}
		
		private void OnChartTraderQudSelectionChange(object sender, EventArgs e)
		{
			if (myQudSelector.Value != null)
			{
				quantity = myQudSelector.Value;
				
				Print(quantity);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WPFManagement.CaptureChartTraderControlsExample[] cacheCaptureChartTraderControlsExample;
		public WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample()
		{
			return CaptureChartTraderControlsExample(Input);
		}

		public WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample(ISeries<double> input)
		{
			if (cacheCaptureChartTraderControlsExample != null)
				for (int idx = 0; idx < cacheCaptureChartTraderControlsExample.Length; idx++)
					if (cacheCaptureChartTraderControlsExample[idx] != null &&  cacheCaptureChartTraderControlsExample[idx].EqualsInput(input))
						return cacheCaptureChartTraderControlsExample[idx];
			return CacheIndicator<WPFManagement.CaptureChartTraderControlsExample>(new WPFManagement.CaptureChartTraderControlsExample(), input, ref cacheCaptureChartTraderControlsExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample()
		{
			return indicator.CaptureChartTraderControlsExample(Input);
		}

		public Indicators.WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample(ISeries<double> input )
		{
			return indicator.CaptureChartTraderControlsExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample()
		{
			return indicator.CaptureChartTraderControlsExample(Input);
		}

		public Indicators.WPFManagement.CaptureChartTraderControlsExample CaptureChartTraderControlsExample(ISeries<double> input )
		{
			return indicator.CaptureChartTraderControlsExample(input);
		}
	}
}

#endregion
