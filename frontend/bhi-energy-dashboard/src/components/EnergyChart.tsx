import React from 'react';
import {
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  ReferenceLine,
  ComposedChart
} from 'recharts';
import { WeeklyConsumption } from '../types';

interface EnergyChartProps {
  weeklyData: WeeklyConsumption | null;
  loading: boolean;
}

const EnergyChart: React.FC<EnergyChartProps> = ({ weeklyData, loading }) => {
  if (loading) {
    return <div className="chart-loading">Loading chart data...</div>;
  }

  if (!weeklyData) {
    return <div className="chart-placeholder">Select a location and week to view energy consumption</div>;
  }

  // Helper function to find consecutive hours exceeding threshold
  const findThresholdExceedingSequences = (hourlyData: number[], baseline: number, threshold: number) => {
    const thresholdValue = baseline + threshold;
    const sequences: number[][] = [];
    
    let currentSequence: number[] = [];
    
    for (let i = 0; i < hourlyData.length; i++) {
      if (hourlyData[i] > thresholdValue) {
        currentSequence.push(i);
      } else {
        // End of consecutive sequence
        if (currentSequence.length >= 2) {
          sequences.push([...currentSequence]);
        }
        currentSequence = [];
      }
    }
    
    // Check if the last sequence qualifies
    if (currentSequence.length >= 2) {
      sequences.push([...currentSequence]);
    }
    
    return sequences;
  };

  // Create individual daily charts
  const renderDayChart = (dayData: any, dayIndex: number) => {
    const date = new Date(dayData.date);
    const dayName = date.toLocaleDateString('en-US', { weekday: 'long' });
    const dateStr = date.toLocaleDateString();
    
    // Create hourly chart data
    const hourlyChartData = dayData.hourlyConsumption.map((consumption: number, hour: number) => ({
      hour: hour,
      hourLabel: `${hour}:00`,
      consumption,
      baseline: weeklyData.baseline,
      threshold: weeklyData.threshold,
      exceedsThreshold: consumption > weeklyData.baseline + weeklyData.threshold
    }));

    // Find sequences exceeding threshold
    const exceedingSequences = findThresholdExceedingSequences(
      dayData.hourlyConsumption, 
      weeklyData.baseline, 
      weeklyData.threshold
    );

    const CustomTooltip = ({ active, payload, label }: any) => {
      if (active && payload && payload.length) {
        const data = payload[0].payload;
        const isInSequence = exceedingSequences.some(seq => 
          seq.includes(data.hour)
        );
        
        return (
          <div className="custom-tooltip">
            <p className="label">{`Hour ${data.hour}:00`}</p>
            <p className="consumption">{`Consumption: ${data.consumption.toFixed(2)} kWh`}</p>
            <p className="baseline">{`Baseline: ${weeklyData.baseline.toFixed(2)} kWh`}</p>
            <p className="threshold">{`Threshold: ${weeklyData.threshold.toFixed(2)} kWh`}</p>
            {isInSequence && (
              <p className="exceeds" style={{ color: 'red', fontWeight: 'bold' }}>
                ⚠️ Part of consecutive sequence exceeding threshold
              </p>
            )}
          </div>
        );
      }
      return null;
    };

    return (
      <div key={dayIndex} className="daily-chart">
        <div className="daily-chart-header">
          <h4>{dayName} ({dateStr})</h4>
          {dayData.exceedsThreshold && (
            <span className="threshold-exceeded">⚠️ Contains threshold-exceeding sequence</span>
          )}
        </div>
        
        <ResponsiveContainer width="100%" height={250}>
          <ComposedChart data={hourlyChartData} margin={{ top: 10, right: 20, left: 10, bottom: 5 }}>
            <CartesianGrid strokeDasharray="3 3" />
            <XAxis 
              dataKey="hour" 
              tickFormatter={(hour) => `${hour}:00`}
              interval={3}
            />
            <YAxis 
              label={{ value: 'kWh', angle: -90, position: 'insideLeft' }} 
              domain={[0, 'dataMax + 5']}
            />
            <Tooltip content={<CustomTooltip />} />
            
            <ReferenceLine 
              y={weeklyData.baseline} 
              stroke="blue" 
              strokeDasharray="5 5" 
              label="Baseline"
            />
            
            <ReferenceLine 
              y={weeklyData.threshold} 
              stroke="red" 
              strokeDasharray="5 5" 
              label="Threshold"
            />
            
            <Line 
              type="monotone" 
              dataKey="consumption" 
              stroke="#8884d8" 
              strokeWidth={2}
              dot={(props: any) => {
                const isInSequence = exceedingSequences.some(seq => 
                  seq.includes(props.payload.hour)
                );
                return (
                  <circle 
                    cx={props.cx} 
                    cy={props.cy} 
                    r={isInSequence ? 6 : 3} 
                    fill={isInSequence ? "red" : "#8884d8"}
                    stroke={isInSequence ? "darkred" : "#8884d8"}
                    strokeWidth={isInSequence ? 2 : 1}
                  />
                );
              }}
              activeDot={{ r: 8 }}
              name="Hourly Consumption"
            />
            
            {/* Note: Area highlighting will be done through dot styling above */}
          </ComposedChart>
        </ResponsiveContainer>
      </div>
    );
  };

  return (
    <div className="energy-chart">
      <div className="chart-header">
        <h3>Weekly Energy Consumption - Location {weeklyData.locationId}</h3>
        <div className="chart-info">
          <span className="charger-status">
            {weeklyData.hasChargers 
              ? weeklyData.isPredicted 
                ? '🔮 Predicted Chargers' 
                : '✅ Confirmed Chargers'
              : '❌ No Chargers'
            }
          </span>
          <span className="baseline-info">
            Baseline: {weeklyData.baseline.toFixed(2)} kWh/hour
          </span>
          <span className="threshold-info">
            Threshold: {weeklyData.threshold.toFixed(2)} kWh/hour
          </span>
        </div>
      </div>
      
      <div className="daily-charts-container">
        {weeklyData.dailyData.map((dayData, index) => renderDayChart(dayData, index))}
      </div>
      
      <div className="chart-legend">
        <div className="legend-item">
          <span className="legend-color" style={{ backgroundColor: '#8884d8' }}></span>
          <span>Hourly Consumption</span>
        </div>
        <div className="legend-item">
          <span className="legend-color" style={{ backgroundColor: 'red' }}></span>
          <span>Consecutive Hours Exceeding Threshold (≥2 hours)</span>
        </div>
        <div className="legend-item">
          <span className="legend-line" style={{ borderColor: 'blue' }}></span>
          <span>Baseline (Average)</span>
        </div>
        <div className="legend-item">
          <span className="legend-line" style={{ borderColor: 'red' }}></span>
          <span>Threshold (Baseline + 2)</span>
        </div>
      </div>
    </div>
  );
};

export default EnergyChart;