import axios from 'axios';

const API_BASE_URL = 'http://localhost:5001/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 120000,
});

export const energyApi = {
  getLocations: async (threshold = 2.0) => {
    const response = await api.get('/energy/locations', {
      params: { threshold }
    });
    return response.data;
  },

  getAvailableWeeks: async (locationId) => {
    const response = await api.get(`/energy/locations/${locationId}/weeks`);
    return response.data;
  },

  getWeeklyConsumption: async (locationId, weekStart, threshold = 2.0) => {
    const response = await api.get(
      `/energy/locations/${locationId}/consumption`,
      {
        params: { weekStart, threshold }
      }
    );
    return response.data;
  },
};

// Transformer API endpoints
export const getTransformers = async (threshold = 2.0) => {
  const response = await api.get('/transformer/list', {
    params: { threshold }
  });
  return response.data;
};

export const getTransformerWeeks = async (transformerId) => {
  const response = await api.get(`/transformer/${transformerId}/weeks`);
  return response.data;
};

export const getTransformerWeeklyAnalysis = async (transformerId, week, threshold = 2.0) => {
  const response = await api.get(
    `/transformer/${transformerId}/weekly-analysis`,
    {
      params: { week, threshold }
    }
  );
  return response.data;
};