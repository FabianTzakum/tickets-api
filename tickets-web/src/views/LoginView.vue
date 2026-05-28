<script setup lang="ts">
import { reactive } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/authStore";

const router = useRouter();
const authStore = useAuthStore();

const form = reactive({
  email: "admin@tickets.local",
  password: "Admin12345"
});

async function handleSubmit() {
  const success = await authStore.login({
    email: form.email,
    password: form.password
  });

  if (success) {
    router.push({ name: "dashboard" });
  }
}
</script>

<template>
  <main class="login-page">
    <section class="login-page__panel">
      <div class="login-page__content">
        <span class="login-page__badge">Tickets API</span>
        <h1>Panel de soporte técnico</h1>
        <p>
          Interfaz Vue conectada a una API ASP.NET Core con JWT, roles,
          dashboard operativo, tickets, SLA e historial de cambios.
        </p>
      </div>

      <form class="login-card" @submit.prevent="handleSubmit">
        <div>
          <p class="login-card__eyebrow">Acceso seguro</p>
          <h2>Iniciar sesión</h2>
        </div>

        <label class="field">
          <span>Correo</span>
          <input v-model="form.email" type="email" autocomplete="email" />
        </label>

        <label class="field">
          <span>Contraseña</span>
          <input v-model="form.password" type="password" autocomplete="current-password" />
        </label>

        <p v-if="authStore.errorMessage" class="form-error">
          {{ authStore.errorMessage }}
        </p>

        <button class="button button--primary" type="submit" :disabled="authStore.isLoading">
          {{ authStore.isLoading ? "Entrando..." : "Entrar al panel" }}
        </button>

        <div class="login-card__demo">
          <strong>Usuario demo</strong>
          <span>admin@tickets.local / Admin12345</span>
        </div>
      </form>
    </section>
  </main>
</template>
