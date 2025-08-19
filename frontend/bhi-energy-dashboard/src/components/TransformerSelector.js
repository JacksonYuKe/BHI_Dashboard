import React from 'react';

const TransformerSelector = ({ transformers, selectedTransformer, onTransformerChange }) => {
  return (
    <div className="transformer-selector">
      <label htmlFor="transformer-select">Select Transformer:</label>
      <select
        id="transformer-select"
        value={selectedTransformer}
        onChange={(e) => onTransformerChange(e.target.value)}
        className="form-select"
      >
        <option value="">-- Select a Transformer --</option>
        {transformers.map((transformer) => (
          <option key={transformer.transformerId} value={transformer.transformerId}>
            {transformer.displayName}
          </option>
        ))}
      </select>
    </div>
  );
};

export default TransformerSelector;