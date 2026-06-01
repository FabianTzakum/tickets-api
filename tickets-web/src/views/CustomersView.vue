<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import {
  Building2,
  Edit3,
  Mail,
  Phone,
  Plus,
  RefreshCw,
  Search,
  UsersRound,
  X
} from "lucide-vue-next";
import { apiClient } from "@/services/apiClient";

interface CustomerSummary {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  companyName: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

interface CustomerDetail {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  companyName: string | null;
  notes: string | null;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string | null;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

const customers = ref<CustomerSummary[]>([]);
const search = ref("");
const isLoading = ref(true);
const isSaving = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);
const isModalOpen = ref(false);
const editingCustomerId = ref<string | null>(null);

const form = reactive({
  name: "",
  email: "",
  phone: "",
  companyName: "",
  notes: "",
  isActive: true
});

const filteredCustomers = computed(() => {
  const term = search.value.trim().toLowerCase();

  if (!term) {
    return customers.value;
  }

  return customers.value.filter((customer) => {
    return (
      customer.name.toLowerCase().includes(term) ||
      customer.email.toLowerCase().includes(term) ||
      customer.companyName?.toLowerCase().includes(term) ||
      customer.phone?.toLowerCase().includes(term)
    );
  });
});

const activeCustomers = computed(() => customers.value.filter((customer) => customer.isActive).length);
const inactiveCustomers = computed(() => customers.value.length - activeCustomers.value);
const isEditing = computed(() => Boolean(editingCustomerId.value));

async function loadCustomers() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await apiClient.get<ApiResponse<CustomerSummary[]>>("/api/customers");

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudieron obtener los clientes.";
      return;
    }

    customers.value = response.data.data;
  } catch {
    errorMessage.value = "No se pudo conectar con la API de clientes.";
  } finally {
    isLoading.value = false;
  }
}

async function openEditModal(customerId: string) {
  errorMessage.value = null;
  successMessage.value = null;

  try {
    const response = await apiClient.get<ApiResponse<CustomerDetail>>(`/api/customers/${customerId}`);

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudo obtener el cliente.";
      return;
    }

    const customer = response.data.data;

    editingCustomerId.value = customer.id;
    form.name = customer.name;
    form.email = customer.email;
    form.phone = customer.phone ?? "";
    form.companyName = customer.companyName ?? "";
    form.notes = customer.notes ?? "";
    form.isActive = customer.isActive;
    isModalOpen.value = true;
  } catch {
    errorMessage.value = "No se pudo cargar la información del cliente.";
  }
}

function openCreateModal() {
  resetForm();
  isModalOpen.value = true;
}

function closeModal() {
  if (isSaving.value) {
    return;
  }

  isModalOpen.value = false;
  resetForm();
}

function resetForm() {
  editingCustomerId.value = null;
  form.name = "";
  form.email = "";
  form.phone = "";
  form.companyName = "";
  form.notes = "";
  form.isActive = true;
}

function validateForm() {
  const errors: string[] = [];

  if (!form.name.trim()) {
    errors.push("El nombre es obligatorio.");
  }

  if (!form.email.trim()) {
    errors.push("El correo es obligatorio.");
  }

  if (form.email.trim() && !form.email.includes("@")) {
    errors.push("El correo no tiene un formato válido.");
  }

  return errors;
}

async function saveCustomer() {
  const validationErrors = validateForm();

  if (validationErrors.length > 0) {
    errorMessage.value = validationErrors.join(" ");
    successMessage.value = null;
    return;
  }

  isSaving.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  const payload = {
    name: form.name.trim(),
    email: form.email.trim(),
    phone: form.phone.trim() || null,
    companyName: form.companyName.trim() || null,
    notes: form.notes.trim() || null,
    isActive: form.isActive
  };

  try {
    const response = isEditing.value
      ? await apiClient.put<ApiResponse<CustomerDetail>>(`/api/customers/${editingCustomerId.value}`, payload)
      : await apiClient.post<ApiResponse<CustomerDetail>>("/api/customers", payload);

    if (!response.data.success) {
      errorMessage.value = response.data.errors.length > 0
        ? response.data.errors.join(" ")
        : response.data.message || "No se pudo guardar el cliente.";

      return;
    }

    successMessage.value = response.data.message || "Cliente guardado correctamente.";
    closeModal();
    await loadCustomers();
  } catch {
    errorMessage.value = "No se pudo guardar el cliente. Verifica los datos e intenta nuevamente.";
  } finally {
    isSaving.value = false;
  }
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

onMounted(loadCustomers);
</script>

<template>
  <div class="page-stack customers-page">
    <section class="page-heading">
      <div>
        <p class="section-label">Administración</p>
        <h1>Clientes</h1>
        <p>Consulta, crea y actualiza clientes registrados.</p>
      </div>

      <div class="page-heading__actions">
        <button class="button button--ghost" type="button" @click="loadCustomers">
          <RefreshCw :size="17" />
          Actualizar
        </button>

        <button class="button button--primary" type="button" @click="openCreateModal">
          <Plus :size="17" />
          Nuevo cliente
        </button>
      </div>
    </section>

    <p v-if="successMessage" class="state-message state-message--success">{{ successMessage }}</p>
    <p v-if="errorMessage" class="state-message state-message--error">{{ errorMessage }}</p>

    <section class="customer-summary-grid">
      <article class="detail-card">
        <div class="detail-card__icon">
          <UsersRound :size="22" />
        </div>

        <div>
          <span>Total clientes</span>
          <strong>{{ customers.length }}</strong>
          <p>Registros disponibles en la API.</p>
        </div>
      </article>

      <article class="detail-card">
        <div class="detail-card__icon">
          <Building2 :size="22" />
        </div>

        <div>
          <span>Activos</span>
          <strong>{{ activeCustomers }}</strong>
          <p>Clientes habilitados para operación.</p>
        </div>
      </article>

      <article class="detail-card">
        <div class="detail-card__icon">
          <UsersRound :size="22" />
        </div>

        <div>
          <span>Inactivos</span>
          <strong>{{ inactiveCustomers }}</strong>
          <p>Clientes deshabilitados o en pausa.</p>
        </div>
      </article>
    </section>

    <section class="filters-panel customers-filter">
      <label class="form-field form-field--search">
        <span>Búsqueda</span>
        <div class="input-control">
          <input
            v-model="search"
            type="search"
            placeholder="Buscar por nombre, empresa, correo o teléfono..."
          />
          <Search class="input-control__icon input-control__icon--right" :size="18" />
        </div>
      </label>
    </section>

    <p v-if="isLoading" class="state-message">Cargando clientes...</p>

    <section v-else class="customer-list">
      <article v-for="customer in filteredCustomers" :key="customer.id" class="customer-card">
        <div class="customer-card__top">
          <div class="customer-card__avatar">
            {{ customer.name.slice(0, 2).toUpperCase() }}
          </div>

          <div>
            <h2>{{ customer.name }}</h2>
            <p>{{ customer.companyName || "Sin empresa registrada" }}</p>
          </div>

          <span class="customer-card__status" :class="{ 'customer-card__status--inactive': !customer.isActive }">
            {{ customer.isActive ? "Activo" : "Inactivo" }}
          </span>
        </div>

        <div class="customer-card__contact">
          <span>
            <Mail :size="16" />
            {{ customer.email }}
          </span>

          <span>
            <Phone :size="16" />
            {{ customer.phone || "Sin teléfono" }}
          </span>

          <span>
            <Building2 :size="16" />
            Registrado: {{ formatDate(customer.createdAtUtc) }}
          </span>
        </div>

        <div class="customer-card__actions">
          <button class="button button--ghost button--small" type="button" @click="openEditModal(customer.id)">
            <Edit3 :size="16" />
            Editar
          </button>
        </div>
      </article>

      <p v-if="filteredCustomers.length === 0" class="state-message">
        No hay clientes que coincidan con la búsqueda.
      </p>
    </section>

    <div v-if="isModalOpen" class="modal-backdrop" role="presentation" @click.self="closeModal">
      <section class="entity-modal" role="dialog" aria-modal="true">
        <header class="entity-modal__header">
          <div>
            <p class="section-label">{{ isEditing ? "Editar cliente" : "Nuevo cliente" }}</p>
            <h2>{{ isEditing ? "Actualizar información" : "Registrar cliente" }}</h2>
          </div>

          <button class="icon-button" type="button" aria-label="Cerrar modal" @click="closeModal">
            <X :size="18" />
          </button>
        </header>

        <form class="entity-form" @submit.prevent="saveCustomer">
          <label class="form-field">
            <span>Nombre</span>
            <div class="input-control">
              <input v-model="form.name" type="text" placeholder="Nombre del cliente" />
            </div>
          </label>

          <label class="form-field">
            <span>Correo</span>
            <div class="input-control">
              <input v-model="form.email" type="email" placeholder="cliente@correo.com" />
            </div>
          </label>

          <label class="form-field">
            <span>Teléfono</span>
            <div class="input-control">
              <input v-model="form.phone" type="text" placeholder="9991234567" />
            </div>
          </label>

          <label class="form-field">
            <span>Empresa</span>
            <div class="input-control">
              <input v-model="form.companyName" type="text" placeholder="Empresa o negocio" />
            </div>
          </label>

          <label class="form-field entity-form__full">
            <span>Notas</span>
            <textarea v-model="form.notes" rows="4" placeholder="Notas internas del cliente..."></textarea>
          </label>

          <label class="checkbox-field entity-form__full">
            <input v-model="form.isActive" type="checkbox" />
            <span>Cliente activo</span>
          </label>

          <footer class="entity-modal__actions">
            <button class="button button--ghost" type="button" @click="closeModal">
              Cancelar
            </button>

            <button class="button button--primary" type="submit" :disabled="isSaving">
              {{ isSaving ? "Guardando..." : "Guardar cliente" }}
            </button>
          </footer>
        </form>
      </section>
    </div>
  </div>
</template>
