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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class SamplePriceModification : Strategy
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					   = @"Modifying the price of stop loss and profit target orders.";
				Name						   = "Sample Price Modification";
				Calculate					   = Calculate.OnBarClose;
				EntriesPerDirection			   = 1;
				EntryHandling				   = EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy   = true;
          		ExitOnSessionCloseSeconds      = 30;
				IsFillLimitOnTouch			   = false;
				MaximumBarsLookBack			   = MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution			   = OrderFillResolution.Standard;
				Slippage					   = 0;
				StartBehavior				   = StartBehavior.WaitUntilFlat;
				TimeInForce					   = TimeInForce.Gtc;
				TraceOrders					   = false;
				RealtimeErrorHandling		   = RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling			   = StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade			   = 20;
				StopLossTicks				   = 20;
				ProfitTargetTicks			   = 100;
			}
			if (State == State.Configure)
		     {
		        /* There are several ways you can use SetStopLoss and SetProfitTarget. You can have them set to a currency value
				or some sort of calculation mode. Calculation modes available are by percent, price, and ticks. SetStopLoss and
				SetProfitTarget will submit real working orders unless you decide to simulate the orders. */
				SetStopLoss(CalculationMode.Ticks, StopLossTicks);
				SetProfitTarget(CalculationMode.Ticks, ProfitTargetTicks);
		     }
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < BarsRequiredToTrade)
				return;
			
			// Resets the stop loss to the original value when all positions are closed
			if (Position.MarketPosition == MarketPosition.Flat)
			{
				SetStopLoss(CalculationMode.Ticks, StopLossTicks);
			}
			
			// If a long position is open, allow for stop loss modification to breakeven
			else if (Position.MarketPosition == MarketPosition.Long)
			{
				// Once the price is greater than entry price+50 ticks, set stop loss to breakeven
				if (Close[0] > Position.AveragePrice + 50 * TickSize)
				{
					SetStopLoss(CalculationMode.Price, Position.AveragePrice);
				}
			}
			
			// Entry Condition: Increasing price along with RSI oversold condition
			if (Close[0] > Close[1] && RSI(14, 3)[0] <= 30)
			{
				EnterLong();
			}
		}

		#region Properties
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="StopLossTicks", Description="Numbers of ticks away from entry price for the Stop Loss order", Order=1, GroupName="Parameters")]
		public int StopLossTicks
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="ProfitTargetTicks", Description="Number of ticks away from entry price for the Profit Target order", Order=2, GroupName="Parameters")]
		public int ProfitTargetTicks
		{ get; set; }
		#endregion

	}
}
