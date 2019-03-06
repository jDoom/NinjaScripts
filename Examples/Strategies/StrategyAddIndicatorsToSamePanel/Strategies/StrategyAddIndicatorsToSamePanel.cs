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
	public class StrategyAddIndicatorsToSamePanel : Strategy
	{
		private CCI CCI1, CCI2;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "StrategyAddIndicatorsToSamePanel";
				Calculate									= Calculate.OnBarClose;
			}
			else if (State == State.DataLoaded)
			{
				CCI1 = CCI(14);
				CCI2 = CCI(30);
				
				AddChartIndicator(CCI1);
				AddChartIndicator(CCI2);
				
				CCI1.Panel = 1;
				CCI2.Panel = 1;
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom strategy logic here.
		}
	}
}
