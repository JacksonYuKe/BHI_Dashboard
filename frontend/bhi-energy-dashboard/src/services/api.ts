import axios from 'axios';
import { LocationInfo, WeeklyConsumption } from '../types';

const API_BASE_URL = 'http://localhost:5001/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 120000,
});

export const energyApi = {
  getLocations: async (): Promise<LocationInfo[]> => {
    const response = await api.get<LocationInfo[]>('/energy/locations');
    return response.data;
  },

  getAvailableWeeks: async (locationId: string): Promise<string[]> => {
    const response = await api.get<string[]>(`/energy/locations/${locationId}/weeks`);
    return response.data;
  },

  getWeeklyConsumption: async (locationId: string, weekStart: string): Promise<WeeklyConsumption> => {
    const response = await api.get<WeeklyConsumption>(
      `/energy/locations/${locationId}/consumption`,
      {
        params: { weekStart }
      }
    );
    return response.data;
  },
};