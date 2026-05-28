import { defineStore } from "pinia";
import { apiClient } from "@/services/apiClient";

type UserRole = 1 | 2 | 3;

interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthUser;
}

interface LoginPayload {
  email: string;
  password: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  isInitialized: boolean;
  isLoading: boolean;
  errorMessage: string | null;
}

export const useAuthStore = defineStore("auth", {
  state: (): AuthState => ({
    token: localStorage.getItem("tickets_api_token"),
    user: null,
    isInitialized: false,
    isLoading: false,
    errorMessage: null
  }),

  getters: {
    isAuthenticated: (state) => Boolean(state.token && state.user),
    userRoleLabel: (state) => {
      if (!state.user) {
        return "Sin sesión";
      }

      const labels: Record<UserRole, string> = {
        1: "Administrador",
        2: "Agente de soporte",
        3: "Cliente"
      };

      return labels[state.user.role];
    }
  },

  actions: {
    async initialize() {
      if (!this.token) {
        this.isInitialized = true;
        return;
      }

      try {
        const response = await apiClient.get<ApiResponse<AuthUser>>("/api/auth/me");

        if (response.data.success && response.data.data) {
          this.user = response.data.data;
        } else {
          this.logout();
        }
      } catch {
        this.logout();
      } finally {
        this.isInitialized = true;
      }
    },

    async login(payload: LoginPayload) {
      this.isLoading = true;
      this.errorMessage = null;

      try {
        const response = await apiClient.post<ApiResponse<LoginResponse>>("/api/auth/login", payload);

        if (!response.data.success || !response.data.data) {
          this.errorMessage = response.data.message || "No se pudo iniciar sesión.";
          return false;
        }

        this.token = response.data.data.accessToken;
        this.user = response.data.data.user;

        localStorage.setItem("tickets_api_token", this.token);

        return true;
      } catch {
        this.errorMessage = "No se pudo conectar con la API. Verifica que el backend esté ejecutándose.";
        return false;
      } finally {
        this.isLoading = false;
      }
    },

    logout() {
      this.token = null;
      this.user = null;
      localStorage.removeItem("tickets_api_token");
    }
  }
});
