import React from 'react';

const LocationSelector = ({
  locations,
  selectedLocation,
  onLocationChange,
  loading
}) => {
  return (
    <div className="location-selector">
      <label htmlFor="location-select">Select Location:</label>
      <select
        id="location-select"
        value={selectedLocation}
        onChange={(e) => onLocationChange(e.target.value)}
        disabled={loading}
        className="select-input"
      >
        <option value="">-- Select a location --</option>
        {locations.map((location) => (
          <option key={location.locationId} value={location.locationId}>
            {location.locationId} 
            {location.hasConfirmedChargers ? ' (Confirmed Chargers)' : ''}
            {location.hasPredictedChargers ? ' (Predicted Chargers)' : ''}
          </option>
        ))}
      </select>
    </div>
  );
};

export default LocationSelector;