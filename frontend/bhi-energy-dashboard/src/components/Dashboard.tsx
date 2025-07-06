import React, { useState, useEffect } from 'react';
import { LocationInfo, WeeklyConsumption } from '../types';
import { energyApi } from '../services/api';
import LocationSelector from './LocationSelector';
import WeekSelector from './WeekSelector';
import EnergyChart from './EnergyChart';

const Dashboard: React.FC = () => {
  const [locations, setLocations] = useState<LocationInfo[]>([]);
  const [selectedLocation, setSelectedLocation] = useState<string>('');
  const [availableWeeks, setAvailableWeeks] = useState<string[]>([]);
  const [selectedWeek, setSelectedWeek] = useState<string>('');
  const [weeklyData, setWeeklyData] = useState<WeeklyConsumption | null>(null);
  const [loading, setLoading] = useState({
    locations: false,
    weeks: false,
    consumption: false
  });
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadLocations();
  }, []);

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
  }, [selectedLocation, selectedWeek]);

  const loadLocations = async () => {
    try {
      setLoading(prev => ({ ...prev, locations: true }));
      setError(null);
      const data = await energyApi.getLocations();
      setLocations(data);
    } catch (err) {
      setError('Failed to load locations. Please check if the backend server is running.');
      console.error('Error loading locations:', err);
    } finally {
      setLoading(prev => ({ ...prev, locations: false }));
    }
  };

  const loadAvailableWeeks = async (locationId: string) => {
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

  const loadWeeklyConsumption = async (locationId: string, weekStart: string) => {
    try {
      setLoading(prev => ({ ...prev, consumption: true }));
      setError(null);
      const data = await energyApi.getWeeklyConsumption(locationId, weekStart);
      setWeeklyData(data);
    } catch (err) {
      setError('Failed to load consumption data.');
      console.error('Error loading consumption:', err);
    } finally {
      setLoading(prev => ({ ...prev, consumption: false }));
    }
  };

  const handleLocationChange = (locationId: string) => {
    setSelectedLocation(locationId);
  };

  const handleWeekChange = (week: string) => {
    setSelectedWeek(week);
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