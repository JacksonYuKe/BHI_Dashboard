import React from 'react';

const TransformerMetricCard = ({ title, value, subtitle, colorClass = '' }) => {
  return (
    <div className={`metric-card ${colorClass}`}>
      <h5 className="metric-title">{title}</h5>
      <div className="metric-value">{value}</div>
      {subtitle && <div className="metric-subtitle">{subtitle}</div>}
    </div>
  );
};

export default TransformerMetricCard;