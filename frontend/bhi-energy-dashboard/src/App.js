import React, { useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import './App.css';
import Dashboard from './components/Dashboard';
import TransformerAnalysisDashboard from './components/TransformerAnalysisDashboard';

function App() {
  const [globalThreshold, setGlobalThreshold] = useState(2.0);

  return (
    <Router>
      <div className="App">
        <nav className="app-nav">
          <div className="nav-container">
            <h2 className="app-title">BHI Energy Dashboard</h2>
            <div className="nav-links">
              <Link to="/" className="nav-link">Energy Consumption</Link>
              <Link to="/transformer-analysis" className="nav-link">Transformer Analysis</Link>
            </div>
          </div>
        </nav>
        
        <div className="app-content">
          <Routes>
            <Route path="/" element={
              <Dashboard 
                threshold={globalThreshold} 
                onThresholdChange={setGlobalThreshold} 
              />
            } />
            <Route path="/transformer-analysis" element={
              <TransformerAnalysisDashboard 
                threshold={globalThreshold} 
                onThresholdChange={setGlobalThreshold} 
              />
            } />
          </Routes>
        </div>
      </div>
    </Router>
  );
}

export default App;