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
	public class VolailityPoacher : Strategy
	{
		private EMA emaFast;
		private SMA smaSlow;
		private StdDev stDevSlow;
		private StdDev stDevFast;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Volaility poaching using standard deviation and trend crossover.";
				Name										= "VolailityPoacher";
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
				FastEMA										= 5;
				SlowSMA										= 20;
				FastStdDev									= 5;
				SlowStdDev									= 20;
				ProfitTarget 								= 0.05;
				StopLoss 									= 0.01;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
			}
			if (State == State.Configure)
			{
								
				// Take profit for every live position, 1%
				SetProfitTarget(CalculationMode.Percent, ProfitTarget);
				
				// Set a stop loss
				SetStopLoss(CalculationMode.Percent, StopLoss);
				
				// Indicator 1, measure trend 
				emaFast = EMA(FastEMA);
				smaSlow = SMA(SlowSMA);
				
				emaFast.Plots[0].Brush = Brushes.Goldenrod;
				smaSlow.Plots[0].Brush = Brushes.SeaGreen;
				
				AddChartIndicator(emaFast);
				AddChartIndicator(smaSlow);
				
				// Indicator 2, measure volatility
				stDevSlow=StdDev(SlowStdDev);
				stDevFast=StdDev(FastStdDev);
				
				stDevSlow.Plots[0].Brush = Brushes.AliceBlue;
				stDevFast.Plots[0].Brush = Brushes.DarkOliveGreen;
				
				AddChartIndicator(stDevSlow);
				AddChartIndicator(stDevFast);
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			// If Spike in volatility and shift in trend, short/long other direction than trend 
			if (CrossAbove(stDevFast, stDevSlow, 1) & CrossAbove(emaFast, smaSlow, 1))
				EnterLong();
			
			if (CrossAbove(stDevFast, stDevSlow, 1) & CrossBelow(emaFast, smaSlow, 1))
				EnterShort();

		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastEMA", GroupName = "NinjaScriptStrategyParameters", Order = 0)]
		public int FastEMA
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowSMA", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int SlowSMA
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastStdDev", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int FastStdDev
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowStdDev", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int SlowStdDev
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ProfitTarget", GroupName="NinjaScriptStrategyParameters", Order=2)]
		public double ProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopLoss", GroupName="NinjaScriptStrategyParameters", Order=2)]
		public double StopLoss
		{ get; set; }
		
		#endregion
	}
}
