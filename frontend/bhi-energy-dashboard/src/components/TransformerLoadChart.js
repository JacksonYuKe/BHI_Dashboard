import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  Legend,
  ReferenceLine,
  ReferenceArea,
  ResponsiveContainer
} from 'recharts';

const TransformerLoadChart = ({ dailyData, ratingKva }) => {
  if (!dailyData || !dailyData.hourlyLoads) {
    return <div>No data available</div>;
  }

  // Prepare chart data
  const chartData = dailyData.hourlyLoads.map((load, index) => ({
    hour: index,
    loadKw: load.loadKw,
    loadRate: load.loadRate,
    isOverload: load.isOverload
  }));

  // Find overload regions for shading
  const overloadRegions = [];
  let startHour = null;
  
  chartData.forEach((point, index) => {
    if (point.isOverload && startHour === null) {
      startHour = index;
    } else if (!point.isOverload && startHour !== null) {
      overloadRegions.push({ start: startHour, end: index - 1 });
      startHour = null;
    }
  });
  
  // Handle case where overload continues to the end
  if (startHour !== null) {
    overloadRegions.push({ start: startHour, end: 23 });
  }

  const formatDate = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
  };

  return (
    <div className="transformer-load-chart">
      <h4>{dailyData.dayOfWeek} - {formatDate(dailyData.date)}</h4>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart
          data={chartData}
          margin={{ top: 5, right: 20, left: 10, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis 
            dataKey="hour" 
            label={{ value: 'Hour', position: 'insideBottom', offset: -5 }}
            domain={[0, 23]}
            ticks={[0, 6, 12, 18, 23]}
          />
          <YAxis 
            label={{ value: 'Load (kW)', angle: -90, position: 'insideLeft' }}
          />
          <Tooltip 
            formatter={(value, name) => {
              if (name === 'loadKw') return [`${value.toFixed(2)} kW`, 'Load'];
              if (name === 'loadRate') return [`${value.toFixed(1)}%`, 'Load Rate'];
              return value;
            }}
          />
          <Legend />
          
          {/* Reference line for transformer rating */}
          <ReferenceLine 
            y={ratingKva} 
            stroke="red" 
            strokeDasharray="5 5"
            label={{ value: `Rated Capacity: ${ratingKva} kVA`, position: 'right' }}
          />
          
          {/* Shaded areas for overload periods */}
          {overloadRegions.map((region, index) => (
            <ReferenceArea
              key={index}
              x1={region.start}
              x2={region.end}
              y1={0}
              y2={ratingKva * 1.5}
              stroke="none"
              fill="red"
              fillOpacity={0.2}
            />
          ))}
          
          <Line
            type="monotone"
            dataKey="loadKw"
            stroke="#2196F3"
            name="Load (kW)"
            strokeWidth={2}
            dot={false}
          />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
};

export default TransformerLoadChart;