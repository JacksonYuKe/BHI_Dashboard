import React, { useState, useEffect } from 'react';
import { energyApi } from '../services/api';
import LocationSelector from './LocationSelector';
import WeekSelector from './WeekSelector';
import EnergyChart from './EnergyChart';

const Dashboard = ({ threshold, onThresholdChange }) => {
  const [locations, setLocations] = useState([]);
  const [selectedLocation, setSelectedLocation] = useState('');
  const [availableWeeks, setAvailableWeeks] = useState([]);
  const [selectedWeek, setSelectedWeek] = useState('');
  const [weeklyData, setWeeklyData] = useState(null);
  const [loading, setLoading] = useState({
    locations: false,
    weeks: false,
    consumption: false
  });
  const [error, setError] = useState(null);

  useEffect(() => {
    loadLocations();
  }, [threshold]);

  useEffect(() => {
    if (selectedLocation) {
      loadAvailableWeeks(selectedLocation);
      setSelectedWeek('');
      setWeeklyData(null);
    }
  }, [selectedLocation]);

  useEffect(() => {
    if (selectedLocation && selectedWeek) {
      loadWeeklyConsumption(selectedLocation, selectedWeek);
    }
  }, [selectedLocation, selectedWeek, threshold]);

  const loadLocations = async () => {
    try {
      setLoading(prev => ({ ...prev, locations: true }));
      setError(null);
      const data = await energyApi.getLocations(threshold);
      setLocations(data);
    } catch (err) {
      setError('Failed to load locations. Please check if the backend server is running.');
      console.error('Error loading locations:', err);
    } finally {
      setLoading(prev => ({ ...prev, locations: false }));
    }
  };

  const loadAvailableWeeks = async (locationId) => {
    try {
      setLoading(prev => ({ ...prev, weeks: true }));
      setError(null);
      const data = await energyApi.getAvailableWeeks(locationId);
      setAvailableWeeks(data);
    } catch (err) {
      setError('Failed to load available weeks.');
      console.error('Error loading weeks:', err);
    } finally {
      setLoading(prev => ({ ...prev, weeks: false }));
    }
  };

  const loadWeeklyConsumption = async (locationId, weekStart) => {
    try {
      setLoading(prev => ({ ...prev, consumption: true }));
      setError(null);
      const data = await energyApi.getWeeklyConsumption(locationId, weekStart, threshold);
      setWeeklyData(data);
    } catch (err) {
      setError('Failed to load consumption data.');
      console.error('Error loading consumption:', err);
    } finally {
      setLoading(prev => ({ ...prev, consumption: false }));
    }
  };

  const handleLocationChange = (locationId) => {
    setSelectedLocation(locationId);
  };

  const handleWeekChange = (week) => {
    setSelectedWeek(week);
  };

  const handleThresholdChange = (e) => {
    const value = parseFloat(e.target.value);
    if (!isNaN(value) && value >= 0.5 && value <= 10) {
      onThresholdChange(value);
    }
  };

  return (
    <div className="dashboard">
      <header className="dashboard-header">
        <h1>BHI Energy Consumption Dashboard</h1>
        <p>Monitor energy consumption patterns and EV charger usage across locations</p>
      </header>

      {error && (
        <div className="error-message">
          <p>⚠️ {error}</p>
          <button onClick={() => window.location.reload()}>Retry</button>
        </div>
      )}

      <div className="dashboard-controls">
        <div className="threshold-control">
          <label htmlFor="threshold-input">
            Threshold (kWh): 
            <input
              id="threshold-input"
              type="number"
              value={threshold}
              onChange={handleThresholdChange}
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

        <LocationSelector
          locations={locations}
          selectedLocation={selectedLocation}
          onLocationChange={handleLocationChange}
          loading={loading.locations}
        />

        <WeekSelector
          availableWeeks={availableWeeks}
          selectedWeek={selectedWeek}
          onWeekChange={handleWeekChange}
          loading={loading.weeks}
        />
      </div>

      <div className="dashboard-content">
        <EnergyChart
          weeklyData={weeklyData}
          loading={loading.consumption}
        />
      </div>

      <div className="dashboard-info">
        <div className="info-section">
          <h3>How to Use</h3>
          <ul>
            <li>Select a location from the dropdown (only locations with confirmed or predicted chargers are shown)</li>
            <li>Choose a week to view energy consumption patterns</li>
            <li>The chart shows daily consumption with baseline and threshold indicators</li>
            <li>Red threshold line indicates consumption levels that suggest EV charging activity</li>
          </ul>
        </div>

        <div className="info-section">
          <h3>Legend</h3>
          <ul>
            <li>✅ Confirmed Chargers: Location has verified EV chargers</li>
            <li>🔮 Predicted Chargers: Algorithm predicts EV chargers based on consumption patterns</li>
            <li>⚠️ Exceeds Threshold: Daily consumption patterns suggest charging activity</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;