<script setup lang="ts">
import { computed, onMounted, reactive, ref } from "vue";
import {
  Edit3,
  Mail,
  Plus,
  RefreshCw,
  Search,
  ShieldCheck,
  UserRound,
  UsersRound,
  X
} from "lucide-vue-next";
import { apiClient } from "@/services/apiClient";

type UserRole = 1 | 2 | 3;

interface UserSummary {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  isActive: boolean;
  createdAtUtc: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[];
}

const users = ref<UserSummary[]>([]);
const search = ref("");
const isLoading = ref(true);
const isSaving = ref(false);
const errorMessage = ref<string | null>(null);
const successMessage = ref<string | null>(null);
const isModalOpen = ref(false);
const editingUserId = ref<string | null>(null);

const form = reactive({
  fullName: "",
  email: "",
  password: "",
  role: 2 as UserRole,
  isActive: true
});

const isEditing = computed(() => Boolean(editingUserId.value));

const filteredUsers = computed(() => {
  const term = search.value.trim().toLowerCase();

  if (!term) {
    return users.value;
  }

  return users.value.filter((user) => {
    return (
      user.fullName.toLowerCase().includes(term) ||
      user.email.toLowerCase().includes(term) ||
      getRoleLabel(user.role).toLowerCase().includes(term)
    );
  });
});

const activeUsers = computed(() => users.value.filter((user) => user.isActive).length);
const adminUsers = computed(() => users.value.filter((user) => user.role === 1).length);

async function loadUsers() {
  isLoading.value = true;
  errorMessage.value = null;

  try {
    const response = await apiClient.get<ApiResponse<UserSummary[]>>("/api/users");

    if (!response.data.success || !response.data.data) {
      errorMessage.value = response.data.message || "No se pudieron obtener los usuarios.";
      return;
    }

    users.value = response.data.data;
  } catch {
    errorMessage.value = "No se pudo conectar con la API de usuarios.";
  } finally {
    isLoading.value = false;
  }
}

function openCreateModal() {
  resetForm();
  isModalOpen.value = true;
}

function openEditModal(user: UserSummary) {
  editingUserId.value = user.id;
  form.fullName = user.fullName;
  form.email = user.email;
  form.password = "";
  form.role = user.role;
  form.isActive = user.isActive;
  errorMessage.value = null;
  successMessage.value = null;
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
  editingUserId.value = null;
  form.fullName = "";
  form.email = "";
  form.password = "";
  form.role = 2;
  form.isActive = true;
}

function validateForm() {
  const errors: string[] = [];

  if (!form.fullName.trim()) {
    errors.push("El nombre completo es obligatorio.");
  }

  if (!form.email.trim()) {
    errors.push("El correo es obligatorio.");
  }

  if (form.email.trim() && !form.email.includes("@")) {
    errors.push("El correo no tiene un formato válido.");
  }

  if (!isEditing.value && form.password.length < 8) {
    errors.push("La contraseña debe contener al menos 8 caracteres.");
  }

  return errors;
}

async function saveUser() {
  const validationErrors = validateForm();

  if (validationErrors.length > 0) {
    errorMessage.value = validationErrors.join(" ");
    successMessage.value = null;
    return;
  }

  isSaving.value = true;
  errorMessage.value = null;
  successMessage.value = null;

  const createPayload = {
    fullName: form.fullName.trim(),
    email: form.email.trim(),
    password: form.password,
    role: form.role
  };

  const updatePayload = {
    fullName: form.fullName.trim(),
    email: form.email.trim(),
    role: form.role,
    isActive: form.isActive
  };

  try {
    const response = isEditing.value
      ? await apiClient.put<ApiResponse<UserSummary>>(`/api/users/${editingUserId.value}`, updatePayload)
      : await apiClient.post<ApiResponse<UserSummary>>("/api/users", createPayload);

    if (!response.data.success) {
      errorMessage.value =
        response.data.errors.length > 0
          ? response.data.errors.join(" ")
          : response.data.message || "No se pudo guardar el usuario.";

      return;
    }

    successMessage.value = response.data.message || "Usuario guardado correctamente.";
    closeModal();
    await loadUsers();
  } catch {
    errorMessage.value = "No se pudo guardar el usuario. Verifica los datos e intenta nuevamente.";
  } finally {
    isSaving.value = false;
  }
}

function getInitials(name: string) {
  return name
    .split(" ")
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0])
    .join("")
    .toUpperCase();
}

function getRoleLabel(role: UserRole) {
  const labels: Record<UserRole, string> = {
    1: "Administrador",
    2: "Agente de soporte",
    3: "Cliente"
  };

  return labels[role];
}

function formatDate(value: string) {
  return new Intl.DateTimeFormat("es-MX", {
    dateStyle: "medium",
    timeStyle: "short"
  }).format(new Date(value));
}

onMounted(loadUsers);
</script>

<template>
  <div class="page-stack users-page">
    <section class="page-heading">
      <div>
        <p class="section-label">Administración</p>
        <h1>Usuarios</h1>
        <p>Administra usuarios, roles y acceso al sistema.</p>
      </div>

      <div class="page-heading__actions">
        <button class="button button--ghost" type="button" @click="loadUsers">
          <RefreshCw :size="17" />
          Actualizar
        </button>

        <button class="button button--primary" type="button" @click="openCreateModal">
          <Plus :size="17" />
          Nuevo usuario
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
          <span>Total usuarios</span>
          <strong>{{ users.length }}</strong>
          <p>Usuarios registrados en la API.</p>
        </div>
      </article>

      <article class="detail-card">
        <div class="detail-card__icon">
          <UserRound :size="22" />
        </div>

        <div>
          <span>Activos</span>
          <strong>{{ activeUsers }}</strong>
          <p>Usuarios habilitados para iniciar sesión.</p>
        </div>
      </article>

      <article class="detail-card">
        <div class="detail-card__icon">
          <ShieldCheck :size="22" />
        </div>

        <div>
          <span>Administradores</span>
          <strong>{{ adminUsers }}</strong>
          <p>Usuarios con permisos completos.</p>
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
            placeholder="Buscar por nombre, correo o rol..."
          />
          <Search class="input-control__icon input-control__icon--right" :size="18" />
        </div>
      </label>
    </section>

    <p v-if="isLoading" class="state-message">Cargando usuarios...</p>

    <section v-else class="customer-list">
      <article v-for="user in filteredUsers" :key="user.id" class="customer-card">
        <div class="customer-card__top">
          <div class="customer-card__avatar">
            {{ getInitials(user.fullName) }}
          </div>

          <div>
            <h2>{{ user.fullName }}</h2>
            <p>{{ getRoleLabel(user.role) }}</p>
          </div>

          <span class="customer-card__status" :class="{ 'customer-card__status--inactive': !user.isActive }">
            {{ user.isActive ? "Activo" : "Inactivo" }}
          </span>
        </div>

        <div class="customer-card__contact">
          <span>
            <Mail :size="16" />
            {{ user.email }}
          </span>

          <span>
            <ShieldCheck :size="16" />
            Rol: {{ getRoleLabel(user.role) }}
          </span>

          <span>
            <UserRound :size="16" />
            Registrado: {{ formatDate(user.createdAtUtc) }}
          </span>
        </div>

        <div class="customer-card__actions">
          <button class="button button--ghost button--small" type="button" @click="openEditModal(user)">
            <Edit3 :size="16" />
            Editar
          </button>
        </div>
      </article>

      <p v-if="filteredUsers.length === 0" class="state-message">
        No hay usuarios que coincidan con la búsqueda.
      </p>
    </section>

    <div v-if="isModalOpen" class="modal-backdrop" role="presentation" @click.self="closeModal">
      <section class="entity-modal" role="dialog" aria-modal="true">
        <header class="entity-modal__header">
          <div>
            <p class="section-label">{{ isEditing ? "Editar usuario" : "Nuevo usuario" }}</p>
            <h2>{{ isEditing ? "Actualizar acceso" : "Registrar usuario" }}</h2>
          </div>

          <button class="icon-button" type="button" aria-label="Cerrar modal" @click="closeModal">
            <X :size="18" />
          </button>
        </header>

        <form class="entity-form" @submit.prevent="saveUser">
          <label class="form-field">
            <span>Nombre completo</span>
            <div class="input-control">
              <input v-model="form.fullName" type="text" placeholder="Nombre del usuario" />
            </div>
          </label>

          <label class="form-field">
            <span>Correo</span>
            <div class="input-control">
              <input v-model="form.email" type="email" placeholder="usuario@correo.com" />
            </div>
          </label>

          <label v-if="!isEditing" class="form-field">
            <span>Contraseña</span>
            <div class="input-control">
              <input v-model="form.password" type="password" placeholder="Mínimo 8 caracteres" />
            </div>
          </label>

          <label class="form-field">
            <span>Rol</span>
            <select v-model="form.role">
              <option :value="1">Administrador</option>
              <option :value="2">Agente de soporte</option>
              <option :value="3">Cliente</option>
            </select>
          </label>

          <label v-if="isEditing" class="checkbox-field entity-form__full">
            <input v-model="form.isActive" type="checkbox" />
            <span>Usuario activo</span>
          </label>

          <footer class="entity-modal__actions">
            <button class="button button--ghost" type="button" @click="closeModal">
              Cancelar
            </button>

            <button class="button button--primary" type="submit" :disabled="isSaving">
              {{ isSaving ? "Guardando..." : "Guardar usuario" }}
            </button>
          </footer>
        </form>
      </section>
    </div>
  </div>
</template>
