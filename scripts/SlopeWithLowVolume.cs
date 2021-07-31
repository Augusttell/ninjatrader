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
		
		private int barNumberOfOrderLong = 0;
		private int barNumberOfOrderShort = 0;
		
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
				VolFastBarLength							= 5;
				VolSlowBarLength							= 20;
				RegSlopeMinAngle							= 0.4;
				RegSlopeMaxAngle							= 1.7;
				VolumeOffsetPos								= 1.1;
				VolumeOffsetNeg								= 0.9;
				ProfitTargetPoints 							= 5;
				StopLossPoints 								= 5;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= false;

			}
			if (State == State.Configure)
			{
												
				// Indicator 1, slope of price
				regSlope = LinRegSlope(Close, RegSlopeBarLength); //20
				
				regSlope.Plots[0].Brush = Brushes.Magenta;
				
				AddChartIndicator(regSlope);

								
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
			
			
			// Entry condition 1
			if (((regSlope[0]>=RegSlopeMinAngle) & (regSlope[0]<=RegSlopeMaxAngle)) & ((volFast[0]<=volSlow[0]*VolumeOffsetPos) & (volFast[0]>=volSlow[0]*VolumeOffsetNeg)))
			{
				// Exists if short is active
				if(Position.MarketPosition == MarketPosition.Short)
				{
					ExitShort();
				}
				EnterLong();
				barNumberOfOrderLong = CurrentBar;
			}
			
			// Entry condition 2
			if (((regSlope[0]<=-RegSlopeMinAngle) & (regSlope[0]>=-RegSlopeMaxAngle)) & ((volFast[0]<=volSlow[0]*VolumeOffsetPos) & (volFast[0]>=volSlow[0]*VolumeOffsetNeg)))
			{
				
				// Exists if long is active
				if(Position.MarketPosition == MarketPosition.Long)
				{
					ExitLong();
				}
				
				EnterShort();
				barNumberOfOrderShort = CurrentBar;
			}
			
			
			// If long active, and we have unrealized profit above target exit 
			if(//(CurrentBar > barNumberOfOrderLong + 5) & 
				(Position.MarketPosition == MarketPosition.Long) & 
				((Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]) >= ProfitTargetPoints) | 
				(Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]) <= -StopLossPoints)))
			{
				Print(Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]));
				ExitLong();
				
			}
			
			// If long active, and we have unrealized profit above target exit 
			if(//(CurrentBar > barNumberOfOrderShort + 5) & 
				(Position.MarketPosition == MarketPosition.Short) & 
				((Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]) >= ProfitTargetPoints) | 
				(Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]) <= -StopLossPoints)))
			{
				Print(Position.GetUnrealizedProfitLoss(PerformanceUnit.Points, Close[0]));
				ExitShort();
				
			}
			
			
			// Add condition, if plan, then leave position 
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "RegSlopeBarLength", GroupName = "NinjaScriptStrategyParameters", Order = 1)]
		public int RegSlopeBarLength
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
		[Range(int.MinValue, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="ProfitTargetPoints", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double ProfitTargetPoints
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(int.MinValue, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="StopLossPoints", GroupName="NinjaScriptStrategyParameters", Order=1)]
		public double StopLossPoints
		{ get; set; }

		#endregion

	}
	
}
