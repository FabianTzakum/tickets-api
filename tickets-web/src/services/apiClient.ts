import axios from "axios";

const API_BASE_URL = "https://localhost:7204";

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json"
  }
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem("tickets_api_token");

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});
