<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import { useRoute, useRouter } from "vue-router";
import {
  ArrowLeft,
  CheckCircle2,
  Clock3,
  History,
  MessageSquareText,
  RefreshCw,
  Send,
  ShieldAlert,
  Ticket,
  UserRound
} from "lucide-vue-next";
import { apiClient } from "@/services/apiClient";
import { useAuthStore } from "@/stores/authStore";

type TicketStatus = 1 | 2 | 3 | 4 | 5;
type TicketPriority = 1 | 2 | 3 | 4;

interface TicketComment {
  id: string;
  authorUserId: string;
  authorName: string;
  message: string;
  isInternal: boolean;
  createdAtUtc: string;
}

interface TicketHistoryItem {
  id: string;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
  changedByName: string | null;
  description: string;
  createdAtUtc: string;
}

interface TicketDetail {
  id: string;
  title: string;
  description: string;
  status: TicketStatus;
  priority: TicketPriority;
  customerId: string;
  customerName: string;
  assignedToUserId: string | null;
  assignedAgentName: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  resolvedAtUtc: string | null;
  closedAtUtc: string | null;
  slaDueAtUtc: string;
  slaHours: number;
  isOverdue: boolean;
  remainingSlaHours: number;
  comments: TicketComment[];
  history: TicketHistoryItem[];
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

const route = useRoute();
const router = useRouter();
const authStore = useAuthStore();

const ticket = ref<TicketDetail | null>(null);
const isLoading = ref(true);
const isSavingStatus = ref(false);
const isSavingComment = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);

const statusForm = reactive({
  status: "" as "" | TicketStatus
});

const commentForm = reactive({
  message: "",
  isInternal: false
});

const ticketId = computed(() => String(route.params.id));

const availableStatuses = [
  { value: 1, label: "Abierto" },
  { value: 2, label: "En progreso" },
  { value: 3, label: "Esperando cliente" },
  { value: 4, label: "Resuelto" },
  { value: 5, label: "Cerrado" }
] as const;

async function loadTicket() {
  isLoading.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  try {
    const response = await apiClient.get<ApiResponse<TicketDetail>>(`/api/tickets/${ticketId.value}`);

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudo obtener el ticket.";
      return;
    }

    ticket.value = response.data.data;
    statusForm.status = response.data.data.status;
  } catch {
    errorMessage.value = "No se pudo conectar con el detalle del ticket.";
  } finally {
    isLoading.value = false;
  }
}

async function changeStatus() {
  if (!ticket.value || !statusForm.status) {
    return;
  }

  if (statusForm.status === ticket.value.status) {
    errorMessage.value = "Selecciona un estado diferente al actual.";
    successMessage.value = null;
    return;
  }

  isSavingStatus.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  try {
    const response = await apiClient.patch<ApiResponse<TicketDetail>>(`/api/tickets/${ticket.value.id}/status`, {
      status: statusForm.status
    });

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudo cambiar el estado del ticket.";

      if (response.data.errors.length > 0) {
        errorMessage.value = response.data.errors.join(" ");
      }

      return;
    }

    ticket.value = response.data.data;
    statusForm.status = response.data.data.status;
    successMessage.value = response.data.message || "Estado actualizado correctamente.";
  } catch {
    errorMessage.value = "No se pudo actualizar el estado. Verifica la transición permitida.";
  } finally {
    isSavingStatus.value = false;
  }
}

async function addComment() {
  if (!ticket.value) {
    return;
  }

  if (!authStore.user?.id) {
    errorMessage.value = "No se pudo identificar al usuario autenticado.";
    return;
  }

  if (!commentForm.message.trim()) {
    errorMessage.value = "El comentario es obligatorio.";
    successMessage.value = null;
    return;
  }

  isSavingComment.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  try {
    const response = await apiClient.post<ApiResponse<TicketDetail>>(`/api/tickets/${ticket.value.id}/comments`, {
      authorUserId: authStore.user.id,
      message: commentForm.message.trim(),
      isInternal: commentForm.isInternal
    });

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudo agregar el comentario.";

      if (response.data.errors.length > 0) {
        errorMessage.value = response.data.errors.join(" ");
      }

      return;
    }

    ticket.value = response.data.data;
    commentForm.message = "";
    commentForm.isInternal = false;
    successMessage.value = response.data.message || "Comentario agregado correctamente.";
  } catch {
    errorMessage.value = "No se pudo agregar el comentario.";
  } finally {
    isSavingComment.value = false;
  }
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

function formatDate(value: string | null) {
  if (!value) {
    return "Sin registro";
  }

  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

function formatRemainingHours(hours: number) {
  if (hours <= 0) {
    return "Sin tiempo restante";
  }

  if (hours < 1) {
    return "Menos de 1 hora";
  }

  return `${hours.toFixed(1)} h restantes`;
}

onMounted(loadTicket);
</script>

<template>
  <div class="page-stack ticket-detail-page">
    <section class="detail-header">
      <button class="button button--ghost button--small" type="button" @click="router.push({ name: 'tickets' })">
        <ArrowLeft :size="16" />
        Volver
      </button>

      <button class="button button--primary button--small" type="button" @click="loadTicket">
        <RefreshCw :size="16" />
        Actualizar
      </button>
    </section>

    <p v-if="isLoading" class="state-message">Cargando detalle del ticket...</p>
    <p v-else-if="errorMessage" class="state-message state-message--error">{{ errorMessage }}</p>
    <p v-if="successMessage" class="state-message state-message--success">{{ successMessage }}</p>

    <template v-if="!isLoading && ticket">
      <section class="detail-hero">
        <div class="detail-hero__icon">
          <Ticket :size="30" />
        </div>

        <div class="detail-hero__content">
          <p class="section-label">#TK-{{ ticket.id.slice(0, 4).toUpperCase() }}</p>
          <h1>{{ ticket.title }}</h1>
          <p>{{ ticket.description }}</p>
        </div>

        <div class="detail-hero__badges">
          <span class="status-pill" :class="`status-pill--${ticket.status}`">
            {{ getStatusLabel(ticket.status) }}
          </span>

          <span class="priority-pill" :class="`priority-pill--${ticket.priority}`">
            {{ getPriorityLabel(ticket.priority) }}
          </span>
        </div>
      </section>

      <section class="detail-grid">
        <article class="detail-card detail-card--strong">
          <div class="detail-card__icon">
            <ShieldAlert :size="22" />
          </div>

          <div>
            <span>SLA</span>
            <strong :class="{ 'is-danger': ticket.isOverdue }">
              {{ ticket.isOverdue ? "Vencido" : "En tiempo" }}
            </strong>
            <p>
              {{ ticket.isOverdue ? "El ticket superó su tiempo de atención." : formatRemainingHours(ticket.remainingSlaHours) }}
            </p>
          </div>
        </article>

        <article class="detail-card">
          <div class="detail-card__icon">
            <Clock3 :size="22" />
          </div>

          <div>
            <span>Vencimiento</span>
            <strong>{{ formatDate(ticket.slaDueAtUtc) }}</strong>
            <p>{{ ticket.slaHours }} horas de SLA.</p>
          </div>
        </article>

        <article class="detail-card">
          <div class="detail-card__icon">
            <UserRound :size="22" />
          </div>

          <div>
            <span>Agente asignado</span>
            <strong>{{ ticket.assignedAgentName || "Sin asignar" }}</strong>
            <p>{{ ticket.customerName }}</p>
          </div>
        </article>
      </section>

      <section class="detail-actions-grid">
        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Cambiar estado</h2>
            <CheckCircle2 :size="20" />
          </div>

          <div class="status-form">
            <label class="form-field">
              <span>Estado actual</span>
              <select v-model="statusForm.status">
                <option v-for="status in availableStatuses" :key="status.value" :value="status.value">
                  {{ status.label }}
                </option>
              </select>
            </label>

            <button class="button button--primary" type="button" :disabled="isSavingStatus" @click="changeStatus">
              {{ isSavingStatus ? "Guardando..." : "Actualizar estado" }}
            </button>
          </div>

          <p class="helper-text">
            El backend valida las transiciones permitidas. Si el cambio no es válido, se mostrará el mensaje de la API.
          </p>
        </article>

        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Agregar comentario</h2>
            <Send :size="20" />
          </div>

          <form class="comment-form" @submit.prevent="addComment">
            <label class="form-field">
              <span>Mensaje</span>
              <textarea
                v-model="commentForm.message"
                rows="4"
                placeholder="Escribe una actualización del ticket..."
              ></textarea>
            </label>

            <label class="checkbox-field">
              <input v-model="commentForm.isInternal" type="checkbox" />
              <span>Comentario interno</span>
            </label>

            <button class="button button--primary" type="submit" :disabled="isSavingComment">
              {{ isSavingComment ? "Guardando..." : "Agregar comentario" }}
            </button>
          </form>
        </article>
      </section>

      <section class="detail-layout">
        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Comentarios</h2>
            <MessageSquareText :size="20" />
          </div>

          <div class="timeline-list">
            <div v-for="comment in ticket.comments" :key="comment.id" class="timeline-item">
              <span class="timeline-item__dot"></span>

              <div>
                <strong>{{ comment.authorName }}</strong>
                <p>{{ comment.message }}</p>
                <small>
                  {{ formatDate(comment.createdAtUtc) }}
                  <b v-if="comment.isInternal"> · Interno</b>
                </small>
              </div>
            </div>

            <p v-if="ticket.comments.length === 0" class="state-message">
              Este ticket todavía no tiene comentarios.
            </p>
          </div>
        </article>

        <article class="panel-card">
          <div class="panel-card__header">
            <h2>Historial</h2>
            <History :size="20" />
          </div>

          <div class="timeline-list">
            <div v-for="item in ticket.history" :key="item.id" class="timeline-item">
              <span class="timeline-item__dot timeline-item__dot--blue"></span>

              <div>
                <strong>{{ item.description }}</strong>
                <p>
                  {{ item.fieldName }}:
                  <span>{{ item.oldValue || "Sin valor" }}</span>
                  →
                  <span>{{ item.newValue || "Sin valor" }}</span>
                </p>
                <small>
                  {{ item.changedByName || "Sistema" }} · {{ formatDate(item.createdAtUtc) }}
                </small>
              </div>
            </div>

            <p v-if="ticket.history.length === 0" class="state-message">
              Este ticket todavía no tiene historial de cambios.
            </p>
          </div>
        </article>
      </section>
    </template>
  </div>
</template>
