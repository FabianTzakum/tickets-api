<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import {
  ArrowLeft,
  ArrowRight,
  Filter,
  Plus,
  RefreshCw,
  Search,
  SlidersHorizontal,
  Ticket
} from "lucide-vue-next";
import { apiClient } from "@/services/apiClient";

type TicketStatus = 1 | 2 | 3 | 4 | 5;
type TicketPriority = 1 | 2 | 3 | 4;

interface TicketSummary {
  id: string;
  title: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerName: string;
  assignedAgentName: string | null;
  createdAtUtc: string;
  slaDueAtUtc: string;
  isOverdue: boolean;
  remainingSlaHours: number;
}

interface PagedResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

const router = useRouter();

const tickets = ref<TicketSummary[]>([]);
const totalItems = ref(0);
const totalPages = ref(0);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const showAdvancedFilters = ref(false);

const filters = reactive({
  page: 1,
  pageSize: 8,
  search: "",
  status: "",
  priority: "",
  isOverdue: "",
  sortBy: "createdAt",
  sortDirection: "desc"
});

const rangeLabel = computed(() => {
  if (totalItems.value === 0) {
    return "0 tickets encontrados";
  }

  const start = (filters.page - 1) * filters.pageSize + 1;
  const end = Math.min(filters.page * filters.pageSize, totalItems.value);

  return `Mostrando ${start}-${end} de ${totalItems.value}`;
});

const hasActiveFilters = computed(() => {
  return Boolean(
    filters.search.trim() ||
      filters.status ||
      filters.priority ||
      filters.isOverdue ||
      filters.sortBy !== "createdAt" ||
      filters.sortDirection !== "desc"
  );
});

async function loadTickets() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const params: Record<string, string | number | boolean> = {
      page: filters.page,
      pageSize: filters.pageSize,
      sortBy: filters.sortBy,
      sortDirection: filters.sortDirection
    };

    if (filters.search.trim()) params.search = filters.search.trim();
    if (filters.status) params.status = filters.status;
    if (filters.priority) params.priority = filters.priority;
    if (filters.isOverdue) params.isOverdue = filters.isOverdue === "true";

    const response = await apiClient.get<ApiResponse<PagedResponse<TicketSummary>>>("/api/tickets", {
      params
    });

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudieron obtener los tickets.";
      return;
    }

    tickets.value = response.data.data.items;
    totalItems.value = response.data.data.totalItems;
    totalPages.value = response.data.data.totalPages;
  } catch {
    errorMessage.value = "No se pudo conectar con la API de tickets.";
  } finally {
    isLoading.value = false;
  }
}

function applyFilters() {
  filters.page = 1;
  loadTickets();
}

function clearFilters() {
  filters.page = 1;
  filters.search = "";
  filters.status = "";
  filters.priority = "";
  filters.isOverdue = "";
  filters.sortBy = "createdAt";
  filters.sortDirection = "desc";
  loadTickets();
}

function nextPage() {
  if (filters.page >= totalPages.value) return;
  filters.page += 1;
  loadTickets();
}

function previousPage() {
  if (filters.page <= 1) return;
  filters.page -= 1;
  loadTickets();
}

function goToDetail(id: string) {
  router.push({ name: "ticket-detail", params: { id } });
}

function getStatusLabel(status: TicketStatus) {
  const labels: Record<TicketStatus, string> = {
    1: "Abierto",
    2: "En progreso",
    3: "Esperando cliente",
    4: "Resuelto",
    5: "Cerrado"
  };

  return labels[status];
}

function getPriorityLabel(priority: TicketPriority) {
  const labels: Record<TicketPriority, string> = {
    1: "Baja",
    2: "Media",
    3: "Alta",
    4: "Crítica"
  };

  return labels[priority];
}

function getAgentInitials(name: string | null) {
  if (!name) return "--";

  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

function formatRemainingHours(hours: number) {
  if (hours <= 0) return "Sin tiempo restante";
  if (hours < 1) return "Menos de 1 hora";
  return `${hours.toFixed(1)} h restantes`;
}

onMounted(loadTickets);
</script>

<template>
  <div class="page-stack tickets-page">
    <section class="page-heading page-heading--compact">
      <div>
        <p class="section-label">Sistema de soporte</p>
        <h1>Tickets</h1>
        <p>Gestiona incidencias, prioridad, SLA y seguimiento operativo.</p>
      </div>
    </section>

    <section class="tickets-hero">
      <div class="tickets-hero__icon">
        <Ticket :size="28" />
      </div>

      <div>
        <h2>Incidencias activas y seguimiento SLA</h2>
        <p>Consulta, filtra y prioriza tickets con datos reales de la API.</p>
      </div>

      <div class="tickets-hero__actions">
        <button class="button button--ghost" type="button" @click="loadTickets">
          <RefreshCw :size="17" />
          Actualizar
        </button>

        <button class="button button--primary" type="button">
          <Plus :size="17" />
          Nuevo ticket
        </button>
      </div>
    </section>

    <section class="filters-panel">
      <label class="form-field form-field--search">
        <span>Búsqueda</span>
        <div class="input-control">
          <input
            v-model="filters.search"
            type="search"
            placeholder="Buscar por título, cliente o ID..."
            @keyup.enter="applyFilters"
          />
          <Search class="input-control__icon input-control__icon--right" :size="18" />
        </div>
      </label>

      <button
        class="filter-toggle"
        :class="{ 'filter-toggle--active': showAdvancedFilters || hasActiveFilters }"
        type="button"
        @click="showAdvancedFilters = !showAdvancedFilters"
      >
        <SlidersHorizontal :size="18" />
        Filtros
        <span v-if="hasActiveFilters"></span>
      </button>

      <div v-show="showAdvancedFilters" class="filters-panel__advanced">
        <label class="form-field">
          <span>Estado</span>
          <select v-model="filters.status">
            <option value="">Todos</option>
            <option value="1">Abierto</option>
            <option value="2">En progreso</option>
            <option value="3">Esperando cliente</option>
            <option value="4">Resuelto</option>
            <option value="5">Cerrado</option>
          </select>
        </label>

        <label class="form-field">
          <span>Prioridad</span>
          <select v-model="filters.priority">
            <option value="">Todas</option>
            <option value="1">Baja</option>
            <option value="2">Media</option>
            <option value="3">Alta</option>
            <option value="4">Crítica</option>
          </select>
        </label>

        <label class="form-field">
          <span>SLA</span>
          <select v-model="filters.isOverdue">
            <option value="">Todos</option>
            <option value="false">En tiempo</option>
            <option value="true">Vencidos</option>
          </select>
        </label>

        <label class="form-field">
          <span>Ordenar por</span>
          <select v-model="filters.sortBy">
            <option value="createdAt">Fecha</option>
            <option value="sla">SLA</option>
            <option value="priority">Prioridad</option>
            <option value="status">Estado</option>
            <option value="title">Título</option>
          </select>
        </label>

        <label class="form-field">
          <span>Dirección</span>
          <select v-model="filters.sortDirection">
            <option value="desc">Descendente</option>
            <option value="asc">Ascendente</option>
          </select>
        </label>
      </div>

      <div class="filters-panel__actions">
        <button class="button button--primary" type="button" @click="applyFilters">
          <Filter :size="17" />
          Aplicar
        </button>

        <button class="button button--ghost" type="button" @click="clearFilters">
          Limpiar
        </button>
      </div>
    </section>

    <div class="tickets-counter">
      <strong>{{ totalItems }} tickets encontrados</strong>
      <span>{{ rangeLabel }}</span>
    </div>

    <p v-if="isLoading" class="state-message">Cargando tickets...</p>
    <p v-else-if="errorMessage" class="state-message state-message--error">{{ errorMessage }}</p>

    <section v-else class="ticket-list">
      <article v-for="ticket in tickets" :key="ticket.id" class="ticket-row">
        <span class="ticket-row__indicator" :class="{ 'ticket-row__indicator--danger': ticket.isOverdue }"></span>

        <div class="ticket-row__main">
          <div class="ticket-row__title">
            <strong>#TK-{{ ticket.id.slice(0, 4).toUpperCase() }}</strong>
            <span v-if="ticket.isOverdue" class="sla-alert">Vencido por SLA</span>
          </div>

          <h3>{{ ticket.title }}</h3>
          <p>{{ ticket.customerName }}</p>

          <div class="ticket-row__badges">
            <div>
              <small>Estado</small>
              <span class="status-pill" :class="`status-pill--${ticket.status}`">
                {{ getStatusLabel(ticket.status) }}
              </span>
            </div>

            <div>
              <small>Prioridad</small>
              <span class="priority-pill" :class="`priority-pill--${ticket.priority}`">
                {{ getPriorityLabel(ticket.priority) }}
              </span>
            </div>

            <div>
              <small>Agente</small>
              <span class="agent-pill">
                <b>{{ getAgentInitials(ticket.assignedAgentName) }}</b>
                {{ ticket.assignedAgentName || "Sin asignar" }}
              </span>
            </div>
          </div>
        </div>

        <div class="ticket-row__side">
          <dl>
            <div>
              <dt>Creado</dt>
              <dd>{{ formatDate(ticket.createdAtUtc) }}</dd>
            </div>

            <div>
              <dt>Vence SLA</dt>
              <dd :class="{ 'is-danger': ticket.isOverdue }">{{ formatDate(ticket.slaDueAtUtc) }}</dd>
            </div>

            <div>
              <dt>Tiempo</dt>
              <dd :class="{ 'is-danger': ticket.isOverdue }">
                {{ ticket.isOverdue ? "SLA vencido" : formatRemainingHours(ticket.remainingSlaHours) }}
              </dd>
            </div>
          </dl>

          <button class="button button--ghost button--small ticket-row__detail" type="button" @click="goToDetail(ticket.id)">
            Ver detalle
            <ArrowRight :size="16" />
          </button>
        </div>
      </article>

      <p v-if="tickets.length === 0" class="state-message">
        No hay tickets con los filtros seleccionados.
      </p>
    </section>

    <footer class="pagination-panel">
      <button class="button button--ghost" type="button" :disabled="filters.page <= 1" @click="previousPage">
        <ArrowLeft :size="16" />
        Anterior
      </button>

      <div class="pagination-panel__pages">
        <strong>{{ filters.page }}</strong>
        <span>de {{ totalPages || 1 }}</span>
      </div>

      <button class="button button--ghost" type="button" :disabled="filters.page >= totalPages" @click="nextPage">
        Siguiente
        <ArrowRight :size="16" />
      </button>
    </footer>
  </div>
</template>
