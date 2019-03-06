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
	public class PlotHigherTimeFrame : Strategy
	{
		private HMA myHMA;
		private HMA myHMA2;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Strategy here.";
				Name										= "Plot Higher Time Frame indicator";
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute, 60);
				AddPlot(Brushes.Green, "HMA1");
				AddPlot(Brushes.Red, "HMA2");
			}
			else if (State == State.DataLoaded)
			{
				myHMA = HMA(Closes[1], 14);
				myHMA2 = HMA(Closes[1], 30);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBars[1] < 30 || BarsInProgress != 0)
				return;
			
			if(CrossAbove(myHMA, myHMA2, 1))
				EnterLong();
			if(CrossBelow(myHMA, myHMA2, 1))
				EnterShort();
			
			// Use strategy plots to display higher time frame indicators
			Values[0][0] = myHMA[0];
			Values[1][0] = myHMA2[0];
		}
	}
}
