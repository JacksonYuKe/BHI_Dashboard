import React, { useState, useEffect } from 'react';
import TransformerSelector from './TransformerSelector';
import WeekSelector from './WeekSelector';
import TransformerLoadChart from './TransformerLoadChart';
import TransformerMetricCard from './TransformerMetricCard';
import { getTransformers, getTransformerWeeks, getTransformerWeeklyAnalysis } from '../services/api';
import './TransformerAnalysisDashboard.css';

const TransformerAnalysisDashboard = ({ threshold, onThresholdChange }) => {
  const [transformers, setTransformers] = useState([]);
  const [selectedTransformer, setSelectedTransformer] = useState('');
  const [weeks, setWeeks] = useState([]);
  const [selectedWeek, setSelectedWeek] = useState('');
  const [weeklyAnalysis, setWeeklyAnalysis] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  // Load transformers on component mount or when threshold changes
  useEffect(() => {
    loadTransformers();
  }, [threshold]);

  // Load weeks when transformer changes
  useEffect(() => {
    if (selectedTransformer) {
      loadWeeks(selectedTransformer);
    } else {
      setWeeks([]);
      setSelectedWeek('');
      setWeeklyAnalysis(null);
    }
  }, [selectedTransformer]);

  // Load analysis when week changes
  useEffect(() => {
    if (selectedTransformer && selectedWeek) {
      loadWeeklyAnalysis(selectedTransformer, selectedWeek);
    }
  }, [selectedTransformer, selectedWeek]);

  const loadTransformers = async () => {
    try {
      setLoading(true);
      const data = await getTransformers(threshold);
      setTransformers(data);
      // Reset selection when transformers change
      if (!data.find(t => t.transformerId === selectedTransformer)) {
        setSelectedTransformer('');
        setSelectedWeek('');
        setWeeklyAnalysis(null);
      }
    } catch (err) {
      setError('Failed to load transformers');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadWeeks = async (transformerId) => {
    try {
      setLoading(true);
      const data = await getTransformerWeeks(transformerId);
      setWeeks(data);
      if (data.length > 0) {
        setSelectedWeek(data[0]); // Select most recent week by default
      }
    } catch (err) {
      setError('Failed to load weeks');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const loadWeeklyAnalysis = async (transformerId, week) => {
    try {
      setLoading(true);
      setError(null);
      const data = await getTransformerWeeklyAnalysis(transformerId, week, threshold);
      console.log('Weekly analysis data:', data); // Debug log
      setWeeklyAnalysis(data);
    } catch (err) {
      setError('Failed to load analysis data');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleTransformerChange = (transformerId) => {
    setSelectedTransformer(transformerId);
  };

  const handleWeekChange = (week) => {
    setSelectedWeek(week);
  };

  const getTransformerDisplayTitle = () => {
    if (!selectedTransformer || !transformers.length) {
      return "Transformer Weekly Load Analysis";
    }
    const transformer = transformers.find(t => t.transformerId === selectedTransformer);
    return transformer ? 
      `${transformer.transformerId} - ${transformer.ratingKva}kVA Transformer Load Analysis` :
      "Transformer Weekly Load Analysis";
  };

  return (
    <div className="transformer-analysis-dashboard">
      <h1>{getTransformerDisplayTitle()}</h1>
      
      <div className="control-panel">
        <div className="threshold-control">
          <label htmlFor="transformer-threshold-input">
            Threshold (kWh): 
            <input
              id="transformer-threshold-input"
              type="number"
              value={threshold}
              onChange={(e) => {
                const value = parseFloat(e.target.value);
                if (!isNaN(value) && value >= 0.5 && value <= 10) {
                  onThresholdChange(value);
                }
              }}
              min="0.5"
              max="10"
              step="0.5"
              className="threshold-input"
            />
          </label>
          <button 
            onClick={() => onThresholdChange(2.0)}
            className="reset-threshold-btn"
          >
            Reset to Default
          </button>
        </div>
        <div>
          <TransformerSelector
            transformers={transformers}
            selectedTransformer={selectedTransformer}
            onTransformerChange={handleTransformerChange}
          />
          {selectedTransformer && (
            <WeekSelector
              availableWeeks={weeks}
              selectedWeek={selectedWeek}
              onWeekChange={handleWeekChange}
              loading={loading}
            />
          )}
        </div>
      </div>

      {error && <div className="error-message">{error}</div>}
      
      {loading && <div className="loading">Loading...</div>}

      {weeklyAnalysis && !loading && (
        <>
          <div className="charts-container">
            {weeklyAnalysis.dailyLoads.map((dailyData, index) => (
              <div key={index} className="chart-wrapper">
                <TransformerLoadChart
                  dailyData={dailyData}
                  ratingKva={weeklyAnalysis.ratingKva}
                />
              </div>
            ))}
          </div>

          <div className="metrics-container">
            <h3>Weekly Summary</h3>
            <div className="load-rate-guide">
              <h4>Load Rate Categories:</h4>
              <div className="category-guide">
                <span className="category normal">🟢 Normal (&lt;80%)</span>
                <span className="category near-capacity">🟡 Near Capacity (80-95%)</span>
                <span className="category light-overload">🟠 Light Overload (95-110%)</span>
                <span className="category severe-overload">🔴 Severe Overload (&gt;110%)</span>
              </div>
            </div>
            <div className="metrics-grid">
              <TransformerMetricCard
                title="Weekly Maximum Load"
                value={`${weeklyAnalysis.metrics.weeklyMaxLoadKw}kW`}
                subtitle={`(${weeklyAnalysis.metrics.weeklyMaxLoadRate}%)`}
                colorClass={weeklyAnalysis.metrics.categoryColor}
              />
              <TransformerMetricCard
                title="Average Load Rate"
                value={`${weeklyAnalysis.metrics.averageLoadRate}%`}
              />
              <TransformerMetricCard
                title="Transformer Rating"
                value={`${weeklyAnalysis.ratingKva}kVA`}
                subtitle={`Feeder: ${weeklyAnalysis.feederId}`}
              />
              <TransformerMetricCard
                title="Connected Locations"
                value={`${weeklyAnalysis.locationCount || 0}`}
                subtitle="Total locations under this transformer"
              />
            </div>
          </div>
        </>
      )}
    </div>
  );
};

export default TransformerAnalysisDashboard;