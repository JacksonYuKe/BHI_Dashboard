export interface LocationInfo {
  locationId: string;
  hasConfirmedChargers: boolean;
  hasPredictedChargers: boolean;
  chargerPredictionProbability: number;
  baseline: number;
  availableWeeks: string[];
}

export interface DailyConsumption {
  date: string;
  hourlyConsumption: number[];
  exceedsThreshold: boolean;
}

export interface WeeklyConsumption {
  locationId: string;
  weekStart: string;
  dailyData: DailyConsumption[];
  baseline: number;
  threshold: number;
  hasChargers: boolean;
  isPredicted: boolean;
}