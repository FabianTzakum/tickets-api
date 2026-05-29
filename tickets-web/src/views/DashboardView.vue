<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import {
  AlertTriangle,
  CheckCircle2,
  Clock3,
  RefreshCw,
  Ticket,
  TimerReset,
  UserRound,
  UsersRound
} from "lucide-vue-next";
import { apiClient } from "@/services/apiClient";

interface MetricItem {
  status?: string;
  priority?: string;
  count: number;
}

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
  ticketsByStatus: MetricItem[];
  ticketsByPriority: MetricItem[];
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

const statusRows = computed(() => {
  if (!summary.value) {
    return [];
  }

  const total = Math.max(summary.value.totalTickets, 1);

  return [
    { label: "Abiertos", value: summary.value.openTickets, color: "blue" },
    { label: "En progreso", value: summary.value.inProgressTickets, color: "yellow" },
    { label: "En espera cliente", value: summary.value.waitingClientTickets, color: "green" },
    { label: "Resueltos", value: summary.value.resolvedTickets, color: "purple" },
    { label: "Cerrados", value: summary.value.closedTickets, color: "gray" }
  ].map((item) => ({
    ...item,
    percent: Math.round((item.value / total) * 100)
  }));
});

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
  <div class="page-stack">
    <section class="page-heading">
      <div>
        <p class="section-label">Bienvenido de vuelta</p>
        <h1>Centro de control</h1>
        <p>Resumen general de la actividad de soporte técnico.</p>
      </div>

      <div class="page-heading__actions">
        <button class="date-button" type="button">
          <Clock3 :size="18" />
          Hoy
        </button>

        <button class="button button--primary" type="button" @click="loadDashboard">
          <RefreshCw :size="17" />
          Actualizar
        </button>
      </div>
    </section>

    <p v-if="isLoading" class="state-message">Cargando dashboard...</p>
    <p v-else-if="errorMessage" class="state-message state-message--error">{{ errorMessage }}</p>

    <template v-else-if="summary">
      <section class="metric-grid">
        <article class="metric-card metric-card--blue">
          <span class="metric-card__icon"><Ticket :size="26" /></span>
          <div>
            <p>Tickets totales</p>
            <strong>{{ summary.totalTickets }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--green">
          <span class="metric-card__icon"><CheckCircle2 :size="26" /></span>
          <div>
            <p>Abiertos</p>
            <strong>{{ summary.openTickets }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--yellow">
          <span class="metric-card__icon"><Clock3 :size="26" /></span>
          <div>
            <p>En progreso</p>
            <strong>{{ summary.inProgressTickets }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--purple">
          <span class="metric-card__icon"><TimerReset :size="26" /></span>
          <div>
            <p>Vencidos por SLA</p>
            <strong>{{ summary.overdueTickets }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--blue">
          <span class="metric-card__icon"><UsersRound :size="26" /></span>
          <div>
            <p>Clientes</p>
            <strong>{{ summary.totalCustomers }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--purple">
          <span class="metric-card__icon"><UserRound :size="26" /></span>
          <div>
            <p>Usuarios</p>
            <strong>{{ summary.totalUsers }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--red">
          <span class="metric-card__icon"><AlertTriangle :size="26" /></span>
          <div>
            <p>Críticos</p>
            <strong>{{ summary.criticalTickets }}</strong>
          </div>
        </article>

        <article class="metric-card metric-card--gray">
          <span class="metric-card__icon"><TimerReset :size="26" /></span>
          <div>
            <p>Sin asignar</p>
            <strong>{{ summary.unassignedTickets }}</strong>
          </div>
        </article>
      </section>

      <section class="dashboard-panels">
        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Tickets por estado</h2>
            <button class="button button--ghost button--small" type="button">Este mes</button>
          </div>

          <div class="status-report">
            <div class="donut-chart">
              <strong>{{ summary.totalTickets }}</strong>
              <span>Total</span>
            </div>

            <div class="status-report__list">
              <div v-for="row in statusRows" :key="row.label" class="status-row">
                <span :class="`dot dot--${row.color}`"></span>
                <span>{{ row.label }}</span>
                <strong>{{ row.value }} ({{ row.percent }}%)</strong>
              </div>
            </div>
          </div>
        </article>

        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Actividad reciente</h2>
            <button class="button button--ghost button--small" type="button">Ver todos</button>
          </div>

          <div class="activity-list">
            <div class="activity-item">
              <span class="activity-item__icon activity-item__icon--blue"><Ticket :size="20" /></span>
              <div>
                <strong>Dashboard actualizado</strong>
                <p>Métricas obtenidas desde la API.</p>
              </div>
              <small>Ahora</small>
            </div>

            <div class="activity-item">
              <span class="activity-item__icon activity-item__icon--green"><CheckCircle2 :size="20" /></span>
              <div>
                <strong>Autenticación activa</strong>
                <p>Sesión protegida con JWT.</p>
              </div>
              <small>Hoy</small>
            </div>

            <div class="activity-item">
              <span class="activity-item__icon activity-item__icon--yellow"><Clock3 :size="20" /></span>
              <div>
                <strong>SLA monitoreado</strong>
                <p>{{ summary.overdueTickets }} tickets vencidos detectados.</p>
              </div>
              <small>Hoy</small>
            </div>
          </div>
        </article>
      </section>
    </template>
  </div>
</template>
