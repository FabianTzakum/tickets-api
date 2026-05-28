<script setup lang="ts">
import { useRouter } from "vue-router";
import { useAuthStore } from "@/stores/authStore";

const router = useRouter();
const authStore = useAuthStore();

function handleLogout() {
  authStore.logout();
  router.push({ name: "login" });
}
</script>

<template>
  <div class="app-shell">
    <aside class="app-shell__sidebar">
      <div class="app-shell__brand">
        <span class="app-shell__brand-mark">T</span>
        <div>
          <strong>Tickets API</strong>
          <small>Panel operativo</small>
        </div>
      </div>

      <nav class="app-shell__nav" aria-label="Navegación principal">
        <RouterLink class="app-shell__nav-link" :to="{ name: 'dashboard' }">
          Dashboard
        </RouterLink>
      </nav>
    </aside>

    <main class="app-shell__main">
      <header class="app-shell__header">
        <div>
          <p class="app-shell__eyebrow">Sistema de soporte</p>
          <h1>Centro de control</h1>
        </div>

        <div class="app-shell__user">
          <div>
            <strong>{{ authStore.user?.fullName }}</strong>
            <small>{{ authStore.userRoleLabel }}</small>
          </div>

          <button class="button button--ghost" type="button" @click="handleLogout">
            Salir
          </button>
        </div>
      </header>

      <section class="app-shell__content">
        <RouterView />
      </section>
    </main>
  </div>
</template>
