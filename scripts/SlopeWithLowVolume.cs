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
	public class SlopeWithLowVolume : Strategy
	{
		private VOLMA volFast;
		private VOLMA volSlow ;
		
		private LinRegSlope regSlope;
		private LinRegIntercept regInter;
		
		private double VolumeSpanPos;
		private double VolumeSpanNeg;
		

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{

				Description									= @"Strategy using regression line slope for estimating trend. Uses volume for controlling stability of trend. ";
				Name										= "SlopeWithLowVolume";
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
				RegSlopeBarLength							= 20;
				RegInterBarLength							= 5;
				VolFastBarLength							= 5;
				VolSlowBarLength							= 20;
				RegSlopeMinAngle							= 0.4;
				RegSlopeMaxAngle							= 1.7;
				VolumeOffsetPos								= 1.1;
				VolumeOffsetNeg								= 0.9;
				ProfitTarget 								= 0.25;
				StopLoss 									= 0.25;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;

			}
			if (State == State.Configure)
			{
								
				// Take profit for every live position, 25%
				SetProfitTarget(CalculationMode.Percent, ProfitTarget);
				
				// Set a stop loss, higher stop loss due to being trend based, lower fluctiations
				SetStopLoss(CalculationMode.Percent, StopLoss);
				
				// Indicator 1, slope of price
				regSlope = LinRegSlope(Close, RegSlopeBarLength); //20
				regInter = LinRegIntercept(Close, RegInterBarLength); // 5
								
				regSlope.Plots[0].Brush = Brushes.Magenta;
				regInter.Plots[0].Brush = Brushes.Aqua;
				
				AddChartIndicator(regSlope);
				AddChartIndicator(regInter);
								
				// Indicator 2, Measure of volume
				volFast = VOLMA(VolFastBarLength); // 5
				volSlow = VOLMA(VolSlowBarLength); // 20
				
				volFast.Plots[0].Brush = Brushes.Goldenrod;
				volSlow.Plots[0].Brush = Brushes.SeaGreen;
				
				AddChartIndicator(volFast);
				AddChartIndicator(volSlow);
				

				
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0) 
				return;
			
// add exists if other activates
			
			if (((regSlope[0]>=RegSlopeMinAngle) & (regSlope[0]<=RegSlopeMaxAngle)) & ((volFast[0]<=volSlow[0]*VolumeOffsetPos) & (volFast[0]>=volSlow[0]*VolumeOffsetNeg)))
			{
				if(Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
				}
				EnterLong();
			}
			
			if (((regSlope[0]<=-RegSlopeMinAngle) & (regSlope[0]>=-RegSlopeMaxAngle)) & ((volFast[0]<=volSlow[0]*VolumeOffsetPos) & (volFast[0]>=volSlow[0]*VolumeOffsetNeg)))
			{
				if(Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
				}
				EnterShort();
			}
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RegSlopeBarLength", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int RegSlopeBarLength
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RegInterBarLength", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int RegInterBarLength
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VolFastBarLength", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int VolFastBarLength
		{ get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "VolSlowBarLength", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int VolSlowBarLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.1, 2.0)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RegSlopeMaxAngle", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double RegSlopeMaxAngle
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.1, 2.0)]
		[Display(ResourceType = typeof(Custom.Resource), Name="RegSlopeMinAngle", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double RegSlopeMinAngle
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1.0, double.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="VolumeOffsetPos", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double VolumeOffsetPos
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(double.MinValue, 1.0)]
		[Display(ResourceType = typeof(Custom.Resource), Name="VolumeOffsetNeg", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double VolumeOffsetNeg
		{ get; set; }		
		
		[NinjaScriptProperty]
		[Range(0.01, 1.0)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ProfitTarget", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double ProfitTarget
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.01, 1.0)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopLoss", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double StopLoss
		{ get; set; }

		#endregion

	}
	
}
