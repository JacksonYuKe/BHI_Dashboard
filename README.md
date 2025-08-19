# BHI Energy Dashboard

A full-stack web application for visualizing energy consumption data and predicting EV charger presence using statistical analysis and machine learning algorithms.

## Overview

The BHI Energy Dashboard analyzes energy consumption patterns from CSV data to detect potential EV charger installations. It uses a sliding window algorithm to identify consecutive hours of elevated energy usage that may indicate electric vehicle charging activity.

## Features

- **Energy Consumption Visualization**: Interactive charts showing hourly consumption patterns for each day of the week
- **EV Charger Detection**: Automated prediction of charger presence using statistical analysis
- **Threshold Highlighting**: Visual indicators for consecutive hours exceeding baseline consumption
- **Location Management**: Browse and analyze multiple energy meter locations
- **Weekly Analysis**: Time-series data organized by weeks for trend analysis
- **Responsive Design**: Works on desktop and mobile devices
- **Real-time Data Processing**: Handles large datasets with efficient caching

## Tech Stack

### Backend (ASP.NET Core)
- **Framework**: C# ASP.NET Core Web API (.NET 9)
- **Data Processing**: CSV parsing with CsvHelper library
- **Performance**: Singleton data service for optimal caching (~3M records)
- **Algorithm**: Statistical sliding window analysis for charger detection
- **API**: RESTful endpoints with CORS support

### Frontend (React TypeScript)
- **Framework**: React 18 with TypeScript
- **Visualization**: Recharts library for interactive charts
- **HTTP Client**: Axios for API communication with 120s timeout
- **Styling**: Responsive CSS with mobile-first design
- **Charts**: 7 separate daily charts per week visualization

## Getting Started

### Prerequisites
- **.NET 9 SDK** (updated from .NET 8)
- **Node.js 16+** and npm
- **CSV data files** in the specified format

### Data Setup
Place your CSV files in: `/Users/jackson/Project/Project_WIth_BHI/Data/Data_By_Month_CSV_Sample/`

Expected naming convention: `YYYYMM.csv` (e.g., `202301.csv`, `202302.csv`)

### Backend Setup
1. Navigate to the backend directory:
   ```bash
   cd backend/BHI.EnergyDashboard.API
   ```

2. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

3. Run the backend:
   ```bash
   dotnet run
   ```

The API will be available at `http://localhost:5001` (changed from 5000 due to macOS AirPlay conflict)

### Frontend Setup
1. Navigate to the frontend directory:
   ```bash
   cd frontend/bhi-energy-dashboard
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

The dashboard will be available at `http://localhost:3000`

## Usage

1. **Access the Dashboard**: Open `http://localhost:3000` in your web browser
2. **Initial Load**: Wait for data loading (first load may take 30+ seconds for ~3M records)
3. **Select Location**: Choose a location from the dropdown menu
4. **Select Week**: Pick a week to analyze from available data
5. **Analyze Charts**: Review the 7 daily charts showing:
   - Hourly consumption patterns
   - Baseline and threshold reference lines
   - Highlighted consecutive sequences exceeding threshold
   - Charger prediction status

## API Endpoints

### `GET /api/energy/locations`
Returns list of all available locations with charger status and prediction confidence.

**Response:**
```json
[
  {
    "locationId": "0021131",
    "hasChargers": true,
    "isPredicted": false,
    "probability": 0.0
  }
]
```

### `GET /api/energy/locations/{locationId}/weeks`
Returns available weeks for a specific location.

**Response:**
```json
["2023-01-01T00:00:00", "2023-01-08T00:00:00", ...]
```

### `GET /api/energy/locations/{locationId}/consumption?weekStart={date}`
Returns detailed weekly consumption data with analysis.

**Response:**
```json
{
  "locationId": "0021131",
  "weekStart": "2023-02-12T00:00:00",
  "baseline": 0.989,
  "threshold": 2.0,
  "hasChargers": true,
  "isPredicted": false,
  "dailyData": [...]
}
```

## Data Format

### Input CSV Structure
```csv
YYYYMMDD,LOCATION,RATECLASS_DESC,# of Chargers,R1,R2,R3,...,R24
20230201,0021131,001: Residential: TOU,0,1.2,1.3,1.1,...,0.8
```

**Field Descriptions:**
- `YYYYMMDD`: Date in format YYYYMMDD
- `LOCATION`: Unique location identifier
- `RATECLASS_DESC`: Rate class description
- `# of Chargers`: Number of confirmed chargers (0 or positive integer, empty = unknown)
- `R1-R24`: Hourly consumption values for hours 1-24 (in kWh)

## EV Charger Detection Algorithm

### Core Logic
The system uses a sliding window approach to detect potential EV charger installations:

1. **Baseline Calculation**: Calculate average hourly consumption across all available data for each location
   ```
   baseline = sum(all_hourly_values) / count(all_hourly_values)
   ```

2. **Threshold Definition**: Fixed threshold of 2.0 kWh above baseline
   ```
   threshold_value = baseline + 2.0
   ```

3. **Sequence Detection**: Identify consecutive sequences of 2+ hours where ALL values exceed threshold
   ```
   for each day:
     find all sequences where hourlyConsumption[i] > threshold_value 
     AND hourlyConsumption[i+1] > threshold_value (for 2+ consecutive hours)
   ```

4. **Weekly Analysis**: Mark weeks as "exceeded" if ANY day contains qualifying sequences

5. **Probability Calculation**: 
   ```
   probability = weeks_with_sequences / total_weeks
   ```

6. **Prediction**: Predict charger presence if probability > 0.5

### Technical Parameters
- **Window Size**: 2 consecutive hours minimum
- **Threshold**: Fixed 2.0 kWh above baseline (corrected from baseline + 2)
- **Prediction Threshold**: 50% probability
- **Analysis Scope**: All available weeks for the location

## Project Structure

```
BHI_Dashboard/
├── backend/
│   └── BHI.EnergyDashboard.API/
│       ├── Controllers/
│       ├── Models/
│       ├── Services/
│       └── Program.cs
├── frontend/
│   └── bhi-energy-dashboard/
│       ├── src/
│       │   ├── components/
│       │   ├── services/
│       │   └── types/
│       └── public/
└── README.md
```

## Chart Interpretation

### Visual Elements
- **Blue Line**: Hourly energy consumption
- **Blue Dashed Line**: Calculated baseline (average consumption)
- **Red Dashed Line**: Threshold line (baseline + 2 kWh)
- **Red Dots**: Hours that are part of consecutive sequences exceeding threshold
- **Green Background**: Days containing threshold-exceeding sequences

### Status Indicators
- ✅ **Confirmed Chargers**: Location has known charger installations
- 🔮 **Predicted Chargers**: Algorithm predicts charger presence (>50% probability)
- ❌ **No Chargers**: No evidence of charger activity
- ⚠️ **Contains threshold-exceeding sequence**: Day has consecutive hours above threshold

## Performance Considerations

- **Data Caching**: Backend uses singleton pattern to cache parsed CSV data
- **Initial Load**: First request may take 30+ seconds to load ~3M records
- **Subsequent Requests**: Cached data provides sub-second response times
- **Memory Usage**: Large datasets are kept in memory for optimal performance

## Troubleshooting

### Common Issues

1. **Port 5000 Conflict (macOS)**
   - **Issue**: macOS AirPlay uses port 5000
   - **Solution**: Backend configured to use port 5001

2. **CORS Errors**
   - **Solution**: Ensure backend CORS policy allows frontend origin

3. **Missing Data**
   - Check CSV file path configuration
   - Verify CSV format matches expected structure

4. **Performance Issues**
   - Initial load is expected to be slow
   - Consider reducing dataset size for testing

5. **Highlighting Not Working**
   - Ensure threshold calculation is correct (fixed threshold of 2.0)
   - Check console for algorithm debugging output

## Development

### Backend Development
```bash
cd backend/BHI.EnergyDashboard.API
dotnet watch run
```

### Frontend Development
```bash
cd frontend/bhi-energy-dashboard
npm start
```

### Building for Production
```bash
# Backend
dotnet publish -c Release

# Frontend
npm run build
```

## Recent Updates

- **Fixed threshold calculation bug**: Changed from dynamic `baseline + 2` to fixed `2.0` threshold
- **Updated to .NET 9**: Migrated from .NET 8 for latest features
- **Port configuration**: Changed from 5000 to 5001 for macOS compatibility
- **Enhanced visualization**: 7 separate daily charts instead of single weekly chart
- **Improved caching**: Singleton data service for better performance
- **Extended timeout**: Increased API timeout to 120 seconds for large datasets

## License

[Add your license information here]

## Support

For questions or issues, please create an issue in the repository.