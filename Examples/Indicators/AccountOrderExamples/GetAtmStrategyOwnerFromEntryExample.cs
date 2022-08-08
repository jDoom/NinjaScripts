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
#endregion

// The NinjaTrader.NinjaScript.Indicators namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators.AccountOrderExamples
{
	public class GetAtmStrategyOwnerFromEntryExample : Indicator
	{
		private Account	myAcct;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"";
				Name										= "DetectAtmEntry";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
			}
			else if (State == State.DataLoaded)
			{
				lock (Account.All)

              	myAcct = Account.All.FirstOrDefault(a => a.Name == "Sim101");
				myAcct.OrderUpdate += OnOrderUpdate;
			}
			else if (State == State.Terminated)
			{
				if(myAcct != null)
					myAcct.OrderUpdate -= OnOrderUpdate;
			}
		}

		protected override void OnBarUpdate() { }
		
		private void OnOrderUpdate(object sender, OrderEventArgs e)
		{
			if (e.Order.Name == "Entry" && e.OrderState == OrderState.Filled)
			{
				AtmStrategy		atmStrategy;
				StrategyBase	stratbase;

				stratbase = e.Order.GetOwnerStrategy() ?? null;
				
				atmStrategy = stratbase as AtmStrategy;
				if (atmStrategy != null)
					Print("Entry order submitted for: " + atmStrategy.Template);
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample[] cacheGetAtmStrategyOwnerFromEntryExample;
		public AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample()
		{
			return GetAtmStrategyOwnerFromEntryExample(Input);
		}

		public AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample(ISeries<double> input)
		{
			if (cacheGetAtmStrategyOwnerFromEntryExample != null)
				for (int idx = 0; idx < cacheGetAtmStrategyOwnerFromEntryExample.Length; idx++)
					if (cacheGetAtmStrategyOwnerFromEntryExample[idx] != null &&  cacheGetAtmStrategyOwnerFromEntryExample[idx].EqualsInput(input))
						return cacheGetAtmStrategyOwnerFromEntryExample[idx];
			return CacheIndicator<AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample>(new AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample(), input, ref cacheGetAtmStrategyOwnerFromEntryExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample()
		{
			return indicator.GetAtmStrategyOwnerFromEntryExample(Input);
		}

		public Indicators.AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample(ISeries<double> input )
		{
			return indicator.GetAtmStrategyOwnerFromEntryExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample()
		{
			return indicator.GetAtmStrategyOwnerFromEntryExample(Input);
		}

		public Indicators.AccountOrderExamples.GetAtmStrategyOwnerFromEntryExample GetAtmStrategyOwnerFromEntryExample(ISeries<double> input )
		{
			return indicator.GetAtmStrategyOwnerFromEntryExample(input);
		}
	}
}

#endregion
