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
	public class AccountItemUpdateExample : Indicator
	{
		private Account account;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "AccountItemUpdateExample";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.DataLoaded)
			{
				// Find our Sim101 account
		        lock (Account.All)
		              account = Account.All.FirstOrDefault(a => a.Name == "Sim101");

		        // Subscribe to account item updates
		        if (account != null)
		              account.AccountItemUpdate += OnAccountItemUpdate;
			}
			else if(State == State.Terminated)
			{
				// Make sure to unsubscribe to the account item subscription
        		if (account != null)
            		account.AccountItemUpdate -= OnAccountItemUpdate;
			}
		}
		
		// This method is fired on any change of an account value
	    private void OnAccountItemUpdate(object sender, AccountItemEventArgs e)
	    {
	         // Output the account item
	         NinjaTrader.Code.Output.Process(string.Format("Account: {0} AccountItem: {1} Value: {2}",
	              e.Account.Name, e.AccountItem, e.Value), PrintTo.OutputTab1);
			
			if(e.AccountItem == AccountItem.UnrealizedProfitLoss)
			{	
				double UnrealizedPnl = e.Value;
				
				if(UnrealizedPnl < 1000)
					Alert("myAlert", Priority.High, "Reached threshold", NinjaTrader.Core.Globals.InstallDir+@"\sounds\Alert1.wav", 10, Brushes.Black, Brushes.Yellow);
			}
	    }

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AccountItemUpdateExample[] cacheAccountItemUpdateExample;
		public AccountItemUpdateExample AccountItemUpdateExample()
		{
			return AccountItemUpdateExample(Input);
		}

		public AccountItemUpdateExample AccountItemUpdateExample(ISeries<double> input)
		{
			if (cacheAccountItemUpdateExample != null)
				for (int idx = 0; idx < cacheAccountItemUpdateExample.Length; idx++)
					if (cacheAccountItemUpdateExample[idx] != null &&  cacheAccountItemUpdateExample[idx].EqualsInput(input))
						return cacheAccountItemUpdateExample[idx];
			return CacheIndicator<AccountItemUpdateExample>(new AccountItemUpdateExample(), input, ref cacheAccountItemUpdateExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AccountItemUpdateExample AccountItemUpdateExample()
		{
			return indicator.AccountItemUpdateExample(Input);
		}

		public Indicators.AccountItemUpdateExample AccountItemUpdateExample(ISeries<double> input )
		{
			return indicator.AccountItemUpdateExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AccountItemUpdateExample AccountItemUpdateExample()
		{
			return indicator.AccountItemUpdateExample(Input);
		}

		public Indicators.AccountItemUpdateExample AccountItemUpdateExample(ISeries<double> input )
		{
			return indicator.AccountItemUpdateExample(input);
		}
	}
}

#endregion
