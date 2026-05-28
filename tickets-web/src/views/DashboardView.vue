<script setup lang="ts">
import { onMounted, ref } from "vue";
import { apiClient } from "@/services/apiClient";

interface DashboardSummary {
  totalCustomers: number;
  totalUsers: number;
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  waitingClientTickets: number;
  resolvedTickets: number;
  closedTickets: number;
  criticalTickets: number;
  highPriorityTickets: number;
  unassignedTickets: number;
  overdueTickets: number;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

const summary = ref<DashboardSummary | null>(null);
const isLoading = ref(true);
const errorMessage = ref<string | null>(null);

async function loadDashboard() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await apiClient.get<ApiResponse<DashboardSummary>>("/api/dashboard/summary");

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudo cargar el dashboard.";
      return;
    }

    summary.value = response.data.data;
  } catch {
    errorMessage.value = "No se pudo conectar con el dashboard.";
  } finally {
    isLoading.value = false;
  }
}

onMounted(loadDashboard);
</script>

<template>
  <div class="dashboard-view">
    <section class="hero-card">
      <div>
        <p class="hero-card__eyebrow">Resumen operativo</p>
        <h2>Tickets, clientes y atención en una sola vista</h2>
      </div>

      <button class="button button--secondary" type="button" @click="loadDashboard">
        Actualizar
      </button>
    </section>

    <p v-if="isLoading" class="state-message">Cargando dashboard...</p>
    <p v-else-if="errorMessage" class="state-message state-message--error">{{ errorMessage }}</p>

    <section v-else-if="summary" class="metrics-grid">
      <article class="metric-card">
        <span>Tickets totales</span>
        <strong>{{ summary.totalTickets }}</strong>
      </article>

      <article class="metric-card">
        <span>Abiertos</span>
        <strong>{{ summary.openTickets }}</strong>
      </article>

      <article class="metric-card">
        <span>En progreso</span>
        <strong>{{ summary.inProgressTickets }}</strong>
      </article>

      <article class="metric-card metric-card--danger">
        <span>Vencidos por SLA</span>
        <strong>{{ summary.overdueTickets }}</strong>
      </article>

      <article class="metric-card">
        <span>Clientes</span>
        <strong>{{ summary.totalCustomers }}</strong>
      </article>

      <article class="metric-card">
        <span>Usuarios</span>
        <strong>{{ summary.totalUsers }}</strong>
      </article>

      <article class="metric-card">
        <span>Críticos</span>
        <strong>{{ summary.criticalTickets }}</strong>
      </article>

      <article class="metric-card">
        <span>Sin asignar</span>
        <strong>{{ summary.unassignedTickets }}</strong>
      </article>
    </section>
  </div>
</template>
