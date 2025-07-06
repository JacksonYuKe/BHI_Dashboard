import React from 'react';

interface WeekSelectorProps {
  availableWeeks: string[];
  selectedWeek: string;
  onWeekChange: (week: string) => void;
  loading: boolean;
}

const WeekSelector: React.FC<WeekSelectorProps> = ({
  availableWeeks,
  selectedWeek,
  onWeekChange,
  loading
}) => {
  const formatWeekDisplay = (weekStart: string) => {
    const date = new Date(weekStart);
    const endDate = new Date(date);
    endDate.setDate(date.getDate() + 6);
    
    return `${date.toLocaleDateString()} - ${endDate.toLocaleDateString()}`;
  };

  return (
    <div className="week-selector">
      <label htmlFor="week-select">Select Week:</label>
      <select
        id="week-select"
        value={selectedWeek}
        onChange={(e) => onWeekChange(e.target.value)}
        disabled={loading || availableWeeks.length === 0}
        className="select-input"
      >
        <option value="">-- Select a week --</option>
        {availableWeeks.map((week) => (
          <option key={week} value={week}>
            {formatWeekDisplay(week)}
          </option>
        ))}
      </select>
    </div>
  );
};

export default WeekSelector;